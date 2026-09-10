using FreeDiskAnalyzer.Core.Utilities;
using Xunit;

namespace FreeDiskAnalyzer.Tests;

public sealed class PathSafetyGuardTests
{
    [Fact]
    public void IsProtected_ReturnsTrueForWindowsFolder()
    {
        var windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        Assert.True(PathSafetyGuard.IsProtected(windowsDir));
    }

    [Fact]
    public void IsProtected_ReturnsTrueForSubfolderOfProtectedRoot()
    {
        var windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var nested = Path.Combine(windowsDir, "System32", "config");
        Assert.True(PathSafetyGuard.IsProtected(nested));
    }

    [Fact]
    public void IsProtected_ReturnsFalseForOrdinaryUserPath()
    {
        var userTemp = Path.Combine(Path.GetTempPath(), "some-user-file.txt");
        Assert.False(PathSafetyGuard.IsProtected(userTemp));
    }

    [Fact]
    public void IsProtected_ReturnsTrueForEmptyOrNullPath()
    {
        Assert.True(PathSafetyGuard.IsProtected(string.Empty));
    }

    [Fact]
    public void IsSafeToDelete_IsInverseOfIsProtected()
    {
        var userTemp = Path.GetTempPath();
        Assert.True(PathSafetyGuard.IsSafeToDelete(userTemp));

        var windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        Assert.False(PathSafetyGuard.IsSafeToDelete(windowsDir));
    }
}
