using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Core.Services;

public interface IDriveEnumerator
{
    /// <summary>Returns all currently ready (accessible) drives.</summary>
    IReadOnlyList<DriveInfoModel> GetAvailableDrives();
}
