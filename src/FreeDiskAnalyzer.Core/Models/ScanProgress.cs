namespace FreeDiskAnalyzer.Core.Models;

/// <summary>
/// A progress snapshot reported periodically while a scan is running.
/// Consumed by the UI to show a live progress view (path being scanned,
/// counts, elapsed time) without blocking the scan itself.
/// </summary>
public sealed record ScanProgress(
    string CurrentPath,
    long FilesScanned,
    long FoldersScanned,
    long BytesScanned,
    TimeSpan Elapsed);
