using CommunityToolkit.Mvvm.ComponentModel;
using FreeDiskAnalyzer.Models;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class StartupItemViewModel : ObservableObject
{
    public StartupItem Item { get; private set; }

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

    /// <summary>
    /// Swaps in the item returned by a successful toggle. Startup Folder
    /// items physically move on disk when toggled, so keeping the old
    /// CommandOrPath around would make the next toggle silently fail
    /// (it would look for the shortcut where it used to be, not where it
    /// actually is now).
    /// </summary>
    public void UpdateItem(StartupItem updatedItem)
    {
        Item = updatedItem;
        IsEnabled = updatedItem.IsEnabled;
        OnPropertyChanged(nameof(CommandOrPath));
    }
}
