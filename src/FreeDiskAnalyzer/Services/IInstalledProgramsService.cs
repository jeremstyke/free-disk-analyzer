using FreeDiskAnalyzer.Models;

namespace FreeDiskAnalyzer.Services;

/// <summary>
/// Read-only listing of installed programs, sourced from the same registry
/// keys Windows' own "Apps &amp; features" reads. Uninstalling always runs the
/// program's own uninstaller, never deletes files or registry keys directly,
/// PurgeCore doesn't guess at what's safe to remove after the fact.
/// </summary>
public interface IInstalledProgramsService
{
    Task<IReadOnlyList<InstalledProgram>> GetInstalledProgramsAsync(CancellationToken cancellationToken = default);
}
