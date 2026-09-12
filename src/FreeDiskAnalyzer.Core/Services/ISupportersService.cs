namespace FreeDiskAnalyzer.Core.Services;

/// <summary>
/// Fetches the list of names on the "Thanks to our supporters" page, from
/// the same JSON file the website reads, so there's a single place to keep
/// it updated rather than two lists that can drift apart.
/// </summary>
public interface ISupportersService
{
    Task<IReadOnlyList<string>> GetSupportersAsync(CancellationToken cancellationToken = default);
}
