using FreeDiskAnalyzer.Models;

namespace FreeDiskAnalyzer.Services;

/// <summary>
/// The subset of app state that survives a restart. Serialized as plain JSON
/// to the user's local app data folder, no telemetry, no cloud sync.
/// </summary>
public sealed class AppSettings
{
    public ThemeMode Theme { get; set; } = ThemeMode.Light;
    public bool StartWithWindows { get; set; }
    public bool AnalyticsEnabled { get; set; } = false;

    // Only "en" is meaningful until Phase 5 (localization) lands.
    public string Language { get; set; } = "en";

    // Domains whose cookies are kept when cleaning browser cookies (e.g.
    // "gmail.com" keeps you signed in there). Matched as a suffix against
    // each cookie's host, so "example.com" also protects "www.example.com".
    public List<string> CookieWhitelist { get; set; } = new();
}
