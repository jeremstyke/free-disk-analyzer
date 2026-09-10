using System.Globalization;
using System.Text;
using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Core.Services;

/// <summary>
/// Builds a plain CSV report from a <see cref="ScanResult"/>: a summary
/// block, then the largest folders, then the largest files. Pure string
/// building, no file I/O, so it's easy to unit test and the caller decides
/// where to save it.
/// </summary>
public static class ScanReportExporter
{
    public static string BuildCsv(ScanResult result)
    {
        var sb = new StringBuilder();
        var culture = CultureInfo.InvariantCulture;

        sb.AppendLine("PurgeCore scan report");
        sb.AppendLine($"Root path,{Csv(result.RootPath)}");
        sb.AppendLine($"Completed (UTC),{result.CompletedAtUtc.ToString("u", culture)}");
        sb.AppendLine($"Total files,{result.TotalFilesScanned}");
        sb.AppendLine($"Total folders,{result.TotalFoldersScanned}");
        sb.AppendLine($"Total bytes,{result.TotalBytesScanned}");
        sb.AppendLine($"Cancelled,{result.WasCancelled}");
        sb.AppendLine();

        sb.AppendLine("Storage by category");
        sb.AppendLine("Category,Bytes");
        foreach (var (category, bytes) in result.BytesByCategory.OrderByDescending(kvp => kvp.Value))
        {
            sb.AppendLine($"{category},{bytes}");
        }
        sb.AppendLine();

        sb.AppendLine("Largest folders");
        sb.AppendLine("Name,Path,Bytes");
        foreach (var folder in result.LargestFolders)
        {
            sb.AppendLine($"{Csv(folder.Name)},{Csv(folder.FullPath)},{folder.SizeBytes}");
        }
        sb.AppendLine();

        sb.AppendLine("Largest files");
        sb.AppendLine("Name,Path,Bytes");
        foreach (var file in result.LargestFiles)
        {
            sb.AppendLine($"{Csv(file.Name)},{Csv(file.FullPath)},{file.SizeBytes}");
        }

        return sb.ToString();
    }

    private static string Csv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}
