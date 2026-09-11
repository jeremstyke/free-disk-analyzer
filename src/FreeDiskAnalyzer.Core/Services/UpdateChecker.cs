using System.Net.Http;
using System.Text.Json;
using FreeDiskAnalyzer.Core.Models;
using FreeDiskAnalyzer.Core.Utilities;

namespace FreeDiskAnalyzer.Core.Services;

public sealed class UpdateChecker : IUpdateChecker
{
    private const string ReleasesApiUrl = "https://api.github.com/repos/jeremstyke/purgecore/releases/latest";
    private const string InstallerAssetName = "PurgeCore-Setup.exe";

    public async Task<UpdateInfo> CheckForUpdateAsync(string currentVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FreeDiskAnalyzer-UpdateChecker");

            using var response = await client.GetAsync(ReleasesApiUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return UpdateInfo.NoUpdate;
            }

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = doc.RootElement;

            if (!root.TryGetProperty("tag_name", out var tagProp))
            {
                return UpdateInfo.NoUpdate;
            }

            var tagName = tagProp.GetString();
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return UpdateInfo.NoUpdate;
            }

            var releaseUrl = root.TryGetProperty("html_url", out var htmlUrlProp) ? htmlUrlProp.GetString() : null;

            string? downloadUrl = null;
            if (root.TryGetProperty("assets", out var assets))
            {
                foreach (var asset in assets.EnumerateArray())
                {
                    if (asset.TryGetProperty("name", out var nameProp) &&
                        nameProp.GetString() == InstallerAssetName &&
                        asset.TryGetProperty("browser_download_url", out var urlProp))
                    {
                        downloadUrl = urlProp.GetString();
                        break;
                    }
                }
            }

            if (downloadUrl is null)
            {
                return UpdateInfo.NoUpdate;
            }

            var latestVersion = tagName.TrimStart('v', 'V');
            var isNewer = ReleaseVersionComparer.IsNewer(latestVersion, currentVersion);

            return new UpdateInfo(isNewer, latestVersion, downloadUrl, releaseUrl);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            // No internet, GitHub unreachable, unexpected response shape:
            // fail quietly rather than bother the user about it.
            return UpdateInfo.NoUpdate;
        }
    }
}
