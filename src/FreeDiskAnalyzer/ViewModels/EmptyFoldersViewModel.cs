using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Core.Utilities;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class EmptyFoldersViewModel : ObservableObject
{
    private readonly ScanResultStore _scanResultStore;
    private readonly ISafeDeleteService _safeDeleteService;

    public ObservableCollection<FolderNode> Folders { get; } = new();

    [ObservableProperty]
    private bool hasScanResult;

    public EmptyFoldersViewModel(ScanResultStore scanResultStore, ISafeDeleteService safeDeleteService)
    {
        _scanResultStore = scanResultStore;
        _safeDeleteService = safeDeleteService;
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
            foreach (var folder in result.EmptyFolders)
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

    [RelayCommand]
    private void DeleteFolder(FolderNode? folder)
    {
        if (folder is null) return;
        if (!PathSafetyGuard.IsSafeToDelete(folder.FullPath)) return;

        var confirmed = MessageBox.Show(
            $"Delete this empty folder?\n{folder.FullPath}\nIt goes to the Recycle Bin, not permanently deleted.",
            "Delete empty folder",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes;

        if (!confirmed) return;

        if (_safeDeleteService.TryDeleteDirectory(folder.FullPath))
        {
            Folders.Remove(folder);
        }
        else
        {
            MessageBox.Show(
                "Couldn't delete this folder. It may be in use by another program.",
                "Delete empty folder",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
