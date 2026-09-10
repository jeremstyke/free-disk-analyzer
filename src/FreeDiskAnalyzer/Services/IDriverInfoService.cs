using FreeDiskAnalyzer.Models;

namespace FreeDiskAnalyzer.Services;

/// <summary>
/// Read-only. Lists installed drivers and their age. Never downloads or
/// installs anything, that's deliberate: getting driver matching wrong
/// (wrong version, wrong hardware) can break a graphics card, network
/// adapter, or chipset in ways a Recycle-Bin-backed file delete never can.
/// Users are pointed to Windows Update and manufacturer sites instead.
/// </summary>
public interface IDriverInfoService
{
    Task<IReadOnlyList<DriverInfo>> GetDriversAsync(CancellationToken cancellationToken = default);
}
