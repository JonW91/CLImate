using CLImate.App.Models;
using CLImate.App.Services;
using FakeItEasy;

namespace CLImate.Tests.Services;

public sealed class WeatherWarningsServiceTests
{
    [Fact]
    public async Task GetDailyWarningsAsync_NoWarnings_ReturnsNoneForAllDates()
    {
        var nws = A.Fake<INwsWarningsClient>();
        var meteo = A.Fake<IMeteoalarmWarningsClient>();
        A.CallTo(() => nws.GetWarningsAsync(A<double>._, A<double>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IReadOnlyList<WeatherWarning>>(Array.Empty<WeatherWarning>()));

        var service = new WeatherWarningsService(nws, meteo);
        var dates = new List<string> { "2026-02-01", "2026-02-02", "2026-02-03" };

        var result = await service.GetDailyWarningsAsync(51.5, -0.1, "US", dates, CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal(WarningStatus.None, result["2026-02-01"].Status);
        Assert.Equal(WarningStatus.None, result["2026-02-02"].Status);
        Assert.Equal(WarningStatus.None, result["2026-02-03"].Status);
    }

    [Fact]
    public async Task GetDailyWarningsAsync_SingleDayWarning_AssignsSummaryToMatchingDate()
    {
        var nws = A.Fake<INwsWarningsClient>();
        var meteo = A.Fake<IMeteoalarmWarningsClient>();
        var warnings = new List<WeatherWarning>
        {
            new WeatherWarning("Heavy rain", DateTimeOffset.Parse("2026-02-02T00:00:00Z"), DateTimeOffset.Parse("2026-02-02T23:59:59Z"))
        };
        A.CallTo(() => nws.GetWarningsAsync(A<double>._, A<double>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IReadOnlyList<WeatherWarning>>(warnings));

        var service = new WeatherWarningsService(nws, meteo);
        var dates = new List<string> { "2026-02-01", "2026-02-02", "2026-02-03" };

        var result = await service.GetDailyWarningsAsync(51.5, -0.1, "US", dates, CancellationToken.None);

        Assert.False(result["2026-02-01"].IsActive);
        Assert.True(result["2026-02-02"].IsActive);
        Assert.Equal("Heavy rain", result["2026-02-02"].Text);
        Assert.False(result["2026-02-03"].IsActive);
    }

    [Fact]
    public async Task GetDailyWarningsAsync_MultiDayWarning_SpansDatesCorrectly()
    {
        var nws = A.Fake<INwsWarningsClient>();
        var meteo = A.Fake<IMeteoalarmWarningsClient>();
        var warnings = new List<WeatherWarning>
        {
            new WeatherWarning("Storm", DateTimeOffset.Parse("2026-02-02T00:00:00Z"), DateTimeOffset.Parse("2026-02-04T00:00:00Z"))
        };
        A.CallTo(() => nws.GetWarningsAsync(A<double>._, A<double>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IReadOnlyList<WeatherWarning>>(warnings));

        var service = new WeatherWarningsService(nws, meteo);
        var dates = new List<string> { "2026-02-01", "2026-02-02", "2026-02-03", "2026-02-04", "2026-02-05" };

        var result = await service.GetDailyWarningsAsync(51.5, -0.1, "US", dates, CancellationToken.None);

        Assert.False(result["2026-02-01"].IsActive);
        Assert.Equal("Storm", result["2026-02-02"].Text);
        Assert.Equal("Storm", result["2026-02-03"].Text);
        Assert.Equal("Storm", result["2026-02-04"].Text);
        Assert.False(result["2026-02-05"].IsActive);
    }

    [Fact]
    public async Task GetDailyWarningsAsync_MultipleWarningsOnSameDay_CombinesSummaries()
    {
        var nws = A.Fake<INwsWarningsClient>();
        var meteo = A.Fake<IMeteoalarmWarningsClient>();
        var warnings = new List<WeatherWarning>
        {
            new WeatherWarning("Heavy rain", DateTimeOffset.Parse("2026-02-02T00:00:00Z"), DateTimeOffset.Parse("2026-02-02T23:59:59Z")),
            new WeatherWarning("Strong winds", DateTimeOffset.Parse("2026-02-02T00:00:00Z"), DateTimeOffset.Parse("2026-02-02T23:59:59Z"))
        };
        A.CallTo(() => nws.GetWarningsAsync(A<double>._, A<double>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IReadOnlyList<WeatherWarning>>(warnings));

        var service = new WeatherWarningsService(nws, meteo);
        var dates = new List<string> { "2026-02-01", "2026-02-02", "2026-02-03" };

        var result = await service.GetDailyWarningsAsync(51.5, -0.1, "US", dates, CancellationToken.None);

        Assert.True(result["2026-02-02"].IsActive);
        Assert.Contains("Heavy rain", result["2026-02-02"].Text);
        Assert.Contains("Strong winds", result["2026-02-02"].Text);
        Assert.Contains(";", result["2026-02-02"].Text);
    }

    [Fact]
    public async Task GetDailyWarningsAsync_WarningWithNullStart_IsIgnored()
    {
        var nws = A.Fake<INwsWarningsClient>();
        var meteo = A.Fake<IMeteoalarmWarningsClient>();
        var warnings = new List<WeatherWarning>
        {
            new WeatherWarning("Unknown event", null, null)
        };
        A.CallTo(() => nws.GetWarningsAsync(A<double>._, A<double>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IReadOnlyList<WeatherWarning>>(warnings));

        var service = new WeatherWarningsService(nws, meteo);
        var dates = new List<string> { "2026-02-01", "2026-02-02" };

        var result = await service.GetDailyWarningsAsync(51.5, -0.1, "US", dates, CancellationToken.None);

        Assert.False(result["2026-02-01"].IsActive);
        Assert.False(result["2026-02-02"].IsActive);
    }

    [Fact]
    public async Task GetDailyWarningsAsync_WarningStartsOnlyNoEnd_UsesStartDateOnly()
    {
        var nws = A.Fake<INwsWarningsClient>();
        var meteo = A.Fake<IMeteoalarmWarningsClient>();
        var warnings = new List<WeatherWarning>
        {
            new WeatherWarning("Flash event", DateTimeOffset.Parse("2026-02-02T12:00:00Z"), null)
        };
        A.CallTo(() => nws.GetWarningsAsync(A<double>._, A<double>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IReadOnlyList<WeatherWarning>>(warnings));

        var service = new WeatherWarningsService(nws, meteo);
        var dates = new List<string> { "2026-02-01", "2026-02-02", "2026-02-03" };

        var result = await service.GetDailyWarningsAsync(51.5, -0.1, "US", dates, CancellationToken.None);

        Assert.False(result["2026-02-01"].IsActive);
        Assert.Equal("Flash event", result["2026-02-02"].Text);
        Assert.False(result["2026-02-03"].IsActive);
    }

    [Fact]
    public async Task GetDailyWarningsAsync_NonCoveredCountry_ReturnsRegionalUnavailable()
    {
        var nws = A.Fake<INwsWarningsClient>();
        var meteo = A.Fake<IMeteoalarmWarningsClient>();

        var service = new WeatherWarningsService(nws, meteo);
        var dates = new List<string> { "2026-02-01" };

        var result = await service.GetDailyWarningsAsync(51.5, -0.1, "IN", dates, CancellationToken.None);

        Assert.Equal(WarningStatus.RegionalUnavailable, result["2026-02-01"].Status);
        Assert.False(result["2026-02-01"].IsActive);
    }

    [Fact]
    public async Task GetDailyWarningsAsync_EuCountry_UsesMeteoalarm()
    {
        var nws = A.Fake<INwsWarningsClient>();
        var meteo = A.Fake<IMeteoalarmWarningsClient>();
        var warnings = new List<WeatherWarning>
        {
            new WeatherWarning("EU alert", DateTimeOffset.Parse("2026-02-02T00:00:00Z"), DateTimeOffset.Parse("2026-02-02T23:59:59Z"))
        };
        A.CallTo(() => meteo.GetWarningsAsync(A<double>._, A<double>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IReadOnlyList<WeatherWarning>>(warnings));

        var service = new WeatherWarningsService(nws, meteo);
        var dates = new List<string> { "2026-02-02" };

        var result = await service.GetDailyWarningsAsync(51.5, -0.1, "DE", dates, CancellationToken.None);

        Assert.Equal("EU alert", result["2026-02-02"].Text);
    }
}
