using CommunityToolkit.Mvvm.ComponentModel;
using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Core.Utilities;
using FreeDiskAnalyzer.Models;
using FreeDiskAnalyzer.Resources;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class SystemCleanupItemViewModel : ObservableObject
{
    public SystemCleanupItem Item { get; }

    public string CategoryLabel => Item.Category switch
    {
        SystemCleanupCategory.TempFiles => Strings.System_TempFiles,
        SystemCleanupCategory.RecycleBin => Strings.System_RecycleBin,
        _ => Item.Category.ToString()
    };

    public string SizeDisplay => ByteSizeFormatter.Format(Item.SizeBytes);
    public RiskLevel RiskLevel => Item.RiskLevel;

    [ObservableProperty]
    private bool isSelected = true;

    public SystemCleanupItemViewModel(SystemCleanupItem item)
    {
        Item = item;
    }
}
