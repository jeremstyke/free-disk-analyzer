using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Core.Services;

/// <summary>
/// Maps a file extension to a broad <see cref="FileCategory"/>, used for the
/// "storage by category" chart. Extension list is intentionally not
/// exhaustive, unknown extensions fall back to <see cref="FileCategory.Other"/>.
/// </summary>
public static class FileCategoryClassifier
{
    private static readonly Dictionary<string, FileCategory> ExtensionMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Documents
            [".doc"] = FileCategory.Documents,
            [".docx"] = FileCategory.Documents,
            [".pdf"] = FileCategory.Documents,
            [".txt"] = FileCategory.Documents,
            [".rtf"] = FileCategory.Documents,
            [".odt"] = FileCategory.Documents,
            [".xls"] = FileCategory.Documents,
            [".xlsx"] = FileCategory.Documents,
            [".csv"] = FileCategory.Documents,
            [".ppt"] = FileCategory.Documents,
            [".pptx"] = FileCategory.Documents,

            // Images
            [".jpg"] = FileCategory.Images,
            [".jpeg"] = FileCategory.Images,
            [".png"] = FileCategory.Images,
            [".gif"] = FileCategory.Images,
            [".bmp"] = FileCategory.Images,
            [".svg"] = FileCategory.Images,
            [".webp"] = FileCategory.Images,
            [".heic"] = FileCategory.Images,
            [".tif"] = FileCategory.Images,
            [".tiff"] = FileCategory.Images,

            // Video
            [".mp4"] = FileCategory.Video,
            [".mkv"] = FileCategory.Video,
            [".avi"] = FileCategory.Video,
            [".mov"] = FileCategory.Video,
            [".wmv"] = FileCategory.Video,
            [".flv"] = FileCategory.Video,
            [".webm"] = FileCategory.Video,

            // Audio
            [".mp3"] = FileCategory.Audio,
            [".wav"] = FileCategory.Audio,
            [".flac"] = FileCategory.Audio,
            [".aac"] = FileCategory.Audio,
            [".ogg"] = FileCategory.Audio,
            [".wma"] = FileCategory.Audio,
            [".m4a"] = FileCategory.Audio,

            // Archives
            [".zip"] = FileCategory.Archives,
            [".rar"] = FileCategory.Archives,
            [".7z"] = FileCategory.Archives,
            [".tar"] = FileCategory.Archives,
            [".gz"] = FileCategory.Archives,
            [".iso"] = FileCategory.Archives,

            // Executables
            [".exe"] = FileCategory.Executables,
            [".msi"] = FileCategory.Executables,
            [".bat"] = FileCategory.Executables,
            [".cmd"] = FileCategory.Executables,

            // Code
            [".cs"] = FileCategory.Code,
            [".js"] = FileCategory.Code,
            [".ts"] = FileCategory.Code,
            [".py"] = FileCategory.Code,
            [".java"] = FileCategory.Code,
            [".cpp"] = FileCategory.Code,
            [".c"] = FileCategory.Code,
            [".h"] = FileCategory.Code,
            [".html"] = FileCategory.Code,
            [".css"] = FileCategory.Code,
            [".json"] = FileCategory.Code,
            [".xml"] = FileCategory.Code,
            [".xaml"] = FileCategory.Code,

            // System
            [".sys"] = FileCategory.System,
            [".dll"] = FileCategory.System,
            [".ini"] = FileCategory.System,
            [".log"] = FileCategory.System,
            [".dat"] = FileCategory.System,
        };

    public static FileCategory Classify(string extension) =>
        ExtensionMap.TryGetValue(extension, out var category) ? category : FileCategory.Other;
}
