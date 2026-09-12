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
    private LanguageOption selectedLanguage;

    [ObservableProperty]
    private bool languageChanged;

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

    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        LanguageChanged = !_isLoading && value.Code != _initialLanguageCode;
        PersistIfNotLoading();
    }

    private void PersistIfNotLoading()
    {
        if (_isLoading) return;

        // Load-modify-save rather than constructing a fresh AppSettings, so
        // this doesn't clobber fields managed elsewhere (the cookie
        // whitelist now lives on the Browsers tab in Cleanup).
        var current = _settingsService.Load();
        current.Theme = Theme;
        current.StartWithWindows = StartWithWindows;
        current.Language = SelectedLanguage.Code;
        _settingsService.Save(current);
    }

    [RelayCommand]
    private void ResetSettings()
    {
        _isLoading = true;
        Theme = ThemeMode.Light;
        StartWithWindows = false;
        SelectedLanguage = LanguageOptions[0];
        LanguageChanged = false;
        _isLoading = false;

        _settingsService.SetStartWithWindows(false);
        ThemeManager.ApplyTheme(ThemeMode.Light);
        _settingsService.Save(new AppSettings());
    }
}
