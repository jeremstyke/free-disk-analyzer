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

        IDriveEnumerator driveEnumerator = new DriveEnumerator();
        IDiskScanner diskScanner = new DiskScanner();
        var mainViewModel = new MainViewModel(driveEnumerator, diskScanner, settingsService);

        var mainWindow = new MainWindow
        {
            DataContext = mainViewModel
        };
        mainWindow.Show();
    }
}
