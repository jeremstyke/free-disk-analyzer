using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Core.Services;
using FreeDiskAnalyzer.Core.Utilities;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class DuplicatesViewModel : ObservableObject
{
    private readonly IDuplicateFinder _duplicateFinder;
    private readonly ISafeDeleteService _safeDeleteService;
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

    public DuplicatesViewModel(IDriveEnumerator driveEnumerator, IDuplicateFinder duplicateFinder, ISafeDeleteService safeDeleteService)
    {
        _duplicateFinder = duplicateFinder;
        _safeDeleteService = safeDeleteService;

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

    [RelayCommand]
    private void DeleteGroup(DuplicateGroup? group)
    {
        if (group is null || group.FilePaths.Count < 2) return;

        var toDelete = group.FilePaths.Skip(1).ToList();

        var confirmed = MessageBox.Show(
            $"Delete {toDelete.Count} of {group.FilePaths.Count} copies (keeping one)?\n" +
            $"This will free about {ByteSizeFormatter.Format(group.SizeBytes * toDelete.Count)}.\n" +
            "Files go to the Recycle Bin, not permanently deleted.",
            "Delete duplicates",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes;

        if (!confirmed) return;

        var (deleted, total) = DeleteGroupFiles(group);

        if (deleted == total)
        {
            Results.Remove(group);
        }
        else
        {
            MessageBox.Show(
                $"Deleted {deleted} of {total} files. Some may be open in another program.",
                "Delete duplicates",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void DeleteAllGroups()
    {
        var groups = Results.Where(g => g.FilePaths.Count >= 2).ToList();
        if (groups.Count == 0) return;

        var totalFiles = groups.Sum(g => g.FilePaths.Count - 1);
        var totalSize = groups.Sum(g => g.SizeBytes * (g.FilePaths.Count - 1));

        var confirmed = MessageBox.Show(
            $"Delete {totalFiles} extra copies across {groups.Count} duplicate groups (keeping one copy of each)?\n" +
            $"This will free about {ByteSizeFormatter.Format(totalSize)}.\n" +
            "Files go to the Recycle Bin, not permanently deleted.",
            "Delete all duplicates",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes;

        if (!confirmed) return;

        var deletedGroups = 0;
        var skippedFiles = 0;

        foreach (var group in groups)
        {
            var (deleted, total) = DeleteGroupFiles(group);
            skippedFiles += total - deleted;

            if (deleted == total)
            {
                Results.Remove(group);
                deletedGroups++;
            }
        }

        if (skippedFiles > 0)
        {
            MessageBox.Show(
                $"Cleaned {deletedGroups} of {groups.Count} groups. {skippedFiles} file(s) were skipped, usually because they're open in another program.",
                "Delete all duplicates",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    /// <summary>Deletes the extra copies for one group (already-confirmed). Always keeps the first file.</summary>
    private (int Deleted, int Total) DeleteGroupFiles(DuplicateGroup group)
    {
        var toDelete = group.FilePaths.Skip(1).ToList();
        var deletedCount = 0;

        foreach (var path in toDelete)
        {
            if (!PathSafetyGuard.IsSafeToDelete(path)) continue;
            if (_safeDeleteService.TryDeleteFile(path)) deletedCount++;
        }

        return (deletedCount, toDelete.Count);
    }
}
