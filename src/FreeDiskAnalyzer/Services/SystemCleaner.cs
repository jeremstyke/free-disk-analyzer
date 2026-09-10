using System.IO;
using System.Runtime.InteropServices;
using FreeDiskAnalyzer.Core.Utilities;
using FreeDiskAnalyzer.Models;

namespace FreeDiskAnalyzer.Services;

public sealed class SystemCleaner : ISystemCleaner
{
    private readonly ISafeDeleteService _safeDeleteService;

    public SystemCleaner(ISafeDeleteService safeDeleteService)
    {
        _safeDeleteService = safeDeleteService;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHQueryRecycleBin(string? pszRootPath, ref ShQueryRbInfo pSHQueryRBInfo);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ShQueryRbInfo
    {
        public int cbSize;
        public long i64Size;
        public long i64NumItems;
    }

    private const uint SherbNoconfirmation = 0x00000001;
    private const uint SherbNosound = 0x00000004;

    public Task<IReadOnlyList<SystemCleanupItem>> ScanAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var items = new List<SystemCleanupItem>();

            var tempSize = GetFolderSizeBytes(Path.GetTempPath());
            items.Add(new SystemCleanupItem { Category = SystemCleanupCategory.TempFiles, SizeBytes = tempSize });

            var recycleBinSize = GetRecycleBinSizeBytes();
            items.Add(new SystemCleanupItem { Category = SystemCleanupCategory.RecycleBin, SizeBytes = recycleBinSize });

            return (IReadOnlyList<SystemCleanupItem>)items;
        }, cancellationToken);
    }

    public Task<SystemCleanupResult> CleanAsync(IEnumerable<SystemCleanupItem> items, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            long freed = 0;
            var cleaned = 0;
            var skipped = 0;

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var success = item.Category switch
                {
                    SystemCleanupCategory.TempFiles => CleanTempFiles(),
                    SystemCleanupCategory.RecycleBin => CleanRecycleBin(),
                    _ => false
                };

                if (success)
                {
                    cleaned++;
                    freed += item.SizeBytes;
                }
                else
                {
                    skipped++;
                }
            }

            return new SystemCleanupResult(freed, cleaned, skipped);
        }, cancellationToken);
    }

    private bool CleanTempFiles()
    {
        var tempPath = Path.GetTempPath();

        // The Temp folder itself is never deleted, only its contents, and
        // only entries outside any protected root (defense in depth, this
        // path is never under Windows/Program Files in practice).
        if (PathSafetyGuard.IsProtected(tempPath)) return false;

        var anyAttempted = false;

        try
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(tempPath))
            {
                anyAttempted = true;

                if (Directory.Exists(entry))
                {
                    _safeDeleteService.TryDeleteDirectory(entry);
                }
                else
                {
                    _safeDeleteService.TryDeleteFile(entry);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return anyAttempted;
        }

        return true;
    }

    private static bool CleanRecycleBin()
    {
        try
        {
            // 0 = success. Other HRESULTs can mean "nothing to empty" on some
            // Windows versions, treat any outcome as best effort rather than
            // a hard failure the user needs to act on.
            SHEmptyRecycleBin(IntPtr.Zero, null, SherbNoconfirmation | SherbNosound);
            return true;
        }
        catch (Exception ex) when (ex is EntryPointNotFoundException or DllNotFoundException)
        {
            return false;
        }
    }

    private static long GetRecycleBinSizeBytes()
    {
        try
        {
            var info = new ShQueryRbInfo { cbSize = Marshal.SizeOf<ShQueryRbInfo>() };
            var hr = SHQueryRecycleBin(null, ref info);
            return hr == 0 ? info.i64Size : 0;
        }
        catch (Exception ex) when (ex is EntryPointNotFoundException or DllNotFoundException)
        {
            return 0;
        }
    }

    private static long GetFolderSizeBytes(string folderPath)
    {
        long total = 0;

        try
        {
            foreach (var file in Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    total += new FileInfo(file).Length;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    // Skip files we can't read the size of, don't fail the whole scan.
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Return whatever was accumulated before the error.
        }

        return total;
    }
}
