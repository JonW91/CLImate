using CLImate.App.Cli;
using CLImate.App.Models;

namespace CLImate.App.Rendering;

public interface IForecastRenderer
{
    void RenderDaily(Forecast forecast, bool showArt, bool useColor);
    void RenderToday(Forecast forecast, bool showArt, bool useColor);
}

public sealed class ForecastRenderer : IForecastRenderer
{
    private readonly IConsoleIO _console;
    private readonly IAsciiArtCatalog _asciiArt;
    private readonly IWeatherCodeCatalog _weatherCodes;
    private readonly IAnsiColorizer _colorizer;
    private readonly ITemperatureColorScale _temperatureColors;
    private readonly IArtColorizer _artColorizer;

    public ForecastRenderer(
        IConsoleIO console,
        IAsciiArtCatalog asciiArt,
        IWeatherCodeCatalog weatherCodes,
        IAnsiColorizer colorizer,
        ITemperatureColorScale temperatureColors,
        IArtColorizer artColorizer)
    {
        _console = console;
        _asciiArt = asciiArt;
        _weatherCodes = weatherCodes;
        _colorizer = colorizer;
        _temperatureColors = temperatureColors;
        _artColorizer = artColorizer;
    }

    public void RenderDaily(Forecast forecast, bool showArt, bool useColor)
    {
        var units = forecast.Units;
        var colorEnabled = _colorizer.ShouldUseColor(useColor);

        _console.WriteLine();
        _console.WriteLine("Daily forecast (today + 6 days):");
        _console.WriteLine();

        foreach (var day in forecast.Days)
        {
            var descriptor = _weatherCodes.Describe(day.WeatherCode);
            var art = BuildArt(descriptor.ArtKey, descriptor.ArtColor, showArt, colorEnabled);

            var dateWithDay = FormatDateWithDayOfWeek(day.Date);
            _console.WriteLine($"{dateWithDay}  {descriptor.Description}");
            if (!string.IsNullOrWhiteSpace(art))
            {
                _console.WriteLine(art);
            }
            var high = ColorizeValue(day.TemperatureMax, units.Temperature, colorEnabled);
            var low = ColorizeValue(day.TemperatureMin, units.Temperature, colorEnabled);
            _console.WriteLine($"  High/Low: {high} / {low}");
            _console.WriteLine($"  Precip:   {FormatValue(day.PrecipitationSum)}{units.Precipitation}");
            _console.WriteLine($"  Wind:     {FormatValue(day.WindSpeedMax)}{units.WindSpeed} (gusts {FormatValue(day.WindGustsMax)}{units.WindGusts})");
            _console.WriteLine($"  Warning:  {GetWarning(forecast, day.Date)}");
            _console.WriteLine();
        }
    }

    public void RenderToday(Forecast forecast, bool showArt, bool useColor)
    {
        var units = forecast.Units;
        var colorEnabled = _colorizer.ShouldUseColor(useColor);
        var today = forecast.Today;

        _console.WriteLine();

        if (today == null || today.Segments.Count == 0)
        {
            RenderSingleDay(forecast, showArt, colorEnabled);
            return;
        }

        var dateWithDay = FormatDateWithDayOfWeek(today.Date);
        _console.WriteLine($"Today ({dateWithDay})");

        var warning = GetWarning(forecast, today.Date);
        if (!string.Equals(warning, "none", StringComparison.OrdinalIgnoreCase))
        {
            _console.WriteLine($"Warning: {warning}");
        }

        foreach (var segment in today.Segments)
        {
            var descriptor = _weatherCodes.Describe(segment.WeatherCode);
            var art = BuildArt(descriptor.ArtKey, descriptor.ArtColor, showArt, colorEnabled);

            _console.WriteLine();
            _console.WriteLine($"{segment.Label}: {descriptor.Description}");
            if (!string.IsNullOrWhiteSpace(art))
            {
                _console.WriteLine(art);
            }

            var temp = ColorizeValue(segment.TemperatureAverage, units.Temperature, colorEnabled);
            _console.WriteLine($"  Temp:    {temp}");
            _console.WriteLine($"  Precip:  {FormatValue(segment.PrecipitationSum)}{units.Precipitation}");
            _console.WriteLine($"  Wind:    {FormatValue(segment.WindSpeedMax)}{units.WindSpeed} (gusts {FormatValue(segment.WindGustsMax)}{units.WindGusts})");
        }
    }

    private string BuildArt(string key, AnsiColor artColor, bool showArt, bool colorEnabled)
    {
        if (!showArt)
        {
            return _colorizer.Apply($"[ {key.Replace('_', ' ').ToUpperInvariant()} ]", artColor, colorEnabled);
        }

        var art = _asciiArt.GetArt(key, GetTerminalWidth());
        if (string.IsNullOrWhiteSpace(art))
        {
            return _colorizer.Apply($"[ {key.Replace('_', ' ').ToUpperInvariant()} ]", artColor, colorEnabled);
        }

        return _artColorizer.Colorize(art, key, colorEnabled);
    }

    private string ColorizeValue(double value, string unit, bool colorEnabled)
    {
        var color = _temperatureColors.GetColor(value);
        var formatted = $"{FormatValue(value)}{unit}";
        return _colorizer.Apply(formatted, color, colorEnabled);
    }

    private static string GetWarning(Forecast forecast, string date)
    {
        if (forecast.WarningsByDate.TryGetValue(date, out var warning))
        {
            return warning;
        }

        return "none";
    }

    private static string FormatValue(double value)
    {
        return double.IsNaN(value) ? "n/a" : value.ToString("0.#");
    }

    private void RenderSingleDay(Forecast forecast, bool showArt, bool colorEnabled)
    {
        if (forecast.Days.Count == 0)
        {
            _console.WriteLine("No daily data available.");
            return;
        }

        var units = forecast.Units;
        var day = forecast.Days[0];
        var descriptor = _weatherCodes.Describe(day.WeatherCode);
        var art = BuildArt(descriptor.ArtKey, descriptor.ArtColor, showArt, colorEnabled);
        var dateWithDay = FormatDateWithDayOfWeek(day.Date);

        _console.WriteLine($"Today ({dateWithDay})  {descriptor.Description}");
        if (!string.IsNullOrWhiteSpace(art))
        {
            _console.WriteLine(art);
        }

        var high = ColorizeValue(day.TemperatureMax, units.Temperature, colorEnabled);
        var low = ColorizeValue(day.TemperatureMin, units.Temperature, colorEnabled);
        _console.WriteLine($"  Temp:    {low} → {high}");
        _console.WriteLine($"  Precip:  {FormatValue(day.PrecipitationSum)}{units.Precipitation}");
        _console.WriteLine($"  Wind:    {FormatValue(day.WindSpeedMax)}{units.WindSpeed} (gusts {FormatValue(day.WindGustsMax)}{units.WindGusts})");
        _console.WriteLine($"  Warning: {GetWarning(forecast, day.Date)}");
    }

    private static string FormatDateWithDayOfWeek(string date)
    {
        if (DateTime.TryParse(date, out var dt))
        {
            return $"{date} ({dt.DayOfWeek})";
        }
        return date;
    }

    private static int? GetTerminalWidth()
    {
        try
        {
            return Console.WindowWidth;
        }
        catch
        {
            return null;
        }
    }
}
