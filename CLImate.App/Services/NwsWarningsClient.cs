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

                var headline = WarningJsonHelpers.GetString(props, "headline") ?? WarningJsonHelpers.GetString(props, "event") ?? "Weather alert";
                var severity = WarningJsonHelpers.GetString(props, "severity");
                var summary = string.IsNullOrWhiteSpace(severity) ? headline : $"{headline} ({severity})";

                var starts = WarningJsonHelpers.ParseDate(props, "effective", "onset");
                var ends = WarningJsonHelpers.ParseDate(props, "ends", "expires");

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
        catch (WeatherApiException)
        {
            // Warnings are best-effort; never let a failing alerts feed block the forecast.
            return Array.Empty<WeatherWarning>();
        }
    }
}
