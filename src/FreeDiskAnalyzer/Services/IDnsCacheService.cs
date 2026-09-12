namespace FreeDiskAnalyzer.Services;

/// <summary>
/// Flushes the Windows DNS resolver cache. Purely a network fix (stale or
/// incorrect DNS entries can cause slow or failed page loads), it does not
/// free disk space or memory. Read-only info, no user data involved.
/// </summary>
public interface IDnsCacheService
{
    Task<bool> FlushAsync(CancellationToken cancellationToken = default);
}
