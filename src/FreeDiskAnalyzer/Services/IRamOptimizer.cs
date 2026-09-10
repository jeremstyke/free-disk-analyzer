using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Services;

public interface IRamOptimizer
{
    /// <summary>
    /// Trims the working set of accessible processes, asking Windows to
    /// reclaim memory they aren't actively using. Runs on a background
    /// thread. Many processes will be skipped (no permission to touch
    /// another user's or a protected system process), that's expected and
    /// not an error.
    /// </summary>
    Task<RamOptimizationResult> OptimizeAsync(CancellationToken cancellationToken = default);
}
