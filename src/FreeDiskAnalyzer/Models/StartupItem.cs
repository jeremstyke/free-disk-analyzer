namespace FreeDiskAnalyzer.Models;

public sealed class StartupItem
{
    public required string Name { get; init; }
    public required string CommandOrPath { get; init; }
    public required StartupItemSource Source { get; init; }
    public required bool IsEnabled { get; init; }
}
