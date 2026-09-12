using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Core.Services;

public interface IBlogFeedService
{
    /// <summary>
    /// Fetches the latest posts from a PurgeCore blog RSS feed. Fails
    /// quietly (returns an empty list) on any network or parsing problem,
    /// this is a nice-to-have, never a reason to disturb the user.
    /// </summary>
    /// <param name="feedUrl">Which language's feed to read, English or French, see BlogFeedService's constants.</param>
    Task<IReadOnlyList<BlogPost>> GetLatestPostsAsync(string feedUrl, int count, CancellationToken cancellationToken = default);
}
