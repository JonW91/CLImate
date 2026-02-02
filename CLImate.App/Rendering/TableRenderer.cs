using CLImate.App.Cli;
using CLImate.App.Models;

namespace CLImate.App.Rendering;

public interface ITableRenderer
{
    void RenderHorizontalTable(Forecast forecast, bool showArt, bool useColour);
    bool CanRenderHorizontally(Forecast forecast, int terminalWidth);
}

public sealed class TableRenderer : ITableRenderer
{
    private readonly IConsoleIO _console;
    private readonly IWeatherCodeCatalogue _weatherCodes;
    private readonly IAnsiColouriser _colouriser;
    private readonly ITemperatureColourScale _temperatureColours;

    private const int MinColumnWidth = 14;
    private const int BorderWidth = 1;
    private const int MinWidthForHorizontal = 100;

    public TableRenderer(
        IConsoleIO console,
        IWeatherCodeCatalogue weatherCodes,
        IAnsiColouriser colouriser,
        ITemperatureColourScale temperatureColours)
    {
        _console = console;
        _weatherCodes = weatherCodes;
        _colouriser = colouriser;
        _temperatureColours = temperatureColours;
    }

    public bool CanRenderHorizontally(Forecast forecast, int terminalWidth)
    {
        if (forecast.Days.Count == 0)
            return false;

        var requiredWidth = (forecast.Days.Count * MinColumnWidth) + (forecast.Days.Count + 1) * BorderWidth;
        return terminalWidth >= requiredWidth && terminalWidth >= MinWidthForHorizontal;
    }

    public void RenderHorizontalTable(Forecast forecast, bool showArt, bool useColour)
    {
        var colourEnabled = _colouriser.ShouldUseColour(useColour);
        var units = forecast.Units;
        var days = forecast.Days;

        if (days.Count == 0)
        {
            _console.WriteLine("No forecast data available.");
            return;
        }

        var columnWidth = Math.Max(MinColumnWidth, 16);
        var totalWidth = (days.Count * columnWidth) + days.Count + 1;

        _console.WriteLine();
        _console.WriteLine("7-Day Forecast:");
        _console.WriteLine();

        // Top border
        _console.WriteLine(BuildHorizontalBorder(days.Count, columnWidth, '┌', '┬', '┐'));

        // Day names row
        var dayNames = days.Select(d => CenterText(FormatDayName(d.Date), columnWidth)).ToList();
        _console.WriteLine(BuildRow(dayNames, '│'));

        // Separator
        _console.WriteLine(BuildHorizontalBorder(days.Count, columnWidth, '├', '┼', '┤'));

        // Weather icons row (compact text representation)
        var weatherIcons = days.Select(d =>
        {
            var descriptor = _weatherCodes.Describe(d.WeatherCode);
            var icon = GetWeatherEmoji(descriptor.ArtKey);
            return CenterText(icon, columnWidth);
        }).ToList();
        _console.WriteLine(BuildRow(weatherIcons, '│'));

        // Weather descriptions
        var weatherDescs = days.Select(d =>
        {
            var descriptor = _weatherCodes.Describe(d.WeatherCode);
            var desc = TruncateText(descriptor.Description, columnWidth - 2);
            return CenterText(desc, columnWidth);
        }).ToList();
        _console.WriteLine(BuildRow(weatherDescs, '│'));

        // Separator
        _console.WriteLine(BuildHorizontalBorder(days.Count, columnWidth, '├', '┼', '┤'));

        // Temperature row
        var temps = days.Select(d =>
        {
            var high = FormatTemp(d.TemperatureMax, units.Temperature, colourEnabled);
            var low = FormatTemp(d.TemperatureMin, units.Temperature, colourEnabled);
            return CenterTextWithAnsi($"{low}/{high}", columnWidth, GetVisibleLength($"{low}/{high}"));
        }).ToList();
        _console.WriteLine(BuildRowWithAnsi(temps, '│'));

        // Precipitation row
        var precip = days.Select(d =>
        {
            var value = $"{FormatValue(d.PrecipitationSum)}{units.Precipitation}";
            return CenterText($"💧 {value}", columnWidth);
        }).ToList();
        _console.WriteLine(BuildRow(precip, '│'));

        // Wind row
        var wind = days.Select(d =>
        {
            var value = $"{FormatValue(d.WindSpeedMax)}{units.WindSpeed}";
            return CenterText($"💨 {value}", columnWidth);
        }).ToList();
        _console.WriteLine(BuildRow(wind, '│'));

        // Warnings row (if any)
        var hasWarnings = days.Any(d => forecast.WarningsByDate.ContainsKey(d.Date));
        if (hasWarnings)
        {
            _console.WriteLine(BuildHorizontalBorder(days.Count, columnWidth, '├', '┼', '┤'));
            var warnings = days.Select(d =>
            {
                if (forecast.WarningsByDate.TryGetValue(d.Date, out var warning) &&
                    !string.Equals(warning, "none", StringComparison.OrdinalIgnoreCase))
                {
                    var truncated = TruncateText(warning, columnWidth - 4);
                    return CenterText($"⚠️ {truncated}", columnWidth);
                }
                return CenterText("—", columnWidth);
            }).ToList();
            _console.WriteLine(BuildRow(warnings, '│'));
        }

        // Bottom border
        _console.WriteLine(BuildHorizontalBorder(days.Count, columnWidth, '└', '┴', '┘'));
    }

    private string FormatTemp(double value, string unit, bool colourEnabled)
    {
        var colour = _temperatureColours.GetColour(value);
        var formatted = $"{FormatValue(value)}{unit}";
        return _colouriser.Apply(formatted, colour, colourEnabled);
    }

    private static string FormatValue(double value)
    {
        return double.IsNaN(value) ? "n/a" : value.ToString("0.#");
    }

    private static string FormatDayName(string date)
    {
        if (DateTime.TryParse(date, out var dt))
        {
            var isToday = dt.Date == DateTime.Today;
            return isToday ? "Today" : dt.ToString("ddd d");
        }
        return date;
    }

    private static string GetWeatherEmoji(string artKey)
    {
        return artKey.ToLowerInvariant() switch
        {
            "clear_day" or "sunny" => "☀️",
            "clear_night" => "🌙",
            "partly_cloudy" or "partly_cloudy_day" => "⛅",
            "partly_cloudy_night" => "☁️",
            "cloudy" or "overcast" => "☁️",
            "fog" or "mist" => "🌫️",
            "drizzle" or "light_rain" => "🌦️",
            "rain" or "moderate_rain" or "heavy_rain" => "🌧️",
            "freezing_rain" or "freezing_drizzle" => "🌨️",
            "snow" or "light_snow" or "heavy_snow" => "❄️",
            "sleet" => "🌨️",
            "thunderstorm" => "⛈️",
            "hail" => "🌨️",
            _ => "🌡️"
        };
    }

    private static string BuildHorizontalBorder(int columns, int columnWidth, char left, char middle, char right)
    {
        var segment = new string('─', columnWidth);
        var parts = Enumerable.Repeat(segment, columns);
        return $"{left}{string.Join(middle.ToString(), parts)}{right}";
    }

    private static string BuildRow(List<string> cells, char border)
    {
        return $"{border}{string.Join(border.ToString(), cells)}{border}";
    }

    private static string BuildRowWithAnsi(List<string> cells, char border)
    {
        return $"{border}{string.Join(border.ToString(), cells)}{border}";
    }

    private static string CenterText(string text, int width)
    {
        var visibleLength = GetVisibleLength(text);
        if (visibleLength >= width)
            return text[..Math.Min(text.Length, width)];

        var padding = width - visibleLength;
        var leftPad = padding / 2;
        var rightPad = padding - leftPad;
        return new string(' ', leftPad) + text + new string(' ', rightPad);
    }

    private static string CenterTextWithAnsi(string text, int width, int visibleLength)
    {
        if (visibleLength >= width)
            return text;

        var padding = width - visibleLength;
        var leftPad = padding / 2;
        var rightPad = padding - leftPad;
        return new string(' ', leftPad) + text + new string(' ', rightPad);
    }

    private static string TruncateText(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text;
        return text[..(maxLength - 1)] + "…";
    }

    private static int GetVisibleLength(string text)
    {
        var length = 0;
        var inEscape = false;
        foreach (var c in text)
        {
            if (c == '\x1b')
            {
                inEscape = true;
                continue;
            }
            if (inEscape)
            {
                if (c == 'm')
                    inEscape = false;
                continue;
            }
            length++;
        }
        return length;
    }
}
