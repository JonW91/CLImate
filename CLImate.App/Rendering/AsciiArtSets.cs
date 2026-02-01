namespace CLImate.App.Rendering;

public sealed class AsciiArtSets
{
    public Dictionary<string, string[]> Small { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string[]> Medium { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string[]> Large { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
