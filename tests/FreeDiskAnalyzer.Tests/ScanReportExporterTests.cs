using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Core.Services;
using Xunit;

namespace FreeDiskAnalyzer.Tests;

public sealed class ScanReportExporterTests
{
    private static ScanResult BuildSampleResult() => new()
    {
        RootPath = @"C:\Test",
        StartedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CompletedAtUtc = new DateTime(2026, 1, 1, 0, 5, 0, DateTimeKind.Utc),
        TotalBytesScanned = 3000,
        TotalFilesScanned = 2,
        TotalFoldersScanned = 1,
        WasCancelled = false,
        LargestFolders = new[]
        {
            new FolderNode { FullPath = @"C:\Test\sub", Name = "sub", SizeBytes = 2000, FileCount = 1, SubfolderCount = 0 }
        },
        LargestFiles = new[]
        {
            new FileEntry(@"C:\Test\a.txt", "a.txt", 1000, ".txt", FileCategory.Documents, null),
            new FileEntry(@"C:\Test\b, with comma.txt", "b, with comma.txt", 2000, ".txt", FileCategory.Documents, null)
        },
        BytesByCategory = new Dictionary<FileCategory, long> { [FileCategory.Documents] = 3000 }
    };

    [Fact]
    public void BuildCsv_IncludesSummaryTotals()
    {
        var csv = ScanReportExporter.BuildCsv(BuildSampleResult());

        Assert.Contains("Total files,2", csv);
        Assert.Contains("Total bytes,3000", csv);
    }

    [Fact]
    public void BuildCsv_QuotesValuesContainingCommas()
    {
        var csv = ScanReportExporter.BuildCsv(BuildSampleResult());

        Assert.Contains("\"b, with comma.txt\"", csv);
    }

    [Fact]
    public void BuildCsv_IncludesFoldersAndFiles()
    {
        var csv = ScanReportExporter.BuildCsv(BuildSampleResult());

        Assert.Contains("sub", csv);
        Assert.Contains("a.txt", csv);
    }
}
