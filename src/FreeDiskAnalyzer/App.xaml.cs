using System.Globalization;
using System.Threading;
using System.Windows;
using FreeDiskAnalyzer.Core.Services;
using FreeDiskAnalyzer.Services;
using FreeDiskAnalyzer.Themes;
using FreeDiskAnalyzer.ViewModels;

namespace FreeDiskAnalyzer;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // No dependency injection container yet, manual composition is enough
        // at this stage. Revisit if the service list grows significantly.
        ISettingsService settingsService = new SettingsService();
        var settings = settingsService.Load();
        ThemeManager.ApplyTheme(settings.Theme);
        ApplyLanguage(settings.Language);

        IDriveEnumerator driveEnumerator = new DriveEnumerator();
        IDiskScanner diskScanner = new DiskScanner();
        IDuplicateFinder duplicateFinder = new DuplicateFinder();
        IRamOptimizer ramOptimizer = new RamOptimizer();
        IUpdateChecker updateChecker = new UpdateChecker();
        IBlogFeedService blogFeedService = new BlogFeedService();
        ISafeDeleteService safeDeleteService = new SafeDeleteService();
        IBrowserCleaner browserCleaner = new BrowserCleaner(safeDeleteService);
        var mainViewModel = new MainViewModel(
            driveEnumerator, diskScanner, duplicateFinder, ramOptimizer, settingsService,
            updateChecker, blogFeedService, safeDeleteService, browserCleaner);

        var mainWindow = new MainWindow
        {
            DataContext = mainViewModel
        };
        mainWindow.Show();
    }

    private static void ApplyLanguage(string languageCode)
    {
        // Applied once at startup: x:Static resource lookups in XAML are
        // resolved when each view is first constructed, not re-evaluated
        // live. Changing the language in Settings takes effect on next
        // launch (see SettingsViewModel.LanguageChanged / the restart note
        // shown in the UI), rather than requiring a live-rebinding
        // localization framework for a two-language app.
        try
        {
            var culture = CultureInfo.GetCultureInfo(languageCode);
            CultureInfo.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
        catch (CultureNotFoundException)
        {
            // Unknown/corrupted setting, fall back to the OS default rather
            // than crashing on startup.
        }
    }
}
