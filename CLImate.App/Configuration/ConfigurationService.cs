using System.Text.Json;
using CLImate.App.Cli;

namespace CLImate.App.Configuration;

public sealed class ConfigurationService : IConfigurationService
{
    private readonly string _configPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IConsoleIO _console;

    public ConfigurationService(IConsoleIO console, string? configDirectory = null)
    {
        _console = console;
        _configPath = Path.Combine(configDirectory ?? ResolveConfigDirectory(), "config.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    private static string ResolveConfigDirectory()
    {
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            // Use XDG_CONFIG_HOME on Linux/macOS if available
            var xdgConfig = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            if (!string.IsNullOrEmpty(xdgConfig))
            {
                return Path.Combine(xdgConfig, "climate");
            }

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config", "climate");
        }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "climate");
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
        catch (JsonException ex)
        {
            _console.WriteLine($"Warning: ignoring invalid config file at {_configPath} ({ex.Message}). Using defaults.");
            return new ClimateConfig();
        }
        catch (IOException ex)
        {
            _console.WriteLine($"Warning: could not read config file at {_configPath} ({ex.Message}). Using defaults.");
            return new ClimateConfig();
        }
        catch (UnauthorizedAccessException)
        {
            _console.WriteLine($"Warning: no permission to read config file at {_configPath}. Using defaults.");
            return new ClimateConfig();
        }
    }

    public async Task SaveConfigAsync(ClimateConfig config)
    {
        // Create the directory lazily — only when actually writing the config —
        // so that read-only invocations (e.g. --help) never touch the filesystem.
        Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);

        var json = JsonSerializer.Serialize(config, _jsonOptions);
        await File.WriteAllTextAsync(_configPath, json);
    }

    public string GetConfigPath() => _configPath;

    public bool ConfigExists() => File.Exists(_configPath);
}
