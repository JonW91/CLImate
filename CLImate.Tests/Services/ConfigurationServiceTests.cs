using CLImate.App.Cli;
using CLImate.App.Configuration;
using CLImate.App.Models;
using FakeItEasy;

namespace CLImate.Tests.Services;

public sealed class ConfigurationServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly IConsoleIO _console;
    private readonly ConfigurationService _service;

    public ConfigurationServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _console = A.Fake<IConsoleIO>();
        _service = new ConfigurationService(_console, _tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void GetConfig_WhenNoFile_ReturnsDefaults()
    {
        var config = _service.GetConfig();

        Assert.Equal(Units.Metric, config.DefaultUnits);
        Assert.Null(config.DefaultCountry);
        Assert.True(config.ShowArt);
        Assert.True(config.UseColour);
        Assert.Empty(config.FavouriteLocations);
    }

    [Fact]
    public async Task SaveAndGetConfig_RoundTrips()
    {
        var original = new ClimateConfig
        {
            DefaultUnits = Units.Imperial,
            DefaultCountry = "GB",
            ShowArt = false,
            UseColour = false
        };

        await _service.SaveConfigAsync(original);
        var loaded = _service.GetConfig();

        Assert.Equal(Units.Imperial, loaded.DefaultUnits);
        Assert.Equal("GB", loaded.DefaultCountry);
        Assert.False(loaded.ShowArt);
        Assert.False(loaded.UseColour);
    }

    [Fact]
    public void GetConfig_WithMalformedJson_WarnsAndReturnsDefaults()
    {
        File.WriteAllText(_service.GetConfigPath(), "{ not valid json }}}");

        var config = _service.GetConfig();

        Assert.Equal(Units.Metric, config.DefaultUnits);
        A.CallTo(() => _console.WriteLine(A<string>.That.Contains("Warning")))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void ConfigExists_WhenFileAbsent_ReturnsFalse()
    {
        Assert.False(_service.ConfigExists());
    }

    [Fact]
    public async Task ConfigExists_AfterSave_ReturnsTrue()
    {
        await _service.SaveConfigAsync(new ClimateConfig());

        Assert.True(_service.ConfigExists());
    }

    [Fact]
    public void GetConfigPath_EndsWithConfigJson()
    {
        Assert.EndsWith("config.json", _service.GetConfigPath());
    }

    [Fact]
    public async Task SaveConfigAsync_WithFavouriteLocations_RoundTrips()
    {
        var config = new ClimateConfig
        {
            FavouriteLocations =
            [
                new FavouriteLocation { Name = "Home", Lat = 51.5, Lon = -0.1 }
            ]
        };

        await _service.SaveConfigAsync(config);
        var loaded = _service.GetConfig();

        Assert.Single(loaded.FavouriteLocations);
        Assert.Equal("Home", loaded.FavouriteLocations[0].Name);
        Assert.Equal(51.5, loaded.FavouriteLocations[0].Lat);
        Assert.Equal(-0.1, loaded.FavouriteLocations[0].Lon);
    }
}
