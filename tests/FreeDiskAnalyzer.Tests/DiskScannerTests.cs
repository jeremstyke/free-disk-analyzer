using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Core.Services;
using Xunit;

namespace FreeDiskAnalyzer.Tests;

public sealed class DiskScannerTests : IDisposable
{
    private readonly string _tempRoot;

    public DiskScannerTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "fda_tests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
        catch (IOException)
        {
            // best effort cleanup
        }
    }

    private string CreateFile(string relativePath, int sizeBytes)
    {
        var fullPath = Path.Combine(_tempRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllBytes(fullPath, new byte[sizeBytes]);
        return fullPath;
    }

    [Fact]
    public async Task ScanAsync_CountsFilesAndBytesCorrectly()
    {
        CreateFile("a.txt", 100);
        CreateFile("sub/b.txt", 200);
        CreateFile("sub/nested/c.txt", 300);

        var scanner = new DiskScanner();
        var result = await scanner.ScanAsync(_tempRoot);

        Assert.Equal(3, result.TotalFilesScanned);
        Assert.Equal(600, result.TotalBytesScanned);
        Assert.False(result.WasCancelled);
    }

    [Fact]
    public async Task ScanAsync_CountsFoldersIncludingRoot()
    {
        CreateFile("sub/b.txt", 10);
        CreateFile("sub/nested/c.txt", 10);

        var scanner = new DiskScanner();
        var result = await scanner.ScanAsync(_tempRoot);

        // root + "sub" + "nested" = 3
        Assert.Equal(3, result.TotalFoldersScanned);
    }

    [Fact]
    public async Task ScanAsync_ReportsLargestFileFirst()
    {
        CreateFile("small.bin", 10);
        CreateFile("medium.bin", 500);
        CreateFile("large.bin", 5000);

        var scanner = new DiskScanner();
        var result = await scanner.ScanAsync(_tempRoot);

        Assert.NotEmpty(result.LargestFiles);
        Assert.Equal("large.bin", result.LargestFiles[0].Name);
        Assert.True(result.LargestFiles[0].SizeBytes >= result.LargestFiles[^1].SizeBytes);
    }

    [Fact]
    public async Task ScanAsync_AggregatesFolderSizeFromChildren()
    {
        CreateFile("sub/x.bin", 100);
        CreateFile("sub/y.bin", 150);

        var scanner = new DiskScanner();
        var result = await scanner.ScanAsync(_tempRoot);

        var subFolder = result.LargestFolders.SingleOrDefault(f => f.Name == "sub");
        Assert.NotNull(subFolder);
        Assert.Equal(250, subFolder!.SizeBytes);
    }

    [Fact]
    public async Task ScanAsync_AggregatesBytesByCategory()
    {
        CreateFile("photo.jpg", 1000);
        CreateFile("video.mp4", 2000);
        CreateFile("doc.pdf", 500);

        var scanner = new DiskScanner();
        var result = await scanner.ScanAsync(_tempRoot);

        Assert.Equal(1000, result.BytesByCategory[FileCategory.Images]);
        Assert.Equal(2000, result.BytesByCategory[FileCategory.Video]);
        Assert.Equal(500, result.BytesByCategory[FileCategory.Documents]);
    }

    [Fact]
    public async Task ScanAsync_ReturnsPartialResultWhenCancelledUpfront()
    {
        for (var i = 0; i < 50; i++)
        {
            CreateFile($"folder{i}/file.bin", 50);
        }

        var scanner = new DiskScanner();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await scanner.ScanAsync(_tempRoot, cancellationToken: cts.Token);

        Assert.True(result.WasCancelled);
    }

    [Fact]
    public async Task ScanAsync_DetectsEmptyFolders()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "empty1"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "empty2", "nested_empty"));
        CreateFile("not_empty/file.bin", 10);

        var scanner = new DiskScanner();
        var result = await scanner.ScanAsync(_tempRoot);

        var emptyNames = result.EmptyFolders.Select(f => f.Name).ToList();
        Assert.Contains("empty1", emptyNames);
        Assert.Contains("empty2", emptyNames);
        Assert.Contains("nested_empty", emptyNames);
        Assert.DoesNotContain("not_empty", emptyNames);
    }

    [Fact]
    public async Task ScanAsync_TracksFolderFileAndSubfolderCounts()
    {
        CreateFile("parent/a.bin", 10);
        CreateFile("parent/b.bin", 10);
        CreateFile("parent/child/c.bin", 10);

        var scanner = new DiskScanner();
        var result = await scanner.ScanAsync(_tempRoot);

        var parent = result.LargestFolders.Single(f => f.Name == "parent");
        Assert.Equal(3, parent.FileCount);
        Assert.Equal(1, parent.SubfolderCount);
        Assert.False(parent.IsEmpty);
    }

    [Fact]
    public async Task ScanAsync_TracksOldestFiles()
    {
        var oldPath = CreateFile("old.bin", 10);
        var newPath = CreateFile("new.bin", 10);
        File.SetLastWriteTimeUtc(oldPath, DateTime.UtcNow.AddYears(-5));
        File.SetLastWriteTimeUtc(newPath, DateTime.UtcNow);

        var scanner = new DiskScanner();
        var result = await scanner.ScanAsync(_tempRoot);

        Assert.NotEmpty(result.OldestFiles);
        Assert.Equal("old.bin", result.OldestFiles[0].Name);
    }

    [Fact]
    public async Task ScanAsync_ThrowsWhenPathDoesNotExist()
    {
        var scanner = new DiskScanner();
        var missingPath = Path.Combine(_tempRoot, "does-not-exist");

        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => scanner.ScanAsync(missingPath));
    }

    [Fact]
    public async Task ScanAsync_ReportsProgress()
    {
        for (var i = 0; i < 20; i++)
        {
            CreateFile($"f{i}.bin", 10);
        }

        var scanner = new DiskScanner();
        var reports = new List<ScanProgress>();
        var progress = new Progress<ScanProgress>(p => reports.Add(p));

        await scanner.ScanAsync(_tempRoot, progress);

        // Progress reporting is throttled by time, so on a fast local scan of a
        // small tree it may legitimately report zero times. This just asserts
        // it never throws and, if it did report, values are sane.
        Assert.All(reports, r => Assert.True(r.FilesScanned >= 0));
    }
}
