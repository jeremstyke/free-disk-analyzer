namespace FreeDiskAnalyzer.Core.Utilities;

/// <summary>
/// Compares version strings, tolerant of a leading "v" (as used in git tags,
/// e.g. "v1.0.1") on either side.
/// </summary>
public static class ReleaseVersionComparer
{
    public static bool IsNewer(string latestVersion, string currentVersion)
    {
        if (!TryParse(latestVersion, out var latest)) return false;
        if (!TryParse(currentVersion, out var current)) return false;
        return latest > current;
    }

    private static bool TryParse(string value, out Version version) =>
        Version.TryParse(value.TrimStart('v', 'V'), out version!);
}
