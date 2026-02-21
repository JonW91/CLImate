using System.Text.Json;
using CLImate.App.Models;

namespace CLImate.App.Services;

public interface IMeteoalarmWarningsClient
{
    Task<IReadOnlyList<WeatherWarning>> GetWarningsAsync(double latitude, double longitude, CancellationToken cancellationToken);
}

public sealed class MeteoalarmWarningsClient : IMeteoalarmWarningsClient
{
    private readonly IJsonHttpClient _client;

    public MeteoalarmWarningsClient(IJsonHttpClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<WeatherWarning>> GetWarningsAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://api.open-meteo.com/v1/warnings?latitude={latitude:F4}&longitude={longitude:F4}&language=en";
            var root = await _client.GetAsync<JsonElement>(url, cancellationToken);
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("warnings", out var warningsEl) || warningsEl.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<WeatherWarning>();
            }

            var results = new List<WeatherWarning>();
            foreach (var warning in warningsEl.EnumerateArray())
            {
                var eventName = GetString(warning, "event") ?? GetString(warning, "headline") ?? "Weather alert";
                var severity = GetString(warning, "severity");
                var summary = string.IsNullOrWhiteSpace(severity) ? eventName : $"{eventName} ({severity})";

                var starts = ParseDate(warning, "start") ?? ParseDate(warning, "onset");
                var ends = ParseDate(warning, "end") ?? ParseDate(warning, "expires");

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
