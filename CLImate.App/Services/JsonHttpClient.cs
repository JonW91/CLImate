using System.Net;
using System.Text.Json;

namespace CLImate.App.Services;

public interface IJsonHttpClient
{
    Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken);
}

/// <summary>
/// Thrown when an upstream weather/geocoding API returns a non-success status code,
/// so callers can distinguish service failures from "no data".
/// </summary>
public sealed class WeatherApiException : Exception
{
    public WeatherApiException(HttpStatusCode statusCode, string url)
        : base($"Request to '{url}' failed with HTTP {(int)statusCode} ({statusCode}).")
    {
        StatusCode = statusCode;
        Url = url;
    }

    public HttpStatusCode StatusCode { get; }
    public string Url { get; }

    public bool IsTransient =>
        StatusCode == HttpStatusCode.TooManyRequests || (int)StatusCode >= 500;
}

public sealed class JsonHttpClient : IJsonHttpClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _options;

    public JsonHttpClient(HttpClient http, JsonSerializerOptions options)
    {
        _http = http;
        _options = options;
    }

    public async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken)
    {
        using var response = await SendWithRetryAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new WeatherApiException(response.StatusCode, url);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(string url, CancellationToken cancellationToken)
    {
        var response = await _http.GetAsync(url, cancellationToken);

        // One retry with a short backoff for transient server-side failures.
        if ((int)response.StatusCode >= 500 || response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            response.Dispose();
            await Task.Delay(TimeSpan.FromMilliseconds(750), cancellationToken);
            response = await _http.GetAsync(url, cancellationToken);
        }

        return response;
    }
}
