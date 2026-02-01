using CLImate.App.Models;

namespace CLImate.Tests.Models;

public sealed class ForecastTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        var days = new List<DailyForecast>
        {
            new DailyForecast("2026-02-01", 1, 15.0, 10.0, 2.5, 12.0, 25.0)
        };
        var units = new ForecastUnits("°C", "mm", "km/h", "km/h");

        var forecast = new Forecast(days, units);

        Assert.Equal(days, forecast.Days);
        Assert.Equal(units, forecast.Units);
        Assert.Empty(forecast.WarningsByDate);
    }

    [Fact]
    public void Constructor_WithWarnings_InitializesPropertiesCorrectly()
    {
        var days = new List<DailyForecast>
        {
            new DailyForecast("2026-02-01", 1, 15.0, 10.0, 2.5, 12.0, 25.0)
        };
        var units = new ForecastUnits("°C", "mm", "km/h", "km/h");
        var warnings = new Dictionary<string, string>
        {
            ["2026-02-01"] = "Heavy rain"
        };

        var forecast = new Forecast(days, units, warnings);

        Assert.Equal(days, forecast.Days);
        Assert.Equal(units, forecast.Units);
        Assert.Single(forecast.WarningsByDate);
        Assert.Equal("Heavy rain", forecast.WarningsByDate["2026-02-01"]);
    }

    [Fact]
    public void WithWarnings_ReturnsNewForecastWithWarnings()
    {
        var days = new List<DailyForecast>
        {
            new DailyForecast("2026-02-01", 1, 15.0, 10.0, 2.5, 12.0, 25.0),
            new DailyForecast("2026-02-02", 2, 16.0, 11.0, 3.0, 14.0, 28.0)
        };
        var units = new ForecastUnits("°C", "mm", "km/h", "km/h");
        var forecast = new Forecast(days, units);

        var warnings = new Dictionary<string, string>
        {
            ["2026-02-01"] = "Heavy rain",
            ["2026-02-02"] = "Strong winds"
        };

        var forecastWithWarnings = forecast.WithWarnings(warnings);

        Assert.Equal(days, forecastWithWarnings.Days);
        Assert.Equal(units, forecastWithWarnings.Units);
        Assert.Equal(2, forecastWithWarnings.WarningsByDate.Count);
        Assert.Equal("Heavy rain", forecastWithWarnings.WarningsByDate["2026-02-01"]);
        Assert.Equal("Strong winds", forecastWithWarnings.WarningsByDate["2026-02-02"]);
        Assert.Empty(forecast.WarningsByDate); // Original should be unchanged
    }

    [Fact]
    public void WithWarnings_OriginalForecastUnchanged()
    {
        var days = new List<DailyForecast>
        {
            new DailyForecast("2026-02-01", 1, 15.0, 10.0, 2.5, 12.0, 25.0)
        };
        var units = new ForecastUnits("°C", "mm", "km/h", "km/h");
        var original = new Forecast(days, units);
        var warnings = new Dictionary<string, string> { ["2026-02-01"] = "Test warning" };

        var updated = original.WithWarnings(warnings);

        Assert.Empty(original.WarningsByDate);
        Assert.Single(updated.WarningsByDate);
    }
}

public sealed class DailyForecastTests
{
    [Fact]
    public void Constructor_InitializesAllProperties()
    {
        var forecast = new DailyForecast(
            date: "2026-02-01",
            weatherCode: 3,
            temperatureMax: 20.5,
            temperatureMin: 12.3,
            precipitationSum: 5.2,
            windSpeedMax: 15.7,
            windGustsMax: 30.1);

        Assert.Equal("2026-02-01", forecast.Date);
        Assert.Equal(3, forecast.WeatherCode);
        Assert.Equal(20.5, forecast.TemperatureMax);
        Assert.Equal(12.3, forecast.TemperatureMin);
        Assert.Equal(5.2, forecast.PrecipitationSum);
        Assert.Equal(15.7, forecast.WindSpeedMax);
        Assert.Equal(30.1, forecast.WindGustsMax);
    }
}

public sealed class ForecastUnitsTests
{
    [Fact]
    public void Constructor_InitializesAllProperties()
    {
        var units = new ForecastUnits(
            temperature: "°F",
            precipitation: "in",
            windSpeed: "mph",
            windGusts: "mph");

        Assert.Equal("°F", units.Temperature);
        Assert.Equal("in", units.Precipitation);
        Assert.Equal("mph", units.WindSpeed);
        Assert.Equal("mph", units.WindGusts);
    }
}

public sealed class WeatherWarningTests
{
    [Fact]
    public void Constructor_InitializesAllProperties()
    {
        var starts = DateTimeOffset.Parse("2026-02-01T12:00:00Z");
        var ends = DateTimeOffset.Parse("2026-02-02T12:00:00Z");
        
        var warning = new WeatherWarning("Heavy rain", starts, ends);

        Assert.Equal("Heavy rain", warning.Summary);
        Assert.Equal(starts, warning.Starts);
        Assert.Equal(ends, warning.Ends);
    }

    [Fact]
    public void Constructor_AllowsNullDates()
    {
        var warning = new WeatherWarning("Unknown event", null, null);

        Assert.Equal("Unknown event", warning.Summary);
        Assert.Null(warning.Starts);
        Assert.Null(warning.Ends);
    }
}
