using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Models;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class UninstallViewModel : ObservableObject
{
    private readonly IInstalledProgramsService _installedProgramsService;
    private List<InstalledProgram> _allPrograms = new();

    public ObservableCollection<InstalledProgram> Programs { get; } = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string searchText = string.Empty;

    public UninstallViewModel(IInstalledProgramsService installedProgramsService)
    {
        _installedProgramsService = installedProgramsService;
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;

        try
        {
            _allPrograms = (await _installedProgramsService.GetInstalledProgramsAsync()).ToList();
            ApplyFilter();
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        Programs.Clear();

        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allPrograms
            : _allPrograms.Where(p => p.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var program in filtered)
        {
            Programs.Add(program);
        }
    }

    /// <summary>
    /// Runs the program's own uninstaller, exactly what Windows' "Apps &amp;
    /// features" would run. PurgeCore never deletes the program's files or
    /// registry keys itself, guessing at what's safe to remove after an
    /// uninstall is exactly the kind of risk the registry cleaner decision
    /// already ruled out.
    /// </summary>
    [RelayCommand]
    private void UninstallProgram(InstalledProgram? program)
    {
        if (program is null) return;

        var confirmed = MessageBox.Show(
            string.Format(Resources.Strings.Uninstall_ConfirmBody, program.DisplayName),
            Resources.Strings.Uninstall_ConfirmTitle,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes;

        if (!confirmed) return;

        try
        {
            // The stored string is a full command line (path + arguments,
            // and for MSI-based installs, "MsiExec.exe /X{GUID}"), running it
            // through cmd.exe handles quoting the same way Windows itself does
            // rather than trying to split the string apart ourselves.
            Process.Start(new ProcessStartInfo("cmd.exe", $"/c \"{program.UninstallString}\"")
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            MessageBox.Show(
                Resources.Strings.Uninstall_LaunchFailedBody,
                Resources.Strings.Uninstall_ConfirmTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
