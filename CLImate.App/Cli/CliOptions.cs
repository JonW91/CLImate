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

    // Legacy property for backwards compatibility
    public bool TodayOnly => ForecastMode == ForecastMode.Today;
}
