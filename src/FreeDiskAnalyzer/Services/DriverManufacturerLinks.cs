namespace FreeDiskAnalyzer.Services;

/// <summary>
/// Maps the raw manufacturer string WMI reports (inconsistent formatting,
/// e.g. "NVIDIA Corporation", "Intel Corporation", "Realtek Semiconductor
/// Corp.") to the right place to check for a driver update. Known,
/// well-established manufacturers get a direct link to their driver page.
/// Anything else falls back to a web search for that manufacturer's name,
/// still never downloads or picks a specific driver automatically.
/// </summary>
public static class DriverManufacturerLinks
{
    private static readonly (string Match, string Label, string Url)[] KnownManufacturers =
    {
        ("NVIDIA", "NVIDIA", "https://www.nvidia.com/en-us/drivers/"),
        ("Advanced Micro Devices", "AMD", "https://www.amd.com/en/support"),
        ("AMD", "AMD", "https://www.amd.com/en/support"),
        ("Intel", "Intel", "https://www.intel.com/content/www/us/en/support/detect.html"),
        ("Realtek", "Realtek", "https://www.realtek.com/en/downloads"),
        ("Qualcomm", "Qualcomm", "https://www.qualcomm.com/support"),
        ("Broadcom", "Broadcom", "https://www.broadcom.com/support/download-search"),
        ("Logitech", "Logitech", "https://www.logitech.com/en-us/software/"),
        ("Microsoft", "Windows Update", "ms-settings:windowsupdate"),
    };

    /// <summary>Returns a short label (site or "Windows Update") and the URL/URI to open for this driver's manufacturer.</summary>
    public static (string Label, string Url) GetLink(string manufacturer, string deviceName)
    {
        if (!string.IsNullOrWhiteSpace(manufacturer))
        {
            foreach (var (match, label, url) in KnownManufacturers)
            {
                if (manufacturer.Contains(match, StringComparison.OrdinalIgnoreCase))
                {
                    return (label, url);
                }
            }
        }

        // Unknown manufacturer: a general web search is the honest fallback,
        // rather than guessing at a direct driver page that may not exist.
        var query = Uri.EscapeDataString($"{manufacturer} {deviceName} driver download".Trim());
        return ("Search", $"https://www.google.com/search?q={query}");
    }
}
