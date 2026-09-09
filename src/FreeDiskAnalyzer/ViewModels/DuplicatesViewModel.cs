using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Core.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class DuplicatesViewModel : ObservableObject
{
    private readonly IDuplicateFinder _duplicateFinder;
    private CancellationTokenSource? _cts;

    public ObservableCollection<DriveInfoModel> Drives { get; } = new();
    public ObservableCollection<DuplicateGroup> Results { get; } = new();

    [ObservableProperty]
    private DriveInfoModel? selectedDrive;

    [ObservableProperty]
    private bool isScanning;

    [ObservableProperty]
    private bool hasRunOnce;

    [ObservableProperty]
    private string currentPath = string.Empty;

    public DuplicatesViewModel(IDriveEnumerator driveEnumerator, IDuplicateFinder duplicateFinder)
    {
        _duplicateFinder = duplicateFinder;

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
        if (SelectedDrive is null) return;

        IsScanning = true;
        Results.Clear();
        CurrentPath = SelectedDrive.RootPath;

        _cts = new CancellationTokenSource();
        var progress = new Progress<DuplicateScanProgress>(p => CurrentPath = p.CurrentPath);

        try
        {
            var groups = await _duplicateFinder.FindDuplicatesAsync(SelectedDrive.RootPath, progress, _cts.Token);
            foreach (var group in groups)
            {
                Results.Add(group);
            }
            HasRunOnce = true;
        }
        catch (OperationCanceledException)
        {
            // Cancelled by the user, leave partial (empty) results rather than throwing.
        }
        catch (DirectoryNotFoundException)
        {
            // Drive became unavailable, nothing to show.
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

    partial void OnIsScanningChanged(bool value)
    {
        StartScanCommand.NotifyCanExecuteChanged();
        CancelScanCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedDriveChanged(DriveInfoModel? value) => StartScanCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private void ShowInExplorer(string? path)
    {
        if (string.IsNullOrEmpty(path)) return;

        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true });
    }
}
