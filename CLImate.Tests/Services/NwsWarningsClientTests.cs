using System.Text.Json;
using CLImate.App.Services;
using FakeItEasy;

namespace CLImate.Tests.Services;

public sealed class NwsWarningsClientTests
{
    private readonly IJsonHttpClient _client;
    private readonly NwsWarningsClient _nws;

    public NwsWarningsClientTests()
    {
        _client = A.Fake<IJsonHttpClient>();
        _nws = new NwsWarningsClient(_client);
    }

    private static JsonElement ParseJson(string json) =>
        JsonSerializer.Deserialize<JsonElement>(json);

    [Fact]
    public async Task GetWarningsAsync_ValidFeatures_ParsesWarning()
    {
        var json = """
            {
              "features": [
                {
                  "properties": {
                    "headline": "Tornado Warning",
                    "severity": "Extreme",
                    "effective": "2026-02-01T10:00:00Z",
                    "ends": "2026-02-01T18:00:00Z"
                  }
                }
              ]
            }
            """;
        A.CallTo(() => _client.GetAsync<JsonElement>(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult(ParseJson(json)));

        var results = await _nws.GetWarningsAsync(35.0, -97.0, CancellationToken.None);

        Assert.Single(results);
        Assert.Contains("Tornado Warning", results[0].Summary);
        Assert.Contains("Extreme", results[0].Summary);
        Assert.NotNull(results[0].Starts);
        Assert.NotNull(results[0].Ends);
    }

    [Fact]
    public async Task GetWarningsAsync_EmptyFeatures_ReturnsEmpty()
    {
        var json = """{ "features": [] }""";
        A.CallTo(() => _client.GetAsync<JsonElement>(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult(ParseJson(json)));

        var results = await _nws.GetWarningsAsync(35.0, -97.0, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetWarningsAsync_MissingFeaturesProperty_ReturnsEmpty()
    {
        var json = """{ "status": "ok" }""";
        A.CallTo(() => _client.GetAsync<JsonElement>(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult(ParseJson(json)));

        var results = await _nws.GetWarningsAsync(35.0, -97.0, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetWarningsAsync_HttpRequestException_ReturnsEmpty()
    {
        A.CallTo(() => _client.GetAsync<JsonElement>(A<string>._, A<CancellationToken>._))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var results = await _nws.GetWarningsAsync(35.0, -97.0, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetWarningsAsync_WeatherApiException_ReturnsEmpty()
    {
        A.CallTo(() => _client.GetAsync<JsonElement>(A<string>._, A<CancellationToken>._))
            .ThrowsAsync(new WeatherApiException(System.Net.HttpStatusCode.ServiceUnavailable, "https://api.weather.gov/alerts"));

        var results = await _nws.GetWarningsAsync(35.0, -97.0, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetWarningsAsync_FallsBackToEventWhenHeadlineMissing()
    {
        var json = """
            {
              "features": [
                {
                  "properties": {
                    "event": "Winter Storm Watch",
                    "effective": "2026-02-01T10:00:00Z"
                  }
                }
              ]
            }
            """;
        A.CallTo(() => _client.GetAsync<JsonElement>(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult(ParseJson(json)));

        var results = await _nws.GetWarningsAsync(35.0, -97.0, CancellationToken.None);

        Assert.Single(results);
        Assert.Contains("Winter Storm Watch", results[0].Summary);
    }

    [Fact]
    public async Task GetWarningsAsync_UsesExpiresWhenEndsIsAbsent()
    {
        var json = """
            {
              "features": [
                {
                  "properties": {
                    "headline": "Flash Flood",
                    "effective": "2026-02-01T10:00:00Z",
                    "expires": "2026-02-01T20:00:00Z"
                  }
                }
              ]
            }
            """;
        A.CallTo(() => _client.GetAsync<JsonElement>(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult(ParseJson(json)));

        var results = await _nws.GetWarningsAsync(35.0, -97.0, CancellationToken.None);

        Assert.Single(results);
        Assert.NotNull(results[0].Ends);
        Assert.Equal(DateTimeOffset.Parse("2026-02-01T20:00:00Z"), results[0].Ends);
    }
}
