namespace CLImate.App.Models;

public enum WarningStatus
{
    None,               // no warning exists for this date
    RegionalUnavailable, // warnings feed not available for this region
    Active              // a real alert is in effect
}

public sealed class WarningResult
{
    public static readonly WarningResult None = new(WarningStatus.None, string.Empty);
    public static readonly WarningResult RegionalUnavailable = new(WarningStatus.RegionalUnavailable, string.Empty);

    private WarningResult(WarningStatus status, string text) { Status = status; Text = text; }

    public static WarningResult Active(string text) => new(WarningStatus.Active, text);

    public WarningStatus Status { get; }
    public string Text { get; }
    public bool IsActive => Status == WarningStatus.Active;
}

public sealed class WeatherWarning
{
    public WeatherWarning(string summary, DateTimeOffset? starts, DateTimeOffset? ends)
    {
        Summary = summary;
        Starts = starts;
        Ends = ends;
    }

    public string Summary { get; }
    public DateTimeOffset? Starts { get; }
    public DateTimeOffset? Ends { get; }
}
