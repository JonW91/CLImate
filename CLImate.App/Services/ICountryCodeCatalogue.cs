namespace CLImate.App.Services;

public interface ICountryCodeCatalogue
{
    string? GetCode(string name);
}
