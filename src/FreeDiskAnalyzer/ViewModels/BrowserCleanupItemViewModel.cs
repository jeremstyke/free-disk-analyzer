using CommunityToolkit.Mvvm.ComponentModel;
using FreeDiskAnalyzer.Core.Utilities;
using FreeDiskAnalyzer.Models;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class BrowserCleanupItemViewModel : ObservableObject
{
    public BrowserCleanupItem Item { get; }

    public string BrowserName => Item.BrowserName;
    public string CategoryLabel => Item.Category.ToString();
    public string SizeDisplay => ByteSizeFormatter.Format(Item.SizeBytes);

    [ObservableProperty]
    private bool isSelected = true;

    public BrowserCleanupItemViewModel(BrowserCleanupItem item)
    {
        Item = item;
    }
}
