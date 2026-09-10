namespace FreeDiskAnalyzer.Core.Models;

/// <summary>
/// Result of a RAM optimization pass. Numbers are real measurements before
/// and after, not a promised or estimated gain, memory can fluctuate for
/// reasons unrelated to this action.
/// </summary>
public sealed record RamOptimizationResult(
    int ProcessesTrimmed,
    int ProcessesSkipped,
    long AvailableMemoryBeforeBytes,
    long AvailableMemoryAfterBytes)
{
    public long AvailableMemoryDeltaBytes => AvailableMemoryAfterBytes - AvailableMemoryBeforeBytes;
}
