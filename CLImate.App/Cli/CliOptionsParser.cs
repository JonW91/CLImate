using CLImate.App.Models;
using CLImate.App.Services;

namespace CLImate.App.Cli;

public interface ICliOptionsParser
{
    CliOptionsParseResult Parse(string[] args);
}

public sealed class CliOptionsParser : ICliOptionsParser
{
    private readonly ILocationInputParser _locationInputParser;
    private readonly ICountryCodeCatalogue _countryCodeCatalogue;

    public CliOptionsParser(ILocationInputParser locationInputParser, ICountryCodeCatalogue countryCodeCatalogue)
    {
        _locationInputParser = locationInputParser;
        _countryCodeCatalogue = countryCodeCatalogue;
    }

    public CliOptionsParseResult Parse(string[] args)
    {
        var options = new CliOptions();
        var locationParts = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            // Normalise "--option=value" into option name + inline value.
            string name = arg;
            string? inlineValue = null;
            var equalsIndex = arg.StartsWith("--", StringComparison.Ordinal) ? arg.IndexOf('=') : -1;
            if (equalsIndex > 0)
            {
                name = arg[..equalsIndex];
                inlineValue = arg[(equalsIndex + 1)..];
            }

            switch (name)
            {
                case "-h" or "--help":
                    options.ShowHelp = true;
                    return CliOptionsParseResult.Success(options);

                case "-v" or "--version":
                    options.ShowVersion = true;
                    return CliOptionsParseResult.Success(options);

                case "-u" or "--units":
                {
                    var value = inlineValue ?? NextValue(args, ref i);
                    if (value is null)
                    {
                        return CliOptionsParseResult.Failure("Missing value for --units.");
                    }

                    var parsedUnits = TryParseUnits(value);
                    if (parsedUnits is null)
                    {
                        return CliOptionsParseResult.Failure($"Invalid units: '{value}'. Use 'metric' or 'imperial'.");
                    }

                    options.Units = parsedUnits.Value;
                    options.UnitsSetExplicitly = true;
                    continue;
                }

                case "-c" or "--country":
                {
                    var value = inlineValue ?? NextValue(args, ref i);
                    if (value is null)
                    {
                        return CliOptionsParseResult.Failure("Missing value for --country.");
                    }

                    var normalised = _locationInputParser.NormaliseCountryCode(value);
                    if (!_countryCodeCatalogue.IsValidCode(normalised ?? value))
                    {
                        return CliOptionsParseResult.Failure($"Invalid country code: '{value}'. Use a 2-letter ISO 3166-1 code (e.g., GB, US, DE).");
                    }

                    options.CountryCode = normalised ?? value.ToUpperInvariant();
                    continue;
                }

                case "--no-art":
                    options.ShowArt = false;
                    options.ShowArtSetExplicitly = true;
                    continue;

                case "--no-colour":
                    options.UseColour = false;
                    options.UseColourSetExplicitly = true;
                    continue;

                case "--colour":
                    options.UseColour = true;
                    options.UseColourSetExplicitly = true;
                    continue;

                case "-t" or "--today":
                    options.ForecastMode = ForecastMode.Today;
                    continue;

                case "--hourly":
                    options.ForecastMode = ForecastMode.Hourly;
                    continue;

                case "-H" or "--horizontal":
                    options.Layout = LayoutMode.Horizontal;
                    continue;

                case "-V" or "--vertical":
                    options.Layout = LayoutMode.Vertical;
                    continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                return CliOptionsParseResult.Failure($"Unknown option: {arg}");
            }

            locationParts.Add(arg);
        }

        if (locationParts.Count > 0)
        {
            options.LocationInput = string.Join(" ", locationParts);
        }

        return CliOptionsParseResult.Success(options);
    }

    private static string? NextValue(string[] args, ref int i)
    {
        if (i + 1 >= args.Length)
        {
            return null;
        }

        return args[++i];
    }

    private static Units? TryParseUnits(string? value)
    {
        if (string.Equals(value, "metric", StringComparison.OrdinalIgnoreCase))
        {
            return Units.Metric;
        }

        if (string.Equals(value, "imperial", StringComparison.OrdinalIgnoreCase))
        {
            return Units.Imperial;
        }

        return null;
    }
}
