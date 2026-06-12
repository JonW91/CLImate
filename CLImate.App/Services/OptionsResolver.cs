using CLImate.App.Cli;
using CLImate.App.Configuration;

namespace CLImate.App.Services;

public class ResolveResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public CliOptions Options { get; init; } = new();
}

public interface IOptionsResolver
{
    ResolveResult ResolveOptions(string[] args);
}

public sealed class OptionsResolver : IOptionsResolver
{
    private readonly ICliOptionsParser _parser;
    private readonly IConfigurationService _configService;

    public OptionsResolver(ICliOptionsParser parser, IConfigurationService configService)
    {
        _parser = parser;
        _configService = configService;
    }

    public ResolveResult ResolveOptions(string[] args)
    {
        var parseResult = _parser.Parse(args);

        if (!parseResult.IsValid)
        {
            return new ResolveResult
            {
                IsValid = false,
                ErrorMessage = parseResult.ErrorMessage,
                Options = parseResult.Options
            };
        }

        var options = parseResult.Options;

        // Config-file values fill in only the settings the user did not pass
        // explicitly on the command line — CLI flags always win.
        var config = _configService.GetConfig();

        if (!options.UnitsSetExplicitly)
        {
            options.Units = config.DefaultUnits;
        }

        if (string.IsNullOrEmpty(options.CountryCode) && !string.IsNullOrEmpty(config.DefaultCountry))
        {
            options.CountryCode = config.DefaultCountry;
        }

        if (!options.ShowArtSetExplicitly)
        {
            options.ShowArt = config.ShowArt;
        }

        if (!options.UseColourSetExplicitly)
        {
            options.UseColour = config.UseColour;
        }

        return new ResolveResult
        {
            IsValid = true,
            Options = options
        };
    }
}
