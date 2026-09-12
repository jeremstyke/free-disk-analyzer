namespace FreeDiskAnalyzer.Models;

public sealed class InstalledProgram
{
    public required string DisplayName { get; init; }
    public string? Publisher { get; init; }
    public string? DisplayVersion { get; init; }
    public long EstimatedSizeBytes { get; init; }
    public DateTime? InstallDate { get; init; }

    /// <summary>The program's own uninstaller command line, exactly as Windows "Apps & features" would run it.</summary>
    public required string UninstallString { get; init; }

    public string SizeDisplay => EstimatedSizeBytes > 0
        ? Core.Utilities.ByteSizeFormatter.Format(EstimatedSizeBytes)
        : "-";
}
