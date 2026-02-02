using CLImate.App.Cli;
using CLImate.App.Models;
using CLImate.App.Rendering;
using FakeItEasy;

namespace CLImate.Tests.Rendering;

public sealed class TableRendererTests
{
    private readonly IConsoleIO _console;
    private readonly IWeatherCodeCatalogue _weatherCodes;
    private readonly IAnsiColouriser _colouriser;
    private readonly ITemperatureColourScale _temperatureColours;
    private readonly IAsciiArtCatalogue _asciiArt;
    private readonly IArtColouriser _artColouriser;
    private readonly TableRenderer _renderer;

    public TableRendererTests()
    {
        _console = A.Fake<IConsoleIO>();
        _weatherCodes = A.Fake<IWeatherCodeCatalogue>();
        _colouriser = A.Fake<IAnsiColouriser>();
        _temperatureColours = A.Fake<ITemperatureColourScale>();
        _asciiArt = A.Fake<IAsciiArtCatalogue>();
        _artColouriser = A.Fake<IArtColouriser>();

        A.CallTo(() => _colouriser.ShouldUseColour(A<bool>._)).Returns(false);
        A.CallTo(() => _colouriser.Apply(A<string>._, A<AnsiColour>._, A<bool>._))
            .ReturnsLazily((string text, AnsiColour _, bool _) => text);
        A.CallTo(() => _temperatureColours.GetColour(A<double>._)).Returns(AnsiColour.Default);
        A.CallTo(() => _weatherCodes.Describe(A<int>._))
            .Returns(new WeatherDescriptor("Clear", "clear", AnsiColour.Yellow));
        A.CallTo(() => _asciiArt.GetArt(A<string>._, A<int?>._)).Returns(string.Empty);
        A.CallTo(() => _artColouriser.Colourise(A<string>._, A<string>._, A<bool>._))
            .ReturnsLazily((string art, string _, bool _) => art);

        _renderer = new TableRenderer(_console, _weatherCodes, _colouriser, _temperatureColours, _asciiArt, _artColouriser);
    }

    [Fact]
    public void CanRenderHorizontally_WithEmptyForecast_ReturnsFalse()
    {
        var forecast = CreateForecast(dayCount: 0);

        var result = _renderer.CanRenderHorizontally(forecast, terminalWidth: 200);

        Assert.False(result);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(80)]
    [InlineData(99)]
    public void CanRenderHorizontally_WithNarrowTerminal_ReturnsFalse(int width)
    {
        var forecast = CreateForecast(dayCount: 7);

        var result = _renderer.CanRenderHorizontally(forecast, terminalWidth: width);

        Assert.False(result);
    }

    [Theory]
    [InlineData(110)]
    [InlineData(120)]
    [InlineData(200)]
    public void CanRenderHorizontally_WithWideTerminal_ReturnsTrue(int width)
    {
        var forecast = CreateForecast(dayCount: 7);

        var result = _renderer.CanRenderHorizontally(forecast, terminalWidth: width);

        Assert.True(result);
    }

    [Fact]
    public void RenderHorizontalTable_WithEmptyForecast_PrintsNoDataMessage()
    {
        var forecast = CreateForecast(dayCount: 0);

        _renderer.RenderHorizontalTable(forecast, showArt: false, useColour: false, terminalWidth: 150);

        A.CallTo(() => _console.WriteLine(A<string>.That.Contains("No forecast data")))
            .MustHaveHappened();
    }

    [Fact]
    public void RenderHorizontalTable_WithValidForecast_PrintsTableBorders()
    {
        var forecast = CreateForecast(dayCount: 3);

        _renderer.RenderHorizontalTable(forecast, showArt: false, useColour: false, terminalWidth: 150);

        A.CallTo(() => _console.WriteLine(A<string>.That.Contains("┌")))
            .MustHaveHappened();
        A.CallTo(() => _console.WriteLine(A<string>.That.Contains("└")))
            .MustHaveHappened();
    }

    [Fact]
    public void RenderHorizontalTable_WithValidForecast_CallsWeatherCodeCatalogue()
    {
        var forecast = CreateForecast(dayCount: 2);

        _renderer.RenderHorizontalTable(forecast, showArt: false, useColour: false, terminalWidth: 150);

        A.CallTo(() => _weatherCodes.Describe(A<int>._))
            .MustHaveHappened();
    }

    [Fact]
    public void RenderHorizontalTable_WithWideTerminal_UsesAsciiArt()
    {
        var forecast = CreateForecast(dayCount: 3);
        A.CallTo(() => _asciiArt.GetArt(A<string>._, A<int?>._))
            .Returns("  .--.  \n .-(  ).\n(__.__)");

        _renderer.RenderHorizontalTable(forecast, showArt: true, useColour: false, terminalWidth: 160);

        A.CallTo(() => _asciiArt.GetArt(A<string>._, A<int?>._))
            .MustHaveHappened();
    }

    [Fact]
    public void RenderHorizontalTable_WithNarrowTerminal_DoesNotUseAsciiArt()
    {
        var forecast = CreateForecast(dayCount: 3);

        _renderer.RenderHorizontalTable(forecast, showArt: true, useColour: false, terminalWidth: 120);

        // With terminalWidth < 140, ASCII art is not used
        A.CallTo(() => _asciiArt.GetArt(A<string>._, A<int?>._))
            .MustNotHaveHappened();
    }

    private static Forecast CreateForecast(int dayCount)
    {
        var days = new List<DailyForecast>();
        var baseDate = DateTime.Today;

        for (int i = 0; i < dayCount; i++)
        {
            days.Add(new DailyForecast(
                date: baseDate.AddDays(i).ToString("yyyy-MM-dd"),
                weatherCode: 1,
                temperatureMax: 20.0 + i,
                temperatureMin: 10.0 + i,
                precipitationSum: 0.5,
                windSpeedMax: 15.0,
                windGustsMax: 25.0
            ));
        }

        var units = new ForecastUnits("°C", "mm", "km/h", "km/h");
        return new Forecast(days, units);
    }
}
