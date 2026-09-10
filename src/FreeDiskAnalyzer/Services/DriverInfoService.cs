using System.Management;
using FreeDiskAnalyzer.Models;

namespace FreeDiskAnalyzer.Services;

public sealed class DriverInfoService : IDriverInfoService
{
    public Task<IReadOnlyList<DriverInfo>> GetDriversAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var drivers = new List<DriverInfo>();

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT DeviceName, DriverVersion, DriverDate, Manufacturer FROM Win32_PnPSignedDriver " +
                    "WHERE DeviceName IS NOT NULL");

                foreach (ManagementBaseObject item in searcher.Get())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using (item)
                    {
                        var deviceName = item["DeviceName"] as string;
                        if (string.IsNullOrWhiteSpace(deviceName)) continue;

                        DateTime? driverDate = null;
                        if (item["DriverDate"] is string wmiDate && !string.IsNullOrWhiteSpace(wmiDate))
                        {
                            try
                            {
                                driverDate = ManagementDateTimeConverter.ToDateTime(wmiDate);
                            }
                            catch (ArgumentException)
                            {
                                // Unparsable date format for this device, leave it unknown.
                            }
                        }

                        drivers.Add(new DriverInfo
                        {
                            DeviceName = deviceName,
                            Manufacturer = item["Manufacturer"] as string ?? "Unknown",
                            Version = item["DriverVersion"] as string ?? "Unknown",
                            DriverDate = driverDate
                        });
                    }
                }
            }
            catch (Exception ex) when (ex is ManagementException or UnauthorizedAccessException)
            {
                // WMI unavailable or restricted: return whatever was gathered
                // before the error rather than failing the whole page.
            }

            return (IReadOnlyList<DriverInfo>)drivers
                .OrderByDescending(d => d.IsPotentiallyOld)
                .ThenBy(d => d.DeviceName)
                .ToList();
        }, cancellationToken);
    }
}
