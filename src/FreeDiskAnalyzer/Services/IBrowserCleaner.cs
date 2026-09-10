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
    /// <param name="cookieWhitelist">
    /// Domains to keep when cleaning a Cookies-category item (matched as a
    /// suffix, so "example.com" also protects "www.example.com"). When
    /// non-empty, cookie cleanup edits the browser's cookie database in
    /// place (deletes matching rows) instead of deleting the whole file.
    /// This touches another program's private database file based on an
    /// assumed schema, less certain to work across every browser version
    /// than the rest of this app, failures are skipped rather than risking
    /// a corrupt file.
    /// </param>
    Task<BrowserCleanupResult> CleanAsync(
        IEnumerable<BrowserCleanupItem> items,
        IReadOnlyList<string>? cookieWhitelist = null,
        CancellationToken cancellationToken = default);
}
