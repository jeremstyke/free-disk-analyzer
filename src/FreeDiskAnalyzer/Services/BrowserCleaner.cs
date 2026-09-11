using System.IO;
using FreeDiskAnalyzer.Core.Utilities;
using FreeDiskAnalyzer.Models;
using Microsoft.Data.Sqlite;

namespace FreeDiskAnalyzer.Services;

public sealed class BrowserCleaner : IBrowserCleaner
{
    private readonly ISafeDeleteService _safeDeleteService;

    public BrowserCleaner(ISafeDeleteService safeDeleteService)
    {
        _safeDeleteService = safeDeleteService;
    }

    public Task<IReadOnlyList<BrowserCleanupItem>> ScanAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var items = new List<BrowserCleanupItem>();
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var chromeProfile = Path.Combine(localAppData, "Google", "Chrome", "User Data", "Default");
            items.AddRange(ScanChromiumBrowser("Chrome", chromeProfile));

            var edgeProfile = Path.Combine(localAppData, "Microsoft", "Edge", "User Data", "Default");
            items.AddRange(ScanChromiumBrowser("Edge", edgeProfile));

            items.AddRange(ScanFirefox());

            return (IReadOnlyList<BrowserCleanupItem>)items;
        }, cancellationToken);
    }

    public Task<BrowserCleanupResult> CleanAsync(
        IEnumerable<BrowserCleanupItem> items,
        IReadOnlyList<string>? cookieWhitelist = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            long freed = 0;
            var cleaned = 0;
            var skipped = 0;
            var hasWhitelist = cookieWhitelist is { Count: > 0 };

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var (anySuccess, bytesFreed) = item.Category == BrowserCleanupCategory.Cookies && hasWhitelist
                    ? CleanCookiesWithWhitelist(item, cookieWhitelist!)
                    : DeleteItemPaths(item);

                if (anySuccess)
                {
                    cleaned++;
                    freed += bytesFreed;
                }
                else
                {
                    skipped++;
                }
            }

            return new BrowserCleanupResult(freed, cleaned, skipped);
        }, cancellationToken);
    }

    /// <summary>
    /// Deletes each path in the item and reports the real, measured size of
    /// only the paths that actually succeeded, rather than the item's total
    /// scanned size, which would overstate the result if one of several
    /// paths (e.g. Cache and Code Cache) was locked and skipped.
    /// </summary>
    private (bool AnySuccess, long BytesFreed) DeleteItemPaths(BrowserCleanupItem item)
    {
        var anySuccess = false;
        long freed = 0;

        foreach (var path in item.Paths)
        {
            // Re-checked here even though scanning already only looked in known
            // browser profile locations, this is the last line of defense.
            if (PathSafetyGuard.IsProtected(path))
            {
                continue;
            }

            var isDirectory = Directory.Exists(path);
            var pathSize = isDirectory ? GetFolderSizeBytes(path) : GetFileSizeBytes(path);

            var success = isDirectory
                ? _safeDeleteService.TryDeleteDirectory(path)
                : _safeDeleteService.TryDeleteFile(path);

            if (success)
            {
                anySuccess = true;
                freed += pathSize;
            }
        }

        return (anySuccess, freed);
    }

    /// <summary>
    /// Deletes cookie rows for domains not in the whitelist, in place,
    /// instead of removing the whole cookies file. Schema differs by
    /// browser (Chromium: table "cookies"/"host_key", Firefox: table
    /// "moz_cookies"/"host"). Any failure (locked file, unexpected schema)
    /// skips that file rather than risking a partial write. Bytes freed is
    /// measured as the actual file size reduction after VACUUM, not assumed.
    /// </summary>
    private (bool AnySuccess, long BytesFreed) CleanCookiesWithWhitelist(BrowserCleanupItem item, IReadOnlyList<string> whitelist)
    {
        var (table, column) = item.BrowserName == "Firefox"
            ? ("moz_cookies", "host")
            : ("cookies", "host_key");

        var anySuccess = false;
        long freed = 0;

        foreach (var path in item.Paths)
        {
            if (PathSafetyGuard.IsProtected(path)) continue;
            if (!File.Exists(path)) { anySuccess = true; continue; }

            var sizeBefore = GetFileSizeBytes(path);

            try
            {
                using var connection = new SqliteConnection($"Data Source={path}");
                connection.Open();

                using var deleteCommand = connection.CreateCommand();
                var whereClauses = string.Join(" AND ", whitelist.Select((_, i) => $"{column} NOT LIKE @p{i}"));
                deleteCommand.CommandText = $"DELETE FROM {table} WHERE {whereClauses}";

                for (var i = 0; i < whitelist.Count; i++)
                {
                    deleteCommand.Parameters.AddWithValue($"@p{i}", "%" + whitelist[i]);
                }

                deleteCommand.ExecuteNonQuery();

                using var vacuumCommand = connection.CreateCommand();
                vacuumCommand.CommandText = "VACUUM";
                vacuumCommand.ExecuteNonQuery();

                anySuccess = true;
                var sizeAfter = GetFileSizeBytes(path);
                freed += Math.Max(0, sizeBefore - sizeAfter);
            }
            catch (Exception ex) when (ex is SqliteException or IOException or UnauthorizedAccessException)
            {
                // Locked (browser still open), unexpected schema, or any
                // other SQL problem: skip this file rather than risk a
                // partial or corrupt write to another program's database.
            }
        }

        return (anySuccess, freed);
    }

    private static IEnumerable<BrowserCleanupItem> ScanChromiumBrowser(string browserName, string profileRoot)
    {
        if (!Directory.Exists(profileRoot))
        {
            yield break;
        }

        var cachePaths = new List<string>();
        long cacheSize = 0;

        foreach (var cacheFolderName in new[] { "Cache", "Code Cache" })
        {
            var cacheDir = Path.Combine(profileRoot, cacheFolderName);
            if (!Directory.Exists(cacheDir)) continue;

            cachePaths.Add(cacheDir);
            cacheSize += GetFolderSizeBytes(cacheDir);
        }

        if (cachePaths.Count > 0)
        {
            yield return new BrowserCleanupItem
            {
                BrowserName = browserName,
                Category = BrowserCleanupCategory.Cache,
                Paths = cachePaths,
                SizeBytes = cacheSize
            };
        }

        // Modern Chromium stores cookies under Network\Cookies, older versions
        // used Cookies directly in the profile root.
        var cookiesPath = File.Exists(Path.Combine(profileRoot, "Network", "Cookies"))
            ? Path.Combine(profileRoot, "Network", "Cookies")
            : File.Exists(Path.Combine(profileRoot, "Cookies"))
                ? Path.Combine(profileRoot, "Cookies")
                : null;

        if (cookiesPath is not null)
        {
            yield return new BrowserCleanupItem
            {
                BrowserName = browserName,
                Category = BrowserCleanupCategory.Cookies,
                Paths = new[] { cookiesPath },
                SizeBytes = GetFileSizeBytes(cookiesPath)
            };
        }

        // "History" is a separate SQLite database from "Bookmarks" in Chromium
        // browsers, safe to clear without touching bookmarks.
        var historyPath = Path.Combine(profileRoot, "History");
        if (File.Exists(historyPath))
        {
            yield return new BrowserCleanupItem
            {
                BrowserName = browserName,
                Category = BrowserCleanupCategory.History,
                Paths = new[] { historyPath },
                SizeBytes = GetFileSizeBytes(historyPath)
            };
        }
    }

    private static IEnumerable<BrowserCleanupItem> ScanFirefox()
    {
        var profilesRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla", "Firefox", "Profiles");

        if (!Directory.Exists(profilesRoot))
        {
            yield break;
        }

        var defaultProfile = SafeEnumerateDirectories(profilesRoot, "*.default-release").FirstOrDefault()
            ?? SafeEnumerateDirectories(profilesRoot, "*.default").FirstOrDefault();

        if (defaultProfile is null)
        {
            yield break;
        }

        var cacheDir = Path.Combine(defaultProfile, "cache2");
        if (Directory.Exists(cacheDir))
        {
            yield return new BrowserCleanupItem
            {
                BrowserName = "Firefox",
                Category = BrowserCleanupCategory.Cache,
                Paths = new[] { cacheDir },
                SizeBytes = GetFolderSizeBytes(cacheDir)
            };
        }

        var cookiesFile = Path.Combine(defaultProfile, "cookies.sqlite");
        if (File.Exists(cookiesFile))
        {
            yield return new BrowserCleanupItem
            {
                BrowserName = "Firefox",
                Category = BrowserCleanupCategory.Cookies,
                Paths = new[] { cookiesFile },
                SizeBytes = GetFileSizeBytes(cookiesFile)
            };
        }

        // Firefox history lives in places.sqlite together with bookmarks,
        // intentionally not offered here, see IBrowserCleaner.
    }

    private static IEnumerable<string> SafeEnumerateDirectories(string root, string pattern)
    {
        try
        {
            return Directory.EnumerateDirectories(root, pattern);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Enumerable.Empty<string>();
        }
    }

    private static long GetFolderSizeBytes(string folderPath)
    {
        long total = 0;

        try
        {
            foreach (var file in Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                total += GetFileSizeBytes(file);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Return whatever was accumulated before the error.
        }

        return total;
    }

    private static long GetFileSizeBytes(string filePath)
    {
        try
        {
            return File.Exists(filePath) ? new FileInfo(filePath).Length : 0;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return 0;
        }
    }
}
