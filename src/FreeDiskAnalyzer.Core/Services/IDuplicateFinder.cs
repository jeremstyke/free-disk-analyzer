using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Core.Services;

public interface IDuplicateFinder
{
    /// <summary>
    /// Finds groups of files with identical content under <paramref name="rootPath"/>.
    /// Two phases: a fast walk grouping files by size, then hashing only the
    /// files that share a size with at least one other file. Files smaller
    /// than a practical threshold are skipped, not worth flagging.
    /// </summary>
    Task<IReadOnlyList<DuplicateGroup>> FindDuplicatesAsync(
        string rootPath,
        IProgress<DuplicateScanProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
