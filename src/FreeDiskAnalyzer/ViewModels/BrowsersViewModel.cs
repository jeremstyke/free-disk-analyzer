using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Utilities;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class BrowsersViewModel : ObservableObject
{
    private readonly IBrowserCleaner _browserCleaner;

    public ObservableCollection<BrowserCleanupItemViewModel> Items { get; } = new();

    [ObservableProperty]
    private bool isScanning;

    [ObservableProperty]
    private bool hasScanned;

    [ObservableProperty]
    private bool isCleaning;

    [ObservableProperty]
    private string? resultSummary;

    public BrowsersViewModel(IBrowserCleaner browserCleaner)
    {
        _browserCleaner = browserCleaner;
        _ = ScanAsync();
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        IsScanning = true;
        Items.Clear();

        try
        {
            var results = await _browserCleaner.ScanAsync();
            foreach (var item in results)
            {
                Items.Add(new BrowserCleanupItemViewModel(item));
            }
            HasScanned = true;
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanClean))]
    private async Task CleanSelectedAsync()
    {
        var selected = Items.Where(i => i.IsSelected).ToList();
        if (selected.Count == 0) return;

        var totalSize = selected.Sum(i => i.Item.SizeBytes);

        var confirmed = MessageBox.Show(
            $"Clear {selected.Count} item(s), about {ByteSizeFormatter.Format(totalSize)}?\n" +
            "Close your browsers first for best results, anything still in use will be skipped rather than failing the whole operation.",
            "Clean browsers",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes;

        if (!confirmed) return;

        IsCleaning = true;

        try
        {
            var result = await _browserCleaner.CleanAsync(selected.Select(vm => vm.Item));

            ResultSummary =
                $"Freed about {ByteSizeFormatter.Format(result.BytesFreed)} " +
                $"({result.ItemsCleaned} cleaned, {result.ItemsSkipped} skipped, usually because a browser was open).";

            // Re-scan so the list reflects what's actually left, rather than
            // guessing which items fully succeeded.
            await ScanAsync();
        }
        finally
        {
            IsCleaning = false;
        }
    }

    private bool CanClean() => !IsCleaning;

    partial void OnIsCleaningChanged(bool value) => CleanSelectedCommand.NotifyCanExecuteChanged();
}
