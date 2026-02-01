using CLImate.App.Models;

namespace CLImate.App.Services;

public interface IWeatherWarningsService
{
    Task<IReadOnlyDictionary<string, string>> GetDailyWarningsAsync(
        double latitude,
        double longitude,
        IReadOnlyList<string> dates,
        CancellationToken cancellationToken);
}

public sealed class WeatherWarningsService : IWeatherWarningsService
{
    private readonly IMeteoBlueWarningsClient _client;

    public WeatherWarningsService(IMeteoBlueWarningsClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetDailyWarningsAsync(
        double latitude,
        double longitude,
        IReadOnlyList<string> dates,
        CancellationToken cancellationToken)
    {
        var output = new Dictionary<string, string>();
        foreach (var date in dates)
        {
            output[date] = "none";
        }

        var warnings = await _client.GetWarningsAsync(latitude, longitude, cancellationToken);
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
                    if (existing == "none")
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
