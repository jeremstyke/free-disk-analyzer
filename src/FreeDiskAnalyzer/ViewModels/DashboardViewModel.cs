using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Core.Services;
using FreeDiskAnalyzer.Core.Utilities;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class DashboardViewModel : ObservableObject
{
    // Affiliate link, disclosed in the UI text right below the button.
    // See AFFILIATE-DISCLOSURE.md.
    public const string NordVpnAffiliateUrl =
        "https://go.nordvpn.net/aff_c?offer_id=15&aff_id=155375&source=Free%20disk%20analyzer";

    public const string BlogUrl = "https://jeremstyke.github.io/free-disk-analyzer/blog/";
    private const int LatestArticleCountPerGroup = 2;
    private const int FetchCount = 10;

    private readonly IDriveEnumerator _driveEnumerator;
    private readonly IBlogFeedService _blogFeedService;
    private readonly ScanResultStore _scanResultStore;

    public ObservableCollection<DriveCardViewModel> Drives { get; } = new();
    public ObservableCollection<BlogPost> LatestReleaseNotes { get; } = new();
    public ObservableCollection<BlogPost> LatestArticles { get; } = new();

    [ObservableProperty]
    private bool hasReleaseNotes;

    [ObservableProperty]
    private bool hasArticles;

    [ObservableProperty]
    private DriveCardViewModel? selectedDrive;

    [ObservableProperty]
    private string lastScanSummary = "No scan yet. Run one from the Analyze tab to see results here.";

    public ObservableCollection<CategoryUsageViewModel> CategoryBreakdown { get; } = new();

    public DashboardViewModel(IDriveEnumerator driveEnumerator, IBlogFeedService blogFeedService, ScanResultStore scanResultStore)
    {
        _driveEnumerator = driveEnumerator;
        _blogFeedService = blogFeedService;
        _scanResultStore = scanResultStore;

        LoadDrives();
        UpdateFromScanResult();
        _scanResultStore.PropertyChanged += OnStoreChanged;

        _ = LoadLatestArticlesAsync();
    }

    private async Task LoadLatestArticlesAsync()
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

    private void OnStoreChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ScanResultStore.LatestResult))
        {
            UpdateFromScanResult();
        }
    }

    private void UpdateFromScanResult()
    {
        var result = _scanResultStore.LatestResult;

        LastScanSummary = result is null
            ? "No scan yet. Run one from the Analyze tab to see results here."
            : $"{result.RootPath}: {result.TotalFilesScanned:N0} files, {result.TotalFoldersScanned:N0} folders, " +
              $"{ByteSizeFormatter.Format(result.TotalBytesScanned)} scanned, completed {result.CompletedAtUtc:g} UTC.";

        CategoryBreakdown.Clear();

        if (result is null || result.BytesByCategory.Count == 0)
        {
            return;
        }

        var maxBytes = result.BytesByCategory.Values.Max();
        if (maxBytes <= 0) return;

        foreach (var (category, bytes) in result.BytesByCategory.OrderByDescending(kvp => kvp.Value))
        {
            if (bytes <= 0) continue;

            CategoryBreakdown.Add(new CategoryUsageViewModel
            {
                Label = category.ToString(),
                BytesDisplay = ByteSizeFormatter.Format(bytes),
                BarPercentage = bytes / (double)maxBytes * 100
            });
        }
    }

    [RelayCommand]
    private void LoadDrives()
    {
        Drives.Clear();

        foreach (var drive in _driveEnumerator.GetAvailableDrives())
        {
            Drives.Add(new DriveCardViewModel(drive));
        }

        SelectedDrive = Drives.FirstOrDefault();
    }

    [RelayCommand]
    private void SelectDrive(DriveCardViewModel? drive)
    {
        if (drive is not null)
        {
            SelectedDrive = drive;
        }
    }

    [RelayCommand]
    private void OpenNordVpn()
    {
        Process.Start(new ProcessStartInfo(NordVpnAffiliateUrl) { UseShellExecute = true });
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
