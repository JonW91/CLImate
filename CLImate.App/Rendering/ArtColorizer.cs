using System.Text;

namespace CLImate.App.Rendering;

public interface IArtColorizer
{
    string Colorize(string art, string key, bool enabled);
}

public sealed class ArtColorizer : IArtColorizer
{
    private static readonly HashSet<char> CloudChars = new(new[] { '.', '-', '(', ')', '_' });
    private static readonly HashSet<char> SunChars = new(new[] { 'o', '\\', '/', '|', '*' });
    private static readonly HashSet<char> RainChars = new(new[] { '/' });
    private static readonly HashSet<char> SnowChars = new(new[] { '*' });
    private static readonly HashSet<char> LightningChars = new(new[] { '/', '_' });
    private static readonly HashSet<char> DotChars = new(new[] { '.' });

    private readonly IAnsiColorizer _colorizer;

    public ArtColorizer(IAnsiColorizer colorizer)
    {
        _colorizer = colorizer;
    }

    public string Colorize(string art, string key, bool enabled)
    {
        if (!enabled || string.IsNullOrWhiteSpace(art))
        {
            return art;
        }

        var sb = new StringBuilder();
        var segment = new StringBuilder();
        var current = AnsiColor.Default;

        foreach (var ch in art)
        {
            if (ch == '\n')
            {
                FlushSegment();
                sb.Append(ch);
                continue;
            }

            var color = GetColorForChar(key, ch);
            if (color != current)
            {
                FlushSegment();
                current = color;
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

            sb.Append(_colorizer.Apply(segment.ToString(), current, enabled));
            segment.Clear();
        }
    }

    private static AnsiColor GetColorForChar(string key, char ch)
    {
        if (char.IsWhiteSpace(ch))
        {
            return AnsiColor.Default;
        }

        // Clear sky - everything is sun/yellow
        if (string.Equals(key, "clear", StringComparison.OrdinalIgnoreCase))
        {
            return AnsiColor.Yellow;
        }

        // Partly cloudy - sun chars are yellow, cloud chars are gray
        if (string.Equals(key, "partly_cloudy", StringComparison.OrdinalIgnoreCase))
        {
            if (SunChars.Contains(ch))
            {
                return AnsiColor.Yellow;
            }

            if (CloudChars.Contains(ch))
            {
                return AnsiColor.Gray;
            }

            return AnsiColor.Default;
        }

        // Thunderstorm - lightning is yellow, clouds are dark gray, hail is white
        if (key.Contains("thunderstorm", StringComparison.OrdinalIgnoreCase))
        {
            if (LightningChars.Contains(ch))
            {
                return AnsiColor.Yellow;
            }

            if (SnowChars.Contains(ch))
            {
                return AnsiColor.White;
            }

            if (CloudChars.Contains(ch))
            {
                return AnsiColor.DarkGray;
            }

            return AnsiColor.Default;
        }

        // Snow - asterisks are white, clouds are gray, dots are white
        if (key.Contains("snow", StringComparison.OrdinalIgnoreCase))
        {
            if (SnowChars.Contains(ch) || DotChars.Contains(ch))
            {
                return AnsiColor.White;
            }

            if (CloudChars.Contains(ch))
            {
                return AnsiColor.Gray;
            }

            return AnsiColor.Default;
        }

        // Rain/drizzle - slashes are blue, asterisks (freezing) are white, clouds are gray
        if (key.Contains("rain", StringComparison.OrdinalIgnoreCase)
            || key.Contains("drizzle", StringComparison.OrdinalIgnoreCase))
        {
            if (RainChars.Contains(ch))
            {
                return AnsiColor.Blue;
            }

            if (SnowChars.Contains(ch))
            {
                return AnsiColor.White;
            }

            if (CloudChars.Contains(ch))
            {
                return AnsiColor.DarkGray;
            }

            return AnsiColor.Default;
        }

        // Fog and overcast - all gray
        if (key.Contains("fog", StringComparison.OrdinalIgnoreCase)
            || key.Contains("overcast", StringComparison.OrdinalIgnoreCase))
        {
            return AnsiColor.Gray;
        }

        // Default: cloud chars are gray
        if (CloudChars.Contains(ch))
        {
            return AnsiColor.Gray;
        }

        return AnsiColor.Default;
    }
}
