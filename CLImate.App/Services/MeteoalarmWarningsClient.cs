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
                var eventName = WarningJsonHelpers.GetString(warning, "event") ?? WarningJsonHelpers.GetString(warning, "headline") ?? "Weather alert";
                var severity = WarningJsonHelpers.GetString(warning, "severity");
                var summary = string.IsNullOrWhiteSpace(severity) ? eventName : $"{eventName} ({severity})";

                var starts = WarningJsonHelpers.ParseDate(warning, "start", "onset");
                var ends = WarningJsonHelpers.ParseDate(warning, "end", "expires");

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
