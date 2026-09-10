using FreeDiskAnalyzer.Core.Utilities;
using Xunit;

namespace FreeDiskAnalyzer.Tests;

public sealed class ReleaseVersionComparerTests
{
    [Theory]
    [InlineData("1.0.1", "1.0.0", true)]
    [InlineData("v1.0.1", "1.0.0", true)]
    [InlineData("1.0.1", "v1.0.0", true)]
    [InlineData("1.1.0", "1.0.9", true)]
    [InlineData("2.0.0", "1.9.9", true)]
    [InlineData("1.0.0", "1.0.0", false)]
    [InlineData("1.0.0", "1.0.1", false)]
    [InlineData("1.0.0", "1.1.0", false)]
    public void IsNewer_ComparesCorrectly(string latest, string current, bool expected)
    {
        Assert.Equal(expected, ReleaseVersionComparer.IsNewer(latest, current));
    }

    [Theory]
    [InlineData("not-a-version", "1.0.0")]
    [InlineData("1.0.0", "not-a-version")]
    [InlineData("", "1.0.0")]
    public void IsNewer_ReturnsFalseForUnparsableInput(string latest, string current)
    {
        Assert.False(ReleaseVersionComparer.IsNewer(latest, current));
    }
}
