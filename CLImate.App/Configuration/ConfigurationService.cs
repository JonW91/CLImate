using System.Text.Json;

namespace CLImate.App.Configuration;

public sealed class ConfigurationService : IConfigurationService
{
    private readonly string _configPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public ConfigurationService()
    {
        var configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "climate");

        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            // Use XDG_CONFIG_HOME on Linux/macOS if available
            var xdgConfig = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            if (!string.IsNullOrEmpty(xdgConfig))
            {
                configDir = Path.Combine(xdgConfig, "climate");
            }
            else
            {
                configDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".config", "climate");
            }
        }

        Directory.CreateDirectory(configDir);
        _configPath = Path.Combine(configDir, "config.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public ClimateConfig GetConfig()
    {
        if (!File.Exists(_configPath))
            return new ClimateConfig();

        try
        {
            var json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<ClimateConfig>(json, _jsonOptions)
                   ?? new ClimateConfig();
        }
        catch
        {
            return new ClimateConfig();
        }
    }

    public async Task SaveConfigAsync(ClimateConfig config)
    {
        var json = JsonSerializer.Serialize(config, _jsonOptions);
        await File.WriteAllTextAsync(_configPath, json);
    }

    public string GetConfigPath() => _configPath;

    public bool ConfigExists() => File.Exists(_configPath);
}
