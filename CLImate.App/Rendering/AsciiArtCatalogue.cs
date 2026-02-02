using System.Text.Json;

namespace CLImate.App.Rendering;

public interface IAsciiArtCatalogue
{
    string GetArt(string key, int? width);
}

public sealed class AsciiArtCatalogue : IAsciiArtCatalogue
{
    private const int LargeMinWidth = 100;
    private const int MediumMinWidth = 70;
    private const int SmallMinWidth = 45;

    private readonly JsonSerializerOptions _options;
    private AsciiArtSets? _sets;

    public AsciiArtCatalogue(JsonSerializerOptions options)
    {
        _options = options;
    }

    public string GetArt(string key, int? width)
    {
        if (width is null || width < SmallMinWidth)
        {
            return string.Empty;
        }

        var sets = _sets ??= LoadSets();
        if (sets == null)
        {
            return string.Empty;
        }

        if (width >= LargeMinWidth && sets.Large.TryGetValue(key, out var large))
        {
            return string.Join('\n', large);
        }

        if (width >= MediumMinWidth && sets.Medium.TryGetValue(key, out var medium))
        {
            return string.Join('\n', medium);
        }

        if (sets.Small.TryGetValue(key, out var small))
        {
            return string.Join('\n', small);
        }

        return string.Empty;
    }

    private AsciiArtSets? LoadSets()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "ascii-art.json");
        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AsciiArtSets>(json, _options);
    }
}
