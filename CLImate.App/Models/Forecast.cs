namespace CLImate.App.Models;

public sealed class Forecast
{
    public Forecast(IReadOnlyList<DailyForecast> days, ForecastUnits units, IReadOnlyDictionary<string, string>? warningsByDate = null)
    {
        Days = days;
        Units = units;
        WarningsByDate = warningsByDate ?? new Dictionary<string, string>();
    }

    public IReadOnlyList<DailyForecast> Days { get; }
    public ForecastUnits Units { get; }
    public IReadOnlyDictionary<string, string> WarningsByDate { get; }

    public Forecast WithWarnings(IReadOnlyDictionary<string, string> warningsByDate)
        => new Forecast(Days, Units, warningsByDate);
}

public sealed class DailyForecast
{
    public DailyForecast(
        string date,
        int weatherCode,
        double temperatureMax,
        double temperatureMin,
        double precipitationSum,
        double windSpeedMax,
        double windGustsMax)
    {
        Date = date;
        WeatherCode = weatherCode;
        TemperatureMax = temperatureMax;
        TemperatureMin = temperatureMin;
        PrecipitationSum = precipitationSum;
        WindSpeedMax = windSpeedMax;
        WindGustsMax = windGustsMax;
    }

    public string Date { get; }
    public int WeatherCode { get; }
    public double TemperatureMax { get; }
    public double TemperatureMin { get; }
    public double PrecipitationSum { get; }
    public double WindSpeedMax { get; }
    public double WindGustsMax { get; }
}

public sealed class ForecastUnits
{
    public ForecastUnits(string temperature, string precipitation, string windSpeed, string windGusts)
    {
        Temperature = temperature;
        Precipitation = precipitation;
        WindSpeed = windSpeed;
        WindGusts = windGusts;
    }

    public string Temperature { get; }
    public string Precipitation { get; }
    public string WindSpeed { get; }
    public string WindGusts { get; }
}
