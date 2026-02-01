namespace CLImate.App.Services;

public interface ICountryCodeCatalog
{
    string? GetCode(string name);
}
