namespace FreeDiskAnalyzer.Core.Models;

/// <summary>
/// A set of files with identical content (same size and hash). All files in
/// the group are the same size, so <see cref="WastedBytes"/> is the space
/// that would be reclaimed by keeping only one copy.
/// </summary>
public sealed class DuplicateGroup
{
    public required long SizeBytes { get; init; }
    public required IReadOnlyList<string> FilePaths { get; init; }

    public long WastedBytes => SizeBytes * Math.Max(FilePaths.Count - 1, 0);
}
