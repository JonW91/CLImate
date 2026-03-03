namespace CLImate.App.Configuration;

public interface IConfigurationService
{
    ClimateConfig GetConfig();
    Task SaveConfigAsync(ClimateConfig config);
    string GetConfigPath();
    bool ConfigExists();
}
