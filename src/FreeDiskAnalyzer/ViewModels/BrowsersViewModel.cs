using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FreeDiskAnalyzer.Core.Utilities;
using FreeDiskAnalyzer.Models;
using FreeDiskAnalyzer.Services;

namespace FreeDiskAnalyzer.ViewModels;

public sealed partial class BrowsersViewModel : ObservableObject
{
    private readonly IBrowserCleaner _browserCleaner;
    private readonly ISettingsService _settingsService;
    private bool _isLoading;

    public ObservableCollection<BrowserCleanupItemViewModel> Items { get; } = new();

    [ObservableProperty]
    private bool isScanning;

    [ObservableProperty]
    private bool hasScanned;

    [ObservableProperty]
    private bool isCleaning;

    [ObservableProperty]
    private string? resultSummary;

    [ObservableProperty]
    private bool hasCleaned;

    [ObservableProperty]
    private string cookieWhitelistText = string.Empty;

    public BrowsersViewModel(IBrowserCleaner browserCleaner, ISettingsService settingsService)
    {
        _browserCleaner = browserCleaner;
        _settingsService = settingsService;

        _isLoading = true;
        CookieWhitelistText = string.Join(Environment.NewLine, _settingsService.Load().CookieWhitelist);
        _isLoading = false;

        _ = ScanAsync();
    }

    partial void OnCookieWhitelistTextChanged(string value)
    {
        if (_isLoading) return;

        var current = _settingsService.Load();
        current.CookieWhitelist = ParseWhitelist(value);
        _settingsService.Save(current);
    }

    private static List<string> ParseWhitelist(string text) =>
        text.Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeDomain)
            .Where(d => d.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>
    /// Strips a scheme and path if the user pastes a full URL instead of a
    /// bare domain (e.g. "https://gmail.com/mail" -> "gmail.com"). Without
    /// this, a pasted URL would never match any real cookie host and that
    /// whitelist entry would silently protect nothing.
    /// </summary>
    private static string NormalizeDomain(string raw)
    {
        var value = raw.Trim();

        var schemeIndex = value.IndexOf("://", StringComparison.Ordinal);
        if (schemeIndex >= 0)
        {
            value = value[(schemeIndex + 3)..];
        }

        var pathIndex = value.IndexOfAny(new[] { '/', '\\', '?', '#' });
        if (pathIndex >= 0)
        {
            value = value[..pathIndex];
        }

        return value.Trim().TrimEnd('.');
    }

    [RelayCommand(CanExecute = nameof(CanScan))]
    private async Task ScanAsync()
    {
        IsScanning = true;
        Items.Clear();

        try
        {
            var results = await _browserCleaner.ScanAsync();
            foreach (var item in results)
            {
                Items.Add(new BrowserCleanupItemViewModel(item));
            }
            HasScanned = true;
        }
        finally
        {
            IsScanning = false;
        }
    }

    private bool CanScan() => !IsScanning && !IsCleaning;

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var item in Items) item.IsSelected = true;
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var item in Items) item.IsSelected = false;
    }

    [RelayCommand(CanExecute = nameof(CanClean))]
    private async Task CleanSelectedAsync()
    {
        var selected = Items.Where(i => i.IsSelected).ToList();
        if (selected.Count == 0) return;

        var totalSize = selected.Sum(i => i.Item.SizeBytes);
        var whitelist = ParseWhitelist(CookieWhitelistText);
        var includesCookies = selected.Any(i => i.Item.Category == BrowserCleanupCategory.Cookies);

        var confirmed = MessageBox.Show(
            $"Clear {selected.Count} item(s), about {ByteSizeFormatter.Format(totalSize)}?\n" +
            "Close your browsers first for best results, anything still in use will be skipped rather than failing the whole operation." +
            (includesCookies && whitelist.Count > 0 ? $"\nCookies for {whitelist.Count} whitelisted domain(s) will be kept." : ""),
            "Clean browsers",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes;

        if (!confirmed) return;

        IsCleaning = true;

        try
        {
            var result = await _browserCleaner.CleanAsync(selected.Select(vm => vm.Item), whitelist);

            ResultSummary =
                $"Freed about {ByteSizeFormatter.Format(result.BytesFreed)} " +
                $"({result.ItemsCleaned} cleaned, {result.ItemsSkipped} skipped, usually because a browser was open).";
            HasCleaned = true;

            // Re-scan so the list reflects what's actually left, rather than
            // guessing which items fully succeeded.
            await ScanAsync();
        }
        finally
        {
            IsCleaning = false;
        }
    }

    private bool CanClean() => !IsCleaning && !IsScanning;

    partial void OnIsCleaningChanged(bool value)
    {
        CleanSelectedCommand.NotifyCanExecuteChanged();
        ScanCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsScanningChanged(bool value)
    {
        CleanSelectedCommand.NotifyCanExecuteChanged();
        ScanCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void OpenDeleteMe()
    {
        Process.Start(new ProcessStartInfo(AnalyzeViewModel.DeleteMeAffiliateUrl) { UseShellExecute = true });
    }
}
