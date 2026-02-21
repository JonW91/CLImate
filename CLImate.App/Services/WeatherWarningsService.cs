using System.Linq;
using CLImate.App.Models;

namespace CLImate.App.Services;

public interface IWeatherWarningsService
{
    Task<IReadOnlyDictionary<string, string>> GetDailyWarningsAsync(
        double latitude,
        double longitude,
        string? countryCode,
        IReadOnlyList<string> dates,
        CancellationToken cancellationToken);
}

public sealed class WeatherWarningsService : IWeatherWarningsService
{
    private readonly INwsWarningsClient _nwsClient;

    public WeatherWarningsService(INwsWarningsClient nwsClient)
    {
        _nwsClient = nwsClient;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetDailyWarningsAsync(
        double latitude,
        double longitude,
        string? countryCode,
        IReadOnlyList<string> dates,
        CancellationToken cancellationToken)
    {
        var output = dates.ToDictionary(d => d, _ => "none");

        if (!string.Equals(countryCode, "US", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var date in dates)
            {
                output[date] = "no warnings available for this region";
            }
            return output;
        }

        var warnings = await _nwsClient.GetWarningsAsync(latitude, longitude, cancellationToken);
        if (warnings.Count == 0)
        {
            return output;
        }

        foreach (var warning in warnings)
        {
            var summary = warning.Summary;
            var start = warning.Starts?.Date;
            var end = warning.Ends?.Date ?? warning.Starts?.Date;

            if (start == null)
            {
                continue;
            }

            var current = start.Value;
            var last = end ?? start.Value;
            while (current <= last)
            {
                var key = current.ToString("yyyy-MM-dd");
                if (output.TryGetValue(key, out var existing))
                {
                    if (existing == "none" || existing.StartsWith("no warnings available", StringComparison.OrdinalIgnoreCase))
                    {
                        output[key] = summary;
                    }
                    else if (!existing.Contains(summary, StringComparison.OrdinalIgnoreCase))
                    {
                        output[key] = $"{existing}; {summary}";
                    }
                }

                current = current.AddDays(1);
            }
        }

        return output;
    }
}
