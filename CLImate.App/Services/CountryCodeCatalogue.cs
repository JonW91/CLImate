using System.Text.Json;

namespace CLImate.App.Services;

public sealed class CountryCodeCatalogue : ICountryCodeCatalogue
{
    private readonly Dictionary<string, string> _mappings;
    private readonly HashSet<string> _validCodes;

    public CountryCodeCatalogue()
    {
        _mappings = LoadMappings();
        _validCodes = new HashSet<string>(Iso3166Codes, StringComparer.OrdinalIgnoreCase);
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

    public bool IsValidCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        return _validCodes.Contains(code.Trim().ToUpperInvariant());
    }

    public IReadOnlySet<string> GetAllCodes() => _validCodes;

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

    private static readonly string[] Iso3166Codes =
    [
        "AD", "AE", "AF", "AG", "AI", "AL", "AM", "AO", "AQ", "AR", "AS", "AT", "AU", "AW", "AX", "AZ",
        "BA", "BB", "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BL", "BM", "BN", "BO", "BQ", "BR", "BS",
        "BT", "BV", "BW", "BY", "BZ", "CA", "CC", "CD", "CF", "CG", "CH", "CI", "CK", "CL", "CM", "CN",
        "CO", "CR", "CU", "CV", "CW", "CX", "CY", "CZ", "DE", "DJ", "DK", "DM", "DO", "DZ", "EC", "EE",
        "EG", "EH", "ER", "ES", "ET", "FI", "FJ", "FK", "FM", "FO", "FR", "GA", "GB", "GD", "GE", "GF",
        "GG", "GH", "GI", "GL", "GM", "GN", "GP", "GQ", "GR", "GS", "GT", "GU", "GW", "GY", "HK", "HM",
        "HN", "HR", "HT", "HU", "ID", "IE", "IL", "IM", "IN", "IO", "IQ", "IR", "IS", "IT", "JE", "JM",
        "JO", "JP", "KE", "KG", "KH", "KI", "KM", "KN", "KP", "KR", "KW", "KY", "KZ", "LA", "LB", "LC",
        "LI", "LK", "LR", "LS", "LT", "LU", "LV", "LY", "MA", "MC", "MD", "ME", "MF", "MG", "MH", "MK",
        "ML", "MM", "MN", "MO", "MP", "MQ", "MR", "MS", "MT", "MU", "MV", "MW", "MX", "MY", "MZ", "NA",
        "NC", "NE", "NF", "NG", "NI", "NL", "NO", "NP", "NR", "NU", "NZ", "OM", "PA", "PE", "PF", "PG",
        "PH", "PK", "PL", "PM", "PN", "PR", "PS", "PT", "PW", "PY", "QA", "RE", "RO", "RS", "RU", "RW",
        "SA", "SB", "SC", "SD", "SE", "SG", "SH", "SI", "SJ", "SK", "SL", "SM", "SN", "SO", "SR", "SS",
        "ST", "SV", "SX", "SY", "SZ", "TC", "TD", "TF", "TG", "TH", "TJ", "TK", "TL", "TM", "TN", "TO",
        "TR", "TT", "TV", "TW", "TZ", "UA", "UG", "UM", "US", "UY", "UZ", "VA", "VC", "VE", "VG", "VI",
        "VN", "VU", "WF", "WS", "YE", "YT", "ZA", "ZM", "ZW"
    ];
}
