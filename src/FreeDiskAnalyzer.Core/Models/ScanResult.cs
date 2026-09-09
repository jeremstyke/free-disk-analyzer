namespace FreeDiskAnalyzer.Core.Models;

/// <summary>
/// Final outcome of a disk scan. Never contains more than a bounded number
/// of largest files/folders, results are pre-trimmed during the scan so the
/// whole tree is never kept in memory.
/// </summary>
public sealed class ScanResult
{
    public required string RootPath { get; init; }
    public DateTime StartedAtUtc { get; init; }
    public DateTime CompletedAtUtc { get; init; }

    public long TotalBytesScanned { get; init; }
    public long TotalFilesScanned { get; init; }
    public long TotalFoldersScanned { get; init; }

    public bool WasCancelled { get; init; }

    /// <summary>Number of entries skipped due to access being denied.</summary>
    public int AccessDeniedCount { get; init; }

    /// <summary>Number of entries skipped due to other I/O errors (locked files, long paths, deleted mid-scan, etc.).</summary>
    public int ErrorCount { get; init; }

    public IReadOnlyList<FolderNode> LargestFolders { get; init; } = Array.Empty<FolderNode>();
    public IReadOnlyList<FileEntry> LargestFiles { get; init; } = Array.Empty<FileEntry>();
    public IReadOnlyList<FileEntry> OldestFiles { get; init; } = Array.Empty<FileEntry>();
    public IReadOnlyList<FolderNode> EmptyFolders { get; init; } = Array.Empty<FolderNode>();
    public IReadOnlyDictionary<FileCategory, long> BytesByCategory { get; init; } =
        new Dictionary<FileCategory, long>();
}
