using System.Security;
using System.Security.Cryptography;
using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Core.Services;

public sealed class DuplicateFinder : IDuplicateFinder
{
    // Files smaller than this are skipped. Not worth flagging as duplicates,
    // and it keeps the size-bucket dictionary from filling up with millions
    // of tiny, mostly-irrelevant matches (icons, empty config files, etc.).
    private const long MinSizeBytes = 1024 * 1024; // 1 MB

    // Safety cap on how many candidate files get hashed in the comparison
    // phase, so a pathological folder (huge number of same-sized files)
    // cannot make this run unbounded.
    private const int MaxCandidatesToHash = 20_000;

    private static readonly TimeSpan ProgressReportInterval = TimeSpan.FromMilliseconds(150);

    public Task<IReadOnlyList<DuplicateGroup>> FindDuplicatesAsync(
        string rootPath,
        IProgress<DuplicateScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Root path is required.", nameof(rootPath));
        }

        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"Path not found: {rootPath}");
        }

        return Task.Run(() => FindDuplicatesInternal(rootPath, progress, cancellationToken), CancellationToken.None);
    }

    private static IReadOnlyList<DuplicateGroup> FindDuplicatesInternal(
        string rootPath,
        IProgress<DuplicateScanProgress>? progress,
        CancellationToken cancellationToken)
    {
        var bySize = new Dictionary<long, List<string>>();
        long filesScanned = 0;
        var lastReport = DateTime.UtcNow;

        void ReportScanning(string path)
        {
            if (progress is null) return;
            var now = DateTime.UtcNow;
            if (now - lastReport < ProgressReportInterval) return;
            lastReport = now;
            progress.Report(new DuplicateScanProgress("Scanning", path, filesScanned, 0));
        }

        void WalkFolder(string path)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<string> entries;
            try
            {
                entries = Directory.EnumerateFileSystemEntries(path);
            }
            catch (UnauthorizedAccessException) { return; }
            catch (SecurityException) { return; }
            catch (PathTooLongException) { return; }
            catch (IOException) { return; }

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var attributes = File.GetAttributes(entry);

                    if (attributes.HasFlag(FileAttributes.Directory))
                    {
                        if (attributes.HasFlag(FileAttributes.ReparsePoint)) continue;
                        WalkFolder(entry);
                    }
                    else
                    {
                        var info = new FileInfo(entry);
                        if (info.Length < MinSizeBytes) continue;

                        if (!bySize.TryGetValue(info.Length, out var list))
                        {
                            list = new List<string>();
                            bySize[info.Length] = list;
                        }
                        list.Add(entry);

                        filesScanned++;
                    }
                }
                catch (UnauthorizedAccessException) { }
                catch (FileNotFoundException) { }
                catch (PathTooLongException) { }
                catch (IOException) { }

                ReportScanning(entry);
            }
        }

        WalkFolder(rootPath);

        // Only sizes shared by 2+ files are worth hashing.
        var candidateGroups = bySize.Values.Where(list => list.Count > 1).ToList();

        var results = new List<DuplicateGroup>();
        long candidatesCompared = 0;
        var hashedSoFar = 0;

        foreach (var candidates in candidateGroups)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (hashedSoFar >= MaxCandidatesToHash)
            {
                break;
            }

            var byHash = new Dictionary<string, List<string>>();

            foreach (var path in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (hashedSoFar >= MaxCandidatesToHash) break;
                hashedSoFar++;
                candidatesCompared++;

                var hash = TryComputeHash(path);
                if (hash is null) continue;

                if (!byHash.TryGetValue(hash, out var list))
                {
                    list = new List<string>();
                    byHash[hash] = list;
                }
                list.Add(path);

                if (progress is not null)
                {
                    var now = DateTime.UtcNow;
                    if (now - lastReport >= ProgressReportInterval)
                    {
                        lastReport = now;
                        progress.Report(new DuplicateScanProgress("Comparing", path, filesScanned, candidatesCompared));
                    }
                }
            }

            foreach (var group in byHash.Values)
            {
                if (group.Count < 2) continue;

                var size = new FileInfo(group[0]).Length;
                results.Add(new DuplicateGroup { SizeBytes = size, FilePaths = group });
            }
        }

        return results.OrderByDescending(g => g.WastedBytes).ToList();
    }

    private static string? TryComputeHash(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            var hashBytes = SHA256.HashData(stream);
            return Convert.ToHexString(hashBytes);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
        {
            return null;
        }
    }
}
