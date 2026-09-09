namespace FreeDiskAnalyzer.Core.Models;

/// <summary>Progress snapshot for a duplicate-file scan, which has two phases.</summary>
public sealed record DuplicateScanProgress(
    string Phase, // "Scanning" or "Comparing"
    string CurrentPath,
    long FilesScanned,
    long CandidatesCompared);
