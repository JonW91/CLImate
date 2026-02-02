using CLImate.App.Cli;
using CLImate.App.Models;

namespace CLImate.App.Rendering;

public interface IForecastRenderer
{
    void RenderDaily(Forecast forecast, bool showArt, bool useColour, LayoutMode layout = LayoutMode.Auto);
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
    private readonly ITerminalInfo _terminalInfo;
    private readonly ITableRenderer _tableRenderer;

    public ForecastRenderer(
        IConsoleIO console,
        IAsciiArtCatalogue asciiArt,
        IWeatherCodeCatalogue weatherCodes,
        IAnsiColouriser colouriser,
        ITemperatureColourScale temperatureColours,
        IArtColouriser artColouriser,
        ITerminalInfo terminalInfo,
        ITableRenderer tableRenderer)
    {
        _console = console;
        _asciiArt = asciiArt;
        _weatherCodes = weatherCodes;
        _colouriser = colouriser;
        _temperatureColours = temperatureColours;
        _artColouriser = artColouriser;
        _terminalInfo = terminalInfo;
        _tableRenderer = tableRenderer;
    }

    public void RenderDaily(Forecast forecast, bool showArt, bool useColour, LayoutMode layout = LayoutMode.Auto)
    {
        var useHorizontal = layout switch
        {
            LayoutMode.Horizontal => true,
            LayoutMode.Vertical => false,
            _ => _tableRenderer.CanRenderHorizontally(forecast, _terminalInfo.Width)
        };

        if (useHorizontal)
        {
            _tableRenderer.RenderHorizontalTable(forecast, showArt, useColour, _terminalInfo.Width);
            return;
        }

        RenderDailyVertical(forecast, showArt, useColour);
    }

    private void RenderDailyVertical(Forecast forecast, bool showArt, bool useColour)
    {
        var units = forecast.Units;
        var colourEnabled = _colouriser.ShouldUseColour(useColour);

        _console.WriteLine();
        _console.WriteLine("7-Day Forecast");
        _console.WriteLine(new string('─', 40));
        _console.WriteLine();

        foreach (var day in forecast.Days)
        {
            var descriptor = _weatherCodes.Describe(day.WeatherCode);
            var art = BuildArt(descriptor.ArtKey, descriptor.ArtColour, showArt, colourEnabled);
            var warning = GetWarning(forecast, day.Date);

            // Header with day name and date
            var dayHeader = FormatDayHeader(day.Date);
            _console.WriteLine($"┌─ {dayHeader} ─────────────────────────");
            _console.WriteLine($"│  {descriptor.Description}");

            // ASCII art (indented)
            if (!string.IsNullOrWhiteSpace(art))
            {
                foreach (var line in art.Split('\n'))
                {
                    _console.WriteLine($"│  {line}");
                }
            }

            // Weather details
            var high = ColouriseValue(day.TemperatureMax, units.Temperature, colourEnabled);
            var low = ColouriseValue(day.TemperatureMin, units.Temperature, colourEnabled);
            _console.WriteLine($"│");
            _console.WriteLine($"│  🌡️  {low} → {high}");
            _console.WriteLine($"│  💧 {FormatValue(day.PrecipitationSum)}{units.Precipitation}");
            _console.WriteLine($"│  💨 {FormatValue(day.WindSpeedMax)}{units.WindSpeed} (gusts {FormatValue(day.WindGustsMax)}{units.WindGusts})");

            // Warning (only if present)
            if (!string.Equals(warning, "none", StringComparison.OrdinalIgnoreCase))
            {
                _console.WriteLine($"│  ⚠️  {warning}");
            }

            _console.WriteLine($"└──────────────────────────────────────");
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

        var dayHeader = FormatDayHeader(today.Date);
        _console.WriteLine($"┌─ {dayHeader} ─────────────────────────");

        var warning = GetWarning(forecast, today.Date);
        if (!string.Equals(warning, "none", StringComparison.OrdinalIgnoreCase))
        {
            _console.WriteLine($"│  ⚠️  {warning}");
        }

        foreach (var segment in today.Segments)
        {
            var descriptor = _weatherCodes.Describe(segment.WeatherCode);
            var art = BuildArt(descriptor.ArtKey, descriptor.ArtColour, showArt, colourEnabled);

            _console.WriteLine($"│");
            _console.WriteLine($"├── {segment.Label.ToUpperInvariant()}");
            _console.WriteLine($"│   {descriptor.Description}");

            if (!string.IsNullOrWhiteSpace(art))
            {
                foreach (var line in art.Split('\n'))
                {
                    _console.WriteLine($"│   {line}");
                }
            }

            var temp = ColouriseValue(segment.TemperatureAverage, units.Temperature, colourEnabled);
            _console.WriteLine($"│   🌡️  {temp}");
            _console.WriteLine($"│   💧 {FormatValue(segment.PrecipitationSum)}{units.Precipitation}");
            _console.WriteLine($"│   💨 {FormatValue(segment.WindSpeedMax)}{units.WindSpeed} (gusts {FormatValue(segment.WindGustsMax)}{units.WindGusts})");
        }

        _console.WriteLine($"└──────────────────────────────────────");
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
        var warning = GetWarning(forecast, day.Date);
        var dayHeader = FormatDayHeader(day.Date);

        _console.WriteLine($"┌─ {dayHeader} ─────────────────────────");
        _console.WriteLine($"│  {descriptor.Description}");

        if (!string.IsNullOrWhiteSpace(art))
        {
            foreach (var line in art.Split('\n'))
            {
                _console.WriteLine($"│  {line}");
            }
        }

        var high = ColouriseValue(day.TemperatureMax, units.Temperature, colourEnabled);
        var low = ColouriseValue(day.TemperatureMin, units.Temperature, colourEnabled);
        _console.WriteLine($"│");
        _console.WriteLine($"│  🌡️  {low} → {high}");
        _console.WriteLine($"│  💧 {FormatValue(day.PrecipitationSum)}{units.Precipitation}");
        _console.WriteLine($"│  💨 {FormatValue(day.WindSpeedMax)}{units.WindSpeed} (gusts {FormatValue(day.WindGustsMax)}{units.WindGusts})");

        if (!string.Equals(warning, "none", StringComparison.OrdinalIgnoreCase))
        {
            _console.WriteLine($"│  ⚠️  {warning}");
        }

        _console.WriteLine($"└──────────────────────────────────────");
    }

    private static string FormatDateWithDayOfWeek(string date)
    {
        if (DateTime.TryParse(date, out var dt))
        {
            return $"{date} ({dt.DayOfWeek})";
        }
        return date;
    }

    private static string FormatDayHeader(string date)
    {
        if (DateTime.TryParse(date, out var dt))
        {
            var isToday = dt.Date == DateTime.Today;
            var isTomorrow = dt.Date == DateTime.Today.AddDays(1);

            if (isToday)
                return $"TODAY · {dt:ddd d MMM}";
            if (isTomorrow)
                return $"TOMORROW · {dt:ddd d MMM}";

            return dt.ToString("dddd · d MMM").ToUpperInvariant();
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
