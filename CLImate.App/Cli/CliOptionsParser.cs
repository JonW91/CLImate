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

    public CliOptionsParser(ILocationInputParser locationInputParser)
    {
        _locationInputParser = locationInputParser;
    }

    public CliOptionsParseResult Parse(string[] args)
    {
        var options = new CliOptions();
        var locationParts = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg is "-h" or "--help")
            {
                options.ShowHelp = true;
                return CliOptionsParseResult.Success(options);
            }

            if (arg is "--units" or "-u")
            {
                if (i + 1 >= args.Length)
                {
                    return CliOptionsParseResult.Failure("Missing value for --units.");
                }

                options.Units = ParseUnits(args[++i]);
                continue;
            }

            if (arg is "--country" or "-c")
            {
                if (i + 1 >= args.Length)
                {
                    return CliOptionsParseResult.Failure("Missing value for --country.");
                }

                options.CountryCode = _locationInputParser.NormaliseCountryCode(args[++i]);
                    continue;
                }

                if (arg is "--no-art")
                {
                    options.ShowArt = false;
                    continue;
                }

                if (arg is "--no-colour")
                {
                    options.UseColour = false;
                    continue;
                }

                if (arg is "--colour")
                {
                    options.UseColour = true;
                    continue;
                }

            if (arg is "--today" or "-t")
            {
                options.TodayOnly = true;
                continue;
            }

            if (arg is "--horizontal" or "-H")
            {
                options.Layout = LayoutMode.Horizontal;
                continue;
            }

            if (arg is "--vertical" or "-V")
            {
                options.Layout = LayoutMode.Vertical;
                continue;
            }

            if (arg.StartsWith("--units=", StringComparison.OrdinalIgnoreCase))
            {
                options.Units = ParseUnits(arg.Substring("--units=".Length));
                continue;
            }

            if (arg.StartsWith("--country=", StringComparison.OrdinalIgnoreCase))
            {
                options.CountryCode = _locationInputParser.NormaliseCountryCode(arg.Substring("--country=".Length));
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

    private static Units ParseUnits(string? value)
        => string.Equals(value, "imperial", StringComparison.OrdinalIgnoreCase)
            ? Units.Imperial
            : Units.Metric;
}
