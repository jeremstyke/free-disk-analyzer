using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Core.Services;
using FreeDiskAnalyzer.Models;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IDriveEnumerator _driveEnumerator;
    private readonly IDiskScanner _diskScanner;
    private readonly IDuplicateFinder _duplicateFinder;
    private readonly IRamOptimizer _ramOptimizer;
    private readonly ISettingsService _settingsService;
    private readonly IUpdateChecker _updateChecker;
    private readonly IBlogFeedService _blogFeedService;
    private readonly ScanResultStore _scanResultStore;

    private UpdateInfo? _updateInfo;

    // Pages are cached per nav key so switching tabs doesn't reload state
    // (e.g. re-enumerate drives) every time.
    private readonly Dictionary<NavKey, object> _pageCache = new();

    public ObservableCollection<NavItem> NavItems { get; }

    [ObservableProperty]
    private NavItem? selectedNavItem;

    [ObservableProperty]
    private object? currentPage;

    [ObservableProperty]
    private bool isUpdateBannerVisible;

    [ObservableProperty]
    private string? latestUpdateVersion;

    [ObservableProperty]
    private string? updateBannerText;

    [ObservableProperty]
    private bool isDownloadingUpdate;

    public MainViewModel(
        IDriveEnumerator driveEnumerator,
        IDiskScanner diskScanner,
        IDuplicateFinder duplicateFinder,
        IRamOptimizer ramOptimizer,
        ISettingsService settingsService,
        IUpdateChecker updateChecker,
        IBlogFeedService blogFeedService)
    {
        _driveEnumerator = driveEnumerator;
        _diskScanner = diskScanner;
        _duplicateFinder = duplicateFinder;
        _ramOptimizer = ramOptimizer;
        _settingsService = settingsService;
        _updateChecker = updateChecker;
        _blogFeedService = blogFeedService;
        _scanResultStore = new ScanResultStore();

        NavItems = new ObservableCollection<NavItem>(BuildNavItems());
        SelectedNavItem = NavItems.First();

        _ = CheckForUpdateSilentlyAsync();
    }

    private async Task CheckForUpdateSilentlyAsync()
    {
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
        var result = await _updateChecker.CheckForUpdateAsync(currentVersion);

        if (result.IsUpdateAvailable)
        {
            _updateInfo = result;
            LatestUpdateVersion = result.LatestVersion;
            UpdateBannerText = string.Format(Resources.Strings.Update_Available, result.LatestVersion);
            IsUpdateBannerVisible = true;
        }
    }

    [RelayCommand]
    private void DismissUpdateBanner() => IsUpdateBannerVisible = false;

    [RelayCommand]
    private async Task DownloadAndInstallUpdateAsync()
    {
        if (_updateInfo?.DownloadUrl is null) return;

        IsDownloadingUpdate = true;

        try
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "FreeDiskAnalyzer-Setup.exe");

            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            var bytes = await client.GetByteArrayAsync(_updateInfo.DownloadUrl);
            await File.WriteAllBytesAsync(tempPath, bytes);

            // The installer is configured (CloseApplications/RestartApplications)
            // to close this app automatically, install over it, and relaunch it.
            Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or UnauthorizedAccessException)
        {
            // Best effort: leave the banner up so the user can try again, or
            // fall back to downloading from the website/GitHub manually.
        }
        finally
        {
            IsDownloadingUpdate = false;
        }
    }

    partial void OnSelectedNavItemChanged(NavItem? value)
    {
        if (value is null) return;
        CurrentPage = ResolvePage(value.Key);
    }

    private object ResolvePage(NavKey key)
    {
        if (_pageCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        object page = key switch
        {
            NavKey.Dashboard => new DashboardViewModel(_driveEnumerator, _blogFeedService, _scanResultStore),
            NavKey.Analyze => new AnalyzeViewModel(_driveEnumerator, _diskScanner, _scanResultStore),
            NavKey.LargeFiles => new LargeFilesViewModel(_scanResultStore),
            NavKey.Folders => new FoldersViewModel(_scanResultStore),
            NavKey.Cleanup => new CleanupViewModel(_driveEnumerator, _duplicateFinder, _ramOptimizer, _scanResultStore),
            NavKey.Settings => new SettingsViewModel(_settingsService),
            NavKey.Privacy => new PrivacyViewModel(),
            NavKey.About => new AboutViewModel(),
            _ => new ComingSoonViewModel(GetTitle(key))
        };

        _pageCache[key] = page;
        return page;
    }

    private static string GetTitle(NavKey key) => key switch
    {
        NavKey.Analyze => "Analyze",
        NavKey.LargeFiles => "Large Files",
        NavKey.Folders => "Folders",
        NavKey.Cleanup => "Cleanup",
        NavKey.Settings => "Settings",
        NavKey.Privacy => "Privacy",
        NavKey.About => "About",
        _ => key.ToString()
    };

    private static IEnumerable<NavItem> BuildNavItems() => new[]
    {
        new NavItem { Key = NavKey.Dashboard, Label = FreeDiskAnalyzer.Resources.Strings.Nav_Dashboard, Glyph = "\uE80F" },
        new NavItem { Key = NavKey.Analyze, Label = FreeDiskAnalyzer.Resources.Strings.Nav_Analyze, Glyph = "\uE721" },
        new NavItem { Key = NavKey.LargeFiles, Label = FreeDiskAnalyzer.Resources.Strings.Nav_LargeFiles, Glyph = "\uE8A5" },
        new NavItem { Key = NavKey.Folders, Label = FreeDiskAnalyzer.Resources.Strings.Nav_Folders, Glyph = "\uE8B7" },
        new NavItem { Key = NavKey.Cleanup, Label = FreeDiskAnalyzer.Resources.Strings.Nav_Cleanup, Glyph = "\uE74D" },
        new NavItem { Key = NavKey.Settings, Label = FreeDiskAnalyzer.Resources.Strings.Nav_Settings, Glyph = "\uE713" },
        new NavItem { Key = NavKey.Privacy, Label = FreeDiskAnalyzer.Resources.Strings.Nav_Privacy, Glyph = "\uE72E" },
        new NavItem { Key = NavKey.About, Label = FreeDiskAnalyzer.Resources.Strings.Nav_About, Glyph = "\uE946" }
    };

    [RelayCommand]
    private void OpenCoffee()
    {
        Process.Start(new ProcessStartInfo(AboutViewModel.CoffeeUrl) { UseShellExecute = true });
    }
}
