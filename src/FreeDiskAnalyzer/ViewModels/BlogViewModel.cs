using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Core.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class BlogViewModel : ObservableObject
{
    public const string EnglishBlogUrl = "https://jeremstyke.github.io/purgecore/blog/";
    public const string FrenchBlogUrl = "https://jeremstyke.github.io/purgecore/fr/blog/";
    private const int LatestArticleCountPerGroup = 4;
    private const int FetchCount = 12;

    private readonly IBlogFeedService _blogFeedService;

    public ObservableCollection<BlogPost> LatestReleaseNotes { get; } = new();
    public ObservableCollection<BlogPost> LatestArticles { get; } = new();

    [ObservableProperty]
    private bool hasReleaseNotes;

    [ObservableProperty]
    private bool hasArticles;

    [ObservableProperty]
    private bool isLoading;

    private static bool IsFrench => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr";

    /// <summary>Exposed as an instance property so the view can bind to it directly.</summary>
    public bool IsFrenchUi => IsFrench;

    public BlogViewModel(IBlogFeedService blogFeedService)
    {
        _blogFeedService = blogFeedService;
        _ = LoadLatestArticlesAsync();
    }

    [RelayCommand]
    private async Task LoadLatestArticlesAsync()
    {
        IsLoading = true;

        try
        {
            // Release notes only exist in English, that's an editorial choice,
            // not a bug, always pulled from the English feed regardless of
            // the app's language. Guides and comparisons are translated, so
            // in French those come from the French feed instead.
            var releaseNotesTask = _blogFeedService.GetLatestPostsAsync(BlogFeedService.EnglishFeedUrl, FetchCount);
            var articlesFeedUrl = IsFrench ? BlogFeedService.FrenchFeedUrl : BlogFeedService.EnglishFeedUrl;
            var articlesTask = IsFrench
                ? _blogFeedService.GetLatestPostsAsync(articlesFeedUrl, FetchCount)
                : releaseNotesTask;

            await Task.WhenAll(releaseNotesTask, articlesTask);

            LatestReleaseNotes.Clear();
            LatestArticles.Clear();

            foreach (var post in (await releaseNotesTask).Where(p => p.Category == "Release notes").Take(LatestArticleCountPerGroup))
            {
                LatestReleaseNotes.Add(post);
            }

            foreach (var post in (await articlesTask).Where(p => p.Category != "Release notes").Take(LatestArticleCountPerGroup))
            {
                LatestArticles.Add(post);
            }

            HasReleaseNotes = LatestReleaseNotes.Count > 0;
            HasArticles = LatestArticles.Count > 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenArticle(BlogPost? post)
    {
        if (post is null) return;
        Process.Start(new ProcessStartInfo(post.Url) { UseShellExecute = true });
    }

    [RelayCommand]
    private void OpenBlog()
    {
        Process.Start(new ProcessStartInfo(IsFrench ? FrenchBlogUrl : EnglishBlogUrl) { UseShellExecute = true });
    }
}
