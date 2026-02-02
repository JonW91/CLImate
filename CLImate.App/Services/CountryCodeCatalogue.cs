using System.Text.Json;

namespace CLImate.App.Services;

public sealed class CountryCodeCatalogue : ICountryCodeCatalogue
{
    private readonly Dictionary<string, string> _mappings;

    public CountryCodeCatalogue()
    {
        _mappings = LoadMappings();
    }

    public string? GetCode(string name)
    {
        var key = name.Trim();
        if (key.Length == 0)
        {
            return null;
        }

        return _mappings.TryGetValue(key, out var code) ? code : null;
    }

    private static Dictionary<string, string> LoadMappings()
    {
        var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in DefaultMappings())
        {
            mappings[pair.Key] = pair.Value;
        }

        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "country-codes.json");
        if (!File.Exists(path))
        {
            return mappings;
        }

        var json = File.ReadAllText(path);
        var custom = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        if (custom == null)
        {
            return mappings;
        }

        foreach (var pair in custom)
        {
            var key = pair.Key?.Trim();
            var value = pair.Value?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            mappings[key] = value;
        }

        return mappings;
    }

    private static Dictionary<string, string> DefaultMappings()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["united states"] = "US",
            ["usa"] = "US",
            ["us"] = "US",
            ["u.s."] = "US",
            ["united kingdom"] = "GB",
            ["uk"] = "GB",
            ["u.k."] = "GB",
            ["great britain"] = "GB",
            ["britain"] = "GB",
            ["england"] = "GB",
            ["scotland"] = "GB",
            ["wales"] = "GB",
            ["northern ireland"] = "GB",
            ["republic of ireland"] = "IE",
            ["ireland"] = "IE",
            ["czech republic"] = "CZ",
            ["czechia"] = "CZ",
            ["south korea"] = "KR",
            ["north korea"] = "KP",
            ["uae"] = "AE",
            ["united arab emirates"] = "AE"
        };
    }
}
