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

    [ObservableProperty]
    private ThemeMode theme;

    [ObservableProperty]
    private bool startWithWindows;

    [ObservableProperty]
    private bool analyticsEnabled;

    public IReadOnlyList<ThemeMode> ThemeOptions { get; } = new[] { ThemeMode.Light, ThemeMode.Dark };

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;

        _isLoading = true;
        var settings = _settingsService.Load();
        Theme = settings.Theme;
        StartWithWindows = settings.StartWithWindows;
        AnalyticsEnabled = settings.AnalyticsEnabled;
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

    private void PersistIfNotLoading()
    {
        if (_isLoading) return;

        _settingsService.Save(new AppSettings
        {
            Theme = Theme,
            StartWithWindows = StartWithWindows,
            AnalyticsEnabled = AnalyticsEnabled
        });
    }

    [RelayCommand]
    private void ResetSettings()
    {
        _isLoading = true;
        Theme = ThemeMode.Light;
        StartWithWindows = false;
        AnalyticsEnabled = false;
        _isLoading = false;

        _settingsService.SetStartWithWindows(false);
        ThemeManager.ApplyTheme(ThemeMode.Light);
        _settingsService.Save(new AppSettings());
    }
}
