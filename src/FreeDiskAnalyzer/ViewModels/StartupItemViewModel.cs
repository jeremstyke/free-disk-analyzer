using CommunityToolkit.Mvvm.ComponentModel;
using FreeDiskAnalyzer.Models;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class StartupItemViewModel : ObservableObject
{
    public StartupItem Item { get; }

    public string Name => Item.Name;
    public string CommandOrPath => Item.CommandOrPath;

    [ObservableProperty]
    private bool isEnabled;

    [ObservableProperty]
    private bool isBusy;

    public StartupItemViewModel(StartupItem item)
    {
        Item = item;
        isEnabled = item.IsEnabled;
    }
}
