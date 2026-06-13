using System.Text.Json;
using CLImate.App.Services;
using FakeItEasy;

namespace CLImate.Tests.Services;

public sealed class MeteoalarmWarningsClientTests
{
    private readonly IJsonHttpClient _client;
    private readonly MeteoalarmWarningsClient _meteoalarm;

    public MeteoalarmWarningsClientTests()
    {
        _client = A.Fake<IJsonHttpClient>();
        _meteoalarm = new MeteoalarmWarningsClient(_client);
    }

    private static JsonElement ParseJson(string json) =>
        JsonSerializer.Deserialize<JsonElement>(json);

    [Fact]
    public async Task GetWarningsAsync_ValidWarnings_ParsesWarning()
    {
        var json = """
            {
              "warnings": [
                {
                  "event": "Wind",
                  "severity": "Moderate",
                  "start": "2026-03-01T06:00:00Z",
                  "end": "2026-03-01T18:00:00Z"
                }
              ]
            }
            """;
        A.CallTo(() => _client.GetAsync<JsonElement>(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult(ParseJson(json)));

        var results = await _meteoalarm.GetWarningsAsync(48.0, 16.0, CancellationToken.None);

        Assert.Single(results);
        Assert.Contains("Wind", results[0].Summary);
        Assert.Contains("Moderate", results[0].Summary);
        Assert.NotNull(results[0].Starts);
        Assert.NotNull(results[0].Ends);
    }

    [Fact]
    public async Task GetWarningsAsync_EmptyWarningsArray_ReturnsEmpty()
    {
        var json = """{ "warnings": [] }""";
        A.CallTo(() => _client.GetAsync<JsonElement>(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult(ParseJson(json)));

        var results = await _meteoalarm.GetWarningsAsync(48.0, 16.0, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetWarningsAsync_MissingWarningsProperty_ReturnsEmpty()
    {
        var json = """{ "status": "ok" }""";
        A.CallTo(() => _client.GetAsync<JsonElement>(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult(ParseJson(json)));

        var results = await _meteoalarm.GetWarningsAsync(48.0, 16.0, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetWarningsAsync_HttpRequestException_ReturnsEmpty()
    {
        A.CallTo(() => _client.GetAsync<JsonElement>(A<string>._, A<CancellationToken>._))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var results = await _meteoalarm.GetWarningsAsync(48.0, 16.0, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetWarningsAsync_WeatherApiException_ReturnsEmpty()
    {
        A.CallTo(() => _client.GetAsync<JsonElement>(A<string>._, A<CancellationToken>._))
            .ThrowsAsync(new WeatherApiException(System.Net.HttpStatusCode.ServiceUnavailable, "https://api.open-meteo.com/v1/warnings"));

        var results = await _meteoalarm.GetWarningsAsync(48.0, 16.0, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetWarningsAsync_FallsBackToHeadlineWhenEventMissing()
    {
        var json = """
            {
              "warnings": [
                {
                  "headline": "Extreme Heat",
                  "start": "2026-08-01T10:00:00Z"
                }
              ]
            }
            """;
        A.CallTo(() => _client.GetAsync<JsonElement>(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult(ParseJson(json)));

        var results = await _meteoalarm.GetWarningsAsync(48.0, 16.0, CancellationToken.None);

        Assert.Single(results);
        Assert.Contains("Extreme Heat", results[0].Summary);
    }

    [Fact]
    public async Task GetWarningsAsync_UsesOnsetWhenStartIsAbsent()
    {
        var json = """
            {
              "warnings": [
                {
                  "event": "Thunderstorm",
                  "onset": "2026-07-15T14:00:00Z",
                  "expires": "2026-07-15T20:00:00Z"
                }
              ]
            }
            """;
        A.CallTo(() => _client.GetAsync<JsonElement>(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult(ParseJson(json)));

        var results = await _meteoalarm.GetWarningsAsync(48.0, 16.0, CancellationToken.None);

        Assert.Single(results);
        Assert.NotNull(results[0].Starts);
        Assert.Equal(DateTimeOffset.Parse("2026-07-15T14:00:00Z"), results[0].Starts);
    }
}
