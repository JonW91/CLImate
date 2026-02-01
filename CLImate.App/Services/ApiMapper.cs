using CLImate.App.Models;

namespace CLImate.App.Services;

public interface IApiMapper
{
    Forecast? MapForecast(ForecastResponse? response);
    List<GeoResult> MapGeocoding(GeocodeResponse? response);
}

public sealed class ApiMapper : IApiMapper
{
    public Forecast? MapForecast(ForecastResponse? response)
    {
        var daily = response?.Daily;
        if (daily == null || daily.Time.Count == 0)
        {
            return null;
        }

        var units = BuildUnits(response?.DailyUnits);
        var days = new List<DailyForecast>(daily.Time.Count);

        for (var i = 0; i < daily.Time.Count; i++)
        {
            var date = daily.Time[i];
            var code = SafeGetInt(daily.WeatherCode, i, 0);
            var max = SafeGetDouble(daily.TemperatureMax, i, double.NaN);
            var min = SafeGetDouble(daily.TemperatureMin, i, double.NaN);
            var precip = SafeGetDouble(daily.PrecipitationSum, i, double.NaN);
            var wind = SafeGetDouble(daily.WindSpeedMax, i, double.NaN);
            var gust = SafeGetDouble(daily.WindGustsMax, i, double.NaN);

            days.Add(new DailyForecast(date, code, max, min, precip, wind, gust));
        }

        return new Forecast(days, units);
    }

    public List<GeoResult> MapGeocoding(GeocodeResponse? response)
        => response?.Results ?? new List<GeoResult>();

    private static ForecastUnits BuildUnits(DailyUnits? units)
    {
        if (units == null)
        {
            return new ForecastUnits("°C", "mm", "m/s", "m/s");
        }

        return new ForecastUnits(
            units.TemperatureMax,
            units.PrecipitationSum,
            units.WindSpeedMax,
            units.WindGustsMax);
    }

    private static int SafeGetInt(List<int>? list, int index, int fallback)
    {
        if (list == null || index >= list.Count) return fallback;
        return list[index];
    }

    private static double SafeGetDouble(List<double>? list, int index, double fallback)
    {
        if (list == null || index >= list.Count) return fallback;
        return list[index];
    }
}
