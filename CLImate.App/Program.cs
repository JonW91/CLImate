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

    if (arg.StartsWith("--units=", StringComparison.OrdinalIgnoreCase))
    {
        units = ParseUnits(arg.Substring("--units=".Length));
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
    $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(locationInput)}&count=5&language=en&format=json");

if (geocode?.Results == null || geocode.Results.Count == 0)
{
    Console.WriteLine("No locations found. Try a more specific query.");
    return;
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

PrintDailyForecast(forecast);

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

static void PrintDailyForecast(ForecastResponse forecast)
{
    var daily = forecast.Daily!;
    var units = forecast.DailyUnits ?? new DailyUnits();

    Console.WriteLine();
    Console.WriteLine("Daily forecast (today + 6 days):");
    Console.WriteLine();

    for (var i = 0; i < daily.Time.Count; i++)
    {
        var date = daily.Time[i];
        var code = SafeGet(daily.WeatherCode, i, 0);
        var (desc, art) = DescribeWeather(code);

        var max = SafeGet(daily.TemperatureMax, i, double.NaN);
        var min = SafeGet(daily.TemperatureMin, i, double.NaN);
        var precip = SafeGet(daily.PrecipitationSum, i, double.NaN);
        var wind = SafeGet(daily.WindSpeedMax, i, double.NaN);
        var gust = SafeGet(daily.WindGustsMax, i, double.NaN);

        Console.WriteLine($"{date}  {desc}");
        Console.WriteLine(art);
        Console.WriteLine($"  High/Low: {FormatValue(max)}{units.TemperatureMax} / {FormatValue(min)}{units.TemperatureMin}");
        Console.WriteLine($"  Precip:   {FormatValue(precip)}{units.PrecipitationSum}");
        Console.WriteLine($"  Wind:     {FormatValue(wind)}{units.WindSpeedMax} (gusts {FormatValue(gust)}{units.WindGustsMax})");
        Console.WriteLine();
    }
}

static (string Description, string Art) DescribeWeather(int code)
{
    return code switch
    {
        0 => ("Clear sky", "   \\   /\n    .-.\n -- ( ) --\n    `-'\n   /   \\"),
        1 or 2 => ("Mainly clear, partly cloudy", "   \\  /\n _ /\"\".-\n   \\_(   ).\n   /(___(__)\n          "),
        3 => ("Overcast", "    .--.\n .-(    ).\n(___.__)__)\n          "),
        45 or 48 => ("Fog", " _ - _ - _\n  _ - _ -\n _ - _ - _"),
        51 or 53 or 55 => ("Drizzle", "    .--.\n .-(    ).\n(___.__)__)\n  '  '  ' "),
        56 or 57 => ("Freezing drizzle", "    .--.\n .-(    ).\n(___.__)__)\n  *  *  * "),
        61 or 63 or 65 => ("Rain", "    .--.\n .-(    ).\n(___.__)__)\n  ' ' ' ' "),
        66 or 67 => ("Freezing rain", "    .--.\n .-(    ).\n(___.__)__)\n  * * * * "),
        71 or 73 or 75 => ("Snow", "    .--.\n .-(    ).\n(___.__)__)\n  *  *  * "),
        77 => ("Snow grains", "    .--.\n .-(    ).\n(___.__)__)\n  *  .  * "),
        80 or 81 or 82 => ("Rain showers", "    .--.\n .-(    ).\n(___.__)__)\n  ' ' '  "),
        85 or 86 => ("Snow showers", "    .--.\n .-(    ).\n(___.__)__)\n  *  *   "),
        95 => ("Thunderstorm", "    .--.\n .-(    ).\n(___.__)__)\n   /_/    "),
        96 or 99 => ("Thunderstorm with hail", "    .--.\n .-(    ).\n(___.__)__)\n  * /_/  "),
        _ => ("Unknown", "    .--.\n .-(    ).\n(___.__)__)")
    };
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

static int SafeGet(List<int>? list, int index, int fallback)
{
    if (list == null || index >= list.Count) return fallback;
    return list[index];
}

static double SafeGet(List<double>? list, int index, double fallback)
{
    if (list == null || index >= list.Count) return fallback;
    return list[index];
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
