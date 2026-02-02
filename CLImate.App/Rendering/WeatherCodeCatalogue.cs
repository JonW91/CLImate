namespace CLImate.App.Rendering;

public interface IWeatherCodeCatalogue
{
    WeatherDescriptor Describe(int code);
}

public sealed class WeatherCodeCatalogue : IWeatherCodeCatalogue
{
    public WeatherDescriptor Describe(int code)
    {
        return code switch
        {
            0 => new WeatherDescriptor("Clear sky", "clear", AnsiColour.Yellow),
            1 or 2 => new WeatherDescriptor("Mainly clear, partly cloudy", "partly_cloudy", AnsiColour.Yellow),
            3 => new WeatherDescriptor("Overcast", "overcast", AnsiColour.Grey),
            45 or 48 => new WeatherDescriptor("Fog", "fog", AnsiColour.Grey),
            51 or 53 or 55 => new WeatherDescriptor("Drizzle", "drizzle", AnsiColour.Blue),
            56 or 57 => new WeatherDescriptor("Freezing drizzle", "freezing_drizzle", AnsiColour.Blue),
            61 or 63 or 65 => new WeatherDescriptor("Rain", "rain", AnsiColour.DarkGrey),
            66 or 67 => new WeatherDescriptor("Freezing rain", "freezing_rain", AnsiColour.DarkGrey),
            71 or 73 or 75 => new WeatherDescriptor("Snow", "snow", AnsiColour.White),
            77 => new WeatherDescriptor("Snow grains", "snow_grains", AnsiColour.White),
            80 or 81 or 82 => new WeatherDescriptor("Rain showers", "rain_showers", AnsiColour.DarkGrey),
            85 or 86 => new WeatherDescriptor("Snow showers", "snow_showers", AnsiColour.White),
            95 => new WeatherDescriptor("Thunderstorm", "thunderstorm", AnsiColour.DarkGrey),
            96 or 99 => new WeatherDescriptor("Thunderstorm with hail", "thunderstorm_hail", AnsiColour.DarkGrey),
            _ => new WeatherDescriptor("Unknown", "unknown", AnsiColour.Default)
        };
    }
}

public sealed record WeatherDescriptor(string Description, string ArtKey, AnsiColour ArtColour);
