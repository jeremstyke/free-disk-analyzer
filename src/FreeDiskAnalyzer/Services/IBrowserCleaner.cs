using FreeDiskAnalyzer.Models;

namespace FreeDiskAnalyzer.Services;

public interface IBrowserCleaner
{
    /// <summary>
    /// Scans for installed browsers (Chrome, Edge, Firefox) and measures
    /// their cache/cookies/history without deleting anything. Firefox
    /// history is intentionally never offered: in Firefox it's stored in
    /// the same database as bookmarks, deleting one risks the other.
    /// </summary>
    Task<IReadOnlyList<BrowserCleanupItem>> ScanAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes exactly the items passed in, nothing else. Each path is
    /// still re-checked against PathSafetyGuard before deletion. Files
    /// locked by a running browser are skipped, not treated as fatal.
    /// </summary>
    Task<BrowserCleanupResult> CleanAsync(IEnumerable<BrowserCleanupItem> items, CancellationToken cancellationToken = default);
}
