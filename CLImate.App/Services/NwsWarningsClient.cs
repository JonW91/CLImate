using System.Text.Json;
using CLImate.App.Models;

namespace CLImate.App.Services;

public interface INwsWarningsClient
{
    Task<IReadOnlyList<WeatherWarning>> GetWarningsAsync(double latitude, double longitude, CancellationToken cancellationToken);
}

public sealed class NwsWarningsClient : INwsWarningsClient
{
    private readonly IJsonHttpClient _client;

    public NwsWarningsClient(IJsonHttpClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<WeatherWarning>> GetWarningsAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://api.weather.gov/alerts/active?point={latitude:F4},{longitude:F4}";
            var root = await _client.GetAsync<JsonElement>(url, cancellationToken);
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("features", out var features))
            {
                return Array.Empty<WeatherWarning>();
            }

            var results = new List<WeatherWarning>();
            foreach (var feature in features.EnumerateArray())
            {
                if (!feature.TryGetProperty("properties", out var props) || props.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var headline = GetString(props, "headline") ?? GetString(props, "event") ?? "Weather alert";
                var severity = GetString(props, "severity");
                var summary = string.IsNullOrWhiteSpace(severity) ? headline : $"{headline} ({severity})";

                var starts = ParseDate(props, "effective", "onset");
                var ends = ParseDate(props, "ends", "expires");

                results.Add(new WeatherWarning(summary, starts, ends));
            }

            return results;
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

    private static string? GetString(JsonElement element, string name)
    {
        if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString();
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }

        return null;
    }

    private static DateTimeOffset? ParseDate(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            var raw = GetString(element, name);
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            if (DateTimeOffset.TryParse(raw, out var dto))
            {
                return dto;
            }
        }

        return null;
    }
}
