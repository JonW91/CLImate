using CLImate.App.Cli;
using CLImate.App.Services;
using FakeItEasy;

namespace CLImate.Tests.Cli;

public sealed class CliOptionsParserTests
{
    private readonly ILocationInputParser _locationInputParser;
    private readonly ICountryCodeCatalogue _countryCodeCatalogue;
    private readonly CliOptionsParser _parser;

    public CliOptionsParserTests()
    {
        _locationInputParser = A.Fake<ILocationInputParser>();
        _countryCodeCatalogue = A.Fake<ICountryCodeCatalogue>();
        
        A.CallTo(() => _countryCodeCatalogue.IsValidCode(A<string>._))
            .ReturnsLazily((string code) => code.Length == 2 && code.All(char.IsLetter));
        A.CallTo(() => _locationInputParser.NormaliseCountryCode(A<string>._))
            .ReturnsLazily((string code) => code?.ToUpperInvariant());
        
        _parser = new CliOptionsParser(_locationInputParser, _countryCodeCatalogue);
    }

    [Fact]
    public void Parse_WithNoArguments_ReturnsValidResult()
    {
        var result = _parser.Parse([]);

        Assert.True(result.IsValid);
        Assert.Null(result.Options.LocationInput);
    }

    [Fact]
    public void Parse_WithLocationOnly_SetsLocationInput()
    {
        var result = _parser.Parse(["London"]);

        Assert.True(result.IsValid);
        Assert.Equal("London", result.Options.LocationInput);
    }

    [Fact]
    public void Parse_WithMultiWordLocation_JoinsLocationParts()
    {
        var result = _parser.Parse(["New", "York", "City"]);

        Assert.True(result.IsValid);
        Assert.Equal("New York City", result.Options.LocationInput);
    }

    [Fact]
    public void Parse_WithHelpFlag_SetsShowHelp()
    {
        var result = _parser.Parse(["--help"]);

        Assert.True(result.IsValid);
        Assert.True(result.Options.ShowHelp);
    }

    [Fact]
    public void Parse_WithShortHelpFlag_SetsShowHelp()
    {
        var result = _parser.Parse(["-h"]);

        Assert.True(result.IsValid);
        Assert.True(result.Options.ShowHelp);
    }

    [Fact]
    public void Parse_WithUnitsMetric_SetsMetricUnits()
    {
        var result = _parser.Parse(["--units", "metric", "London"]);

        Assert.True(result.IsValid);
        Assert.Equal(App.Models.Units.Metric, result.Options.Units);
    }

    [Fact]
    public void Parse_WithUnitsImperial_SetsImperialUnits()
    {
        var result = _parser.Parse(["--units", "imperial", "London"]);

        Assert.True(result.IsValid);
        Assert.Equal(App.Models.Units.Imperial, result.Options.Units);
    }

    [Fact]
    public void Parse_WithShortUnitsFlag_Works()
    {
        var result = _parser.Parse(["-u", "imperial", "London"]);

        Assert.True(result.IsValid);
        Assert.Equal(App.Models.Units.Imperial, result.Options.Units);
    }

    [Fact]
    public void Parse_WithValidCountryCode_SetsCountryCode()
    {
        var result = _parser.Parse(["--country", "GB", "London"]);

        Assert.True(result.IsValid);
        Assert.Equal("GB", result.Options.CountryCode);
    }

    [Fact]
    public void Parse_WithInvalidCountryCode_ReturnsFailure()
    {
        A.CallTo(() => _countryCodeCatalogue.IsValidCode("XYZ"))
            .Returns(false);

        var result = _parser.Parse(["--country", "XYZ", "London"]);

        Assert.False(result.IsValid);
        Assert.Contains("Invalid country code", result.ErrorMessage);
    }

    [Fact]
    public void Parse_WithNoArtFlag_DisablesArt()
    {
        var result = _parser.Parse(["--no-art", "London"]);

        Assert.True(result.IsValid);
        Assert.False(result.Options.ShowArt);
    }

    [Fact]
    public void Parse_WithNoColourFlag_DisablesColour()
    {
        var result = _parser.Parse(["--no-colour", "London"]);

        Assert.True(result.IsValid);
        Assert.False(result.Options.UseColour);
    }

    [Fact]
    public void Parse_WithColourFlag_EnablesColour()
    {
        var result = _parser.Parse(["--colour", "London"]);

        Assert.True(result.IsValid);
        Assert.True(result.Options.UseColour);
    }

    [Fact]
    public void Parse_WithTodayFlag_SetsTodayOnly()
    {
        var result = _parser.Parse(["--today", "London"]);

        Assert.True(result.IsValid);
        Assert.True(result.Options.TodayOnly);
    }

    [Fact]
    public void Parse_WithShortTodayFlag_SetsTodayOnly()
    {
        var result = _parser.Parse(["-t", "London"]);

        Assert.True(result.IsValid);
        Assert.True(result.Options.TodayOnly);
    }

    [Fact]
    public void Parse_WithHorizontalFlag_SetsHorizontalLayout()
    {
        var result = _parser.Parse(["--horizontal", "London"]);

        Assert.True(result.IsValid);
        Assert.Equal(LayoutMode.Horizontal, result.Options.Layout);
    }

    [Fact]
    public void Parse_WithShortHorizontalFlag_SetsHorizontalLayout()
    {
        var result = _parser.Parse(["-H", "London"]);

        Assert.True(result.IsValid);
        Assert.Equal(LayoutMode.Horizontal, result.Options.Layout);
    }

    [Fact]
    public void Parse_WithVerticalFlag_SetsVerticalLayout()
    {
        var result = _parser.Parse(["--vertical", "London"]);

        Assert.True(result.IsValid);
        Assert.Equal(LayoutMode.Vertical, result.Options.Layout);
    }

    [Fact]
    public void Parse_WithShortVerticalFlag_SetsVerticalLayout()
    {
        var result = _parser.Parse(["-V", "London"]);

        Assert.True(result.IsValid);
        Assert.Equal(LayoutMode.Vertical, result.Options.Layout);
    }

    [Fact]
    public void Parse_WithUnknownOption_ReturnsFailure()
    {
        var result = _parser.Parse(["--unknown", "London"]);

        Assert.False(result.IsValid);
        Assert.Contains("Unknown option", result.ErrorMessage);
    }

    [Fact]
    public void Parse_WithMissingUnitsValue_ReturnsFailure()
    {
        var result = _parser.Parse(["--units"]);

        Assert.False(result.IsValid);
        Assert.Contains("Missing value for --units", result.ErrorMessage);
    }

    [Fact]
    public void Parse_WithMissingCountryValue_ReturnsFailure()
    {
        var result = _parser.Parse(["--country"]);

        Assert.False(result.IsValid);
        Assert.Contains("Missing value for --country", result.ErrorMessage);
    }

    [Fact]
    public void Parse_DefaultLayoutIsAuto()
    {
        var result = _parser.Parse(["London"]);

        Assert.True(result.IsValid);
        Assert.Equal(LayoutMode.Auto, result.Options.Layout);
    }

    [Fact]
    public void Parse_DefaultUnitsIsMetric()
    {
        var result = _parser.Parse(["London"]);

        Assert.True(result.IsValid);
        Assert.Equal(App.Models.Units.Metric, result.Options.Units);
    }

    [Fact]
    public void Parse_DefaultShowArtIsTrue()
    {
        var result = _parser.Parse(["London"]);

        Assert.True(result.IsValid);
        Assert.True(result.Options.ShowArt);
    }

    [Fact]
    public void Parse_DefaultUseColourIsTrue()
    {
        var result = _parser.Parse(["London"]);

        Assert.True(result.IsValid);
        Assert.True(result.Options.UseColour);
    }
}
