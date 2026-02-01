using CLImate.App.Models;
using CLImate.App.Services;
using FakeItEasy;

namespace CLImate.Tests.Services;

public sealed class WeatherWarningsServiceTests
{
    [Fact]
    public async Task GetDailyWarningsAsync_NoWarnings_ReturnsNoneForAllDates()
    {
        var client = A.Fake<IMeteoBlueWarningsClient>();
        A.CallTo(() => client.GetWarningsAsync(A<double>._, A<double>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IReadOnlyList<WeatherWarning>>(Array.Empty<WeatherWarning>()));

        var service = new WeatherWarningsService(client);
        var dates = new List<string> { "2026-02-01", "2026-02-02", "2026-02-03" };

        var result = await service.GetDailyWarningsAsync(51.5, -0.1, dates, CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal("none", result["2026-02-01"]);
        Assert.Equal("none", result["2026-02-02"]);
        Assert.Equal("none", result["2026-02-03"]);
    }

    [Fact]
    public async Task GetDailyWarningsAsync_SingleDayWarning_AssignsSummaryToMatchingDate()
    {
        var client = A.Fake<IMeteoBlueWarningsClient>();
        var warnings = new List<WeatherWarning>
        {
            new WeatherWarning("Heavy rain", DateTimeOffset.Parse("2026-02-02T00:00:00Z"), DateTimeOffset.Parse("2026-02-02T23:59:59Z"))
        };
        A.CallTo(() => client.GetWarningsAsync(A<double>._, A<double>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IReadOnlyList<WeatherWarning>>(warnings));

        var service = new WeatherWarningsService(client);
        var dates = new List<string> { "2026-02-01", "2026-02-02", "2026-02-03" };

        var result = await service.GetDailyWarningsAsync(51.5, -0.1, dates, CancellationToken.None);

        Assert.Equal("none", result["2026-02-01"]);
        Assert.Equal("Heavy rain", result["2026-02-02"]);
        Assert.Equal("none", result["2026-02-03"]);
    }

    [Fact]
    public async Task GetDailyWarningsAsync_MultiDayWarning_SpansDatesCorrectly()
    {
        var client = A.Fake<IMeteoBlueWarningsClient>();
        var warnings = new List<WeatherWarning>
        {
            new WeatherWarning("Storm", DateTimeOffset.Parse("2026-02-02T00:00:00Z"), DateTimeOffset.Parse("2026-02-04T00:00:00Z"))
        };
        A.CallTo(() => client.GetWarningsAsync(A<double>._, A<double>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IReadOnlyList<WeatherWarning>>(warnings));

        var service = new WeatherWarningsService(client);
        var dates = new List<string> { "2026-02-01", "2026-02-02", "2026-02-03", "2026-02-04", "2026-02-05" };

        var result = await service.GetDailyWarningsAsync(51.5, -0.1, dates, CancellationToken.None);

        Assert.Equal("none", result["2026-02-01"]);
        Assert.Equal("Storm", result["2026-02-02"]);
        Assert.Equal("Storm", result["2026-02-03"]);
        Assert.Equal("Storm", result["2026-02-04"]);
        Assert.Equal("none", result["2026-02-05"]);
    }

    [Fact]
    public async Task GetDailyWarningsAsync_MultipleWarningsOnSameDay_CombinesSummaries()
    {
        var client = A.Fake<IMeteoBlueWarningsClient>();
        var warnings = new List<WeatherWarning>
        {
            new WeatherWarning("Heavy rain", DateTimeOffset.Parse("2026-02-02T00:00:00Z"), DateTimeOffset.Parse("2026-02-02T23:59:59Z")),
            new WeatherWarning("Strong winds", DateTimeOffset.Parse("2026-02-02T00:00:00Z"), DateTimeOffset.Parse("2026-02-02T23:59:59Z"))
        };
        A.CallTo(() => client.GetWarningsAsync(A<double>._, A<double>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IReadOnlyList<WeatherWarning>>(warnings));

        var service = new WeatherWarningsService(client);
        var dates = new List<string> { "2026-02-01", "2026-02-02", "2026-02-03" };

        var result = await service.GetDailyWarningsAsync(51.5, -0.1, dates, CancellationToken.None);

        Assert.Contains("Heavy rain", result["2026-02-02"]);
        Assert.Contains("Strong winds", result["2026-02-02"]);
        Assert.Contains(";", result["2026-02-02"]);
    }

    [Fact]
    public async Task GetDailyWarningsAsync_WarningWithNullStart_IsIgnored()
    {
        var client = A.Fake<IMeteoBlueWarningsClient>();
        var warnings = new List<WeatherWarning>
        {
            new WeatherWarning("Unknown event", null, null)
        };
        A.CallTo(() => client.GetWarningsAsync(A<double>._, A<double>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IReadOnlyList<WeatherWarning>>(warnings));

        var service = new WeatherWarningsService(client);
        var dates = new List<string> { "2026-02-01", "2026-02-02" };

        var result = await service.GetDailyWarningsAsync(51.5, -0.1, dates, CancellationToken.None);

        Assert.Equal("none", result["2026-02-01"]);
        Assert.Equal("none", result["2026-02-02"]);
    }

    [Fact]
    public async Task GetDailyWarningsAsync_WarningStartsOnlyNoEnd_UsesStartDateOnly()
    {
        var client = A.Fake<IMeteoBlueWarningsClient>();
        var warnings = new List<WeatherWarning>
        {
            new WeatherWarning("Flash event", DateTimeOffset.Parse("2026-02-02T12:00:00Z"), null)
        };
        A.CallTo(() => client.GetWarningsAsync(A<double>._, A<double>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IReadOnlyList<WeatherWarning>>(warnings));

        var service = new WeatherWarningsService(client);
        var dates = new List<string> { "2026-02-01", "2026-02-02", "2026-02-03" };

        var result = await service.GetDailyWarningsAsync(51.5, -0.1, dates, CancellationToken.None);

        Assert.Equal("none", result["2026-02-01"]);
        Assert.Equal("Flash event", result["2026-02-02"]);
        Assert.Equal("none", result["2026-02-03"]);
    }
}
