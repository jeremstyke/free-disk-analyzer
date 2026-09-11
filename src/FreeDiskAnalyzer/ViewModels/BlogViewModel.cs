using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Core.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class BlogViewModel : ObservableObject
{
    public const string BlogUrl = "https://jeremstyke.github.io/purgecore/blog/";
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
            var posts = await _blogFeedService.GetLatestPostsAsync(FetchCount);

            LatestReleaseNotes.Clear();
            LatestArticles.Clear();

            foreach (var post in posts.Where(p => p.Category == "Release notes").Take(LatestArticleCountPerGroup))
            {
                LatestReleaseNotes.Add(post);
            }

            foreach (var post in posts.Where(p => p.Category != "Release notes").Take(LatestArticleCountPerGroup))
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
        Process.Start(new ProcessStartInfo(BlogUrl) { UseShellExecute = true });
    }
}
