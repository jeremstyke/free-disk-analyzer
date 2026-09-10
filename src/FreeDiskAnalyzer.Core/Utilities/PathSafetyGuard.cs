namespace FreeDiskAnalyzer.Core.Utilities;

/// <summary>
/// Last line of defense before any delete operation anywhere in the app:
/// refuses to consider a path safe if it falls under Windows, Program
/// Files, or Program Files (x86), regardless of what feature or code path
/// produced that path. This does not replace per-feature scoping (only
/// well-defined categories like confirmed-empty folders, selected
/// duplicates, or known browser cache paths are ever offered for deletion
/// in the first place), it's a guardrail in case of a bug elsewhere.
/// </summary>
public static class PathSafetyGuard
{
    private static readonly string[] ProtectedRoots = BuildProtectedRoots();

    private static string[] BuildProtectedRoots()
    {
        var candidates = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
        };

        return candidates.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
    }

    /// <summary>True if the path is under a protected system directory and must never be deleted.</summary>
    public static bool IsProtected(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return true;

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            // Unparsable path: treat as protected rather than risk deleting something unexpected.
            return true;
        }

        return ProtectedRoots.Any(root => fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsSafeToDelete(string path) => !IsProtected(path);
}
