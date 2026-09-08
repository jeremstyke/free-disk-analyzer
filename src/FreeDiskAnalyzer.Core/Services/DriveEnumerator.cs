using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Core.Services;

public sealed class DriveEnumerator : IDriveEnumerator
{
    public IReadOnlyList<DriveInfoModel> GetAvailableDrives()
    {
        var result = new List<DriveInfoModel>();

        foreach (var drive in DriveInfo.GetDrives())
        {
            if (!drive.IsReady)
            {
                // Empty card readers, disconnected network drives, etc.
                continue;
            }

            try
            {
                result.Add(new DriveInfoModel(
                    drive.Name,
                    drive.RootDirectory.FullName,
                    drive.TotalSize,
                    drive.TotalFreeSpace,
                    drive.DriveFormat,
                    drive.DriveType.ToString()));
            }
            catch (IOException)
            {
                // Drive became unavailable between IsReady check and read, skip it.
            }
        }

        return result;
    }
}
