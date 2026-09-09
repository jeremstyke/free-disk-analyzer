using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class OldFilesViewModel : ObservableObject
{
    private readonly ScanResultStore _scanResultStore;

    public ObservableCollection<FileEntry> Files { get; } = new();

    [ObservableProperty]
    private bool hasScanResult;

    public OldFilesViewModel(ScanResultStore scanResultStore)
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
        Files.Clear();

        var result = _scanResultStore.LatestResult;
        HasScanResult = result is not null;

        if (result is not null)
        {
            foreach (var file in result.OldestFiles)
            {
                Files.Add(file);
            }
        }
    }

    [RelayCommand]
    private void OpenFile(FileEntry? file)
    {
        if (file is null) return;

        try
        {
            Process.Start(new ProcessStartInfo(file.FullPath) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or System.IO.FileNotFoundException)
        {
            // File may have been moved or deleted since the scan, ignore.
        }
    }

    [RelayCommand]
    private void ShowInExplorer(FileEntry? file)
    {
        if (file is null) return;

        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{file.FullPath}\"") { UseShellExecute = true });
    }
}
