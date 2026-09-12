using Microsoft.Win32;
using FreeDiskAnalyzer.Models;

namespace FreeDiskAnalyzer.Services;

public sealed class InstalledProgramsService : IInstalledProgramsService
{
    // Same three locations Windows' own "Apps & features" reads: per-machine
    // 64-bit, per-machine 32-bit (on 64-bit Windows, WOW6432Node), and
    // per-user installs. No elevation needed for any of these.
    private static readonly (RegistryHive Hive, string Path)[] UninstallKeyLocations =
    {
        (RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
        (RegistryHive.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
        (RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")
    };

    public Task<IReadOnlyList<InstalledProgram>> GetInstalledProgramsAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var programs = new List<InstalledProgram>();
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var (hive, path) in UninstallKeyLocations)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                    using var uninstallKey = baseKey.OpenSubKey(path);
                    if (uninstallKey is null) continue;

                    foreach (var subKeyName in uninstallKey.GetSubKeyNames())
                    {
                        using var entry = uninstallKey.OpenSubKey(subKeyName);
                        if (entry is null) continue;

                        var displayName = entry.GetValue("DisplayName") as string;
                        var uninstallString = entry.GetValue("UninstallString") as string;

                        // Entries with no name or no way to uninstall aren't
                        // real programs a user would recognize or act on,
                        // Windows updates, shared components, driver
                        // packages, etc. typically show up this way.
                        if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(uninstallString))
                        {
                            continue;
                        }

                        // Hide entries Windows itself hides from "Apps & features".
                        if (Convert.ToInt32(entry.GetValue("SystemComponent", 0)) == 1)
                        {
                            continue;
                        }

                        if (!seenNames.Add(displayName))
                        {
                            continue;
                        }

                        var sizeKb = entry.GetValue("EstimatedSize");
                        var sizeBytes = sizeKb is int kb ? kb * 1024L : 0;

                        programs.Add(new InstalledProgram
                        {
                            DisplayName = displayName,
                            Publisher = entry.GetValue("Publisher") as string,
                            DisplayVersion = entry.GetValue("DisplayVersion") as string,
                            EstimatedSizeBytes = sizeBytes,
                            InstallDate = TryParseInstallDate(entry.GetValue("InstallDate") as string),
                            UninstallString = uninstallString
                        });
                    }
                }
                catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException)
                {
                    // Skip this location, still show whatever the others found.
                }
            }

            return (IReadOnlyList<InstalledProgram>)programs
                .OrderByDescending(p => p.EstimatedSizeBytes)
                .ToList();
        }, cancellationToken);
    }

    private static DateTime? TryParseInstallDate(string? raw)
    {
        // Stored as "yyyyMMdd" when present at all, frequently missing.
        if (raw is { Length: 8 } && DateTime.TryParseExact(raw, "yyyyMMdd", null,
                System.Globalization.DateTimeStyles.None, out var date))
        {
            return date;
        }

        return null;
    }
}
