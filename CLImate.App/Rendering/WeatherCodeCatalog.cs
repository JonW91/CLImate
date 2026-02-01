namespace CLImate.App.Rendering;

public interface IWeatherCodeCatalog
{
    WeatherDescriptor Describe(int code);
}

public sealed class WeatherCodeCatalog : IWeatherCodeCatalog
{
    public WeatherDescriptor Describe(int code)
    {
        return code switch
        {
            0 => new WeatherDescriptor("Clear sky", "clear", AnsiColor.Yellow),
            1 or 2 => new WeatherDescriptor("Mainly clear, partly cloudy", "partly_cloudy", AnsiColor.Yellow),
            3 => new WeatherDescriptor("Overcast", "overcast", AnsiColor.Gray),
            45 or 48 => new WeatherDescriptor("Fog", "fog", AnsiColor.Gray),
            51 or 53 or 55 => new WeatherDescriptor("Drizzle", "drizzle", AnsiColor.Blue),
            56 or 57 => new WeatherDescriptor("Freezing drizzle", "freezing_drizzle", AnsiColor.Blue),
            61 or 63 or 65 => new WeatherDescriptor("Rain", "rain", AnsiColor.DarkGray),
            66 or 67 => new WeatherDescriptor("Freezing rain", "freezing_rain", AnsiColor.DarkGray),
            71 or 73 or 75 => new WeatherDescriptor("Snow", "snow", AnsiColor.White),
            77 => new WeatherDescriptor("Snow grains", "snow_grains", AnsiColor.White),
            80 or 81 or 82 => new WeatherDescriptor("Rain showers", "rain_showers", AnsiColor.DarkGray),
            85 or 86 => new WeatherDescriptor("Snow showers", "snow_showers", AnsiColor.White),
            95 => new WeatherDescriptor("Thunderstorm", "thunderstorm", AnsiColor.DarkGray),
            96 or 99 => new WeatherDescriptor("Thunderstorm with hail", "thunderstorm_hail", AnsiColor.DarkGray),
            _ => new WeatherDescriptor("Unknown", "unknown", AnsiColor.Default)
        };
    }
}

public sealed record WeatherDescriptor(string Description, string ArtKey, AnsiColor ArtColor);
