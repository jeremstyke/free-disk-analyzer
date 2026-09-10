using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FreeDiskAnalyzer.ViewModels;

/// <summary>
/// Purely informational "coming soon" page for the planned built-in VPN.
/// No payment flow here, that's deliberate, see docs/ROADMAP.md: the real
/// VPN backend doesn't exist yet, so nothing here can promise or sell
/// working VPN access. The support link goes to Gumroad but is framed as
/// funding future development, not as purchasing a feature that works today.
/// </summary>
public sealed partial class VpnViewModel : ObservableObject
{
    public const string SupportDevelopmentUrl = "https://jeremstyke.gumroad.com/coffee";
    public const string VpnPageUrl = "https://jeremstyke.github.io/free-disk-analyzer/vpn.html";

    [RelayCommand]
    private void OpenNordVpn()
    {
        Process.Start(new ProcessStartInfo(DashboardViewModel.NordVpnAffiliateUrl) { UseShellExecute = true });
    }

    [RelayCommand]
    private void OpenSupportDevelopment()
    {
        Process.Start(new ProcessStartInfo(SupportDevelopmentUrl) { UseShellExecute = true });
    }

    [RelayCommand]
    private void OpenVpnPage()
    {
        Process.Start(new ProcessStartInfo(VpnPageUrl) { UseShellExecute = true });
    }
}
