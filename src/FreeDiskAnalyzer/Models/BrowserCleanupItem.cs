namespace FreeDiskAnalyzer.Models;

/// <summary>
/// One cleanable item found during a browser scan: a specific category
/// (cache/cookies/history) for a specific browser, with the exact files or
/// folders that would be deleted and their total size. Nothing is deleted
/// until the user confirms.
/// </summary>
public sealed class BrowserCleanupItem
{
    public required string BrowserName { get; init; }
    public required BrowserCleanupCategory Category { get; init; }
    public required IReadOnlyList<string> Paths { get; init; }
    public required long SizeBytes { get; init; }
}
