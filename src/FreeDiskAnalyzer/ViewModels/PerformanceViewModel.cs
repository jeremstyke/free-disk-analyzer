using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Utilities;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class PerformanceViewModel : ObservableObject
{
    private readonly IRamOptimizer _ramOptimizer;
    private readonly IStartupManager _startupManager;

    [ObservableProperty]
    private bool isRunning;

    [ObservableProperty]
    private bool hasRunOnce;

    [ObservableProperty]
    private string resultSummary = string.Empty;

    public ObservableCollection<StartupItemViewModel> StartupItems { get; } = new();

    [ObservableProperty]
    private bool isLoadingStartupItems;

    [ObservableProperty]
    private bool hasNoStartupItems;

    public PerformanceViewModel(IRamOptimizer ramOptimizer, IStartupManager startupManager)
    {
        _ramOptimizer = ramOptimizer;
        _startupManager = startupManager;

        _ = LoadStartupItemsAsync();
    }

    [RelayCommand]
    private async Task FreeUpRamAsync()
    {
        IsRunning = true;

        try
        {
            var result = await _ramOptimizer.OptimizeAsync();
            HasRunOnce = true;

            var deltaSign = result.AvailableMemoryDeltaBytes >= 0 ? "+" : "-";
            var deltaDisplay = ByteSizeFormatter.Format(Math.Abs(result.AvailableMemoryDeltaBytes));

            ResultSummary =
                $"Trimmed {result.ProcessesTrimmed:N0} processes ({result.ProcessesSkipped:N0} skipped, " +
                $"no permission). Available memory: {ByteSizeFormatter.Format(result.AvailableMemoryBeforeBytes)} " +
                $"-> {ByteSizeFormatter.Format(result.AvailableMemoryAfterBytes)} ({deltaSign}{deltaDisplay}).";
        }
        finally
        {
            IsRunning = false;
        }
    }

    [RelayCommand]
    private async Task LoadStartupItemsAsync()
    {
        IsLoadingStartupItems = true;
        StartupItems.Clear();

        try
        {
            var items = await _startupManager.GetStartupItemsAsync();
            foreach (var item in items.OrderBy(i => i.Name))
            {
                StartupItems.Add(new StartupItemViewModel(item));
            }
            HasNoStartupItems = StartupItems.Count == 0;
        }
        finally
        {
            IsLoadingStartupItems = false;
        }
    }

    [RelayCommand]
    private async Task ToggleStartupItemAsync(StartupItemViewModel? vm)
    {
        if (vm is null || vm.IsBusy) return;

        var targetState = !vm.IsEnabled;
        vm.IsBusy = true;

        try
        {
            var updated = await _startupManager.SetEnabledAsync(vm.Item, targetState);
            if (updated is not null)
            {
                vm.UpdateItem(updated);
            }
        }
        finally
        {
            vm.IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteStartupItemAsync(StartupItemViewModel? vm)
    {
        if (vm is null) return;

        var confirmed = MessageBox.Show(
            $"Remove \"{vm.Name}\" from startup permanently?\nThis only removes the startup entry, the program itself is not uninstalled.",
            "Remove startup item",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes;

        if (!confirmed) return;

        vm.IsBusy = true;

        try
        {
            if (await _startupManager.DeleteAsync(vm.Item))
            {
                StartupItems.Remove(vm);
                HasNoStartupItems = StartupItems.Count == 0;
            }
        }
        finally
        {
            vm.IsBusy = false;
        }
    }
}
