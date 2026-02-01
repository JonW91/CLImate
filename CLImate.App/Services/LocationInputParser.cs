namespace CLImate.App.Services;

public interface ILocationInputParser
{
    string ExtractPrimaryName(string input);
    string? InferCountryCode(string input);
    string? NormalizeCountryCode(string? input);
}

public sealed class LocationInputParser : ILocationInputParser
{
    private readonly ICountryCodeCatalog _countryCodeCatalog;

    public LocationInputParser(ICountryCodeCatalog countryCodeCatalog)
    {
        _countryCodeCatalog = countryCodeCatalog;
    }

    public string ExtractPrimaryName(string input)
    {
        var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length > 0 ? parts[0] : input;
    }

    public string? InferCountryCode(string input)
    {
        var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
        {
            return null;
        }

        return CountryNameToCode(parts[^1]);
    }

    public string? NormalizeCountryCode(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var trimmed = input.Trim();
        if (trimmed.Length == 2)
        {
            return trimmed.ToUpperInvariant();
        }

        return CountryNameToCode(trimmed);
    }

    private string? CountryNameToCode(string name) => _countryCodeCatalog.GetCode(name);
}
