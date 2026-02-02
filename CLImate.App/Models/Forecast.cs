namespace CLImate.App.Models;

public sealed class Forecast
{
    public Forecast(
        IReadOnlyList<DailyForecast> days,
        ForecastUnits units,
        TodayForecast? today = null,
        HourlyForecast? hourly = null,
        IReadOnlyDictionary<string, string>? warningsByDate = null)
    {
        Days = days;
        Units = units;
        Today = today;
        Hourly = hourly;
        WarningsByDate = warningsByDate ?? new Dictionary<string, string>();
    }

    public IReadOnlyList<DailyForecast> Days { get; }
    public ForecastUnits Units { get; }
    public TodayForecast? Today { get; }
    public HourlyForecast? Hourly { get; }
    public IReadOnlyDictionary<string, string> WarningsByDate { get; }

    public Forecast WithWarnings(IReadOnlyDictionary<string, string> warningsByDate)
        => new Forecast(Days, Units, Today, Hourly, warningsByDate);
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

public sealed class TodayForecast
{
    public TodayForecast(string date, IReadOnlyList<DayPartForecast> segments)
    {
        Date = date;
        Segments = segments;
    }

    public string Date { get; }
    public IReadOnlyList<DayPartForecast> Segments { get; }
}

public sealed class DayPartForecast
{
    public DayPartForecast(
        string label,
        int weatherCode,
        double temperatureAverage,
        double precipitationSum,
        double windSpeedMax,
        double windGustsMax)
    {
        Label = label;
        WeatherCode = weatherCode;
        TemperatureAverage = temperatureAverage;
        PrecipitationSum = precipitationSum;
        WindSpeedMax = windSpeedMax;
        WindGustsMax = windGustsMax;
    }

    public string Label { get; }
    public int WeatherCode { get; }
    public double TemperatureAverage { get; }
    public double PrecipitationSum { get; }
    public double WindSpeedMax { get; }
    public double WindGustsMax { get; }
}

public sealed class HourlyForecast
{
    public HourlyForecast(string date, IReadOnlyList<HourForecast> hours)
    {
        Date = date;
        Hours = hours;
    }

    public string Date { get; }
    public IReadOnlyList<HourForecast> Hours { get; }
}

public sealed class HourForecast
{
    public HourForecast(
        DateTime time,
        int weatherCode,
        double temperature,
        double precipitation,
        double windSpeed,
        double windGusts)
    {
        Time = time;
        WeatherCode = weatherCode;
        Temperature = temperature;
        Precipitation = precipitation;
        WindSpeed = windSpeed;
        WindGusts = windGusts;
    }

    public DateTime Time { get; }
    public int WeatherCode { get; }
    public double Temperature { get; }
    public double Precipitation { get; }
    public double WindSpeed { get; }
    public double WindGusts { get; }

    public string TimeLabel => Time.ToString("HH:mm");
    public int Hour => Time.Hour;
}
