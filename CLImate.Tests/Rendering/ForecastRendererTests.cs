using CLImate.App.Cli;
using CLImate.App.Models;
using CLImate.App.Rendering;
using FakeItEasy;

namespace CLImate.Tests.Rendering;

public sealed class ForecastRendererTests
{
    private readonly IConsoleIO _console;
    private readonly IAsciiArtCatalogue _asciiArt;
    private readonly IWeatherCodeCatalogue _weatherCodes;
    private readonly IAnsiColouriser _colouriser;
    private readonly ITemperatureColourScale _temperatureColours;
    private readonly IArtColouriser _artColouriser;
    private readonly ITerminalInfo _terminalInfo;
    private readonly ITableRenderer _tableRenderer;
    private readonly ForecastRenderer _renderer;

    public ForecastRendererTests()
    {
        _console = A.Fake<IConsoleIO>();
        _asciiArt = A.Fake<IAsciiArtCatalogue>();
        _weatherCodes = A.Fake<IWeatherCodeCatalogue>();
        _colouriser = A.Fake<IAnsiColouriser>();
        _temperatureColours = A.Fake<ITemperatureColourScale>();
        _artColouriser = A.Fake<IArtColouriser>();
        _terminalInfo = A.Fake<ITerminalInfo>();
        _tableRenderer = A.Fake<ITableRenderer>();

        A.CallTo(() => _colouriser.ShouldUseColour(A<bool>._)).Returns(false);
        A.CallTo(() => _colouriser.Apply(A<string>._, A<AnsiColour>._, A<bool>._))
            .ReturnsLazily((string text, AnsiColour _, bool _) => text);
        A.CallTo(() => _temperatureColours.GetColour(A<double>._)).Returns(AnsiColour.Default);
        A.CallTo(() => _weatherCodes.Describe(A<int>._))
            .Returns(new WeatherDescriptor("Clear", "clear", AnsiColour.Yellow));
        A.CallTo(() => _asciiArt.GetArt(A<string>._, A<int?>._)).Returns(string.Empty);
        A.CallTo(() => _artColouriser.Colourise(A<string>._, A<string>._, A<bool>._))
            .ReturnsLazily((string art, string _, bool _) => art);
        A.CallTo(() => _terminalInfo.Width).Returns(120);

        _renderer = new ForecastRenderer(
            _console,
            _asciiArt,
            _weatherCodes,
            _colouriser,
            _temperatureColours,
            _artColouriser,
            _terminalInfo,
            _tableRenderer);
    }

    [Fact]
    public void RenderDaily_WithWideTerminal_UsesTableRenderer()
    {
        var forecast = CreateForecast(dayCount: 7);
        A.CallTo(() => _tableRenderer.CanRenderHorizontally(forecast, 120)).Returns(true);

        _renderer.RenderDaily(forecast, showArt: true, useColour: false);

        A.CallTo(() => _tableRenderer.RenderHorizontalTable(forecast, true, false, 120))
            .MustHaveHappened();
    }

    [Fact]
    public void RenderDaily_WithNarrowTerminal_UsesVerticalLayout()
    {
        var forecast = CreateForecast(dayCount: 7);
        A.CallTo(() => _terminalInfo.Width).Returns(80);
        A.CallTo(() => _tableRenderer.CanRenderHorizontally(forecast, 80)).Returns(false);

        _renderer.RenderDaily(forecast, showArt: true, useColour: false);

        A.CallTo(() => _tableRenderer.RenderHorizontalTable(A<Forecast>._, A<bool>._, A<bool>._, A<int>._))
            .MustNotHaveHappened();
        A.CallTo(() => _console.WriteLine(A<string>.That.Contains("7-Day Forecast")))
            .MustHaveHappened();
    }

    [Fact]
    public void RenderDaily_WithVerticalLayoutForced_UsesVerticalLayout()
    {
        var forecast = CreateForecast(dayCount: 7);

        _renderer.RenderDaily(forecast, showArt: true, useColour: false, LayoutMode.Vertical);

        A.CallTo(() => _tableRenderer.RenderHorizontalTable(A<Forecast>._, A<bool>._, A<bool>._, A<int>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public void RenderDaily_WithHorizontalLayoutForced_UsesTableRenderer()
    {
        var forecast = CreateForecast(dayCount: 7);

        _renderer.RenderDaily(forecast, showArt: true, useColour: false, LayoutMode.Horizontal);

        A.CallTo(() => _tableRenderer.RenderHorizontalTable(forecast, true, false, 120))
            .MustHaveHappened();
    }

    [Fact]
    public void RenderToday_WithWideTerminal_UsesTableRenderer()
    {
        var forecast = CreateForecast(dayCount: 1);
        A.CallTo(() => _tableRenderer.CanRenderTodayHorizontally(forecast, 120)).Returns(true);

        _renderer.RenderToday(forecast, showArt: true, useColour: false);

        A.CallTo(() => _tableRenderer.RenderTodayTable(forecast, true, false, 120))
            .MustHaveHappened();
    }

    [Fact]
    public void RenderToday_WithNarrowTerminal_UsesVerticalLayout()
    {
        var forecast = CreateForecast(dayCount: 1);
        A.CallTo(() => _terminalInfo.Width).Returns(60);
        A.CallTo(() => _tableRenderer.CanRenderTodayHorizontally(forecast, 60)).Returns(false);

        _renderer.RenderToday(forecast, showArt: true, useColour: false);

        A.CallTo(() => _tableRenderer.RenderTodayTable(A<Forecast>._, A<bool>._, A<bool>._, A<int>._))
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
