using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FreeDiskAnalyzer.Core.Services;
using FreeDiskAnalyzer.Models;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

/// <summary>
/// Groups Old Files, Duplicates, and Empty Folders under one sidebar entry
/// with internal pill-style sub-navigation, instead of three separate
/// top-level tabs. Each sub-page keeps its own view model and state, this
/// just decides which one is currently visible.
/// </summary>
public sealed partial class CleanupViewModel : ObservableObject
{
    public OldFilesViewModel OldFiles { get; }
    public DuplicatesViewModel Duplicates { get; }
    public EmptyFoldersViewModel EmptyFolders { get; }
    public PerformanceViewModel Performance { get; }

    public ObservableCollection<CleanupTab> Tabs { get; }

    [ObservableProperty]
    private CleanupTab? selectedTab;

    [ObservableProperty]
    private object? currentContent;

    public CleanupViewModel(
        IDriveEnumerator driveEnumerator,
        IDuplicateFinder duplicateFinder,
        IRamOptimizer ramOptimizer,
        ScanResultStore scanResultStore)
    {
        OldFiles = new OldFilesViewModel(scanResultStore);
        Duplicates = new DuplicatesViewModel(driveEnumerator, duplicateFinder);
        EmptyFolders = new EmptyFoldersViewModel(scanResultStore);
        Performance = new PerformanceViewModel(ramOptimizer);

        Tabs = new ObservableCollection<CleanupTab>
        {
            new(Resources.Strings.OldFiles_Title, OldFiles),
            new(Resources.Strings.Duplicates_Title, Duplicates),
            new(Resources.Strings.EmptyFolders_Title, EmptyFolders),
            new(Resources.Strings.Performance_Title, Performance)
        };

        SelectedTab = Tabs[0];
    }

    partial void OnSelectedTabChanged(CleanupTab? value) => CurrentContent = value?.Content;
}
