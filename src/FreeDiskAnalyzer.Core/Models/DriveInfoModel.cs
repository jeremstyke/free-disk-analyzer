namespace FreeDiskAnalyzer.Core.Models;

/// <summary>
/// A ready, available drive as shown on the Dashboard.
/// </summary>
public sealed record DriveInfoModel(
    string Name,
    string RootPath,
    long TotalBytes,
    long FreeBytes,
    string DriveFormat,
    string DriveType)
{
    public long UsedBytes => TotalBytes - FreeBytes;

    public double UsedPercentage => TotalBytes == 0
        ? 0
        : Math.Round(UsedBytes / (double)TotalBytes * 100, 1);
}
