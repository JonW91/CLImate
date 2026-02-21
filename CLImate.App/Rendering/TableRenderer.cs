using CLImate.App.Cli;
using CLImate.App.Models;

namespace CLImate.App.Rendering;

public interface ITableRenderer
{
    void RenderHorizontalTable(Forecast forecast, bool showArt, bool useColour, int terminalWidth);
    void RenderTodayTable(Forecast forecast, bool showArt, bool useColour, int terminalWidth);
    bool CanRenderHorizontally(Forecast forecast, int terminalWidth);
    bool CanRenderTodayHorizontally(Forecast forecast, int terminalWidth);
}

public sealed class TableRenderer : ITableRenderer
{
    private readonly IConsoleIO _console;
    private readonly IWeatherCodeCatalogue _weatherCodes;
    private readonly IAnsiColouriser _colouriser;
    private readonly ITemperatureColourScale _temperatureColours;
    private readonly IAsciiArtCatalogue _asciiArt;
    private readonly IArtColouriser _artColouriser;

    private const int MinColumnWidth = 14;
    private const int ArtColumnWidth = 18;
    private const int BorderWidth = 1;
    private const int MinWidthForHorizontal = 100;
    private const int MinWidthForArtTable = 140;

    public TableRenderer(
        IConsoleIO console,
        IWeatherCodeCatalogue weatherCodes,
        IAnsiColouriser colouriser,
        ITemperatureColourScale temperatureColours,
        IAsciiArtCatalogue asciiArt,
        IArtColouriser artColouriser)
    {
        _console = console;
        _weatherCodes = weatherCodes;
        _colouriser = colouriser;
        _temperatureColours = temperatureColours;
        _asciiArt = asciiArt;
        _artColouriser = artColouriser;
    }

    public bool CanRenderHorizontally(Forecast forecast, int terminalWidth)
    {
        if (forecast.Days.Count == 0)
            return false;

        var requiredWidth = (forecast.Days.Count * MinColumnWidth) + (forecast.Days.Count + 1) * BorderWidth;
        return terminalWidth >= requiredWidth && terminalWidth >= MinWidthForHorizontal;
    }

    public void RenderHorizontalTable(Forecast forecast, bool showArt, bool useColour, int terminalWidth)
    {
        var colourEnabled = _colouriser.ShouldUseColour(useColour);
        var units = forecast.Units;
        var days = forecast.Days;

        if (days.Count == 0)
        {
            _console.WriteLine("No forecast data available.");
            return;
        }

        // Determine if we have enough width for ASCII art in table
        var useAsciiArt = showArt && terminalWidth >= MinWidthForArtTable;
        var columnWidth = useAsciiArt ? ArtColumnWidth : Math.Max(MinColumnWidth, 16);
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

        // Weather art/icons - multiple rows if using ASCII art
        if (useAsciiArt)
        {
            RenderAsciiArtRows(days, columnWidth, colourEnabled);
        }
        else
        {
            // Fallback to simple text icons for narrow terminals
            var weatherIcons = days.Select(d =>
            {
                var descriptor = _weatherCodes.Describe(d.WeatherCode);
                var icon = GetCompactWeatherIcon(descriptor.ArtKey);
                return CenterText(icon, columnWidth);
            }).ToList();
            _console.WriteLine(BuildRow(weatherIcons, '│'));
        }

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
            return CenterText($"Rain: {value}", columnWidth);
        }).ToList();
        _console.WriteLine(BuildRow(precip, '│'));

        // Wind row
        var wind = days.Select(d =>
        {
            var value = $"{FormatValue(d.WindSpeedMax)}{units.WindSpeed}";
            return CenterText($"Wind: {value}", columnWidth);
        }).ToList();
        _console.WriteLine(BuildRow(wind, '│'));

        // Warnings row (if any)
        var hasWarnings = days.Any(d =>
            forecast.WarningsByDate.TryGetValue(d.Date, out var warning)
            && !string.Equals(warning, "none", StringComparison.OrdinalIgnoreCase));
        if (hasWarnings)
        {
            _console.WriteLine(BuildHorizontalBorder(days.Count, columnWidth, '├', '┼', '┤'));
            var warnings = days.Select(d =>
            {
                if (forecast.WarningsByDate.TryGetValue(d.Date, out var warning) &&
                    !string.Equals(warning, "none", StringComparison.OrdinalIgnoreCase))
                {
                    var truncated = TruncateText(warning, columnWidth - 4);
                    return CenterText($"! {truncated}", columnWidth);
                }
                return CenterText("—", columnWidth);
            }).ToList();
            _console.WriteLine(BuildRow(warnings, '│'));
        }

        // Bottom border
        _console.WriteLine(BuildHorizontalBorder(days.Count, columnWidth, '└', '┴', '┘'));
    }

    private void RenderAsciiArtRows(IReadOnlyList<DailyForecast> days, int columnWidth, bool colourEnabled)
    {
        // Get art for each day - use small size for table cells
        var artLines = new List<string[]>();
        var maxLines = 0;

        foreach (var day in days)
        {
            var descriptor = _weatherCodes.Describe(day.WeatherCode);
            var artKey = descriptor.ArtKey;

            // Get small art and split into lines, then colorize
            var art = _asciiArt.GetArt(artKey, 50); // Request small art (width < 70 triggers small)
            var lines = string.IsNullOrEmpty(art) 
                ? [GetCompactWeatherIcon(artKey)] 
                : art.Split('\n');

            // Colorize if enabled
            if (colourEnabled && lines.Length > 1)
            {
                lines = lines.Select(line => _artColouriser.Colourise(line, artKey, colourEnabled)).ToArray();
            }

            artLines.Add(lines);
            maxLines = Math.Max(maxLines, lines.Length);
        }

        // Render each line of art across all days
        for (var lineIdx = 0; lineIdx < maxLines; lineIdx++)
        {
            var rowCells = new List<string>();
            for (var dayIdx = 0; dayIdx < days.Count; dayIdx++)
            {
                var lines = artLines[dayIdx];
                var line = lineIdx < lines.Length ? lines[lineIdx].TrimEnd() : "";

                // Center the art line in the column, accounting for ANSI codes
                var visibleLen = GetVisibleLength(line);
                rowCells.Add(CenterTextWithAnsi(line, columnWidth, visibleLen));
            }
            _console.WriteLine(BuildRowWithAnsi(rowCells, '│'));
        }
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

    private static string GetCompactWeatherIcon(string artKey)
    {
        // Universal ASCII icons that work on all terminals
        return artKey.ToLowerInvariant() switch
        {
            "clear" or "clear_day" or "sunny" => @"\ | /  (O)  / | \",
            "clear_night" => "( C )",
            "partly_cloudy" or "partly_cloudy_day" => @"\|/ .--.",
            "partly_cloudy_night" => "C .--.",
            "cloudy" or "overcast" => ".--. .--.  ",
            "fog" or "mist" => "- _ - _ -",
            "drizzle" or "light_rain" => ".--. ' '",
            "rain" or "moderate_rain" or "heavy_rain" => ".--. / / /",
            "freezing_rain" or "freezing_drizzle" => ".--. * / *",
            "snow" or "light_snow" or "heavy_snow" => ".--. * * *",
            "sleet" => ".--. /* /*",
            "thunderstorm" => ".--.  /\\/\\",
            "thunderstorm_hail" => ".--.  *//\\",
            "hail" or "snow_grains" => ".--. o o o",
            _ => "  ?  "
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

    public bool CanRenderTodayHorizontally(Forecast forecast, int terminalWidth)
    {
        var today = forecast.Today;
        if (today == null || today.Segments.Count == 0)
        {
            // Fall back to single day view
            return forecast.Days.Count > 0 && terminalWidth >= 80;
        }

        var segmentCount = today.Segments.Count;
        var requiredWidth = (segmentCount * MinColumnWidth) + (segmentCount + 1) * BorderWidth;
        return terminalWidth >= requiredWidth && terminalWidth >= 70;
    }

    public void RenderTodayTable(Forecast forecast, bool showArt, bool useColour, int terminalWidth)
    {
        var colourEnabled = _colouriser.ShouldUseColour(useColour);
        var units = forecast.Units;
        var today = forecast.Today;

        if (today == null || today.Segments.Count == 0)
        {
            RenderSingleDayTable(forecast, showArt, colourEnabled, terminalWidth);
            return;
        }

        var segments = today.Segments;
        var useAsciiArt = showArt && terminalWidth >= MinWidthForArtTable;
        var columnWidth = useAsciiArt ? ArtColumnWidth : Math.Max(MinColumnWidth, 16);

        _console.WriteLine();
        _console.WriteLine($"Today's Forecast · {FormatTodayDate(today.Date)}");
        _console.WriteLine();

        // Check for warnings
        if (forecast.WarningsByDate.TryGetValue(today.Date, out var warning) &&
            !string.Equals(warning, "none", StringComparison.OrdinalIgnoreCase))
        {
            _console.WriteLine($"  ! Warning: {warning}");
            _console.WriteLine();
        }

        // Top border
        _console.WriteLine(BuildHorizontalBorder(segments.Count, columnWidth, '┌', '┬', '┐'));

        // Time period headers (Morning, Afternoon, etc.)
        var headers = segments.Select(s => CenterText(s.Label.ToUpperInvariant(), columnWidth)).ToList();
        _console.WriteLine(BuildRow(headers, '│'));

        // Separator
        _console.WriteLine(BuildHorizontalBorder(segments.Count, columnWidth, '├', '┼', '┤'));

        // Weather art rows
        if (useAsciiArt)
        {
            RenderTodayAsciiArtRows(segments, columnWidth, colourEnabled);
        }
        else
        {
            var weatherIcons = segments.Select(s =>
            {
                var descriptor = _weatherCodes.Describe(s.WeatherCode);
                var icon = GetCompactWeatherIcon(descriptor.ArtKey);
                return CenterText(icon, columnWidth);
            }).ToList();
            _console.WriteLine(BuildRow(weatherIcons, '│'));
        }

        // Weather descriptions
        var weatherDescs = segments.Select(s =>
        {
            var descriptor = _weatherCodes.Describe(s.WeatherCode);
            var desc = TruncateText(descriptor.Description, columnWidth - 2);
            return CenterText(desc, columnWidth);
        }).ToList();
        _console.WriteLine(BuildRow(weatherDescs, '│'));

        // Separator
        _console.WriteLine(BuildHorizontalBorder(segments.Count, columnWidth, '├', '┼', '┤'));

        // Temperature row
        var temps = segments.Select(s =>
        {
            var temp = FormatTemp(s.TemperatureAverage, units.Temperature, colourEnabled);
            return CenterTextWithAnsi(temp, columnWidth, GetVisibleLength(temp));
        }).ToList();
        _console.WriteLine(BuildRowWithAnsi(temps, '│'));

        // Precipitation row
        var precip = segments.Select(s =>
        {
            var value = $"{FormatValue(s.PrecipitationSum)}{units.Precipitation}";
            return CenterText($"Rain: {value}", columnWidth);
        }).ToList();
        _console.WriteLine(BuildRow(precip, '│'));

        // Wind row
        var wind = segments.Select(s =>
        {
            var value = $"{FormatValue(s.WindSpeedMax)}{units.WindSpeed}";
            return CenterText($"Wind: {value}", columnWidth);
        }).ToList();
        _console.WriteLine(BuildRow(wind, '│'));

        // Bottom border
        _console.WriteLine(BuildHorizontalBorder(segments.Count, columnWidth, '└', '┴', '┘'));
    }

    private void RenderTodayAsciiArtRows(IReadOnlyList<DayPartForecast> segments, int columnWidth, bool colourEnabled)
    {
        var artLines = new List<string[]>();
        var maxLines = 0;

        foreach (var segment in segments)
        {
            var descriptor = _weatherCodes.Describe(segment.WeatherCode);
            var artKey = descriptor.ArtKey;

            var art = _asciiArt.GetArt(artKey, 50);
            var lines = string.IsNullOrEmpty(art)
                ? [GetCompactWeatherIcon(artKey)]
                : art.Split('\n');

            if (colourEnabled && lines.Length > 1)
            {
                lines = lines.Select(line => _artColouriser.Colourise(line, artKey, colourEnabled)).ToArray();
            }

            artLines.Add(lines);
            maxLines = Math.Max(maxLines, lines.Length);
        }

        for (var lineIdx = 0; lineIdx < maxLines; lineIdx++)
        {
            var rowCells = new List<string>();
            for (var segIdx = 0; segIdx < segments.Count; segIdx++)
            {
                var lines = artLines[segIdx];
                var line = lineIdx < lines.Length ? lines[lineIdx].TrimEnd() : "";
                var visibleLen = GetVisibleLength(line);
                rowCells.Add(CenterTextWithAnsi(line, columnWidth, visibleLen));
            }
            _console.WriteLine(BuildRowWithAnsi(rowCells, '│'));
        }
    }

    private void RenderSingleDayTable(Forecast forecast, bool showArt, bool colourEnabled, int terminalWidth)
    {
        if (forecast.Days.Count == 0)
        {
            _console.WriteLine("No forecast data available.");
            return;
        }

        var units = forecast.Units;
        var day = forecast.Days[0];
        var descriptor = _weatherCodes.Describe(day.WeatherCode);
        var useAsciiArt = showArt && terminalWidth >= 80;

        _console.WriteLine();
        _console.WriteLine($"Today's Forecast · {FormatTodayDate(day.Date)}");
        _console.WriteLine();

        // Check for warnings
        if (forecast.WarningsByDate.TryGetValue(day.Date, out var warning) &&
            !string.Equals(warning, "none", StringComparison.OrdinalIgnoreCase))
        {
            _console.WriteLine($"  ! Warning: {warning}");
            _console.WriteLine();
        }

        var columnWidth = useAsciiArt ? 24 : 20;

        // Top border
        _console.WriteLine(BuildHorizontalBorder(1, columnWidth, '┌', '┬', '┐'));

        // Weather description header
        _console.WriteLine(BuildRow([CenterText(descriptor.Description, columnWidth)], '│'));

        // Separator
        _console.WriteLine(BuildHorizontalBorder(1, columnWidth, '├', '┼', '┤'));

        // ASCII art
        if (useAsciiArt)
        {
            var art = _asciiArt.GetArt(descriptor.ArtKey, 50);
            if (!string.IsNullOrEmpty(art))
            {
                var lines = art.Split('\n');
                if (colourEnabled)
                {
                    lines = lines.Select(line => _artColouriser.Colourise(line, descriptor.ArtKey, colourEnabled)).ToArray();
                }
                foreach (var line in lines)
                {
                    var visibleLen = GetVisibleLength(line.TrimEnd());
                    _console.WriteLine(BuildRowWithAnsi([CenterTextWithAnsi(line.TrimEnd(), columnWidth, visibleLen)], '│'));
                }
            }
        }
        else
        {
            var icon = GetCompactWeatherIcon(descriptor.ArtKey);
            _console.WriteLine(BuildRow([CenterText(icon, columnWidth)], '│'));
        }

        // Separator
        _console.WriteLine(BuildHorizontalBorder(1, columnWidth, '├', '┼', '┤'));

        // Temperature
        var high = FormatTemp(day.TemperatureMax, units.Temperature, colourEnabled);
        var low = FormatTemp(day.TemperatureMin, units.Temperature, colourEnabled);
        var tempText = $"{low} / {high}";
        _console.WriteLine(BuildRowWithAnsi([CenterTextWithAnsi(tempText, columnWidth, GetVisibleLength(tempText))], '│'));

        // Precipitation
        var precipText = $"Rain: {FormatValue(day.PrecipitationSum)}{units.Precipitation}";
        _console.WriteLine(BuildRow([CenterText(precipText, columnWidth)], '│'));

        // Wind
        var windText = $"Wind: {FormatValue(day.WindSpeedMax)}{units.WindSpeed}";
        _console.WriteLine(BuildRow([CenterText(windText, columnWidth)], '│'));

        // Gusts
        var gustText = $"Gusts: {FormatValue(day.WindGustsMax)}{units.WindGusts}";
        _console.WriteLine(BuildRow([CenterText(gustText, columnWidth)], '│'));

        // Bottom border
        _console.WriteLine(BuildHorizontalBorder(1, columnWidth, '└', '┴', '┘'));
    }

    private static string FormatTodayDate(string date)
    {
        if (DateTime.TryParse(date, out var dt))
        {
            return dt.ToString("dddd, d MMMM yyyy");
        }
        return date;
    }
}
