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

                var (success, bytesFreed) = item.Category switch
                {
                    SystemCleanupCategory.TempFiles => CleanTempFiles(),
                    SystemCleanupCategory.RecycleBin => CleanRecycleBin(),
                    _ => (false, 0L)
                };

                if (success)
                {
                    cleaned++;
                    freed += bytesFreed;
                }
                else
                {
                    skipped++;
                }
            }

            return new SystemCleanupResult(freed, cleaned, skipped);
        }, cancellationToken);
    }

    /// <summary>
    /// Deletes each entry in Temp individually and tracks the real, measured
    /// size of only the entries that actually succeeded. Many Temp files are
    /// locked by running programs, so this is not a rare edge case, without
    /// tracking real per-entry success the app could report "freed X GB"
    /// when almost nothing was actually deleted.
    /// </summary>
    private (bool AnySuccess, long BytesFreed) CleanTempFiles()
    {
        var tempPath = Path.GetTempPath();

        if (PathSafetyGuard.IsProtected(tempPath)) return (false, 0);

        var anySuccess = false;
        long freed = 0;

        try
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(tempPath))
            {
                var isDirectory = Directory.Exists(entry);
                var entrySize = isDirectory ? GetFolderSizeBytes(entry) : GetFileSizeBytes(entry);

                var success = isDirectory
                    ? _safeDeleteService.TryDeleteDirectory(entry)
                    : _safeDeleteService.TryDeleteFile(entry);

                if (success)
                {
                    anySuccess = true;
                    freed += entrySize;
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Return whatever succeeded before the error rather than failing
            // the whole operation over one problem entry.
        }

        return (anySuccess, freed);
    }

    /// <summary>
    /// Empties the Recycle Bin and measures the real size difference before
    /// and after, rather than trusting SHEmptyRecycleBin's return code alone,
    /// since "already empty" and some transient failures can both return a
    /// non-zero HRESULT on certain Windows versions.
    /// </summary>
    private static (bool AnySuccess, long BytesFreed) CleanRecycleBin()
    {
        try
        {
            var sizeBefore = GetRecycleBinSizeBytes();
            if (sizeBefore <= 0) return (true, 0);

            SHEmptyRecycleBin(IntPtr.Zero, null, SherbNoconfirmation | SherbNosound);

            var sizeAfter = GetRecycleBinSizeBytes();
            var freed = Math.Max(0, sizeBefore - sizeAfter);

            // Treat "did the size actually go down" as the real signal of
            // success, not the HRESULT, which can be unreliable across
            // Windows versions for this particular API.
            return (freed > 0, freed);
        }
        catch (Exception ex) when (ex is EntryPointNotFoundException or DllNotFoundException)
        {
            return (false, 0);
        }
    }

    private static long GetFileSizeBytes(string path)
    {
        try
        {
            return new FileInfo(path).Length;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return 0;
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
