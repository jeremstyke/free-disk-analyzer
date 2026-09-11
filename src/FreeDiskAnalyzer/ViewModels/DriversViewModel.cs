using System.Collections.ObjectModel;
using System.Diagnostics;
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

    [RelayCommand]
    private void OpenDriverLink(DriverInfo? driver)
    {
        if (driver is null) return;
        Process.Start(new ProcessStartInfo(driver.LinkUrl) { UseShellExecute = true });
    }
}
