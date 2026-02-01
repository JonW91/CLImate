using CLImate.App.Models;

namespace CLImate.App.Services;

public interface IGeocodingService
{
    Task<List<GeoResult>> SearchAsync(string location, string? countryCode, CancellationToken cancellationToken);
}

public sealed class GeocodingService : IGeocodingService
{
    private readonly IJsonHttpClient _client;
    private readonly IApiMapper _mapper;

    public GeocodingService(IJsonHttpClient client, IApiMapper mapper)
    {
        _client = client;
        _mapper = mapper;
    }

    public async Task<List<GeoResult>> SearchAsync(string location, string? countryCode, CancellationToken cancellationToken)
    {
        var url =
            "https://geocoding-api.open-meteo.com/v1/search" +
            $"?name={Uri.EscapeDataString(location)}&count=5&language=en&format=json" +
            BuildCountryCodeParameter(countryCode);

        var response = await _client.GetAsync<GeocodeResponse>(url, cancellationToken);
        return _mapper.MapGeocoding(response);
    }

    private static string BuildCountryCodeParameter(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return string.Empty;
        }

        return $"&countryCode={Uri.EscapeDataString(countryCode)}";
    }
}
