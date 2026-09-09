using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class AboutViewModel : ObservableObject
{
    public const string GitHubUrl = "https://github.com/jeremstyke/free-disk-analyzer";
    public const string CleanTabUrl = "https://getcleantab.com/";

    public string VersionDisplay { get; } =
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    [RelayCommand]
    private void OpenGitHub()
    {
        Process.Start(new ProcessStartInfo(GitHubUrl) { UseShellExecute = true });
    }

    [RelayCommand]
    private void OpenCleanTab()
    {
        Process.Start(new ProcessStartInfo(CleanTabUrl) { UseShellExecute = true });
    }
}
