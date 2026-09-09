namespace FreeDiskAnalyzer.Core.Models;

/// <summary>
/// A scanned folder with its aggregated size. Size, file count, and folder
/// count all include everything beneath it (recursive), computed during
/// the scan.
/// </summary>
public sealed class FolderNode
{
    public required string FullPath { get; init; }
    public required string Name { get; init; }
    public long SizeBytes { get; init; }

    /// <summary>Total files anywhere beneath this folder.</summary>
    public int FileCount { get; init; }

    /// <summary>Total subfolders anywhere beneath this folder.</summary>
    public int SubfolderCount { get; init; }

    /// <summary>True when this folder and everything beneath it contains no files at all.</summary>
    public bool IsEmpty => FileCount == 0;
}
