namespace FreeDiskAnalyzer.Core.Models;

/// <summary>
/// Broad category a file belongs to, based on its extension. Used to build the
/// "storage by category" breakdown shown on the Dashboard.
/// </summary>
public enum FileCategory
{
    Documents,
    Images,
    Video,
    Audio,
    Archives,
    Executables,
    Code,
    System,
    Other
}
