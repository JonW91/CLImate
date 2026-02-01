using System.Text.Json;

namespace CLImate.App.Services;

public interface IJsonHttpClient
{
    Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken);
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
        using var response = await _http.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken);
    }
}
