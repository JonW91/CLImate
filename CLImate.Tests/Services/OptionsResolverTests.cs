using CLImate.App.Cli;
using CLImate.App.Configuration;
using CLImate.App.Models;
using CLImate.App.Services;
using FakeItEasy;

namespace CLImate.Tests.Services;

public sealed class OptionsResolverTests
{
    private readonly ICliOptionsParser _parser;
    private readonly IConfigurationService _configService;
    private readonly OptionsResolver _resolver;

    public OptionsResolverTests()
    {
        _parser = A.Fake<ICliOptionsParser>();
        _configService = A.Fake<IConfigurationService>();
        _resolver = new OptionsResolver(_parser, _configService);
    }

    private void SetupParse(CliOptions options) =>
        A.CallTo(() => _parser.Parse(A<string[]>._)).Returns(CliOptionsParseResult.Success(options));

    private void SetupConfig(ClimateConfig config) =>
        A.CallTo(() => _configService.GetConfig()).Returns(config);

    [Fact]
    public void ExplicitMetricUnits_AreNotOverriddenByImperialConfig()
    {
        SetupParse(new CliOptions { Units = Units.Metric, UnitsSetExplicitly = true });
        SetupConfig(new ClimateConfig { DefaultUnits = Units.Imperial });

        var result = _resolver.ResolveOptions(["--units", "metric", "London"]);

        Assert.True(result.IsValid);
        Assert.Equal(Units.Metric, result.Options.Units);
    }

    [Fact]
    public void ConfigUnits_ApplyWhenNotSetOnCommandLine()
    {
        SetupParse(new CliOptions());
        SetupConfig(new ClimateConfig { DefaultUnits = Units.Imperial });

        var result = _resolver.ResolveOptions(["London"]);

        Assert.Equal(Units.Imperial, result.Options.Units);
    }

    [Fact]
    public void ExplicitColour_IsNotOverriddenByConfig()
    {
        SetupParse(new CliOptions { UseColour = true, UseColourSetExplicitly = true });
        SetupConfig(new ClimateConfig { UseColour = false });

        var result = _resolver.ResolveOptions(["--colour", "London"]);

        Assert.True(result.Options.UseColour);
    }

    [Fact]
    public void ConfigShowArtFalse_AppliesWhenNotSetOnCommandLine()
    {
        SetupParse(new CliOptions());
        SetupConfig(new ClimateConfig { ShowArt = false });

        var result = _resolver.ResolveOptions(["London"]);

        Assert.False(result.Options.ShowArt);
    }

    [Fact]
    public void ExplicitCountry_TakesPrecedenceOverConfigDefault()
    {
        SetupParse(new CliOptions { CountryCode = "US" });
        SetupConfig(new ClimateConfig { DefaultCountry = "GB" });

        var result = _resolver.ResolveOptions(["--country", "US", "Portland"]);

        Assert.Equal("US", result.Options.CountryCode);
    }

    [Fact]
    public void ConfigCountry_AppliesWhenNoCountryPassed()
    {
        SetupParse(new CliOptions());
        SetupConfig(new ClimateConfig { DefaultCountry = "GB" });

        var result = _resolver.ResolveOptions(["London"]);

        Assert.Equal("GB", result.Options.CountryCode);
    }

    [Fact]
    public void ParseFailure_IsReturnedWithoutTouchingConfig()
    {
        A.CallTo(() => _parser.Parse(A<string[]>._))
            .Returns(CliOptionsParseResult.Failure("Unknown option: --bogus"));

        var result = _resolver.ResolveOptions(["--bogus"]);

        Assert.False(result.IsValid);
        Assert.Contains("Unknown option", result.ErrorMessage);
    }
}
