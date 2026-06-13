using CLImate.App.Models;

namespace CLImate.App.Cli;

public enum LayoutMode
{
    Auto,
    Horizontal,
    Vertical
}

public enum ForecastMode
{
    Daily,
    Today,
    Hourly
}

public sealed class ConfigSubcommand
{
    public string Action { get; init; } = string.Empty; // "show" | "set" | "add-favourite"
    public string? Key { get; init; }
    public string? Value { get; init; }
}

public sealed class CliOptions
{
    public Units Units { get; set; } = Units.Metric;
    public string? CountryCode { get; set; }
    public bool ShowArt { get; set; } = true;
    public bool UseColour { get; set; } = true;
    public string? LocationInput { get; set; }
    public bool ShowHelp { get; set; }
    public bool ShowVersion { get; set; }
    public ForecastMode ForecastMode { get; set; } = ForecastMode.Daily;
    public LayoutMode Layout { get; set; } = LayoutMode.Auto;
    public ConfigSubcommand? Config { get; set; }

    // Track which values were explicitly provided on the command line so that
    // configuration-file defaults never override an explicit user choice.
    public bool UnitsSetExplicitly { get; set; }
    public bool ShowArtSetExplicitly { get; set; }
    public bool UseColourSetExplicitly { get; set; }

    // Legacy property for backwards compatibility
    public bool TodayOnly => ForecastMode == ForecastMode.Today;
}
