namespace FreeDiskAnalyzer.Core.Utilities;

/// <summary>
/// Formats a byte count into a short human-readable string (e.g. "512.3 GB").
/// Uses binary (1024-based) units, the convention Windows itself uses for
/// drive and file sizes.
/// </summary>
public static class ByteSizeFormatter
{
    private static readonly string[] Units = { "B", "KB", "MB", "GB", "TB", "PB" };

    public static string Format(long bytes)
    {
        if (bytes <= 0)
        {
            return "0 B";
        }

        double size = bytes;
        var unitIndex = 0;

        while (size >= 1024 && unitIndex < Units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.#} {Units[unitIndex]}";
    }
}
