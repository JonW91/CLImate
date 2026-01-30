using System.Net.Http.Headers;
using System.Text.Json;

var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

using var http = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(20)
};

http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CLImate", "0.1"));
http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(Fedora-Termux)"));

if (args.Any(a => a is "-h" or "--help"))
{
    PrintHelp();
    return;
}

var units = Units.Metric;
var locationParts = new List<string>();
var showArt = true;
string? countryCode = null;

for (var i = 0; i < args.Length; i++)
{
    var arg = args[i];
    if (arg is "--units" or "-u")
    {
        if (i + 1 < args.Length)
        {
            units = ParseUnits(args[++i]);
        }
        continue;
    }

    if (arg is "--country" or "-c")
    {
        if (i + 1 < args.Length)
        {
            countryCode = NormalizeCountryCode(args[++i]);
        }
        continue;
    }

    if (arg is "--no-art")
    {
        showArt = false;
        continue;
    }

    if (arg.StartsWith("--units=", StringComparison.OrdinalIgnoreCase))
    {
        units = ParseUnits(arg.Substring("--units=".Length));
        continue;
    }

    if (arg.StartsWith("--country=", StringComparison.OrdinalIgnoreCase))
    {
        countryCode = NormalizeCountryCode(arg.Substring("--country=".Length));
        continue;
    }

    if (arg.StartsWith("-", StringComparison.Ordinal))
    {
        Console.WriteLine($"Unknown option: {arg}");
        PrintHelp();
        return;
    }

    locationParts.Add(arg);
}

var locationInput = locationParts.Count > 0
    ? string.Join(" ", locationParts)
    : Prompt("Enter a location (city, region, or address): ");
if (string.IsNullOrWhiteSpace(locationInput))
{
    Console.WriteLine("No location provided. Exiting.");
    return;
}

var geocode = await GetJsonAsync<GeocodeResponse>(http, options,
    $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(locationInput)}&count=5&language=en&format=json{BuildCountryCodeParameter(countryCode)}");

if (geocode?.Results == null || geocode.Results.Count == 0)
{
    var fallbackCode = countryCode ?? InferCountryCode(locationInput);
    if (!string.IsNullOrWhiteSpace(fallbackCode))
    {
        var primaryName = ExtractPrimaryName(locationInput);
        geocode = await GetJsonAsync<GeocodeResponse>(http, options,
            $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(primaryName)}&count=5&language=en&format=json{BuildCountryCodeParameter(fallbackCode)}");
    }

    if (geocode?.Results == null || geocode.Results.Count == 0)
    {
        Console.WriteLine("No locations found. Try a more specific query or pass --country.");
        return;
    }
}

var selected = PickLocation(geocode.Results);
if (selected == null)
{
    Console.WriteLine("No location selected. Exiting.");
    return;
}

if (selected.Latitude == null || selected.Longitude == null)
{
    Console.WriteLine("Selected location is missing coordinates.");
    return;
}

Console.WriteLine();
Console.WriteLine($"CLImate - Forecast for {FormatPlace(selected)}");
Console.WriteLine(new string('-', 52));

var forecastUrl =
    "https://api.open-meteo.com/v1/forecast" +
    $"?latitude={selected.Latitude.Value:F4}&longitude={selected.Longitude.Value:F4}" +
    "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_sum,wind_speed_10m_max,wind_gusts_10m_max" +
    $"&timezone=auto{BuildUnitParameters(units)}";

var forecast = await GetJsonAsync<ForecastResponse>(http, options, forecastUrl);
if (forecast?.Daily == null || forecast.Daily.Time == null || forecast.Daily.Time.Count == 0)
{
    Console.WriteLine("Could not fetch forecast data.");
    return;
}

PrintDailyForecast(forecast, showArt);

Console.WriteLine();
Console.WriteLine("Weather warnings: Not available yet (planned).");

static async Task<T?> GetJsonAsync<T>(HttpClient http, JsonSerializerOptions options, string url)
{
    using var response = await http.GetAsync(url);
    if (!response.IsSuccessStatusCode)
    {
        return default;
    }

    await using var stream = await response.Content.ReadAsStreamAsync();
    return await JsonSerializer.DeserializeAsync<T>(stream, options);
}

static string Prompt(string prompt)
{
    Console.Write(prompt);
    return Console.ReadLine() ?? string.Empty;
}

static void PrintHelp()
{
    Console.WriteLine("CLImate - CLI weather forecasts");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  CLImate.App [options] \"San Francisco, CA\"");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  CLImate.App London");
    Console.WriteLine("  CLImate.App --units imperial \"New York, NY\"");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  -u, --units <metric|imperial>   Units for output (default: metric)");
    Console.WriteLine("  -c, --country <code>            2-letter country code filter (e.g., GB, US)");
    Console.WriteLine("  --no-art                       Disable ASCII art (use text labels)");
}

static GeoResult? PickLocation(List<GeoResult> results)
{
    if (results.Count == 1)
    {
        return results[0];
    }

    Console.WriteLine("Select a location:");
    for (var i = 0; i < results.Count; i++)
    {
        Console.WriteLine($"  {i + 1}. {FormatPlace(results[i])}");
    }

    var input = Prompt("Choose 1-" + results.Count + " (default 1): ");
    if (string.IsNullOrWhiteSpace(input))
    {
        return results[0];
    }

    if (int.TryParse(input, out var index) && index >= 1 && index <= results.Count)
    {
        return results[index - 1];
    }

    Console.WriteLine("Invalid selection.");
    return null;
}

static string FormatPlace(GeoResult result)
{
    var parts = new List<string>();
    if (!string.IsNullOrWhiteSpace(result.Name)) parts.Add(result.Name);
    if (!string.IsNullOrWhiteSpace(result.Admin1)) parts.Add(result.Admin1);
    if (!string.IsNullOrWhiteSpace(result.Country)) parts.Add(result.Country);
    return string.Join(", ", parts);
}

static void PrintDailyForecast(ForecastResponse forecast, bool showArt)
{
    var daily = forecast.Daily!;
    var units = forecast.DailyUnits ?? new DailyUnits();

    Console.WriteLine();
    Console.WriteLine("Daily forecast (today + 6 days):");
    Console.WriteLine();

    for (var i = 0; i < daily.Time.Count; i++)
    {
        var date = daily.Time[i];
        var code = SafeGetInt(daily.WeatherCode, i, 0);
        var (desc, art) = DescribeWeather(code, showArt);

        var max = SafeGetDouble(daily.TemperatureMax, i, double.NaN);
        var min = SafeGetDouble(daily.TemperatureMin, i, double.NaN);
        var precip = SafeGetDouble(daily.PrecipitationSum, i, double.NaN);
        var wind = SafeGetDouble(daily.WindSpeedMax, i, double.NaN);
        var gust = SafeGetDouble(daily.WindGustsMax, i, double.NaN);

        Console.WriteLine($"{date}  {desc}");
        if (!string.IsNullOrWhiteSpace(art))
        {
            Console.WriteLine(art);
        }
        Console.WriteLine($"  High/Low: {FormatValue(max)}{units.TemperatureMax} / {FormatValue(min)}{units.TemperatureMin}");
        Console.WriteLine($"  Precip:   {FormatValue(precip)}{units.PrecipitationSum}");
        Console.WriteLine($"  Wind:     {FormatValue(wind)}{units.WindSpeedMax} (gusts {FormatValue(gust)}{units.WindGustsMax})");
        Console.WriteLine();
    }
}

static (string Description, string Art) DescribeWeather(int code, bool showArt)
{
    var (description, key) = code switch
    {
        0 => ("Clear sky", "clear"),
        1 or 2 => ("Mainly clear, partly cloudy", "partly_cloudy"),
        3 => ("Overcast", "overcast"),
        45 or 48 => ("Fog", "fog"),
        51 or 53 or 55 => ("Drizzle", "drizzle"),
        56 or 57 => ("Freezing drizzle", "freezing_drizzle"),
        61 or 63 or 65 => ("Rain", "rain"),
        66 or 67 => ("Freezing rain", "freezing_rain"),
        71 or 73 or 75 => ("Snow", "snow"),
        77 => ("Snow grains", "snow_grains"),
        80 or 81 or 82 => ("Rain showers", "rain_showers"),
        85 or 86 => ("Snow showers", "snow_showers"),
        95 => ("Thunderstorm", "thunderstorm"),
        96 or 99 => ("Thunderstorm with hail", "thunderstorm_hail"),
        _ => ("Unknown", "unknown")
    };

    if (!showArt)
    {
        return (description, $"[ {key.Replace('_', ' ').ToUpperInvariant()} ]");
    }

    var art = AsciiArtCatalog.GetArt(key, GetTerminalWidth());
    if (string.IsNullOrWhiteSpace(art))
    {
        return (description, $"[ {key.Replace('_', ' ').ToUpperInvariant()} ]");
    }

    return (description, art);
}

static string FormatValue(double value)
{
    return double.IsNaN(value) ? "n/a" : value.ToString("0.#");
}

static Units ParseUnits(string? value)
{
    if (string.Equals(value, "imperial", StringComparison.OrdinalIgnoreCase))
    {
        return Units.Imperial;
    }

    return Units.Metric;
}

static string BuildUnitParameters(Units units)
{
    return units switch
    {
        Units.Imperial => "&temperature_unit=fahrenheit&wind_speed_unit=mph&precipitation_unit=inch",
        _ => string.Empty
    };
}

static string BuildCountryCodeParameter(string? countryCode)
{
    if (string.IsNullOrWhiteSpace(countryCode))
    {
        return string.Empty;
    }

    return $"&countryCode={Uri.EscapeDataString(countryCode)}";
}

static string ExtractPrimaryName(string input)
{
    var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    return parts.Length > 0 ? parts[0] : input;
}

static string? NormalizeCountryCode(string? input)
{
    if (string.IsNullOrWhiteSpace(input))
    {
        return null;
    }

    var trimmed = input.Trim();
    if (trimmed.Length == 2)
    {
        return trimmed.ToUpperInvariant();
    }

    return CountryNameToCode(trimmed);
}

static string? InferCountryCode(string input)
{
    var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (parts.Length < 2)
    {
        return null;
    }

    return CountryNameToCode(parts[^1]);
}

static string? CountryNameToCode(string name)
{
    return CountryCodeCatalog.GetCode(name);
}

static int SafeGetInt(List<int>? list, int index, int fallback)
{
    if (list == null || index >= list.Count) return fallback;
    return list[index];
}

static double SafeGetDouble(List<double>? list, int index, double fallback)
{
    if (list == null || index >= list.Count) return fallback;
    return list[index];
}

static int? GetTerminalWidth()
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

static class AsciiArtCatalog
{
    private const int LargeMinWidth = 100;
    private const int MediumMinWidth = 70;
    private const int SmallMinWidth = 45;
    private static AsciiArtSets? _sets;

    public static string GetArt(string key, int? width)
    {
        if (width is null || width < SmallMinWidth)
        {
            return string.Empty;
        }

        var sets = _sets ??= LoadSets();
        if (sets == null)
        {
            return string.Empty;
        }

        if (width >= LargeMinWidth && sets.Large.TryGetValue(key, out var large))
        {
            return string.Join('\n', large);
        }

        if (width >= MediumMinWidth && sets.Medium.TryGetValue(key, out var medium))
        {
            return string.Join('\n', medium);
        }

        if (sets.Small.TryGetValue(key, out var small))
        {
            return string.Join('\n', small);
        }

        return string.Empty;
    }

    private static AsciiArtSets? LoadSets()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "ascii-art.json");
        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        var sets = JsonSerializer.Deserialize<AsciiArtSets>(json);
        return sets;
    }
}

sealed class AsciiArtSets
{
    public Dictionary<string, string[]> Small { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string[]> Medium { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string[]> Large { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

static class CountryCodeCatalog
{
    private static Dictionary<string, string>? _mappings;

    public static string? GetCode(string name)
    {
        var key = name.Trim();
        if (key.Length == 0)
        {
            return null;
        }

        var mappings = _mappings ??= LoadMappings();
        return mappings.TryGetValue(key, out var code) ? code : null;
    }

    private static Dictionary<string, string> LoadMappings()
    {
        var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in DefaultMappings())
        {
            mappings[pair.Key] = pair.Value;
        }

        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "country-codes.json");
        if (!File.Exists(path))
        {
            return mappings;
        }

        var json = File.ReadAllText(path);
        var custom = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        if (custom == null)
        {
            return mappings;
        }

        foreach (var pair in custom)
        {
            var key = pair.Key?.Trim();
            var value = pair.Value?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            mappings[key] = value;
        }

        return mappings;
    }

    private static Dictionary<string, string> DefaultMappings()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["united states"] = "US",
            ["usa"] = "US",
            ["us"] = "US",
            ["u.s."] = "US",
            ["united kingdom"] = "GB",
            ["uk"] = "GB",
            ["u.k."] = "GB",
            ["great britain"] = "GB",
            ["britain"] = "GB",
            ["scotland"] = "GB",
            ["england"] = "GB",
            ["wales"] = "GB",
            ["northern ireland"] = "GB",
            ["ireland"] = "IE",
            ["eire"] = "IE",
            ["canada"] = "CA",
            ["australia"] = "AU",
            ["new zealand"] = "NZ",
            ["germany"] = "DE",
            ["deutschland"] = "DE",
            ["france"] = "FR",
            ["spain"] = "ES",
            ["espana"] = "ES",
            ["italy"] = "IT",
            ["italia"] = "IT"
        };
    }
}

sealed class GeocodeResponse
{
    public List<GeoResult>? Results { get; set; }
}

sealed class GeoResult
{
    public string? Name { get; set; }
    public string? Admin1 { get; set; }
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Timezone { get; set; }
}

sealed class ForecastResponse
{
    public Daily? Daily { get; set; }
    public DailyUnits? DailyUnits { get; set; }
}

sealed class Daily
{
    public List<string> Time { get; set; } = new();
    public List<int> WeatherCode { get; set; } = new();
    public List<double> TemperatureMax { get; set; } = new();
    public List<double> TemperatureMin { get; set; } = new();
    public List<double> PrecipitationSum { get; set; } = new();
    public List<double> WindSpeedMax { get; set; } = new();
    public List<double> WindGustsMax { get; set; } = new();
}

sealed class DailyUnits
{
    public string TemperatureMax { get; set; } = "°C";
    public string TemperatureMin { get; set; } = "°C";
    public string PrecipitationSum { get; set; } = "mm";
    public string WindSpeedMax { get; set; } = "m/s";
    public string WindGustsMax { get; set; } = "m/s";
}

enum Units
{
    Metric,
    Imperial
}
