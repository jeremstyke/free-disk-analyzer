using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
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
    private readonly IStartupManager _startupManager;
    private readonly IDriverInfoService _driverInfoService;
    private readonly ISettingsService _settingsService;
    private readonly IUpdateChecker _updateChecker;
    private readonly IBlogFeedService _blogFeedService;
    private readonly ISafeDeleteService _safeDeleteService;
    private readonly IBrowserCleaner _browserCleaner;
    private readonly ISystemCleaner _systemCleaner;
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
        IStartupManager startupManager,
        IDriverInfoService driverInfoService,
        ISettingsService settingsService,
        IUpdateChecker updateChecker,
        IBlogFeedService blogFeedService,
        ISafeDeleteService safeDeleteService,
        IBrowserCleaner browserCleaner,
        ISystemCleaner systemCleaner)
    {
        _driveEnumerator = driveEnumerator;
        _diskScanner = diskScanner;
        _duplicateFinder = duplicateFinder;
        _ramOptimizer = ramOptimizer;
        _startupManager = startupManager;
        _driverInfoService = driverInfoService;
        _settingsService = settingsService;
        _updateChecker = updateChecker;
        _blogFeedService = blogFeedService;
        _safeDeleteService = safeDeleteService;
        _browserCleaner = browserCleaner;
        _systemCleaner = systemCleaner;
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
            var tempPath = Path.Combine(Path.GetTempPath(), "PurgeCore-Setup.exe");

            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            var bytes = await client.GetByteArrayAsync(_updateInfo.DownloadUrl);
            await File.WriteAllBytesAsync(tempPath, bytes);

            // The installer is configured (CloseApplications/RestartApplications)
            // to close this app automatically, install over it, and relaunch it.
            Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or UnauthorizedAccessException
                                        or OperationCanceledException or Win32Exception)
        {
            // Covers: network drop or timeout during download (OperationCanceledException,
            // not HttpRequestException), and the installer failing to launch, e.g. the
            // user declined the Windows SmartScreen/UAC prompt (Win32Exception).
            // Previously these were silent, the user saw nothing happen at all.
            MessageBox.Show(
                Resources.Strings.Update_DownloadFailedBody,
                Resources.Strings.Update_DownloadFailedTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
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
            NavKey.Dashboard => new DashboardViewModel(_driveEnumerator, _scanResultStore),
            NavKey.Analyze => new AnalyzeViewModel(_driveEnumerator, _diskScanner, _scanResultStore),
            NavKey.Explore => new ExploreViewModel(_scanResultStore, _driverInfoService),
            NavKey.Cleanup => new CleanupViewModel(_driveEnumerator, _duplicateFinder, _ramOptimizer, _startupManager, _safeDeleteService, _browserCleaner, _systemCleaner, _settingsService, _scanResultStore),
            NavKey.Blog => new BlogViewModel(_blogFeedService),
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
        NavKey.Explore => "Explore",
        NavKey.Cleanup => "Cleanup",
        NavKey.Blog => "Blog",
        NavKey.Settings => "Settings",
        NavKey.Privacy => "Privacy",
        NavKey.About => "About",
        _ => key.ToString()
    };

    private static IEnumerable<NavItem> BuildNavItems()
    {
        var res = Application.Current.Resources;
        return new[]
        {
            new NavItem { Key = NavKey.Dashboard, Label = FreeDiskAnalyzer.Resources.Strings.Nav_Dashboard, Glyph = "\uE80F", IconBrush = (Brush)res["AccentBrush"] },
            new NavItem { Key = NavKey.Analyze, Label = FreeDiskAnalyzer.Resources.Strings.Nav_Analyze, Glyph = "\uE721", IconBrush = (Brush)res["TealBrush"] },
            new NavItem { Key = NavKey.Explore, Label = FreeDiskAnalyzer.Resources.Strings.Nav_Explore, Glyph = "\uE8A5", IconBrush = (Brush)res["VioletBrush"] },
            new NavItem { Key = NavKey.Cleanup, Label = FreeDiskAnalyzer.Resources.Strings.Nav_Cleanup, Glyph = "\uE74D", IconBrush = (Brush)res["AmberBrush"] },
            new NavItem { Key = NavKey.Blog, Label = FreeDiskAnalyzer.Resources.Strings.Nav_Blog, Glyph = "\uE8A9", IconBrush = (Brush)res["RoseBrush"] },
            new NavItem { Key = NavKey.Settings, Label = FreeDiskAnalyzer.Resources.Strings.Nav_Settings, Glyph = "\uE713", IconBrush = (Brush)res["RoseBrush"] },
            new NavItem { Key = NavKey.Privacy, Label = FreeDiskAnalyzer.Resources.Strings.Nav_Privacy, Glyph = "\uE72E", IconBrush = (Brush)res["TealBrush"] },
            new NavItem { Key = NavKey.About, Label = FreeDiskAnalyzer.Resources.Strings.Nav_About, Glyph = "\uE946", IconBrush = (Brush)res["VioletBrush"] }
        };
    }

    [RelayCommand]
    private void OpenCoffee()
    {
        Process.Start(new ProcessStartInfo(AboutViewModel.CoffeeUrl) { UseShellExecute = true });
    }

    [RelayCommand]
    private void ReportBug()
    {
        Process.Start(new ProcessStartInfo(ReportBugUrl) { UseShellExecute = true });
    }

    public const string ReportBugUrl = "mailto:juryjeremy@gmail.com?subject=PurgeCore%20-%20Bug%20report";
}
