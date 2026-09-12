using System.IO;
using System.Text.Json;
using Microsoft.Win32;

namespace FreeDiskAnalyzer.Services;

public interface ISettingsService
{
    AppSettings Load();
    void Save(AppSettings settings);
    void SetStartWithWindows(bool enabled);
}

public sealed class SettingsService : ISettingsService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string RunValueName = "PurgeCore";

    private readonly string _settingsFilePath;

    public SettingsService()
    {
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FreeDiskAnalyzer");

        Directory.CreateDirectory(appDataFolder);
        _settingsFilePath = Path.Combine(appDataFolder, "settings.json");
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(_settingsFilePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // Corrupt or unreadable settings file, fall back to defaults rather
            // than crashing the app on startup.
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Best effort: a failed save shouldn't crash the app, the setting
            // just won't persist across restarts this time.
        }
    }

    public void SetStartWithWindows(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        if (key is null) return;

        // Cleans up the entry from before the app was renamed from Free Disk
        // Analyzer to PurgeCore, so nobody who enabled this pre-rename is
        // left with a stale, orphaned registry value under the old name.
        key.DeleteValue("FreeDiskAnalyzer", throwOnMissingValue: false);

        if (enabled)
        {
            var exePath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(exePath))
            {
                key.SetValue(RunValueName, $"\"{exePath}\"");
            }
        }
        else
        {
            key.DeleteValue(RunValueName, throwOnMissingValue: false);
        }
    }
}
