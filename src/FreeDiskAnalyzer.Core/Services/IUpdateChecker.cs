using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Core.Services;

public interface IUpdateChecker
{
    /// <summary>
    /// Checks GitHub Releases for a version newer than <paramref name="currentVersion"/>.
    /// Fails quietly (returns <see cref="UpdateInfo.NoUpdate"/>) on any network
    /// or parsing problem, this is a background convenience check, not core
    /// functionality, it should never be the reason the app has a problem.
    /// </summary>
    Task<UpdateInfo> CheckForUpdateAsync(string currentVersion, CancellationToken cancellationToken = default);
}
