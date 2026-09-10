namespace FreeDiskAnalyzer.Services;

/// <summary>
/// Every delete operation in the app goes through this, never a direct
/// File.Delete/Directory.Delete call. Sends to the Recycle Bin rather than
/// permanently deleting, and refuses anything under a protected system
/// directory (see FreeDiskAnalyzer.Core.Utilities.PathSafetyGuard).
/// </summary>
public interface ISafeDeleteService
{
    /// <summary>Attempts to delete a file to the Recycle Bin. Returns false on failure (locked, missing, protected path), never throws.</summary>
    bool TryDeleteFile(string path);

    /// <summary>Attempts to delete a folder to the Recycle Bin. Returns false on failure (locked, missing, protected path), never throws.</summary>
    bool TryDeleteDirectory(string path);
}
