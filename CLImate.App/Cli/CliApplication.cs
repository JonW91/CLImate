using CLImate.App.Configuration;
using CLImate.App.Models;
using CLImate.App.Rendering;
using CLImate.App.Services;

namespace CLImate.App.Cli;

public interface ICliApplication
{
    Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default);
}

public sealed class CliApplication : ICliApplication
{
    private readonly IConsoleIO _console;
    private readonly IOptionsResolver _optionsResolver;
    private readonly ICliHelp _help;
    private readonly IGeocodingService _geocodingService;
    private readonly IForecastService _forecastService;
    private readonly ILocationSelector _locationSelector;
    private readonly ILocationFormatter _locationFormatter;
    private readonly ILocationInputParser _locationInputParser;
    private readonly IForecastRenderer _forecastRenderer;
    private readonly IWeatherWarningsService _warningsService;
    private readonly ITerminalInfo _terminalInfo;
    private readonly IConfigurationService _configService;

    public CliApplication(
        IConsoleIO console,
        IOptionsResolver optionsResolver,
        ICliHelp help,
        IGeocodingService geocodingService,
        IForecastService forecastService,
        ILocationSelector locationSelector,
        ILocationFormatter locationFormatter,
        ILocationInputParser locationInputParser,
        IForecastRenderer forecastRenderer,
        IWeatherWarningsService warningsService,
        ITerminalInfo terminalInfo,
        IConfigurationService configService)
    {
        _console = console;
        _optionsResolver = optionsResolver;
        _help = help;
        _geocodingService = geocodingService;
        _forecastService = forecastService;
        _locationSelector = locationSelector;
        _locationFormatter = locationFormatter;
        _locationInputParser = locationInputParser;
        _forecastRenderer = forecastRenderer;
        _warningsService = warningsService;
        _terminalInfo = terminalInfo;
        _configService = configService;
    }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        try
        {
            return await RunCoreAsync(args, cancellationToken);
        }
        catch (WeatherApiException ex)
        {
            _console.WriteLine(ex.IsTransient
                ? $"The weather service is temporarily unavailable (HTTP {(int)ex.StatusCode}). Please try again in a moment."
                : $"The weather service rejected the request (HTTP {(int)ex.StatusCode}). If this persists, please report it at https://github.com/JonW91/CLImate/issues.");
            return 1;
        }
        catch (HttpRequestException)
        {
            _console.WriteLine("Unable to reach weather service. Check your internet connection and try again.");
            return 1;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _console.WriteLine("Request timed out. The weather service may be unavailable. Please try again.");
            return 1;
        }
        catch (OperationCanceledException)
        {
            _console.WriteLine("Operation cancelled.");
            return 1;
        }
    }

    private async Task<int> RunCoreAsync(string[] args, CancellationToken cancellationToken)
    {
        var resolveResult = _optionsResolver.ResolveOptions(args);
        if (!resolveResult.IsValid)
        {
            _console.WriteLine(resolveResult.ErrorMessage ?? "Invalid arguments.");
            _help.Print();
            return 1;
        }

        var options = resolveResult.Options;
        if (options.ShowHelp)
        {
            _help.Print();
            return 0;
        }

        if (options.ShowVersion)
        {
            PrintVersion();
            return 0;
        }

        if (options.Config != null)
        {
            return await RunConfigCommandAsync(options.Config, cancellationToken);
        }

        var locationInput = options.LocationInput;
        if (string.IsNullOrWhiteSpace(locationInput))
        {
            if (_terminalInfo.IsInputRedirected)
            {
                _console.WriteLine("No location provided. Pass a location as an argument (e.g. climate London).");
                return 1;
            }

            locationInput = PromptForLocation();
        }

        if (string.IsNullOrWhiteSpace(locationInput))
        {
            _console.WriteLine("No location provided. Exiting.");
            return 1;
        }

        // When the user picked a saved favourite by number, skip geocoding entirely.
        if (locationInput.StartsWith("__fav:", StringComparison.Ordinal)
            && int.TryParse(locationInput["__fav:".Length..], out var favIdx))
        {
            var fav = _configService.GetConfig().FavouriteLocations[favIdx];
            var favResult = new GeoResult
            {
                Name = fav.Name,
                Latitude = fav.Lat,
                Longitude = fav.Lon
            };
            return await RunForecastAsync(favResult, options, cancellationToken);
        }

        var results = await _geocodingService.SearchAsync(locationInput, options.CountryCode, cancellationToken);
        if (results.Count == 0)
        {
            var fallbackCode = options.CountryCode ?? _locationInputParser.InferCountryCode(locationInput);
            if (!string.IsNullOrWhiteSpace(fallbackCode))
            {
                var primaryName = _locationInputParser.ExtractPrimaryName(locationInput);
                results = await _geocodingService.SearchAsync(primaryName, fallbackCode, cancellationToken);
            }
        }

        if (results.Count == 0)
        {
            _console.WriteLine("No locations found. Try a more specific query or pass --country.");
            return 1;
        }

        var selected = _locationSelector.SelectLocation(results);
        if (selected == null)
        {
            _console.WriteLine("No location selected. Exiting.");
            return 1;
        }

        return await RunForecastAsync(selected, options, cancellationToken);
    }

    private async Task<int> RunForecastAsync(GeoResult selected, CliOptions options, CancellationToken cancellationToken)
    {
        if (selected.Latitude == null || selected.Longitude == null)
        {
            _console.WriteLine("Selected location is missing coordinates.");
            return 1;
        }

        _console.WriteLine();
        _console.WriteLine($"CLImate - Forecast for {_locationFormatter.Format(selected)}");
        _console.WriteLine(new string('-', 52));

        var forecast = await _forecastService.GetForecastAsync(
            selected.Latitude.Value,
            selected.Longitude.Value,
            options.Units,
            cancellationToken);

        if (forecast == null || forecast.Days.Count == 0)
        {
            _console.WriteLine("Could not fetch forecast data.");
            return 1;
        }

        var warningDates = forecast.Days.Select(day => day.Date).ToList();
        var warnings = await _warningsService.GetDailyWarningsAsync(
            selected.Latitude.Value,
            selected.Longitude.Value,
            selected.CountryCode,
            warningDates,
            cancellationToken);

        var forecastWithWarnings = forecast.WithWarnings(warnings);

        switch (options.ForecastMode)
        {
            case ForecastMode.Today:
                _forecastRenderer.RenderToday(forecastWithWarnings, options.ShowArt, options.UseColour);
                break;
            case ForecastMode.Hourly:
                _forecastRenderer.RenderHourly(forecastWithWarnings, options.ShowArt, options.UseColour);
                break;
            default:
                _forecastRenderer.RenderDaily(forecastWithWarnings, options.ShowArt, options.UseColour, options.Layout);
                break;
        }

        return 0;
    }

    private string PromptForLocation()
    {
        var config = _configService.GetConfig();
        var favourites = config.FavouriteLocations;

        if (favourites.Count > 0)
        {
            _console.WriteLine("Saved locations:");
            for (var i = 0; i < favourites.Count; i++)
            {
                _console.WriteLine($"  {i + 1}. {favourites[i].Name}");
            }
            _console.WriteLine();
        }

        var input = Prompt("Enter a location (city, region, or address): ");

        // If the user typed a number that matches a saved favourite, use it directly.
        if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input.Trim(), out var pick)
            && pick >= 1 && pick <= favourites.Count)
        {
            var fav = favourites[pick - 1];
            // Return a synthetic location key that GeocodingService won't be called for;
            // instead the caller resolves this via coordinates already in the favourite.
            return $"__fav:{pick - 1}";
        }

        return input;
    }

    private string Prompt(string prompt)
    {
        _console.Write(prompt);
        return _console.ReadLine() ?? string.Empty;
    }

    private async Task<int> RunConfigCommandAsync(ConfigSubcommand cmd, CancellationToken cancellationToken)
    {
        switch (cmd.Action)
        {
            case "show":
                return ShowConfig();

            case "set":
                return await SetConfigAsync(cmd.Key!, cmd.Value!, cancellationToken);

            case "add-favourite":
                return await AddFavouriteAsync(cmd.Value!, cancellationToken);

            default:
                _console.WriteLine($"Unknown config action: {cmd.Action}");
                return 1;
        }
    }

    private int ShowConfig()
    {
        var config = _configService.GetConfig();
        _console.WriteLine($"Config file: {_configService.GetConfigPath()}");
        _console.WriteLine($"  units:   {config.DefaultUnits.ToString().ToLowerInvariant()}");
        _console.WriteLine($"  country: {config.DefaultCountry ?? "(none)"}");
        _console.WriteLine($"  art:     {(config.ShowArt ? "on" : "off")}");
        _console.WriteLine($"  colour:  {(config.UseColour ? "on" : "off")}");
        if (config.FavouriteLocations.Count > 0)
        {
            _console.WriteLine("  favourites:");
            foreach (var fav in config.FavouriteLocations)
            {
                _console.WriteLine($"    {fav.Name} ({fav.Lat:F4}, {fav.Lon:F4})");
            }
        }
        return 0;
    }

    private async Task<int> SetConfigAsync(string key, string value, CancellationToken cancellationToken)
    {
        var config = _configService.GetConfig();

        switch (key)
        {
            case "country":
                config.DefaultCountry = string.IsNullOrWhiteSpace(value) ? null : value.ToUpperInvariant();
                break;

            case "units":
                if (string.Equals(value, "metric", StringComparison.OrdinalIgnoreCase))
                    config.DefaultUnits = Models.Units.Metric;
                else if (string.Equals(value, "imperial", StringComparison.OrdinalIgnoreCase))
                    config.DefaultUnits = Models.Units.Imperial;
                else
                {
                    _console.WriteLine($"Unknown units value '{value}'. Use 'metric' or 'imperial'.");
                    return 1;
                }
                break;

            case "art":
                if (!TryParseBool(value, out var artOn))
                {
                    _console.WriteLine($"Unknown value '{value}'. Use 'on' or 'off'.");
                    return 1;
                }
                config.ShowArt = artOn;
                break;

            case "colour":
                if (!TryParseBool(value, out var colourOn))
                {
                    _console.WriteLine($"Unknown value '{value}'. Use 'on' or 'off'.");
                    return 1;
                }
                config.UseColour = colourOn;
                break;

            default:
                _console.WriteLine($"Unknown key '{key}'. Valid keys: country, units, art, colour.");
                return 1;
        }

        await _configService.SaveConfigAsync(config);
        _console.WriteLine($"Saved: {key} = {value}");
        return 0;
    }

    private async Task<int> AddFavouriteAsync(string locationQuery, CancellationToken cancellationToken)
    {
        _console.WriteLine($"Searching for '{locationQuery}'...");
        var results = await _geocodingService.SearchAsync(locationQuery, null, cancellationToken);
        if (results.Count == 0)
        {
            _console.WriteLine("No locations found.");
            return 1;
        }

        var picked = results[0];
        if (picked.Latitude == null || picked.Longitude == null)
        {
            _console.WriteLine("Location is missing coordinates.");
            return 1;
        }

        var name = picked.Name ?? locationQuery;
        var config = _configService.GetConfig();

        // Replace if a favourite with the same name already exists.
        config.FavouriteLocations.RemoveAll(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
        config.FavouriteLocations.Add(new Configuration.FavouriteLocation
        {
            Name = name,
            Lat = picked.Latitude.Value,
            Lon = picked.Longitude.Value
        });

        await _configService.SaveConfigAsync(config);
        _console.WriteLine($"Saved favourite: {name} ({picked.Latitude.Value:F4}, {picked.Longitude.Value:F4})");
        return 0;
    }

    private static bool TryParseBool(string value, out bool result)
    {
        if (string.Equals(value, "on", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            return true;
        }
        if (string.Equals(value, "off", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "false", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "0", StringComparison.OrdinalIgnoreCase))
        {
            result = false;
            return true;
        }
        result = false;
        return false;
    }

    private void PrintVersion()
    {
        var assembly = typeof(CliApplication).Assembly;
        var version = assembly.GetName().Version?.ToString(3) ?? "0.0.0";
        _console.WriteLine($"CLImate version {version}");
    }
}
