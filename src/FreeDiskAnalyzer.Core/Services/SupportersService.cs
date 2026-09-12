using System.Text.Json;

namespace FreeDiskAnalyzer.Core.Services;

public sealed class SupportersService : ISupportersService
{
    private const string SupportersUrl = "https://jeremstyke.github.io/purgecore/supporters.json";

    public async Task<IReadOnlyList<string>> GetSupportersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FreeDiskAnalyzer-App");

            var json = await client.GetStringAsync(SupportersUrl, cancellationToken);
            var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("supporters", out var supportersElement) ||
                supportersElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<string>();
            }

            var names = new List<string>();
            foreach (var item in supportersElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String && item.GetString() is { Length: > 0 } name)
                {
                    names.Add(name);
                }
            }

            return names;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException or TaskCanceledException)
        {
            // Nice-to-have, never worth disturbing the user over: a missed
            // fetch just means an empty list, same as no supporters yet.
            return Array.Empty<string>();
        }
    }
}
