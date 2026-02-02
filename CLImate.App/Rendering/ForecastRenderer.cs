using CLImate.App.Cli;
using CLImate.App.Models;

namespace CLImate.App.Rendering;

public interface IForecastRenderer
{
    void RenderDaily(Forecast forecast, bool showArt, bool useColour);
    void RenderToday(Forecast forecast, bool showArt, bool useColour);
}

public sealed class ForecastRenderer : IForecastRenderer
{
    private readonly IConsoleIO _console;
    private readonly IAsciiArtCatalogue _asciiArt;
    private readonly IWeatherCodeCatalogue _weatherCodes;
    private readonly IAnsiColouriser _colouriser;
    private readonly ITemperatureColourScale _temperatureColours;
    private readonly IArtColouriser _artColouriser;

    public ForecastRenderer(
        IConsoleIO console,
        IAsciiArtCatalogue asciiArt,
        IWeatherCodeCatalogue weatherCodes,
        IAnsiColouriser colouriser,
        ITemperatureColourScale temperatureColours,
        IArtColouriser artColouriser)
    {
        _console = console;
        _asciiArt = asciiArt;
        _weatherCodes = weatherCodes;
        _colouriser = colouriser;
        _temperatureColours = temperatureColours;
        _artColouriser = artColouriser;
    }

    public void RenderDaily(Forecast forecast, bool showArt, bool useColour)
    {
        var units = forecast.Units;
        var colourEnabled = _colouriser.ShouldUseColour(useColour);

        _console.WriteLine();
        _console.WriteLine("Daily forecast (today + 6 days):");
        _console.WriteLine();

        foreach (var day in forecast.Days)
        {
            var descriptor = _weatherCodes.Describe(day.WeatherCode);
            var art = BuildArt(descriptor.ArtKey, descriptor.ArtColour, showArt, colourEnabled);

            var dateWithDay = FormatDateWithDayOfWeek(day.Date);
            _console.WriteLine($"{dateWithDay}  {descriptor.Description}");
            if (!string.IsNullOrWhiteSpace(art))
            {
                _console.WriteLine(art);
            }
            var high = ColouriseValue(day.TemperatureMax, units.Temperature, colourEnabled);
            var low = ColouriseValue(day.TemperatureMin, units.Temperature, colourEnabled);
            _console.WriteLine($"  High/Low: {high} / {low}");
            _console.WriteLine($"  Precip:   {FormatValue(day.PrecipitationSum)}{units.Precipitation}");
            _console.WriteLine($"  Wind:     {FormatValue(day.WindSpeedMax)}{units.WindSpeed} (gusts {FormatValue(day.WindGustsMax)}{units.WindGusts})");
            _console.WriteLine($"  Warning:  {GetWarning(forecast, day.Date)}");
            _console.WriteLine();
        }
    }

    public void RenderToday(Forecast forecast, bool showArt, bool useColour)
    {
        var units = forecast.Units;
        var colourEnabled = _colouriser.ShouldUseColour(useColour);
        var today = forecast.Today;

        _console.WriteLine();

        if (today == null || today.Segments.Count == 0)
        {
            RenderSingleDay(forecast, showArt, colourEnabled);
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
            var art = BuildArt(descriptor.ArtKey, descriptor.ArtColour, showArt, colourEnabled);

            _console.WriteLine();
            _console.WriteLine($"{segment.Label}: {descriptor.Description}");
            if (!string.IsNullOrWhiteSpace(art))
            {
                _console.WriteLine(art);
            }

            var temp = ColouriseValue(segment.TemperatureAverage, units.Temperature, colourEnabled);
            _console.WriteLine($"  Temp:    {temp}");
            _console.WriteLine($"  Precip:  {FormatValue(segment.PrecipitationSum)}{units.Precipitation}");
            _console.WriteLine($"  Wind:    {FormatValue(segment.WindSpeedMax)}{units.WindSpeed} (gusts {FormatValue(segment.WindGustsMax)}{units.WindGusts})");
        }
    }

    private string BuildArt(string key, AnsiColour artColour, bool showArt, bool colourEnabled)
    {
        if (!showArt)
        {
            return _colouriser.Apply($"[ {key.Replace('_', ' ').ToUpperInvariant()} ]", artColour, colourEnabled);
        }

        var art = _asciiArt.GetArt(key, GetTerminalWidth());
        if (string.IsNullOrWhiteSpace(art))
        {
            return _colouriser.Apply($"[ {key.Replace('_', ' ').ToUpperInvariant()} ]", artColour, colourEnabled);
        }

        return _artColouriser.Colourise(art, key, colourEnabled);
    }

    private string ColouriseValue(double value, string unit, bool colourEnabled)
    {
        var colour = _temperatureColours.GetColour(value);
        var formatted = $"{FormatValue(value)}{unit}";
        return _colouriser.Apply(formatted, colour, colourEnabled);
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

    private void RenderSingleDay(Forecast forecast, bool showArt, bool colourEnabled)
    {
        if (forecast.Days.Count == 0)
        {
            _console.WriteLine("No daily data available.");
            return;
        }

        var units = forecast.Units;
        var day = forecast.Days[0];
        var descriptor = _weatherCodes.Describe(day.WeatherCode);
        var art = BuildArt(descriptor.ArtKey, descriptor.ArtColour, showArt, colourEnabled);
        var dateWithDay = FormatDateWithDayOfWeek(day.Date);

        _console.WriteLine($"Today ({dateWithDay})  {descriptor.Description}");
        if (!string.IsNullOrWhiteSpace(art))
        {
            _console.WriteLine(art);
        }

        var high = ColouriseValue(day.TemperatureMax, units.Temperature, colourEnabled);
        var low = ColouriseValue(day.TemperatureMin, units.Temperature, colourEnabled);
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
