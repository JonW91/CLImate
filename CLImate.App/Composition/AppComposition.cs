using System.Net.Http.Headers;
using System.Text.Json;
using CLImate.App.Cli;
using CLImate.App.Rendering;
using CLImate.App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CLImate.App.Composition;

public static class AppComposition
{
    public static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        services.AddSingleton<HttpClient>(_ =>
        {
            var http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20)
            };

            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CLImate", "0.1"));
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(Fedora-Termux)"));

            return http;
        });

        services.AddSingleton<IConsoleIO, ConsoleIO>();
        services.AddSingleton<ICliHelp, CliHelp>();
        services.AddSingleton<ICliOptionsParser, CliOptionsParser>();
        services.AddSingleton<ICountryCodeCatalogue, CountryCodeCatalogue>();
        services.AddSingleton<ILocationInputParser, LocationInputParser>();
        services.AddSingleton<ILocationFormatter, LocationFormatter>();
        services.AddSingleton<ILocationSelector, LocationSelector>();
        services.AddSingleton<IJsonHttpClient, JsonHttpClient>();
        services.AddSingleton<IApiMapper, ApiMapper>();
        services.AddSingleton<IGeocodingService, GeocodingService>();
        services.AddSingleton<IForecastService, ForecastService>();
        services.AddSingleton<IMeteoBlueWarningsClient, MeteoBlueWarningsClient>();
        services.AddSingleton<IWeatherWarningsService, WeatherWarningsService>();
        services.AddSingleton<IAsciiArtCatalogue, AsciiArtCatalogue>();
        services.AddSingleton<IAnsiColouriser, AnsiColouriser>();
        services.AddSingleton<IArtColouriser, ArtColouriser>();
        services.AddSingleton<ITemperatureColourScale, TemperatureColourScale>();
        services.AddSingleton<IWeatherCodeCatalogue, WeatherCodeCatalogue>();
        services.AddSingleton<IForecastRenderer, ForecastRenderer>();
        services.AddSingleton<ICliApplication, CliApplication>();

        return services.BuildServiceProvider();
    }
}
