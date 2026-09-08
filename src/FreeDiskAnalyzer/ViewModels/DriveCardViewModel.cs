using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Core.Utilities;

namespace FreeDiskAnalyzer.ViewModels;

/// <summary>
/// Display-ready wrapper around a <see cref="DriveInfoModel"/> for the
/// Dashboard's drive cards.
/// </summary>
public sealed class DriveCardViewModel
{
    public DriveCardViewModel(DriveInfoModel model)
    {
        Name = model.Name;
        RootPath = model.RootPath;
        UsedPercentage = model.UsedPercentage;
        TotalDisplay = ByteSizeFormatter.Format(model.TotalBytes);
        UsedDisplay = ByteSizeFormatter.Format(model.UsedBytes);
        FreeDisplay = ByteSizeFormatter.Format(model.FreeBytes);
    }

    public string Name { get; }
    public string RootPath { get; }
    public double UsedPercentage { get; }
    public string TotalDisplay { get; }
    public string UsedDisplay { get; }
    public string FreeDisplay { get; }
}
