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
    private readonly ICliOptionsParser _optionsParser;
    private readonly ICliHelp _help;
    private readonly IGeocodingService _geocodingService;
    private readonly IForecastService _forecastService;
    private readonly ILocationSelector _locationSelector;
    private readonly ILocationFormatter _locationFormatter;
    private readonly ILocationInputParser _locationInputParser;
    private readonly IForecastRenderer _forecastRenderer;
    private readonly IWeatherWarningsService _warningsService;

    public CliApplication(
        IConsoleIO console,
        ICliOptionsParser optionsParser,
        ICliHelp help,
        IGeocodingService geocodingService,
        IForecastService forecastService,
        ILocationSelector locationSelector,
        ILocationFormatter locationFormatter,
        ILocationInputParser locationInputParser,
        IForecastRenderer forecastRenderer,
        IWeatherWarningsService warningsService)
    {
        _console = console;
        _optionsParser = optionsParser;
        _help = help;
        _geocodingService = geocodingService;
        _forecastService = forecastService;
        _locationSelector = locationSelector;
        _locationFormatter = locationFormatter;
        _locationInputParser = locationInputParser;
        _forecastRenderer = forecastRenderer;
        _warningsService = warningsService;
    }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var parseResult = _optionsParser.Parse(args);
        if (!parseResult.IsValid)
        {
            _console.WriteLine(parseResult.ErrorMessage ?? "Invalid arguments.");
            _help.Print();
            return 1;
        }

        var options = parseResult.Options;
        if (options.ShowHelp)
        {
            _help.Print();
            return 0;
        }

        var locationInput = options.LocationInput;
        if (string.IsNullOrWhiteSpace(locationInput))
        {
            locationInput = Prompt("Enter a location (city, region, or address): ");
        }

        if (string.IsNullOrWhiteSpace(locationInput))
        {
            _console.WriteLine("No location provided. Exiting.");
            return 1;
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
            warningDates,
            cancellationToken);

        var forecastWithWarnings = forecast.WithWarnings(warnings);
        if (options.TodayOnly)
        {
            _forecastRenderer.RenderToday(forecastWithWarnings, options.ShowArt, options.UseColour);
        }
        else
        {
            _forecastRenderer.RenderDaily(forecastWithWarnings, options.ShowArt, options.UseColour);
        }

        return 0;
    }

    private string Prompt(string prompt)
    {
        _console.Write(prompt);
        return _console.ReadLine() ?? string.Empty;
    }
}
