using System.IO;
using System.Security;
using FreeDiskAnalyzer.Core.Utilities;
using FreeDiskAnalyzer.Models;
using Microsoft.Win32;

namespace FreeDiskAnalyzer.Services;

public sealed class StartupManager : IStartupManager
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string DisabledBackupKeyPath = @"Software\FreeDiskAnalyzer\DisabledStartupItems";

    // Must match SettingsService.RunValueName: PurgeCore's own "Start with
    // Windows" entry lives in this same registry key, but it's managed by
    // its own dedicated Settings toggle, not this general-purpose list.
    // Showing it here too would let someone disable/remove it through two
    // different, uncoordinated paths and leave the Settings checkbox out of
    // sync with what's actually in the registry.
    private const string SelfRunValueName = "PurgeCore";

    private readonly ISafeDeleteService _safeDeleteService;

    public StartupManager(ISafeDeleteService safeDeleteService)
    {
        _safeDeleteService = safeDeleteService;
    }

    public Task<IReadOnlyList<StartupItem>> GetStartupItemsAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var items = new List<StartupItem>();
            items.AddRange(GetRegistryItems());
            items.AddRange(GetStartupFolderItems());
            return (IReadOnlyList<StartupItem>)items;
        }, cancellationToken);
    }

    public Task<StartupItem?> SetEnabledAsync(StartupItem item, bool enabled, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                return item.Source == StartupItemSource.RegistryRun
                    ? SetRegistryEnabled(item, enabled)
                    : SetStartupFolderEnabled(item, enabled);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
            {
                return null;
            }
        }, cancellationToken);
    }

    public Task<bool> DeleteAsync(StartupItem item, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                if (item.Source == StartupItemSource.RegistryRun)
                {
                    using var runKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
                    using var disabledKey = Registry.CurrentUser.OpenSubKey(DisabledBackupKeyPath, writable: true);
                    runKey?.DeleteValue(item.Name, throwOnMissingValue: false);
                    disabledKey?.DeleteValue(item.Name, throwOnMissingValue: false);
                    return true;
                }

                if (PathSafetyGuard.IsProtected(item.CommandOrPath)) return false;
                return _safeDeleteService.TryDeleteFile(item.CommandOrPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
            {
                return false;
            }
        }, cancellationToken);
    }

    private static IEnumerable<StartupItem> GetRegistryItems()
    {
        using var runKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        if (runKey is not null)
        {
            foreach (var name in runKey.GetValueNames())
            {
                if (string.IsNullOrEmpty(name)) continue;
                if (string.Equals(name, SelfRunValueName, StringComparison.OrdinalIgnoreCase)) continue;

                yield return new StartupItem
                {
                    Name = name,
                    CommandOrPath = runKey.GetValue(name) as string ?? string.Empty,
                    Source = StartupItemSource.RegistryRun,
                    IsEnabled = true
                };
            }
        }

        using var disabledKey = Registry.CurrentUser.OpenSubKey(DisabledBackupKeyPath, writable: false);
        if (disabledKey is not null)
        {
            foreach (var name in disabledKey.GetValueNames())
            {
                if (string.IsNullOrEmpty(name)) continue;
                if (string.Equals(name, SelfRunValueName, StringComparison.OrdinalIgnoreCase)) continue;

                yield return new StartupItem
                {
                    Name = name,
                    CommandOrPath = disabledKey.GetValue(name) as string ?? string.Empty,
                    Source = StartupItemSource.RegistryRun,
                    IsEnabled = false
                };
            }
        }
    }

    private static IEnumerable<StartupItem> GetStartupFolderItems()
    {
        var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        foreach (var file in SafeEnumerateFiles(startupFolder))
        {
            yield return new StartupItem
            {
                Name = Path.GetFileNameWithoutExtension(file),
                CommandOrPath = file,
                Source = StartupItemSource.StartupFolder,
                IsEnabled = true
            };
        }

        var disabledFolder = GetDisabledStartupFolderPath();
        foreach (var file in SafeEnumerateFiles(disabledFolder))
        {
            yield return new StartupItem
            {
                Name = Path.GetFileNameWithoutExtension(file),
                CommandOrPath = file,
                Source = StartupItemSource.StartupFolder,
                IsEnabled = false
            };
        }
    }

    private static IEnumerable<string> SafeEnumerateFiles(string folder)
    {
        if (!Directory.Exists(folder)) return Enumerable.Empty<string>();

        try
        {
            return Directory.EnumerateFiles(folder, "*.lnk").ToList();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Enumerable.Empty<string>();
        }
    }

    private static string GetDisabledStartupFolderPath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FreeDiskAnalyzer", "DisabledStartupItems");

    private static StartupItem? SetRegistryEnabled(StartupItem item, bool enabled)
    {
        using var runKey = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
        using var disabledKey = Registry.CurrentUser.CreateSubKey(DisabledBackupKeyPath, writable: true);
        if (runKey is null || disabledKey is null) return null;

        if (enabled)
        {
            runKey.SetValue(item.Name, item.CommandOrPath);
            disabledKey.DeleteValue(item.Name, throwOnMissingValue: false);
        }
        else
        {
            disabledKey.SetValue(item.Name, item.CommandOrPath);
            runKey.DeleteValue(item.Name, throwOnMissingValue: false);
        }

        // Registry items keep the same "path" (a command line, not a file
        // location) either way, only IsEnabled actually changes here.
        return new StartupItem
        {
            Name = item.Name,
            CommandOrPath = item.CommandOrPath,
            Source = item.Source,
            IsEnabled = enabled
        };
    }

    private StartupItem? SetStartupFolderEnabled(StartupItem item, bool enabled)
    {
        if (!File.Exists(item.CommandOrPath)) return null;
        if (PathSafetyGuard.IsProtected(item.CommandOrPath)) return null;

        var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        var disabledFolder = GetDisabledStartupFolderPath();
        Directory.CreateDirectory(disabledFolder);

        var targetFolder = enabled ? startupFolder : disabledFolder;
        var destination = Path.Combine(targetFolder, Path.GetFileName(item.CommandOrPath));

        if (PathSafetyGuard.IsProtected(destination)) return null;

        File.Move(item.CommandOrPath, destination, overwrite: true);

        // The file physically moved, so the path genuinely changed. Returning
        // it (rather than the caller reusing the old, now-stale path) is what
        // makes toggling the same item back and forth actually work.
        return new StartupItem
        {
            Name = item.Name,
            CommandOrPath = destination,
            Source = item.Source,
            IsEnabled = enabled
        };
    }
}
