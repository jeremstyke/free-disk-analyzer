using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Utilities;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class PerformanceViewModel : ObservableObject
{
    private readonly IRamOptimizer _ramOptimizer;

    [ObservableProperty]
    private bool isRunning;

    [ObservableProperty]
    private bool hasRunOnce;

    [ObservableProperty]
    private string resultSummary = string.Empty;

    public PerformanceViewModel(IRamOptimizer ramOptimizer)
    {
        _ramOptimizer = ramOptimizer;
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
}
