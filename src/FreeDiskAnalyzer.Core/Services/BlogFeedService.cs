using System.Net.Http;
using System.Xml.Linq;
using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Core.Services;

public sealed class BlogFeedService : IBlogFeedService
{
    public const string EnglishFeedUrl = "https://jeremstyke.github.io/purgecore/blog/rss.xml";
    public const string FrenchFeedUrl = "https://jeremstyke.github.io/purgecore/fr/blog/rss.xml";

    public async Task<IReadOnlyList<BlogPost>> GetLatestPostsAsync(string feedUrl, int count, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FreeDiskAnalyzer-App");

            var xml = await client.GetStringAsync(feedUrl, cancellationToken);
            var doc = XDocument.Parse(xml);

            var posts = new List<BlogPost>();

            foreach (var item in doc.Descendants("item"))
            {
                if (posts.Count >= count) break;

                var title = item.Element("title")?.Value;
                var link = item.Element("link")?.Value;
                var category = item.Element("category")?.Value;

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link)) continue;

                posts.Add(new BlogPost(title, link, string.IsNullOrWhiteSpace(category) ? "Guide" : category));
            }

            return posts;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Xml.XmlException)
        {
            return Array.Empty<BlogPost>();
        }
    }
}
