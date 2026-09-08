using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Core.Services;

public interface IDiskScanner
{
    /// <summary>
    /// Scans <paramref name="rootPath"/> recursively. Never blocks the calling
    /// thread's synchronization context: the actual walk runs on a background
    /// thread. Reports progress periodically via <paramref name="progress"/>
    /// and stops promptly when <paramref name="cancellationToken"/> is
    /// cancelled, returning a partial result with <c>WasCancelled = true</c>
    /// rather than throwing.
    /// </summary>
    Task<ScanResult> ScanAsync(
        string rootPath,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
