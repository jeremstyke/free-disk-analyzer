namespace FreeDiskAnalyzer.Core.Models;

/// <summary>Result of checking GitHub Releases for a newer version.</summary>
public sealed record UpdateInfo(bool IsUpdateAvailable, string? LatestVersion, string? DownloadUrl, string? ReleaseUrl)
{
    public static UpdateInfo NoUpdate { get; } = new(false, null, null, null);
}
