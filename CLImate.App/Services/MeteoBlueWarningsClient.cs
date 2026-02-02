using System.Text.Json;
using CLImate.App.Models;

namespace CLImate.App.Services;

public interface IMeteoBlueWarningsClient
{
    Task<IReadOnlyList<WeatherWarning>> GetWarningsAsync(double latitude, double longitude, CancellationToken cancellationToken);
    bool IsConfigured { get; }
}

public sealed class MeteoBlueWarningsClient : IMeteoBlueWarningsClient
{
    private readonly IJsonHttpClient _client;
    private readonly string? _apiKey;

    public MeteoBlueWarningsClient(IJsonHttpClient client)
    {
        _client = client;
        _apiKey = Environment.GetEnvironmentVariable("METEOBLUE_API_KEY");
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<IReadOnlyList<WeatherWarning>> GetWarningsAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            return Array.Empty<WeatherWarning>();
        }

        try
        {
            var url =
                "https://my.meteoblue.com/warnings/list" +
                $"?apikey={Uri.EscapeDataString(_apiKey!)}" +
                $"&lat={latitude:F4}&lon={longitude:F4}";

            var root = await _client.GetAsync<JsonElement>(url, cancellationToken);
            if (root.ValueKind != JsonValueKind.Object)
            {
                return Array.Empty<WeatherWarning>();
            }

            if (!root.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<WeatherWarning>();
            }

            var warnings = new List<WeatherWarning>();
            foreach (var item in results.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var summary = FirstString(item, "headline", "event", "event_en", "type", "category") ?? "Weather warning";
                var severity = FirstString(item, "severity", "severity_en");
                if (!string.IsNullOrWhiteSpace(severity))
                {
                    summary = $"{summary} ({severity})";
                }

                var starts = ParseDate(item, "starts", "start", "effective");
                var ends = ParseDate(item, "ends", "end", "expires", "expires_at");

                warnings.Add(new WeatherWarning(summary, starts, ends));
            }

            return warnings;
        }
        catch (HttpRequestException)
        {
            return Array.Empty<WeatherWarning>();
        }
        catch (JsonException)
        {
            return Array.Empty<WeatherWarning>();
        }
    }

    private static string? FirstString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
        }

        return null;
    }

    private static DateTimeOffset? ParseDate(JsonElement element, params string[] names)
    {
        var raw = FirstString(element, names);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(raw, out var dto))
        {
            return dto;
        }

        return null;
    }
}
