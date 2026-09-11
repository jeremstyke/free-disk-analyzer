using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Models;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class DriversViewModel : ObservableObject
{
    private readonly IDriverInfoService _driverInfoService;

    public const string WindowsUpdateUri = "ms-settings:windowsupdate";

    public ObservableCollection<DriverInfo> Drivers { get; } = new();

    [ObservableProperty]
    private bool isLoading;

    public DriversViewModel(IDriverInfoService driverInfoService)
    {
        _driverInfoService = driverInfoService;
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        Drivers.Clear();

        try
        {
            var drivers = await _driverInfoService.GetDriversAsync();
            foreach (var driver in drivers)
            {
                Drivers.Add(driver);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenWindowsUpdate() => Process.Start(new ProcessStartInfo(WindowsUpdateUri) { UseShellExecute = true });

    /// <summary>
    /// Clicking a driver shows what it is, how old it looks, and a plain
    /// explanation of what updating would and wouldn't do, before offering
    /// to open the manufacturer's site, rather than jumping straight there.
    /// </summary>
    [RelayCommand]
    private void ShowDriverDetails(DriverInfo? driver)
    {
        if (driver is null) return;

        var body =
            $"{driver.DeviceName}\n{driver.Manufacturer} - v{driver.Version} - {driver.AgeDisplay}\n\n" +
            $"{Resources.Strings.Drivers_DetailExplanation}\n\n" +
            string.Format(Resources.Strings.Drivers_DetailOpenPrompt, driver.LinkLabel);

        var confirmed = MessageBox.Show(
            body,
            Resources.Strings.Drivers_DetailTitle,
            MessageBoxButton.YesNo,
            MessageBoxImage.Information) == MessageBoxResult.Yes;

        if (confirmed)
        {
            Process.Start(new ProcessStartInfo(driver.LinkUrl) { UseShellExecute = true });
        }
    }
}
