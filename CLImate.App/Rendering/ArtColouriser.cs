using System.Text;

namespace CLImate.App.Rendering;

public interface IArtColouriser
{
    string Colourise(string art, string key, bool enabled);
}

public sealed class ArtColouriser : IArtColouriser
{
    // The sun is drawn in bold yellow. Some terminals show thin yellow hyphens
    // (the horizontal rays) as white unless they are bold, and bolding only the
    // rays looks uneven, so the whole sun is bold.
    private const AnsiColour SunColour = AnsiColour.BoldYellow;

    private static readonly HashSet<char> CloudChars = new(new[] { '.', '-', '(', ')', '_' });
    private static readonly HashSet<char> SunChars = new(new[] { 'o', '\\', '/', '|', '*' });
    private static readonly HashSet<char> RainChars = new(new[] { '/' });
    private static readonly HashSet<char> DrizzleChars = new(new[] { '\'' });
    private static readonly HashSet<char> SnowChars = new(new[] { '*' });
    private static readonly HashSet<char> LightningChars = new(new[] { '/', '\\' });
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
        var sideRays = string.Equals(key, "partly_cloudy", StringComparison.OrdinalIgnoreCase)
            ? FindSideRays(art)
            : null;

        for (var i = 0; i < art.Length; i++)
        {
            var ch = art[i];
            if (ch == '\n')
            {
                FlushSegment();
                sb.Append(ch);
                continue;
            }

            var colour = sideRays is not null && sideRays[i] ? SunColour : GetColourForChar(key, ch);
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

    /// <summary>
    /// Marks the hyphens at the very start and end of each line. In partly-cloudy
    /// art those are the sun's side rays poking out past the cloud; hyphens inside
    /// the line belong to the cloud outline and stay grey.
    /// </summary>
    private static bool[] FindSideRays(string art)
    {
        var rays = new bool[art.Length];
        var lineStart = 0;

        while (lineStart <= art.Length)
        {
            var lineEnd = art.IndexOf('\n', lineStart);
            if (lineEnd < 0)
            {
                lineEnd = art.Length;
            }

            var first = lineStart;
            while (first < lineEnd && char.IsWhiteSpace(art[first]))
            {
                first++;
            }

            var last = lineEnd - 1;
            while (last >= first && char.IsWhiteSpace(art[last]))
            {
                last--;
            }

            for (var i = first; i <= last && art[i] == '-'; i++)
            {
                rays[i] = true;
            }

            for (var i = last; i >= first && art[i] == '-'; i--)
            {
                rays[i] = true;
            }

            lineStart = lineEnd + 1;
        }

        return rays;
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
            return SunColour;
        }

        // Partly cloudy - sun chars are yellow, cloud chars are grey
        if (string.Equals(key, "partly_cloudy", StringComparison.OrdinalIgnoreCase))
        {
            if (SunChars.Contains(ch))
            {
                return SunColour;
            }

            if (CloudChars.Contains(ch))
            {
                return AnsiColour.Grey;
            }

            return AnsiColour.Default;
        }

        // Thunderstorm - lightning is bold yellow, clouds are bold dark grey, hail is white
        if (key.Contains("thunderstorm", StringComparison.OrdinalIgnoreCase))
        {
            if (LightningChars.Contains(ch))
            {
                return AnsiColour.BoldYellow;
            }

            if (SnowChars.Contains(ch))
            {
                return AnsiColour.White;
            }

            if (CloudChars.Contains(ch))
            {
                return AnsiColour.BoldDarkGrey;
            }

            return AnsiColour.Default;
        }

        // Snow - asterisks are white, clouds are grey, dots are white for snow grains
        if (key.Contains("snow", StringComparison.OrdinalIgnoreCase))
        {
            if (CloudChars.Contains(ch))
            {
                return AnsiColour.Grey;
            }

            if (SnowChars.Contains(ch) || DotChars.Contains(ch))
            {
                return AnsiColour.White;
            }

            return AnsiColour.Default;
        }

        // Rain/drizzle - slashes and drizzle drops are blue, asterisks (freezing) are white,
        // rain clouds are bold dark grey, drizzle clouds are grey
        if (key.Contains("rain", StringComparison.OrdinalIgnoreCase)
            || key.Contains("drizzle", StringComparison.OrdinalIgnoreCase))
        {
            if (RainChars.Contains(ch) || DrizzleChars.Contains(ch))
            {
                return AnsiColour.Blue;
            }

            if (SnowChars.Contains(ch))
            {
                return AnsiColour.White;
            }

            if (CloudChars.Contains(ch))
            {
                // Rain falls from thick, dark cloud; drizzle from thin, paler cloud.
                return key.Contains("rain", StringComparison.OrdinalIgnoreCase)
                    ? AnsiColour.BoldDarkGrey
                    : AnsiColour.Grey;
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
