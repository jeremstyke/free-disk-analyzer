namespace FreeDiskAnalyzer.Core.Models;

/// <summary>
/// A scanned folder with its aggregated size. Size includes all files and
/// subfolders beneath it, computed during the scan.
/// </summary>
public sealed class FolderNode
{
    public required string FullPath { get; init; }
    public required string Name { get; init; }
    public long SizeBytes { get; init; }
}
