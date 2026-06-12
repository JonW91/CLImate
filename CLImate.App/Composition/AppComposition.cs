using System.Net.Http.Headers;
using System.Text.Json;
using CLImate.App.Cli;
using CLImate.App.Configuration;
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

            // api.weather.gov requires a descriptive User-Agent with a point of contact.
            var version = typeof(AppComposition).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CLImate", version));
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(+https://github.com/JonW91/CLImate)"));

            return http;
        });

        services.AddSingleton<IConsoleIO, ConsoleIO>();
        services.AddSingleton<ITerminalInfo, TerminalInfo>();
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
        services.AddSingleton<INwsWarningsClient, NwsWarningsClient>();
        services.AddSingleton<IMeteoalarmWarningsClient, MeteoalarmWarningsClient>();
        services.AddSingleton<IWeatherWarningsService, WeatherWarningsService>();
        services.AddSingleton<IAsciiArtCatalogue, AsciiArtCatalogue>();
        services.AddSingleton<IAnsiColouriser, AnsiColouriser>();
        services.AddSingleton<IArtColouriser, ArtColouriser>();
        services.AddSingleton<ITemperatureColourScale, TemperatureColourScale>();
        services.AddSingleton<IWeatherCodeCatalogue, WeatherCodeCatalogue>();
        services.AddSingleton<ITableRenderer, TableRenderer>();
        services.AddSingleton<IForecastRenderer, ForecastRenderer>();
        services.AddSingleton<ICliApplication, CliApplication>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IOptionsResolver, OptionsResolver>();

        return services.BuildServiceProvider();
    }
}
