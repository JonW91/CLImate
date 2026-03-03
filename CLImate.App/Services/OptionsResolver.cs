using CLImate.App.Cli;
using CLImate.App.Configuration;
using CLImate.App.Models;

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
        
        // If parsing failed, return the error
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
        
        // Load config and apply defaults where CLI args aren't explicitly set
        var config = _configService.GetConfig();
        
        // Apply config defaults - CLI args take precedence
        // Only apply if the CLI default is still in place (user didn't override)
        if (options.Units == Units.Metric && config.DefaultUnits == Units.Imperial)
        {
            options.Units = config.DefaultUnits;
        }
        
        if (string.IsNullOrEmpty(options.CountryCode) && !string.IsNullOrEmpty(config.DefaultCountry))
        {
            options.CountryCode = config.DefaultCountry;
        }
        
        // ShowArt and UseColour default to true, so only apply if config says false
        if (options.ShowArt && !config.ShowArt)
        {
            options.ShowArt = config.ShowArt;
        }
        
        if (options.UseColour && !config.UseColour)
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
