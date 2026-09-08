namespace FreeDiskAnalyzer.Core.Models;

/// <summary>
/// A single scanned file. Immutable snapshot taken at scan time.
/// </summary>
public sealed record FileEntry(
    string FullPath,
    string Name,
    long SizeBytes,
    string Extension,
    FileCategory Category,
    DateTime? LastWriteTimeUtc);
