namespace FreeDiskAnalyzer.Models;

/// <summary>One selectable language: a BCP-47-ish code ("en", "fr") and its display name.</summary>
public sealed record LanguageOption(string Code, string DisplayName);
