using CLImate.App.Models;

namespace CLImate.App.Cli;

public sealed class CliOptions
{
    public Units Units { get; set; } = Units.Metric;
    public string? CountryCode { get; set; }
    public bool ShowArt { get; set; } = true;
    public bool UseColor { get; set; } = true;
    public string? LocationInput { get; set; }
    public bool ShowHelp { get; set; }
    public bool TodayOnly { get; set; }
}
