namespace FreeDiskAnalyzer.Models;

public sealed class DriverInfo
{
    public required string DeviceName { get; init; }
    public required string Manufacturer { get; init; }
    public required string Version { get; init; }
    public required DateTime? DriverDate { get; init; }

    // A simple, conservative signal, not a diagnosis: just flags drivers
    // that haven't been updated in a while so the user knows where to look
    // first, if they want to.
    public bool IsPotentiallyOld => DriverDate is { } date && date < DateTime.Now.AddYears(-3);
}
