using System.Windows.Media;

namespace FreeDiskAnalyzer.Models;

/// <summary>
/// One entry in the sidebar. Glyph is a Segoe Fluent Icons / Segoe MDL2 Assets
/// codepoint; verify these render correctly once run on an actual Windows
/// machine, glyph availability can vary slightly by Windows version.
/// </summary>
public sealed class NavItem
{
    public required NavKey Key { get; init; }
    public required string Label { get; init; }
    public required string Glyph { get; init; }

    /// <summary>Cycles through the app's accent palette so the sidebar isn't monochrome.</summary>
    public required Brush IconBrush { get; init; }
}
