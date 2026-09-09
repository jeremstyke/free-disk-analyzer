using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Models;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class LargeFilesViewModel : ObservableObject
{
    private readonly ScanResultStore _scanResultStore;
    private readonly List<FileEntry> _allFiles = new();

    public ObservableCollection<FileEntry> FilteredFiles { get; } = new();

    public IReadOnlyList<SizeFilterOption> FilterOptions { get; } = new[]
    {
        new SizeFilterOption("100 MB", 100L * 1024 * 1024),
        new SizeFilterOption("500 MB", 500L * 1024 * 1024),
        new SizeFilterOption("1 GB", 1024L * 1024 * 1024),
        new SizeFilterOption("5 GB", 5L * 1024 * 1024 * 1024),
        new SizeFilterOption("Custom", -1)
    };

    [ObservableProperty]
    private SizeFilterOption selectedFilter;

    [ObservableProperty]
    private double customThresholdMb = 100;

    [ObservableProperty]
    private bool hasScanResult;

    public LargeFilesViewModel(ScanResultStore scanResultStore)
    {
        _scanResultStore = scanResultStore;
        selectedFilter = FilterOptions[0];

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
        _allFiles.Clear();

        var result = _scanResultStore.LatestResult;
        HasScanResult = result is not null;

        if (result is not null)
        {
            _allFiles.AddRange(result.LargestFiles);
        }

        ApplyFilter();
    }

    partial void OnSelectedFilterChanged(SizeFilterOption value) => ApplyFilter();

    partial void OnCustomThresholdMbChanged(double value) => ApplyFilter();

    private void ApplyFilter()
    {
        var thresholdBytes = SelectedFilter.Bytes >= 0
            ? SelectedFilter.Bytes
            : (long)(CustomThresholdMb * 1024 * 1024);

        FilteredFiles.Clear();

        foreach (var file in _allFiles.Where(f => f.SizeBytes >= thresholdBytes).OrderByDescending(f => f.SizeBytes))
        {
            FilteredFiles.Add(file);
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
        catch (Exception ex) when (ex is Win32Exception or FileNotFoundException)
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
