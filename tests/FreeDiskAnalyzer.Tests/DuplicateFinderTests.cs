using FreeDiskAnalyzer.Core.Services;
using Xunit;

namespace FreeDiskAnalyzer.Tests;

public sealed class DuplicateFinderTests : IDisposable
{
    private readonly string _tempRoot;

    public DuplicateFinderTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "fda_dupe_tests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempRoot, recursive: true); }
        catch (IOException) { }
    }

    private string CreateFile(string relativePath, byte[] content)
    {
        var fullPath = Path.Combine(_tempRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllBytes(fullPath, content);
        return fullPath;
    }

    // 1.5 MB, above the finder's 1 MB minimum size threshold.
    private static byte[] BigContent(byte fill) => Enumerable.Repeat(fill, 1024 * 1024 + 512 * 1024).ToArray();

    [Fact]
    public async Task FindDuplicatesAsync_FindsIdenticalFiles()
    {
        var content = BigContent(0xAB);
        CreateFile("a/one.bin", content);
        CreateFile("b/two.bin", content);
        CreateFile("c/unique.bin", BigContent(0xCD));

        var finder = new DuplicateFinder();
        var groups = await finder.FindDuplicatesAsync(_tempRoot);

        Assert.Single(groups);
        Assert.Equal(2, groups[0].FilePaths.Count);
    }

    [Fact]
    public async Task FindDuplicatesAsync_IgnoresFilesBelowSizeThreshold()
    {
        var smallContent = new byte[] { 1, 2, 3 };
        CreateFile("a/small1.bin", smallContent);
        CreateFile("b/small2.bin", smallContent);

        var finder = new DuplicateFinder();
        var groups = await finder.FindDuplicatesAsync(_tempRoot);

        Assert.Empty(groups);
    }

    [Fact]
    public async Task FindDuplicatesAsync_DoesNotGroupDifferentContentOfSameSize()
    {
        CreateFile("a/x.bin", BigContent(0x11));
        CreateFile("b/y.bin", BigContent(0x22));

        var finder = new DuplicateFinder();
        var groups = await finder.FindDuplicatesAsync(_tempRoot);

        Assert.Empty(groups);
    }

    [Fact]
    public async Task FindDuplicatesAsync_ThrowsWhenPathDoesNotExist()
    {
        var finder = new DuplicateFinder();
        var missing = Path.Combine(_tempRoot, "does-not-exist");

        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => finder.FindDuplicatesAsync(missing));
    }
}
