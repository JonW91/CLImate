using System.Text;

namespace CLImate.App.Rendering;

public interface IArtColouriser
{
    string Colourise(string art, string key, bool enabled);
}

public sealed class ArtColouriser : IArtColouriser
{
    private static readonly HashSet<char> CloudChars = new(new[] { '.', '-', '(', ')', '_' });
    private static readonly HashSet<char> SunChars = new(new[] { 'o', '\\', '/', '|', '*' });
    private static readonly HashSet<char> RainChars = new(new[] { '/' });
    private static readonly HashSet<char> SnowChars = new(new[] { '*' });
    private static readonly HashSet<char> LightningChars = new(new[] { '/', '_' });
    private static readonly HashSet<char> DotChars = new(new[] { '.' });

    private readonly IAnsiColouriser _colouriser;

    public ArtColouriser(IAnsiColouriser colouriser)
    {
        _colouriser = colouriser;
    }

    public string Colourise(string art, string key, bool enabled)
    {
        if (!enabled || string.IsNullOrWhiteSpace(art))
        {
            return art;
        }

        var sb = new StringBuilder();
        var segment = new StringBuilder();
        var current = AnsiColour.Default;

        foreach (var ch in art)
        {
            if (ch == '\n')
            {
                FlushSegment();
                sb.Append(ch);
                continue;
            }

            var colour = GetColourForChar(key, ch);
            if (colour != current)
            {
                FlushSegment();
                current = colour;
            }

            segment.Append(ch);
        }

        FlushSegment();
        return sb.ToString();

        void FlushSegment()
        {
            if (segment.Length == 0)
            {
                return;
            }

            sb.Append(_colouriser.Apply(segment.ToString(), current, enabled));
            segment.Clear();
        }
    }

    private static AnsiColour GetColourForChar(string key, char ch)
    {
        if (char.IsWhiteSpace(ch))
        {
            return AnsiColour.Default;
        }

        // Clear sky - everything is sun/yellow
        if (string.Equals(key, "clear", StringComparison.OrdinalIgnoreCase))
        {
            return AnsiColour.Yellow;
        }

        // Partly cloudy - sun chars are yellow, cloud chars are grey
        if (string.Equals(key, "partly_cloudy", StringComparison.OrdinalIgnoreCase))
        {
            if (SunChars.Contains(ch))
            {
                return AnsiColour.Yellow;
            }

            if (CloudChars.Contains(ch))
            {
                return AnsiColour.Grey;
            }

            return AnsiColour.Default;
        }

        // Thunderstorm - lightning is yellow, clouds are dark grey, hail is white
        if (key.Contains("thunderstorm", StringComparison.OrdinalIgnoreCase))
        {
            if (LightningChars.Contains(ch))
            {
                return AnsiColour.Yellow;
            }

            if (SnowChars.Contains(ch))
            {
                return AnsiColour.White;
            }

            if (CloudChars.Contains(ch))
            {
                return AnsiColour.DarkGrey;
            }

            return AnsiColour.Default;
        }

        // Snow - asterisks are white, clouds are grey, dots are white
        if (key.Contains("snow", StringComparison.OrdinalIgnoreCase))
        {
            if (SnowChars.Contains(ch) || DotChars.Contains(ch))
            {
                return AnsiColour.White;
            }

            if (CloudChars.Contains(ch))
            {
                return AnsiColour.Grey;
            }

            return AnsiColour.Default;
        }

        // Rain/drizzle - slashes are blue, asterisks (freezing) are white, clouds are grey
        if (key.Contains("rain", StringComparison.OrdinalIgnoreCase)
            || key.Contains("drizzle", StringComparison.OrdinalIgnoreCase))
        {
            if (RainChars.Contains(ch))
            {
                return AnsiColour.Blue;
            }

            if (SnowChars.Contains(ch))
            {
                return AnsiColour.White;
            }

            if (CloudChars.Contains(ch))
            {
                return AnsiColour.DarkGrey;
            }

            return AnsiColour.Default;
        }

        // Fog and overcast - all grey
        if (key.Contains("fog", StringComparison.OrdinalIgnoreCase)
            || key.Contains("overcast", StringComparison.OrdinalIgnoreCase))
        {
            return AnsiColour.Grey;
        }

        // Default: cloud chars are grey
        if (CloudChars.Contains(ch))
        {
            return AnsiColour.Grey;
        }

        return AnsiColour.Default;
    }
}
