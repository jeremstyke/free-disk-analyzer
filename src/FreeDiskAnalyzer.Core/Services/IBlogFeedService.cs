using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Core.Services;

public interface IBlogFeedService
{
    /// <summary>
    /// Fetches the latest posts from the Free Disk Analyzer blog's RSS feed.
    /// Fails quietly (returns an empty list) on any network or parsing
    /// problem, this is a nice-to-have, never a reason to disturb the user.
    /// </summary>
    Task<IReadOnlyList<BlogPost>> GetLatestPostsAsync(int count, CancellationToken cancellationToken = default);
}
