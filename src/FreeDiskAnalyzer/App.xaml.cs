using System.Windows;
using FreeDiskAnalyzer.Core.Services;
using FreeDiskAnalyzer.Models;
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
        ThemeManager.ApplyTheme(ThemeMode.Light);

        IDriveEnumerator driveEnumerator = new DriveEnumerator();
        var mainViewModel = new MainViewModel(driveEnumerator);

        var mainWindow = new MainWindow
        {
            DataContext = mainViewModel
        };
        mainWindow.Show();
    }
}
