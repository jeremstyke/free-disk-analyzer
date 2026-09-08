using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Core.Services;
using Xunit;

namespace FreeDiskAnalyzer.Tests;

public sealed class FileCategoryClassifierTests
{
    [Theory]
    [InlineData(".jpg", FileCategory.Images)]
    [InlineData(".PNG", FileCategory.Images)]
    [InlineData(".mp4", FileCategory.Video)]
    [InlineData(".mp3", FileCategory.Audio)]
    [InlineData(".zip", FileCategory.Archives)]
    [InlineData(".exe", FileCategory.Executables)]
    [InlineData(".cs", FileCategory.Code)]
    [InlineData(".pdf", FileCategory.Documents)]
    [InlineData(".dll", FileCategory.System)]
    [InlineData(".xyz123", FileCategory.Other)]
    public void Classify_ReturnsExpectedCategory(string extension, FileCategory expected)
    {
        Assert.Equal(expected, FileCategoryClassifier.Classify(extension));
    }
}
