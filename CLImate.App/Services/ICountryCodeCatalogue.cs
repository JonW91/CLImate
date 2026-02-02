namespace CLImate.App.Services;

public interface ICountryCodeCatalogue
{
    string? GetCode(string name);
    bool IsValidCode(string code);
    IReadOnlySet<string> GetAllCodes();
}
