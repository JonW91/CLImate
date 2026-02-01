namespace CLImate.App.Models;

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
