using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class AboutViewModel : ObservableObject
{
    public const string GitHubUrl = "https://github.com/jeremstyke/purgecore";
    public const string CoffeeUrl = "https://jeremstyke.gumroad.com/coffee";
    public const string ReportBugUrl = "mailto:juryjeremy@gmail.com?subject=PurgeCore%20-%20Bug%20report";

    private readonly ISupportersService _supportersService;

    public string VersionDisplay { get; } =
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    public ObservableCollection<string> Supporters { get; } = new();

    [ObservableProperty]
    private bool hasSupporters;

    public AboutViewModel(ISupportersService supportersService)
    {
        _supportersService = supportersService;
        _ = LoadSupportersAsync();
    }

    [RelayCommand]
    private async Task LoadSupportersAsync()
    {
        var names = await _supportersService.GetSupportersAsync();

        Supporters.Clear();
        foreach (var name in names)
        {
            Supporters.Add(name);
        }

        HasSupporters = Supporters.Count > 0;
    }

    [RelayCommand]
    private void OpenGitHub()
    {
        Process.Start(new ProcessStartInfo(GitHubUrl) { UseShellExecute = true });
    }

    [RelayCommand]
    private void OpenCoffee()
    {
        Process.Start(new ProcessStartInfo(CoffeeUrl) { UseShellExecute = true });
    }

    [RelayCommand]
    private void ReportBug()
    {
        Process.Start(new ProcessStartInfo(ReportBugUrl) { UseShellExecute = true });
    }
}
