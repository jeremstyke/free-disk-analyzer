using FreeDiskAnalyzer.Models;

namespace FreeDiskAnalyzer.Services;

public interface IStartupManager
{
    /// <summary>
    /// Lists apps registered to start with Windows: HKCU Run key entries
    /// and shortcuts in the user's own Startup folder. Only per-user
    /// locations, nothing that needs administrator rights (this app never
    /// runs elevated), so HKLM Run entries and the shared "All Users"
    /// Startup folder are intentionally out of scope.
    /// </summary>
    Task<IReadOnlyList<StartupItem>> GetStartupItemsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables or disables an item without losing it: disabling moves the
    /// registry value or shortcut into an app-controlled "disabled" location,
    /// enabling moves it back. Nothing is deleted here.
    /// </summary>
    Task<bool> SetEnabledAsync(StartupItem item, bool enabled, CancellationToken cancellationToken = default);

    /// <summary>Permanently removes the startup entry (registry value, or the shortcut via the Recycle Bin). Does not uninstall the underlying program.</summary>
    Task<bool> DeleteAsync(StartupItem item, CancellationToken cancellationToken = default);
}
