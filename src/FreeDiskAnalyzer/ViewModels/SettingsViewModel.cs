using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Models;
using FreeDiskAnalyzer.Services;
using FreeDiskAnalyzer.Themes;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private bool _isLoading;
    private readonly string _initialLanguageCode;

    [ObservableProperty]
    private ThemeMode theme;

    [ObservableProperty]
    private bool startWithWindows;

    [ObservableProperty]
    private bool analyticsEnabled;

    [ObservableProperty]
    private LanguageOption selectedLanguage;

    [ObservableProperty]
    private bool languageChanged;

    [ObservableProperty]
    private string cookieWhitelistText = string.Empty;

    public IReadOnlyList<ThemeMode> ThemeOptions { get; } = new[] { ThemeMode.Light, ThemeMode.Dark };

    public IReadOnlyList<LanguageOption> LanguageOptions { get; } = new[]
    {
        new LanguageOption("en", "English"),
        new LanguageOption("fr", "Français")
    };

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;

        _isLoading = true;
        var settings = _settingsService.Load();
        Theme = settings.Theme;
        StartWithWindows = settings.StartWithWindows;
        AnalyticsEnabled = settings.AnalyticsEnabled;
        CookieWhitelistText = string.Join(Environment.NewLine, settings.CookieWhitelist);

        _initialLanguageCode = settings.Language;
        selectedLanguage = LanguageOptions.FirstOrDefault(l => l.Code == settings.Language) ?? LanguageOptions[0];
        _isLoading = false;
    }

    partial void OnThemeChanged(ThemeMode value)
    {
        ThemeManager.ApplyTheme(value);
        PersistIfNotLoading();
    }

    partial void OnStartWithWindowsChanged(bool value)
    {
        if (!_isLoading)
        {
            _settingsService.SetStartWithWindows(value);
        }
        PersistIfNotLoading();
    }

    partial void OnAnalyticsEnabledChanged(bool value) => PersistIfNotLoading();

    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        LanguageChanged = !_isLoading && value.Code != _initialLanguageCode;
        PersistIfNotLoading();
    }

    partial void OnCookieWhitelistTextChanged(string value) => PersistIfNotLoading();

    private static List<string> ParseWhitelist(string text) =>
        text.Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private void PersistIfNotLoading()
    {
        if (_isLoading) return;

        _settingsService.Save(new AppSettings
        {
            Theme = Theme,
            StartWithWindows = StartWithWindows,
            AnalyticsEnabled = AnalyticsEnabled,
            Language = SelectedLanguage.Code,
            CookieWhitelist = ParseWhitelist(CookieWhitelistText)
        });
    }

    [RelayCommand]
    private void ResetSettings()
    {
        _isLoading = true;
        Theme = ThemeMode.Light;
        StartWithWindows = false;
        AnalyticsEnabled = false;
        SelectedLanguage = LanguageOptions[0];
        LanguageChanged = false;
        CookieWhitelistText = string.Empty;
        _isLoading = false;

        _settingsService.SetStartWithWindows(false);
        ThemeManager.ApplyTheme(ThemeMode.Light);
        _settingsService.Save(new AppSettings());
    }
}
