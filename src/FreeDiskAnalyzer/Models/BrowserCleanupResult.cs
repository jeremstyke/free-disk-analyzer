namespace FreeDiskAnalyzer.Models;

public sealed record BrowserCleanupResult(long BytesFreed, int ItemsCleaned, int ItemsSkipped);
