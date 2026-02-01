namespace CLImate.App.Rendering;

public interface IAnsiColorizer
{
    string Apply(string text, AnsiColor color, bool enabled);
    bool ShouldUseColor(bool userEnabled);
}

public sealed class AnsiColorizer : IAnsiColorizer
{
    private const string Reset = "\u001b[0m";

    public string Apply(string text, AnsiColor color, bool enabled)
    {
        if (!enabled || color == AnsiColor.Default)
        {
            return text;
        }

        return $"{GetCode(color)}{text}{Reset}";
    }

    public bool ShouldUseColor(bool userEnabled)
    {
        if (!userEnabled)
        {
            return false;
        }

        if (Console.IsOutputRedirected)
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("NO_COLOR"));
    }

    private static string GetCode(AnsiColor color)
    {
        return color switch
        {
            AnsiColor.Red => "\u001b[31m",
            AnsiColor.Yellow => "\u001b[33m",
            AnsiColor.Blue => "\u001b[34m",
            AnsiColor.Gray => "\u001b[90m",
            AnsiColor.White => "\u001b[97m",
            _ => string.Empty
        };
    }
}
