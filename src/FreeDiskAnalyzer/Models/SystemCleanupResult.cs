namespace FreeDiskAnalyzer.Models;

public sealed record SystemCleanupResult(long BytesFreed, int CategoriesCleaned, int CategoriesSkipped);
