using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Core.Services;
using FreeDiskAnalyzer.Core.Utilities;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class AnalyzeViewModel : ObservableObject
{
    private readonly IDiskScanner _diskScanner;
    private readonly ScanResultStore _scanResultStore;
    private CancellationTokenSource? _cts;

    public ObservableCollection<DriveInfoModel> Drives { get; } = new();

    [ObservableProperty]
    private DriveInfoModel? selectedDrive;

    [ObservableProperty]
    private bool isScanning;

    [ObservableProperty]
    private string currentPath = string.Empty;

    [ObservableProperty]
    private long filesScanned;

    [ObservableProperty]
    private long foldersScanned;

    [ObservableProperty]
    private string bytesScannedDisplay = "0 B";

    [ObservableProperty]
    private string elapsedDisplay = "00:00";

    [ObservableProperty]
    private string? completionMessage;

    public AnalyzeViewModel(IDriveEnumerator driveEnumerator, IDiskScanner diskScanner, ScanResultStore scanResultStore)
    {
        _diskScanner = diskScanner;
        _scanResultStore = scanResultStore;

        foreach (var drive in driveEnumerator.GetAvailableDrives())
        {
            Drives.Add(drive);
        }

        SelectedDrive = Drives.FirstOrDefault();
    }

    private bool CanStartScan() => !IsScanning && SelectedDrive is not null;

    [RelayCommand(CanExecute = nameof(CanStartScan))]
    private async Task StartScanAsync()
    {
        if (SelectedDrive is null)
        {
            return;
        }

        IsScanning = true;
        CompletionMessage = null;
        FilesScanned = 0;
        FoldersScanned = 0;
        BytesScannedDisplay = "0 B";
        ElapsedDisplay = "00:00";
        CurrentPath = SelectedDrive.RootPath;

        _cts = new CancellationTokenSource();
        var progress = new Progress<ScanProgress>(OnProgress);

        try
        {
            var result = await _diskScanner.ScanAsync(SelectedDrive.RootPath, progress, _cts.Token);
            _scanResultStore.LatestResult = result;

            CompletionMessage = result.WasCancelled
                ? "Scan cancelled. Partial results are shown in Large Files and Folders."
                : $"Scan complete: {result.TotalFilesScanned:N0} files, {result.TotalFoldersScanned:N0} folders, " +
                  $"{ByteSizeFormatter.Format(result.TotalBytesScanned)} scanned.";
        }
        catch (DirectoryNotFoundException)
        {
            CompletionMessage = "This drive is no longer available.";
        }
        finally
        {
            IsScanning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private bool CanCancelScan() => IsScanning;

    [RelayCommand(CanExecute = nameof(CanCancelScan))]
    private void CancelScan() => _cts?.Cancel();

    private void OnProgress(ScanProgress progress)
    {
        CurrentPath = progress.CurrentPath;
        FilesScanned = progress.FilesScanned;
        FoldersScanned = progress.FoldersScanned;
        BytesScannedDisplay = ByteSizeFormatter.Format(progress.BytesScanned);
        ElapsedDisplay = progress.Elapsed.ToString(@"mm\:ss");
    }

    partial void OnIsScanningChanged(bool value)
    {
        StartScanCommand.NotifyCanExecuteChanged();
        CancelScanCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedDriveChanged(DriveInfoModel? value) => StartScanCommand.NotifyCanExecuteChanged();
}
