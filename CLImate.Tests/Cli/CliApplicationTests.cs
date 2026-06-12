using CLImate.App.Cli;
using CLImate.App.Configuration;
using CLImate.App.Models;
using CLImate.App.Rendering;
using CLImate.App.Services;
using FakeItEasy;

namespace CLImate.Tests.Cli;

public sealed class CliApplicationTests
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
    private readonly CliApplication _app;

    private static readonly ForecastUnits DefaultUnits = new("°C", "mm", "km/h", "km/h");
    private static readonly List<DailyForecast> OneDayForecast =
    [
        new DailyForecast("2026-02-01", 0, 15.0, 5.0, 1.0, 10.0, 20.0)
    ];

    public CliApplicationTests()
    {
        _console = A.Fake<IConsoleIO>();
        _optionsResolver = A.Fake<IOptionsResolver>();
        _help = A.Fake<ICliHelp>();
        _geocodingService = A.Fake<IGeocodingService>();
        _forecastService = A.Fake<IForecastService>();
        _locationSelector = A.Fake<ILocationSelector>();
        _locationFormatter = A.Fake<ILocationFormatter>();
        _locationInputParser = A.Fake<ILocationInputParser>();
        _forecastRenderer = A.Fake<IForecastRenderer>();
        _warningsService = A.Fake<IWeatherWarningsService>();
        _terminalInfo = A.Fake<ITerminalInfo>();
        _configService = A.Fake<IConfigurationService>();

        A.CallTo(() => _configService.GetConfig()).Returns(new ClimateConfig());

        _app = new CliApplication(
            _console, _optionsResolver, _help,
            _geocodingService, _forecastService,
            _locationSelector, _locationFormatter, _locationInputParser,
            _forecastRenderer, _warningsService, _terminalInfo, _configService);
    }

    private void SetupResolveSuccess(CliOptions options) =>
        A.CallTo(() => _optionsResolver.ResolveOptions(A<string[]>._))
            .Returns(new ResolveResult { IsValid = true, Options = options });

    private void SetupResolveFailure(string error) =>
        A.CallTo(() => _optionsResolver.ResolveOptions(A<string[]>._))
            .Returns(new ResolveResult { IsValid = false, ErrorMessage = error, Options = new CliOptions() });

    private void SetupHappyPath(string location = "London")
    {
        var options = new CliOptions { LocationInput = location };
        SetupResolveSuccess(options);

        var geoResult = new GeoResult { Name = location, CountryCode = "GB", Latitude = 51.5, Longitude = -0.1 };
        A.CallTo(() => _geocodingService.SearchAsync(A<string>._, A<string?>._, A<CancellationToken>._))
            .Returns(Task.FromResult(new List<GeoResult> { geoResult }));
        A.CallTo(() => _locationSelector.SelectLocation(A<IReadOnlyList<GeoResult>>._))
            .Returns(geoResult);
        A.CallTo(() => _forecastService.GetForecastAsync(A<double>._, A<double>._, A<Units>._, A<CancellationToken>._))
            .Returns(Task.FromResult<Forecast?>(new Forecast(OneDayForecast, DefaultUnits)));
        A.CallTo(() => _warningsService.GetDailyWarningsAsync(A<double>._, A<double>._, A<string?>._, A<IReadOnlyList<string>>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IReadOnlyDictionary<string, WarningResult>>(new Dictionary<string, WarningResult>()));
    }

    [Fact]
    public async Task RunAsync_WithHelpFlag_PrintsHelpAndReturnsZero()
    {
        SetupResolveSuccess(new CliOptions { ShowHelp = true });

        var result = await _app.RunAsync(["--help"]);

        Assert.Equal(0, result);
        A.CallTo(() => _help.Print()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RunAsync_WithVersionFlag_ReturnsZero()
    {
        SetupResolveSuccess(new CliOptions { ShowVersion = true });

        var result = await _app.RunAsync(["--version"]);

        Assert.Equal(0, result);
        A.CallTo(() => _forecastService.GetForecastAsync(A<double>._, A<double>._, A<Units>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task RunAsync_WithInvalidArgs_PrintsErrorAndReturnsOne()
    {
        SetupResolveFailure("Unknown option: --bogus");

        var result = await _app.RunAsync(["--bogus"]);

        Assert.Equal(1, result);
        A.CallTo(() => _console.WriteLine(A<string>.That.Contains("Unknown option")))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RunAsync_WithNoLocationAndStdinRedirected_ReturnsOne()
    {
        SetupResolveSuccess(new CliOptions { LocationInput = null });
        A.CallTo(() => _terminalInfo.IsInputRedirected).Returns(true);

        var result = await _app.RunAsync([]);

        Assert.Equal(1, result);
        A.CallTo(() => _geocodingService.SearchAsync(A<string>._, A<string?>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task RunAsync_WhenGeocodingReturnsEmpty_ReturnsOne()
    {
        SetupResolveSuccess(new CliOptions { LocationInput = "Nowhere" });
        A.CallTo(() => _geocodingService.SearchAsync(A<string>._, A<string?>._, A<CancellationToken>._))
            .Returns(Task.FromResult(new List<GeoResult>()));
        A.CallTo(() => _locationInputParser.InferCountryCode(A<string>._)).Returns(null as string);

        var result = await _app.RunAsync(["Nowhere"]);

        Assert.Equal(1, result);
        A.CallTo(() => _console.WriteLine(A<string>.That.Contains("No locations found")))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RunAsync_HappyPath_CallsRendererAndReturnsZero()
    {
        SetupHappyPath();

        var result = await _app.RunAsync(["London"]);

        Assert.Equal(0, result);
        A.CallTo(() => _forecastRenderer.RenderDaily(A<Forecast>._, A<bool>._, A<bool>._, A<LayoutMode>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RunAsync_WithTodayMode_CallsRenderToday()
    {
        var options = new CliOptions { LocationInput = "London", ForecastMode = ForecastMode.Today };
        SetupResolveSuccess(options);

        var geoResult = new GeoResult { Name = "London", CountryCode = "GB", Latitude = 51.5, Longitude = -0.1 };
        A.CallTo(() => _geocodingService.SearchAsync(A<string>._, A<string?>._, A<CancellationToken>._))
            .Returns(Task.FromResult(new List<GeoResult> { geoResult }));
        A.CallTo(() => _locationSelector.SelectLocation(A<IReadOnlyList<GeoResult>>._)).Returns(geoResult);
        A.CallTo(() => _forecastService.GetForecastAsync(A<double>._, A<double>._, A<Units>._, A<CancellationToken>._))
            .Returns(Task.FromResult<Forecast?>(new Forecast(OneDayForecast, DefaultUnits)));
        A.CallTo(() => _warningsService.GetDailyWarningsAsync(A<double>._, A<double>._, A<string?>._, A<IReadOnlyList<string>>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IReadOnlyDictionary<string, WarningResult>>(new Dictionary<string, WarningResult>()));

        await _app.RunAsync(["-t", "London"]);

        A.CallTo(() => _forecastRenderer.RenderToday(A<Forecast>._, A<bool>._, A<bool>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RunAsync_WeatherApiException_PrintsMessageAndReturnsOne()
    {
        SetupResolveSuccess(new CliOptions { LocationInput = "London" });
        A.CallTo(() => _geocodingService.SearchAsync(A<string>._, A<string?>._, A<CancellationToken>._))
            .ThrowsAsync(new WeatherApiException(System.Net.HttpStatusCode.ServiceUnavailable, "https://api.open-meteo.com"));

        var result = await _app.RunAsync(["London"]);

        Assert.Equal(1, result);
        A.CallTo(() => _console.WriteLine(A<string>.That.Contains("temporarily unavailable")))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RunAsync_WhenForecastIsNull_ReturnsOne()
    {
        SetupResolveSuccess(new CliOptions { LocationInput = "London" });
        var geoResult = new GeoResult { Name = "London", CountryCode = "GB", Latitude = 51.5, Longitude = -0.1 };
        A.CallTo(() => _geocodingService.SearchAsync(A<string>._, A<string?>._, A<CancellationToken>._))
            .Returns(Task.FromResult(new List<GeoResult> { geoResult }));
        A.CallTo(() => _locationSelector.SelectLocation(A<IReadOnlyList<GeoResult>>._)).Returns(geoResult);
        A.CallTo(() => _forecastService.GetForecastAsync(A<double>._, A<double>._, A<Units>._, A<CancellationToken>._))
            .Returns(Task.FromResult<Forecast?>(null));

        var result = await _app.RunAsync(["London"]);

        Assert.Equal(1, result);
    }
}
