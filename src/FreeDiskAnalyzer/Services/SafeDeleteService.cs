using System.ComponentModel;
using System.IO;
using FreeDiskAnalyzer.Core.Utilities;
using Microsoft.VisualBasic.FileIO;

namespace FreeDiskAnalyzer.Services;

public sealed class SafeDeleteService : ISafeDeleteService
{
    public bool TryDeleteFile(string path)
    {
        if (PathSafetyGuard.IsProtected(path)) return false;

        try
        {
            if (!File.Exists(path)) return true;

            FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Win32Exception or OperationCanceledException)
        {
            return false;
        }
    }

    public bool TryDeleteDirectory(string path)
    {
        if (PathSafetyGuard.IsProtected(path)) return false;

        try
        {
            if (!Directory.Exists(path)) return true;

            FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Win32Exception or OperationCanceledException)
        {
            return false;
        }
    }
}
