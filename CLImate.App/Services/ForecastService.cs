using CLImate.App.Models;

namespace CLImate.App.Services;

public interface IForecastService
{
    Task<Forecast?> GetForecastAsync(double latitude, double longitude, Units units, CancellationToken cancellationToken);
}

public sealed class ForecastService : IForecastService
{
    private readonly IJsonHttpClient _client;
    private readonly IApiMapper _mapper;

    public ForecastService(IJsonHttpClient client, IApiMapper mapper)
    {
        _client = client;
        _mapper = mapper;
    }

    public async Task<Forecast?> GetForecastAsync(double latitude, double longitude, Units units, CancellationToken cancellationToken)
    {
        var url =
            "https://api.open-meteo.com/v1/forecast" +
            $"?latitude={latitude:F4}&longitude={longitude:F4}" +
            "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_sum,wind_speed_10m_max,wind_gusts_10m_max" +
            "&hourly=weather_code,temperature_2m,precipitation,wind_speed_10m,wind_gusts_10m" +
            $"&timezone=auto{BuildUnitParameters(units)}";

        var response = await _client.GetAsync<ForecastResponse>(url, cancellationToken);
        return _mapper.MapForecast(response);
    }

    private static string BuildUnitParameters(Units units)
    {
        return units switch
        {
            Units.Imperial => "&temperature_unit=fahrenheit&wind_speed_unit=mph&precipitation_unit=inch",
            _ => string.Empty
        };
    }
}
