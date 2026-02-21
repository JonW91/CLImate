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
    private readonly IMeteoalarmWarningsClient _meteoalarmClient;

    public WeatherWarningsService(INwsWarningsClient nwsClient, IMeteoalarmWarningsClient meteoalarmClient)
    {
        _nwsClient = nwsClient;
        _meteoalarmClient = meteoalarmClient;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetDailyWarningsAsync(
        double latitude,
        double longitude,
        string? countryCode,
        IReadOnlyList<string> dates,
        CancellationToken cancellationToken)
    {
        var output = dates.ToDictionary(d => d, _ => "none");

        if (IsEuCountry(countryCode))
        {
            return await PopulateWarningsAsync(
                output,
                () => _meteoalarmClient.GetWarningsAsync(latitude, longitude, cancellationToken));
        }

        if (string.Equals(countryCode, "US", StringComparison.OrdinalIgnoreCase))
        {
            return await PopulateWarningsAsync(
                output,
                () => _nwsClient.GetWarningsAsync(latitude, longitude, cancellationToken));
        }

        foreach (var date in dates)
        {
            output[date] = "no warnings available for this region";
        }

        return output;
    }

    private static async Task<IReadOnlyDictionary<string, string>> PopulateWarningsAsync(
        Dictionary<string, string> output,
        Func<Task<IReadOnlyList<WeatherWarning>>> fetch)
    {
        var warnings = await fetch();
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

    private static bool IsEuCountry(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return false;
        }

        return EuCountryCodes.Contains(countryCode.Trim().ToUpperInvariant());
    }

    private static readonly HashSet<string> EuCountryCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        // EU + EEA + UK (Meteoalarm coverage)
        "AT","BE","BG","HR","CY","CZ","DK","EE","FI","FR","DE","GR","HU","IE","IT","LV","LT","LU","MT","NL","PL","PT","RO","SK","SI","ES","SE",
        "IS","NO","LI","GB","UK","CH"
    };
}
