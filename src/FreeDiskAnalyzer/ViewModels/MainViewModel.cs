using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FreeDiskAnalyzer.Core.Services;
using FreeDiskAnalyzer.Models;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IDriveEnumerator _driveEnumerator;
    private readonly IDiskScanner _diskScanner;
    private readonly ScanResultStore _scanResultStore;

    // Pages are cached per nav key so switching tabs doesn't reload state
    // (e.g. re-enumerate drives) every time.
    private readonly Dictionary<NavKey, object> _pageCache = new();

    public ObservableCollection<NavItem> NavItems { get; }

    [ObservableProperty]
    private NavItem? selectedNavItem;

    [ObservableProperty]
    private object? currentPage;

    public MainViewModel(IDriveEnumerator driveEnumerator, IDiskScanner diskScanner)
    {
        _driveEnumerator = driveEnumerator;
        _diskScanner = diskScanner;
        _scanResultStore = new ScanResultStore();

        NavItems = new ObservableCollection<NavItem>(BuildNavItems());
        SelectedNavItem = NavItems.First();
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
            NavKey.LargeFiles => new LargeFilesViewModel(_scanResultStore),
            NavKey.Folders => new FoldersViewModel(_scanResultStore),
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
        NavKey.Utilities => "Utilities",
        NavKey.Settings => "Settings",
        NavKey.Privacy => "Privacy",
        NavKey.About => "About",
        _ => key.ToString()
    };

    private static IEnumerable<NavItem> BuildNavItems() => new[]
    {
        new NavItem { Key = NavKey.Dashboard, Label = "Dashboard", Glyph = "\uE80F" },
        new NavItem { Key = NavKey.Analyze, Label = "Analyze", Glyph = "\uE721" },
        new NavItem { Key = NavKey.LargeFiles, Label = "Large Files", Glyph = "\uE8A5" },
        new NavItem { Key = NavKey.Folders, Label = "Folders", Glyph = "\uE8B7" },
        new NavItem { Key = NavKey.Utilities, Label = "Utilities", Glyph = "\uE90F" },
        new NavItem { Key = NavKey.Settings, Label = "Settings", Glyph = "\uE713" },
        new NavItem { Key = NavKey.Privacy, Label = "Privacy", Glyph = "\uE72E" },
        new NavItem { Key = NavKey.About, Label = "About", Glyph = "\uE946" }
    };
}
