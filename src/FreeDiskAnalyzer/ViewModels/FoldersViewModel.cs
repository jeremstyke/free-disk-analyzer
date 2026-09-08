using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class FoldersViewModel : ObservableObject
{
    private readonly ScanResultStore _scanResultStore;

    public ObservableCollection<FolderNode> Folders { get; } = new();

    [ObservableProperty]
    private bool hasScanResult;

    public FoldersViewModel(ScanResultStore scanResultStore)
    {
        _scanResultStore = scanResultStore;
        LoadFromStore();
        _scanResultStore.PropertyChanged += OnStoreChanged;
    }

    private void OnStoreChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ScanResultStore.LatestResult))
        {
            LoadFromStore();
        }
    }

    private void LoadFromStore()
    {
        Folders.Clear();

        var result = _scanResultStore.LatestResult;
        HasScanResult = result is not null;

        if (result is not null)
        {
            foreach (var folder in result.LargestFolders.OrderByDescending(f => f.SizeBytes))
            {
                Folders.Add(folder);
            }
        }
    }

    [RelayCommand]
    private void OpenFolder(FolderNode? folder)
    {
        if (folder is null) return;

        Process.Start(new ProcessStartInfo(folder.FullPath) { UseShellExecute = true });
    }

    [RelayCommand]
    private void ShowInExplorer(FolderNode? folder)
    {
        if (folder is null) return;

        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{folder.FullPath}\"") { UseShellExecute = true });
    }
}
