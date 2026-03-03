using CLImate.App.Models;
using CLImate.App.Services;

namespace CLImate.Tests.Services;

public sealed class ApiMapperTests
{
    private readonly ApiMapper _mapper = new();

    #region MapForecast Tests

    [Fact]
    public void MapForecast_NullResponse_ReturnsNull()
    {
        var result = _mapper.MapForecast(null);
        
        Assert.Null(result);
    }

    [Fact]
    public void MapForecast_NullDaily_ReturnsNull()
    {
        var response = new ForecastResponse
        {
            Daily = null
        };

        var result = _mapper.MapForecast(response);
        
        Assert.Null(result);
    }

    [Fact]
    public void MapForecast_EmptyTimeList_ReturnsNull()
    {
        var response = new ForecastResponse
        {
            Daily = new Daily
            {
                Time = new List<string>()
            }
        };

        var result = _mapper.MapForecast(response);
        
        Assert.Null(result);
    }

    [Fact]
    public void MapForecast_ValidMinimalResponse_CreatesForecast()
    {
        var response = new ForecastResponse
        {
            Daily = new Daily
            {
                Time = new List<string> { "2026-02-01" },
                WeatherCode = new List<int> { 1 },
                TemperatureMax = new List<double> { 20.5 },
                TemperatureMin = new List<double> { 10.2 },
                PrecipitationSum = new List<double> { 0.0 },
                WindSpeedMax = new List<double> { 5.3 },
                WindGustsMax = new List<double> { 8.1 }
            },
            DailyUnits = new DailyUnits
            {
                TemperatureMax = "°C",
                PrecipitationSum = "mm",
                WindSpeedMax = "m/s",
                WindGustsMax = "m/s"
            }
        };

        var result = _mapper.MapForecast(response);

        Assert.NotNull(result);
        Assert.Single(result.Days);
        
        var day = result.Days[0];
        Assert.Equal("2026-02-01", day.Date);
        Assert.Equal(1, day.WeatherCode);
        Assert.Equal(20.5, day.TemperatureMax);
        Assert.Equal(10.2, day.TemperatureMin);
        Assert.Equal(0.0, day.PrecipitationSum);
        Assert.Equal(5.3, day.WindSpeedMax);
        Assert.Equal(8.1, day.WindGustsMax);
    }

    [Fact]
    public void MapForecast_MultipleDays_CreatesAllDailyForecasts()
    {
        var response = new ForecastResponse
        {
            Daily = new Daily
            {
                Time = new List<string> { "2026-02-01", "2026-02-02", "2026-02-03" },
                WeatherCode = new List<int> { 1, 2, 3 },
                TemperatureMax = new List<double> { 20.5, 18.3, 22.1 },
                TemperatureMin = new List<double> { 10.2, 8.7, 12.4 },
                PrecipitationSum = new List<double> { 0.0, 2.5, 0.1 },
                WindSpeedMax = new List<double> { 5.3, 7.8, 4.2 },
                WindGustsMax = new List<double> { 8.1, 12.3, 6.9 }
            }
        };

        var result = _mapper.MapForecast(response);

        Assert.NotNull(result);
        Assert.Equal(3, result.Days.Count);
        
        Assert.Equal("2026-02-01", result.Days[0].Date);
        Assert.Equal("2026-02-02", result.Days[1].Date);
        Assert.Equal("2026-02-03", result.Days[2].Date);
        
        Assert.Equal(2, result.Days[1].WeatherCode);
        Assert.Equal(18.3, result.Days[1].TemperatureMax);
    }

    [Fact]
    public void MapForecast_MissingDataAtIndex_UsesFallbackValues()
    {
        var response = new ForecastResponse
        {
            Daily = new Daily
            {
                Time = new List<string> { "2026-02-01", "2026-02-02" },
                WeatherCode = new List<int> { 1 }, // Missing second value
                TemperatureMax = new List<double> { 20.5 }, // Missing second value
                TemperatureMin = new List<double> { 10.2, 8.7 },
                PrecipitationSum = new List<double> { 0.0, 2.5 },
                WindSpeedMax = new List<double> { 5.3, 7.8 },
                WindGustsMax = new List<double> { 8.1, 12.3 }
            }
        };

        var result = _mapper.MapForecast(response);

        Assert.NotNull(result);
        Assert.Equal(2, result.Days.Count);
        
        var secondDay = result.Days[1];
        Assert.Equal(0, secondDay.WeatherCode); // Fallback for int
        Assert.Equal(double.NaN, secondDay.TemperatureMax); // Fallback for double
        Assert.Equal(8.7, secondDay.TemperatureMin); // Present value
    }

    [Fact]
    public void MapForecast_NullDailyUnits_UsesDefaultUnits()
    {
        var response = new ForecastResponse
        {
            Daily = new Daily
            {
                Time = new List<string> { "2026-02-01" },
                WeatherCode = new List<int> { 1 },
                TemperatureMax = new List<double> { 20.5 },
                TemperatureMin = new List<double> { 10.2 },
                PrecipitationSum = new List<double> { 0.0 },
                WindSpeedMax = new List<double> { 5.3 },
                WindGustsMax = new List<double> { 8.1 }
            },
            DailyUnits = null
        };

        var result = _mapper.MapForecast(response);

        Assert.NotNull(result);
        Assert.Equal("°C", result.Units.Temperature);
        Assert.Equal("mm", result.Units.Precipitation);
        Assert.Equal("m/s", result.Units.WindSpeed);
        Assert.Equal("m/s", result.Units.WindGusts);
    }

    [Fact]
    public void MapForecast_CustomUnits_MapsUnitsCorrectly()
    {
        var response = new ForecastResponse
        {
            Daily = new Daily
            {
                Time = new List<string> { "2026-02-01" },
                WeatherCode = new List<int> { 1 },
                TemperatureMax = new List<double> { 68.9 },
                TemperatureMin = new List<double> { 50.4 },
                PrecipitationSum = new List<double> { 0.0 },
                WindSpeedMax = new List<double> { 11.9 },
                WindGustsMax = new List<double> { 18.1 }
            },
            DailyUnits = new DailyUnits
            {
                TemperatureMax = "°F",
                PrecipitationSum = "in",
                WindSpeedMax = "mph",
                WindGustsMax = "mph"
            }
        };

        var result = _mapper.MapForecast(response);

        Assert.NotNull(result);
        Assert.Equal("°F", result.Units.Temperature);
        Assert.Equal("in", result.Units.Precipitation);
        Assert.Equal("mph", result.Units.WindSpeed);
        Assert.Equal("mph", result.Units.WindGusts);
    }

    [Fact]
    public void MapForecast_WithValidHourlyData_BuildsTodayForecast()
    {
        var todayDate = "2026-02-01";
        var response = new ForecastResponse
        {
            Daily = new Daily
            {
                Time = new List<string> { todayDate },
                WeatherCode = new List<int> { 1 },
                TemperatureMax = new List<double> { 20.5 },
                TemperatureMin = new List<double> { 10.2 },
                PrecipitationSum = new List<double> { 0.0 },
                WindSpeedMax = new List<double> { 5.3 },
                WindGustsMax = new List<double> { 8.1 }
            },
            Hourly = new Hourly
            {
                Time = new List<string> 
                { 
                    "2026-02-01T00:00:00",
                    "2026-02-01T06:00:00",
                    "2026-02-01T12:00:00",
                    "2026-02-01T18:00:00"
                },
                WeatherCode = new List<int> { 0, 1, 2, 1 },
                Temperature = new List<double> { 12.0, 15.0, 20.0, 18.0 },
                Precipitation = new List<double> { 0.0, 0.0, 0.1, 0.0 },
                WindSpeed = new List<double> { 3.0, 4.0, 5.0, 4.5 },
                WindGusts = new List<double> { 5.0, 6.0, 8.0, 7.0 }
            }
        };

        var result = _mapper.MapForecast(response);

        Assert.NotNull(result);
        Assert.NotNull(result.Today);
        Assert.Equal(todayDate, result.Today.Date);
        Assert.NotEmpty(result.Today.Segments);
    }

    [Fact]
    public void MapForecast_WithValidHourlyData_BuildsHourlyForecast()
    {
        var todayDate = "2026-02-01";
        var response = new ForecastResponse
        {
            Daily = new Daily
            {
                Time = new List<string> { todayDate },
                WeatherCode = new List<int> { 1 },
                TemperatureMax = new List<double> { 20.5 },
                TemperatureMin = new List<double> { 10.2 },
                PrecipitationSum = new List<double> { 0.0 },
                WindSpeedMax = new List<double> { 5.3 },
                WindGustsMax = new List<double> { 8.1 }
            },
            Hourly = new Hourly
            {
                Time = new List<string> 
                { 
                    "2026-02-01T00:00:00",
                    "2026-02-01T01:00:00"
                },
                WeatherCode = new List<int> { 0, 1 },
                Temperature = new List<double> { 12.0, 13.0 },
                Precipitation = new List<double> { 0.0, 0.0 },
                WindSpeed = new List<double> { 3.0, 3.5 },
                WindGusts = new List<double> { 5.0, 6.0 }
            }
        };

        var result = _mapper.MapForecast(response);

        Assert.NotNull(result);
        Assert.NotNull(result.Hourly);
        Assert.Equal(todayDate, result.Hourly.Date);
        Assert.Equal(2, result.Hourly.Hours.Count);
    }

    #endregion

    #region MapGeocoding Tests

    [Fact]
    public void MapGeocoding_NullResponse_ReturnsEmptyList()
    {
        var result = _mapper.MapGeocoding(null);
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void MapGeocoding_NullResults_ReturnsEmptyList()
    {
        var response = new GeocodeResponse
        {
            Results = null
        };

        var result = _mapper.MapGeocoding(response);
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void MapGeocoding_EmptyResults_ReturnsEmptyList()
    {
        var response = new GeocodeResponse
        {
            Results = new List<GeoResult>()
        };

        var result = _mapper.MapGeocoding(response);
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void MapGeocoding_ValidResults_ReturnsSameList()
    {
        var geoResults = new List<GeoResult>
        {
            new GeoResult
            {
                Name = "London",
                Admin1 = "England",
                Country = "United Kingdom",
                CountryCode = "GB",
                Latitude = 51.5074,
                Longitude = -0.1278,
                Timezone = "Europe/London"
            },
            new GeoResult
            {
                Name = "New York",
                Admin1 = "New York",
                Country = "United States",
                CountryCode = "US",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Timezone = "America/New_York"
            }
        };

        var response = new GeocodeResponse
        {
            Results = geoResults
        };

        var result = _mapper.MapGeocoding(response);
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("London", result[0].Name);
        Assert.Equal("New York", result[1].Name);
        Assert.Equal(51.5074, result[0].Latitude);
        Assert.Equal(-74.0060, result[1].Longitude);
    }

    [Fact]
    public void MapGeocoding_ResultsWithNullValues_PreservesNulls()
    {
        var geoResults = new List<GeoResult>
        {
            new GeoResult
            {
                Name = "Unknown Place",
                Admin1 = null,
                Country = null,
                CountryCode = null,
                Latitude = null,
                Longitude = null,
                Timezone = null
            }
        };

        var response = new GeocodeResponse
        {
            Results = geoResults
        };

        var result = _mapper.MapGeocoding(response);
        
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Unknown Place", result[0].Name);
        Assert.Null(result[0].Admin1);
        Assert.Null(result[0].Country);
        Assert.Null(result[0].CountryCode);
        Assert.Null(result[0].Latitude);
        Assert.Null(result[0].Longitude);
        Assert.Null(result[0].Timezone);
    }

    #endregion

    #region TodayForecast Integration Tests

    [Fact]
    public void MapForecast_HourlyDataSpanningFullDay_CreatesFourDaySegments()
    {
        var todayDate = "2026-02-01";
        var response = new ForecastResponse
        {
            Daily = new Daily
            {
                Time = new List<string> { todayDate },
                WeatherCode = new List<int> { 1 },
                TemperatureMax = new List<double> { 20.5 },
                TemperatureMin = new List<double> { 10.2 },
                PrecipitationSum = new List<double> { 0.0 },
                WindSpeedMax = new List<double> { 5.3 },
                WindGustsMax = new List<double> { 8.1 }
            },
            Hourly = new Hourly
            {
                Time = new List<string> 
                { 
                    "2026-02-01T02:00:00", // Night
                    "2026-02-01T08:00:00", // Morning
                    "2026-02-01T14:00:00", // Afternoon
                    "2026-02-01T20:00:00"  // Evening
                },
                WeatherCode = new List<int> { 0, 1, 2, 1 },
                Temperature = new List<double> { 12.0, 15.0, 20.0, 18.0 },
                Precipitation = new List<double> { 0.0, 0.0, 0.1, 0.0 },
                WindSpeed = new List<double> { 3.0, 4.0, 5.0, 4.5 },
                WindGusts = new List<double> { 5.0, 6.0, 8.0, 7.0 }
            }
        };

        var result = _mapper.MapForecast(response);

        Assert.NotNull(result);
        Assert.NotNull(result.Today);
        Assert.Equal(4, result.Today.Segments.Count);
        
        var segments = result.Today.Segments;
        Assert.Equal("Night", segments[0].Label);
        Assert.Equal("Morning", segments[1].Label);
        Assert.Equal("Afternoon", segments[2].Label);
        Assert.Equal("Evening", segments[3].Label);
    }

    [Fact]
    public void MapForecast_HourlyDataDifferentDate_ExcludesFromTodayForecast()
    {
        var todayDate = "2026-02-01";
        var response = new ForecastResponse
        {
            Daily = new Daily
            {
                Time = new List<string> { todayDate },
                WeatherCode = new List<int> { 1 },
                TemperatureMax = new List<double> { 20.5 },
                TemperatureMin = new List<double> { 10.2 },
                PrecipitationSum = new List<double> { 0.0 },
                WindSpeedMax = new List<double> { 5.3 },
                WindGustsMax = new List<double> { 8.1 }
            },
            Hourly = new Hourly
            {
                Time = new List<string> 
                { 
                    "2026-02-02T08:00:00" // Different date
                },
                WeatherCode = new List<int> { 1 },
                Temperature = new List<double> { 15.0 },
                Precipitation = new List<double> { 0.0 },
                WindSpeed = new List<double> { 4.0 },
                WindGusts = new List<double> { 6.0 }
            }
        };

        var result = _mapper.MapForecast(response);

        Assert.NotNull(result);
        Assert.Null(result.Today); // Should be null because no hourly data matches today's date
    }

    [Fact]
    public void MapForecast_InvalidHourlyTimeFormat_SkipsInvalidEntries()
    {
        var todayDate = "2026-02-01";
        var response = new ForecastResponse
        {
            Daily = new Daily
            {
                Time = new List<string> { todayDate },
                WeatherCode = new List<int> { 1 },
                TemperatureMax = new List<double> { 20.5 },
                TemperatureMin = new List<double> { 10.2 },
                PrecipitationSum = new List<double> { 0.0 },
                WindSpeedMax = new List<double> { 5.3 },
                WindGustsMax = new List<double> { 8.1 }
            },
            Hourly = new Hourly
            {
                Time = new List<string> 
                { 
                    "invalid-time-format",
                    "2026-02-01T08:00:00" // Valid
                },
                WeatherCode = new List<int> { 1, 2 },
                Temperature = new List<double> { 15.0, 18.0 },
                Precipitation = new List<double> { 0.0, 0.1 },
                WindSpeed = new List<double> { 4.0, 5.0 },
                WindGusts = new List<double> { 6.0, 7.0 }
            }
        };

        var result = _mapper.MapForecast(response);

        Assert.NotNull(result);
        Assert.NotNull(result.Today);
        // Should only process the valid hourly entry
        Assert.Single(result.Today.Segments); // Only morning segment
        Assert.Equal("Morning", result.Today.Segments[0].Label);
    }

    [Fact]
    public void MapForecast_InvalidTodayDateFormat_ReturnsNullTodayForecast()
    {
        var response = new ForecastResponse
        {
            Daily = new Daily
            {
                Time = new List<string> { "invalid-date-format" },
                WeatherCode = new List<int> { 1 },
                TemperatureMax = new List<double> { 20.5 },
                TemperatureMin = new List<double> { 10.2 },
                PrecipitationSum = new List<double> { 0.0 },
                WindSpeedMax = new List<double> { 5.3 },
                WindGustsMax = new List<double> { 8.1 }
            },
            Hourly = new Hourly
            {
                Time = new List<string> { "2026-02-01T08:00:00" },
                WeatherCode = new List<int> { 1 },
                Temperature = new List<double> { 15.0 },
                Precipitation = new List<double> { 0.0 },
                WindSpeed = new List<double> { 4.0 },
                WindGusts = new List<double> { 6.0 }
            }
        };

        var result = _mapper.MapForecast(response);

        Assert.NotNull(result);
        Assert.Single(result.Days); // Daily forecast should still work
        Assert.Null(result.Today); // Today forecast should be null due to invalid date
        Assert.Null(result.Hourly); // Hourly forecast should be null due to invalid date
    }

    #endregion
}