namespace CLImate.App.Rendering;

public interface IAnsiColouriser
{
    string Apply(string text, AnsiColour colour, bool enabled);
    bool ShouldUseColour(bool userEnabled);
}

public sealed class AnsiColouriser : IAnsiColouriser
{
    private const string Reset = "\u001b[0m";

    public string Apply(string text, AnsiColour colour, bool enabled)
    {
        if (!enabled || colour == AnsiColour.Default)
        {
            return text;
        }

        return $"{GetCode(colour)}{text}{Reset}";
    }

    public bool ShouldUseColour(bool userEnabled)
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

    private static string GetCode(AnsiColour colour)
    {
        return colour switch
        {
            AnsiColour.DarkGrey => "\u001b[90m",
            // A fixed 256-colour grey rather than "bright black" (90): many terminals
            // brighten the basic colours when bold, which made 1;90 look light grey.
            AnsiColour.BoldDarkGrey => "\u001b[1;38;5;238m",
            AnsiColour.Red => "\u001b[31m",
            AnsiColour.Yellow => "\u001b[33m",
            AnsiColour.BoldYellow => "\u001b[1;33m",
            AnsiColour.Blue => "\u001b[34m",
            AnsiColour.Grey => "\u001b[37m",
            AnsiColour.White => "\u001b[97m",
            _ => string.Empty
        };
    }
}
