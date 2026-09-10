using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FreeDiskAnalyzer.Core.Services;
using FreeDiskAnalyzer.Models;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

/// <summary>
/// Groups Duplicates, Empty Folders, Performance, Browsers, and System under
/// one sidebar entry with internal pill-style sub-navigation. Everything
/// here can delete or change something (files, RAM, browser data, temp
/// files), that's what separates this tab from Explore, which is purely
/// read-only browsing.
/// </summary>
public sealed partial class CleanupViewModel : ObservableObject
{
    public DuplicatesViewModel Duplicates { get; }
    public EmptyFoldersViewModel EmptyFolders { get; }
    public PerformanceViewModel Performance { get; }
    public BrowsersViewModel Browsers { get; }
    public SystemViewModel System { get; }

    public ObservableCollection<CleanupTab> Tabs { get; }

    [ObservableProperty]
    private CleanupTab? selectedTab;

    [ObservableProperty]
    private object? currentContent;

    public CleanupViewModel(
        IDriveEnumerator driveEnumerator,
        IDuplicateFinder duplicateFinder,
        IRamOptimizer ramOptimizer,
        ISafeDeleteService safeDeleteService,
        IBrowserCleaner browserCleaner,
        ISystemCleaner systemCleaner,
        ISettingsService settingsService,
        ScanResultStore scanResultStore)
    {
        Duplicates = new DuplicatesViewModel(driveEnumerator, duplicateFinder, safeDeleteService);
        EmptyFolders = new EmptyFoldersViewModel(scanResultStore, safeDeleteService);
        Performance = new PerformanceViewModel(ramOptimizer);
        Browsers = new BrowsersViewModel(browserCleaner, settingsService);
        System = new SystemViewModel(systemCleaner);

        Tabs = new ObservableCollection<CleanupTab>
        {
            new(Resources.Strings.Duplicates_Title, Duplicates),
            new(Resources.Strings.EmptyFolders_Title, EmptyFolders),
            new(Resources.Strings.Performance_Title, Performance),
            new(Resources.Strings.Browsers_Title, Browsers),
            new(Resources.Strings.System_Title, System)
        };

        SelectedTab = Tabs[0];
    }

    partial void OnSelectedTabChanged(CleanupTab? value) => CurrentContent = value?.Content;
}
