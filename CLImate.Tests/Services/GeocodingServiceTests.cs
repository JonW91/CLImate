using CLImate.App.Models;
using CLImate.App.Services;
using FakeItEasy;

namespace CLImate.Tests.Services;

public sealed class GeocodingServiceTests
{
    private readonly IJsonHttpClient _client;
    private readonly IApiMapper _mapper;
    private readonly GeocodingService _service;

    public GeocodingServiceTests()
    {
        _client = A.Fake<IJsonHttpClient>();
        _mapper = A.Fake<IApiMapper>();
        _service = new GeocodingService(_client, _mapper);

        A.CallTo(() => _mapper.MapGeocoding(A<GeocodeResponse?>._))
            .Returns(new List<GeoResult>());
    }

    [Fact]
    public async Task SearchAsync_WithoutCountryCode_UrlHasNoCountryCodeParam()
    {
        string? capturedUrl = null;
        A.CallTo(() => _client.GetAsync<GeocodeResponse>(A<string>._, A<CancellationToken>._))
            .Invokes(call => capturedUrl = call.GetArgument<string>(0))
            .Returns(Task.FromResult<GeocodeResponse?>(null));

        await _service.SearchAsync("London", null, CancellationToken.None);

        Assert.NotNull(capturedUrl);
        Assert.Contains("name=London", capturedUrl);
        Assert.DoesNotContain("countryCode", capturedUrl);
    }

    [Fact]
    public async Task SearchAsync_WithCountryCode_UrlIncludesCountryCodeParam()
    {
        string? capturedUrl = null;
        A.CallTo(() => _client.GetAsync<GeocodeResponse>(A<string>._, A<CancellationToken>._))
            .Invokes(call => capturedUrl = call.GetArgument<string>(0))
            .Returns(Task.FromResult<GeocodeResponse?>(null));

        await _service.SearchAsync("London", "GB", CancellationToken.None);

        Assert.NotNull(capturedUrl);
        Assert.Contains("countryCode=GB", capturedUrl);
    }

    [Fact]
    public async Task SearchAsync_SpecialCharsInLocation_AreUrlEncoded()
    {
        string? capturedUrl = null;
        A.CallTo(() => _client.GetAsync<GeocodeResponse>(A<string>._, A<CancellationToken>._))
            .Invokes(call => capturedUrl = call.GetArgument<string>(0))
            .Returns(Task.FromResult<GeocodeResponse?>(null));

        await _service.SearchAsync("São Paulo", null, CancellationToken.None);

        Assert.NotNull(capturedUrl);
        Assert.DoesNotContain("São Paulo", capturedUrl);
        Assert.Contains("S%C3%A3o", capturedUrl);
    }

    [Fact]
    public async Task SearchAsync_WithCountryCode_FiltersResultsByCountry()
    {
        var allResults = new List<GeoResult>
        {
            new GeoResult { Name = "Springfield", CountryCode = "US" },
            new GeoResult { Name = "Springfield", CountryCode = "GB" },
            new GeoResult { Name = "Springfield", CountryCode = "AU" }
        };
        A.CallTo(() => _client.GetAsync<GeocodeResponse>(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<GeocodeResponse?>(null));
        A.CallTo(() => _mapper.MapGeocoding(A<GeocodeResponse?>._)).Returns(allResults);

        var results = await _service.SearchAsync("Springfield", "US", CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("US", results[0].CountryCode);
    }

    [Fact]
    public async Task SearchAsync_WithCountryCode_FallsBackToAllResultsWhenNoneMatch()
    {
        var allResults = new List<GeoResult>
        {
            new GeoResult { Name = "Springfield", CountryCode = "AU" }
        };
        A.CallTo(() => _client.GetAsync<GeocodeResponse>(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<GeocodeResponse?>(null));
        A.CallTo(() => _mapper.MapGeocoding(A<GeocodeResponse?>._)).Returns(allResults);

        // "US" requested but only AU results returned — fall back to all results
        var results = await _service.SearchAsync("Springfield", "US", CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("AU", results[0].CountryCode);
    }
}
