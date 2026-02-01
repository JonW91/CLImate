using System.Text.Json.Serialization;

namespace CLImate.App.Models;

public sealed class ForecastResponse
{
    [JsonPropertyName("daily")]
    public Daily? Daily { get; set; }

    [JsonPropertyName("daily_units")]
    public DailyUnits? DailyUnits { get; set; }
}

public sealed class Daily
{
    [JsonPropertyName("time")]
    public List<string> Time { get; set; } = new();

    [JsonPropertyName("weather_code")]
    public List<int> WeatherCode { get; set; } = new();

    [JsonPropertyName("temperature_2m_max")]
    public List<double> TemperatureMax { get; set; } = new();

    [JsonPropertyName("temperature_2m_min")]
    public List<double> TemperatureMin { get; set; } = new();

    [JsonPropertyName("precipitation_sum")]
    public List<double> PrecipitationSum { get; set; } = new();

    [JsonPropertyName("wind_speed_10m_max")]
    public List<double> WindSpeedMax { get; set; } = new();

    [JsonPropertyName("wind_gusts_10m_max")]
    public List<double> WindGustsMax { get; set; } = new();
}

public sealed class DailyUnits
{
    [JsonPropertyName("temperature_2m_max")]
    public string TemperatureMax { get; set; } = "°C";

    [JsonPropertyName("temperature_2m_min")]
    public string TemperatureMin { get; set; } = "°C";

    [JsonPropertyName("precipitation_sum")]
    public string PrecipitationSum { get; set; } = "mm";

    [JsonPropertyName("wind_speed_10m_max")]
    public string WindSpeedMax { get; set; } = "m/s";

    [JsonPropertyName("wind_gusts_10m_max")]
    public string WindGustsMax { get; set; } = "m/s";
}
