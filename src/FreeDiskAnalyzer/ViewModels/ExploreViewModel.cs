using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FreeDiskAnalyzer.Models;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

/// <summary>
/// Groups Large Files, Folders, and Old Files under one sidebar entry with
/// internal pill-style sub-navigation. All three are read-only browsing
/// views of the last scan, none of them delete anything, that's what
/// separates this tab from Cleanup.
/// </summary>
public sealed partial class ExploreViewModel : ObservableObject
{
    public LargeFilesViewModel LargeFiles { get; }
    public FoldersViewModel Folders { get; }
    public OldFilesViewModel OldFiles { get; }
    public DriversViewModel Drivers { get; }

    public ObservableCollection<CleanupTab> Tabs { get; }

    [ObservableProperty]
    private CleanupTab? selectedTab;

    [ObservableProperty]
    private object? currentContent;

    public ExploreViewModel(ScanResultStore scanResultStore, IDriverInfoService driverInfoService)
    {
        LargeFiles = new LargeFilesViewModel(scanResultStore);
        Folders = new FoldersViewModel(scanResultStore);
        OldFiles = new OldFilesViewModel(scanResultStore);
        Drivers = new DriversViewModel(driverInfoService);

        Tabs = new ObservableCollection<CleanupTab>
        {
            new(Resources.Strings.LargeFiles_Title, LargeFiles),
            new(Resources.Strings.Folders_Title, Folders),
            new(Resources.Strings.OldFiles_Title, OldFiles),
            new(Resources.Strings.Drivers_Title, Drivers)
        };

        SelectedTab = Tabs[0];
    }

    partial void OnSelectedTabChanged(CleanupTab? value) => CurrentContent = value?.Content;
}
