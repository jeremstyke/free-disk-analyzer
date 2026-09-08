using System.Diagnostics;
using System.Security;
using FreeDiskAnalyzer.Core.Internal;
using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Core.Services;

/// <summary>
/// Walks a directory tree and produces a <see cref="ScanResult"/>. Runs
/// entirely on a background thread (via <see cref="Task.Run(Action)"/>) so it
/// never blocks the UI thread. Access-denied, locked files, long paths, and
/// files deleted mid-scan are all handled per-entry, none of them abort the
/// overall scan.
/// </summary>
public sealed class DiskScanner : IDiskScanner
{
    // Bounded memory: only the top N largest files/folders are ever kept.
    private const int TrackerBufferCapacity = 1000;
    private const int TopResultCount = 200;

    private static readonly TimeSpan ProgressReportInterval = TimeSpan.FromMilliseconds(150);

    public Task<ScanResult> ScanAsync(
        string rootPath,
        IProgress<ScanProgress>? progress = null,
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

        // Intentionally not passing cancellationToken to Task.Run itself: if the
        // token were already cancelled, Task.Run would cancel the task before the
        // delegate ever runs, and callers would get a partial/none result via an
        // exception instead of a proper ScanResult with WasCancelled = true.
        return Task.Run(() => ScanInternal(rootPath, progress, cancellationToken));
    }

    private static ScanResult ScanInternal(
        string rootPath,
        IProgress<ScanProgress>? progress,
        CancellationToken cancellationToken)
    {
        var startedAtUtc = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        long totalBytes = 0;
        long totalFiles = 0;
        long totalFolders = 0;
        int accessDeniedCount = 0;
        int errorCount = 0;
        var wasCancelled = false;

        var largestFiles = new TopNTracker<FileEntry>(
            TrackerBufferCapacity, TopResultCount, (a, b) => a.SizeBytes.CompareTo(b.SizeBytes));
        var largestFolders = new TopNTracker<FolderNode>(
            TrackerBufferCapacity, TopResultCount, (a, b) => a.SizeBytes.CompareTo(b.SizeBytes));
        var bytesByCategory = new Dictionary<FileCategory, long>();

        var lastReportElapsed = TimeSpan.Zero;

        void ReportIfDue(string currentPath)
        {
            if (progress is null) return;
            var elapsed = stopwatch.Elapsed;
            if (elapsed - lastReportElapsed < ProgressReportInterval) return;
            lastReportElapsed = elapsed;
            progress.Report(new ScanProgress(currentPath, totalFiles, totalFolders, totalBytes, elapsed));
        }

        long ScanFolder(string path)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<string> entries;
            try
            {
                entries = Directory.EnumerateFileSystemEntries(path);
            }
            catch (UnauthorizedAccessException)
            {
                accessDeniedCount++;
                return 0;
            }
            catch (SecurityException)
            {
                accessDeniedCount++;
                return 0;
            }
            catch (PathTooLongException)
            {
                errorCount++;
                return 0;
            }
            catch (IOException)
            {
                errorCount++;
                return 0;
            }

            long folderSize = 0;

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var attributes = File.GetAttributes(entry);

                    if (attributes.HasFlag(FileAttributes.Directory))
                    {
                        // Skip reparse points (junctions/symlinks) to avoid cycles
                        // and double-counting space that lives elsewhere.
                        if (attributes.HasFlag(FileAttributes.ReparsePoint))
                        {
                            continue;
                        }

                        totalFolders++;
                        var subfolderSize = ScanFolder(entry);
                        folderSize += subfolderSize;

                        var name = Path.GetFileName(entry);
                        largestFolders.Offer(new FolderNode
                        {
                            FullPath = entry,
                            Name = string.IsNullOrEmpty(name) ? entry : name,
                            SizeBytes = subfolderSize
                        });
                    }
                    else
                    {
                        var info = new FileInfo(entry);
                        var size = info.Length;

                        folderSize += size;
                        totalBytes += size;
                        totalFiles++;

                        var extension = info.Extension;
                        var category = FileCategoryClassifier.Classify(extension);
                        bytesByCategory[category] = bytesByCategory.GetValueOrDefault(category) + size;

                        largestFiles.Offer(new FileEntry(
                            entry, info.Name, size, extension, category, SafeGetLastWriteTimeUtc(info)));
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    accessDeniedCount++;
                }
                catch (FileNotFoundException)
                {
                    // Deleted between enumeration and inspection, skip silently.
                }
                catch (PathTooLongException)
                {
                    errorCount++;
                }
                catch (IOException)
                {
                    // Covers locked files and other transient I/O errors.
                    errorCount++;
                }

                ReportIfDue(entry);
            }

            return folderSize;
        }

        try
        {
            totalFolders++; // count the root itself
            ScanFolder(rootPath);
        }
        catch (OperationCanceledException)
        {
            wasCancelled = true;
        }

        return new ScanResult
        {
            RootPath = rootPath,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = DateTime.UtcNow,
            TotalBytesScanned = totalBytes,
            TotalFilesScanned = totalFiles,
            TotalFoldersScanned = totalFolders,
            WasCancelled = wasCancelled,
            AccessDeniedCount = accessDeniedCount,
            ErrorCount = errorCount,
            LargestFolders = largestFolders.GetTopDescending(),
            LargestFiles = largestFiles.GetTopDescending(),
            BytesByCategory = bytesByCategory
        };
    }

    private static DateTime? SafeGetLastWriteTimeUtc(FileInfo info)
    {
        try
        {
            return info.LastWriteTimeUtc;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }
}
