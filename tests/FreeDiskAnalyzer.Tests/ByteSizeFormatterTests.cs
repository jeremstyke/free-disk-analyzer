using FreeDiskAnalyzer.Core.Utilities;
using Xunit;

namespace FreeDiskAnalyzer.Tests;

public sealed class ByteSizeFormatterTests
{
    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(512, "512 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(1073741824, "1 GB")]
    [InlineData(1610612736, "1.5 GB")]
    public void Format_ReturnsExpectedString(long bytes, string expected)
    {
        Assert.Equal(expected, ByteSizeFormatter.Format(bytes));
    }
}
