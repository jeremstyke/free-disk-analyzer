using FreeDiskAnalyzer.Models;

namespace FreeDiskAnalyzer.Services;

public interface ISystemCleaner
{
    /// <summary>Measures the user's Temp folder and the Recycle Bin, deletes nothing.</summary>
    Task<IReadOnlyList<SystemCleanupItem>> ScanAsync(CancellationToken cancellationToken = default);

    /// <summary>Cleans exactly the categories passed in.</summary>
    Task<SystemCleanupResult> CleanAsync(IEnumerable<SystemCleanupItem> items, CancellationToken cancellationToken = default);
}
