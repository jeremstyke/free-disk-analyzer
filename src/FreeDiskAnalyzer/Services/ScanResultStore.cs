using CommunityToolkit.Mvvm.ComponentModel;
using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Services;

/// <summary>
/// Holds the most recent <see cref="ScanResult"/> so every page (Dashboard,
/// Large Files, Folders) reflects the same scan without re-running it or
/// passing results through navigation parameters. A single instance is
/// created once in App.xaml.cs and shared across view models.
/// </summary>
public sealed partial class ScanResultStore : ObservableObject
{
    [ObservableProperty]
    private ScanResult? latestResult;
}
