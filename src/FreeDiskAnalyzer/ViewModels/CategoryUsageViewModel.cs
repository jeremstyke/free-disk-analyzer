using System.Windows.Media;

namespace FreeDiskAnalyzer.ViewModels;

/// <summary>One row of the storage-by-category breakdown on the Dashboard.</summary>
public sealed class CategoryUsageViewModel
{
    public required string Label { get; init; }
    public required string BytesDisplay { get; init; }

    /// <summary>0-100, relative to the largest category in the current scan, drives the bar width.</summary>
    public required double BarPercentage { get; init; }

    /// <summary>Cycles through the app's accent palette so categories are easy to tell apart at a glance.</summary>
    public required Brush BarBrush { get; init; }
}
