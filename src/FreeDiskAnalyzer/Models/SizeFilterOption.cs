namespace FreeDiskAnalyzer.Models;

/// <summary>A selectable minimum-size filter. Bytes is -1 for the "Custom" option.</summary>
public sealed record SizeFilterOption(string Label, long Bytes);
