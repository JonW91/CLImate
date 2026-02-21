using CLImate.App.Models;

namespace CLImate.App.Services;

public interface IApiMapper
{
    Forecast? MapForecast(ForecastResponse? response);
    List<GeoResult> MapGeocoding(GeocodeResponse? response);
}

public sealed class ApiMapper : IApiMapper
{
    public Forecast? MapForecast(ForecastResponse? response)
    {
        var daily = response?.Daily;
        if (daily == null || daily.Time.Count == 0)
        {
            return null;
        }

        var units = BuildUnits(response?.DailyUnits);
        var days = new List<DailyForecast>(daily.Time.Count);

        for (var i = 0; i < daily.Time.Count; i++)
        {
            var date = daily.Time[i];
            var code = SafeGetInt(daily.WeatherCode, i, 0);
            var max = SafeGetDouble(daily.TemperatureMax, i, double.NaN);
            var min = SafeGetDouble(daily.TemperatureMin, i, double.NaN);
            var precip = SafeGetDouble(daily.PrecipitationSum, i, double.NaN);
            var wind = SafeGetDouble(daily.WindSpeedMax, i, double.NaN);
            var gust = SafeGetDouble(daily.WindGustsMax, i, double.NaN);

            days.Add(new DailyForecast(date, code, max, min, precip, wind, gust));
        }

        var today = BuildTodayForecast(response?.Hourly, daily.Time[0]);
        var hourly = BuildHourlyForecast(response?.Hourly, daily.Time[0]);
        return new Forecast(days, units, today, hourly);
    }

    public List<GeoResult> MapGeocoding(GeocodeResponse? response)
        => response?.Results ?? new List<GeoResult>();

    private static ForecastUnits BuildUnits(DailyUnits? units)
    {
        if (units == null)
        {
            return new ForecastUnits("°C", "mm", "m/s", "m/s");
        }

        return new ForecastUnits(
            units.TemperatureMax,
            units.PrecipitationSum,
            units.WindSpeedMax,
            units.WindGustsMax);
    }

    private static int SafeGetInt(List<int>? list, int index, int fallback)
    {
        if (list == null || index >= list.Count) return fallback;
        return list[index];
    }

    private static double SafeGetDouble(List<double>? list, int index, double fallback)
    {
        if (list == null || index >= list.Count) return fallback;
        return list[index];
    }

    private static TodayForecast? BuildTodayForecast(Hourly? hourly, string? todayDate)
    {
        if (hourly == null || hourly.Time.Count == 0)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(todayDate) || !DateTime.TryParse(todayDate, out var today))
        {
            return null;
        }

        var segments = new[]
        {
            new SegmentAccumulator("Night", 0, 5),
            new SegmentAccumulator("Morning", 6, 11),
            new SegmentAccumulator("Afternoon", 12, 17),
            new SegmentAccumulator("Evening", 18, 23)
        };

        for (var i = 0; i < hourly.Time.Count; i++)
        {
            if (!DateTime.TryParse(hourly.Time[i], out var timestamp))
            {
                continue;
            }

            if (timestamp.Date != today.Date)
            {
                continue;
            }

            var hour = timestamp.Hour;
            var code = SafeGetInt(hourly.WeatherCode, i, 0);
            var temp = SafeGetDouble(hourly.Temperature, i, double.NaN);
            var precip = SafeGetDouble(hourly.Precipitation, i, double.NaN);
            var wind = SafeGetDouble(hourly.WindSpeed, i, double.NaN);
            var gust = SafeGetDouble(hourly.WindGusts, i, double.NaN);

            foreach (var segment in segments)
            {
                if (segment.Contains(hour))
                {
                    segment.Add(code, temp, precip, wind, gust);
                    break;
                }
            }
        }

        var dayParts = new List<DayPartForecast>(segments.Length);
        foreach (var segment in segments)
        {
            var part = segment.ToForecast();
            if (part != null)
            {
                dayParts.Add(part);
            }
        }

        return dayParts.Count == 0 ? null : new TodayForecast(todayDate, dayParts);
    }

    private sealed class SegmentAccumulator
    {
        private readonly Dictionary<int, int> _codeCounts = new();
        private readonly int _startHour;
        private readonly int _endHour;
        private int _totalCount;
        private int _tempCount;
        private int _precipCount;
        private double _tempSum;
        private double _precipSum;
        private double _windMax = double.NaN;
        private double _gustMax = double.NaN;

        public SegmentAccumulator(string label, int startHour, int endHour)
        {
            Label = label;
            _startHour = startHour;
            _endHour = endHour;
        }

        public string Label { get; }

        public bool Contains(int hour) => hour >= _startHour && hour <= _endHour;

        public void Add(int code, double temp, double precip, double wind, double gust)
        {
            _totalCount++;

            if (_codeCounts.TryGetValue(code, out var count))
            {
                _codeCounts[code] = count + 1;
            }
            else
            {
                _codeCounts[code] = 1;
            }

            if (!double.IsNaN(temp))
            {
                _tempSum += temp;
                _tempCount++;
            }

            if (!double.IsNaN(precip))
            {
                _precipSum += precip;
                _precipCount++;
            }

            if (!double.IsNaN(wind))
            {
                _windMax = double.IsNaN(_windMax) ? wind : Math.Max(_windMax, wind);
            }

            if (!double.IsNaN(gust))
            {
                _gustMax = double.IsNaN(_gustMax) ? gust : Math.Max(_gustMax, gust);
            }
        }

        public DayPartForecast? ToForecast()
        {
            if (_totalCount == 0)
            {
                return null;
            }

            var tempAvg = _tempCount == 0 ? double.NaN : _tempSum / _tempCount;
            var precipSum = _precipCount == 0 ? double.NaN : _precipSum;
            var code = MostCommonCode();

            return new DayPartForecast(Label, code, tempAvg, precipSum, _windMax, _gustMax);
        }

        private int MostCommonCode()
        {
            var bestCode = 0;
            var bestCount = -1;

            foreach (var pair in _codeCounts)
            {
                if (pair.Value > bestCount)
                {
                    bestCount = pair.Value;
                    bestCode = pair.Key;
                }
            }

            return bestCode;
        }
    }

    private static HourlyForecast? BuildHourlyForecast(Hourly? hourly, string? todayDate)
    {
        if (hourly == null || hourly.Time.Count == 0)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(todayDate) || !DateTime.TryParse(todayDate, out var today))
        {
            return null;
        }

        var hours = new List<HourForecast>();

        for (var i = 0; i < hourly.Time.Count; i++)
        {
            if (!DateTime.TryParse(hourly.Time[i], out var timestamp))
            {
                continue;
            }

            if (timestamp.Date != today.Date)
            {
                continue;
            }

            var code = SafeGetInt(hourly.WeatherCode, i, 0);
            var temp = SafeGetDouble(hourly.Temperature, i, double.NaN);
            var precip = SafeGetDouble(hourly.Precipitation, i, double.NaN);
            var wind = SafeGetDouble(hourly.WindSpeed, i, double.NaN);
            var gust = SafeGetDouble(hourly.WindGusts, i, double.NaN);

            hours.Add(new HourForecast(timestamp, code, temp, precip, wind, gust));
        }

        return hours.Count == 0 ? null : new HourlyForecast(todayDate, hours);
    }
}
