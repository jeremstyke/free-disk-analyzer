using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Utilities;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class SystemViewModel : ObservableObject
{
    private readonly ISystemCleaner _systemCleaner;

    public ObservableCollection<SystemCleanupItemViewModel> Items { get; } = new();

    [ObservableProperty]
    private bool isScanning;

    [ObservableProperty]
    private bool isCleaning;

    [ObservableProperty]
    private string? resultSummary;

    public SystemViewModel(ISystemCleaner systemCleaner)
    {
        _systemCleaner = systemCleaner;
        _ = ScanAsync();
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        IsScanning = true;
        Items.Clear();

        try
        {
            var results = await _systemCleaner.ScanAsync();
            foreach (var item in results)
            {
                Items.Add(new SystemCleanupItemViewModel(item));
            }
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
            $"Clean {selected.Count} item(s), about {ByteSizeFormatter.Format(totalSize)}?",
            "Clean system",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes;

        if (!confirmed) return;

        IsCleaning = true;

        try
        {
            var result = await _systemCleaner.CleanAsync(selected.Select(vm => vm.Item));

            ResultSummary =
                $"Freed about {ByteSizeFormatter.Format(result.BytesFreed)} " +
                $"({result.CategoriesCleaned} cleaned, {result.CategoriesSkipped} skipped).";

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
