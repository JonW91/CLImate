using CLImate.App.Cli;

namespace CLImate.App.Rendering;

public interface IArtGallery
{
    void Render(bool useColour);
}

/// <summary>
/// Prints every weather icon at each art size, so the art and its colours can
/// be checked without waiting for a location to forecast each condition.
/// </summary>
public sealed class ArtGallery : IArtGallery
{
    // One WMO weather code per distinct icon, in the order they are shown.
    private static readonly int[] WeatherCodes = [0, 2, 3, 45, 51, 56, 61, 66, 80, 71, 77, 85, 95, 96];

    // The width asked of the catalogue selects the size tier.
    private static readonly (string Title, int CatalogueWidth)[] Sizes =
    [
        ("LARGE · list view, terminal 100+ columns", 100),
        ("MEDIUM · list view, terminal 70-99 columns", 70),
        ("SMALL · table cells, and list view under 70 columns", 45)
    ];

    private const int CellGap = 3;

    private readonly IConsoleIO _console;
    private readonly IAsciiArtCatalogue _asciiArt;
    private readonly IArtColouriser _artColouriser;
    private readonly IAnsiColouriser _colouriser;
    private readonly IWeatherCodeCatalogue _weatherCodes;
    private readonly ITerminalInfo _terminalInfo;

    public ArtGallery(
        IConsoleIO console,
        IAsciiArtCatalogue asciiArt,
        IArtColouriser artColouriser,
        IAnsiColouriser colouriser,
        IWeatherCodeCatalogue weatherCodes,
        ITerminalInfo terminalInfo)
    {
        _console = console;
        _asciiArt = asciiArt;
        _artColouriser = artColouriser;
        _colouriser = colouriser;
        _weatherCodes = weatherCodes;
        _terminalInfo = terminalInfo;
    }

    public void Render(bool useColour)
    {
        var colourEnabled = _colouriser.ShouldUseColour(useColour);
        var icons = WeatherCodes.Select(code => _weatherCodes.Describe(code)).ToList();

        _console.WriteLine();
        _console.WriteLine("CLImate art gallery");

        foreach (var (title, catalogueWidth) in Sizes)
        {
            var cells = icons
                .Select(icon => (icon.Description, icon.ArtKey, Lines: SplitArt(_asciiArt.GetArt(icon.ArtKey, catalogueWidth))))
                .ToList();

            _console.WriteLine();
            _console.WriteLine(title);
            _console.WriteLine(new string('─', Math.Min(title.Length, Math.Max(1, _terminalInfo.Width))));

            if (cells.All(c => c.Lines.Length == 0))
            {
                _console.WriteLine("  (art assets not found)");
                continue;
            }

            var artWidth = cells.Max(c => c.Lines.Length == 0 ? 0 : c.Lines.Max(l => l.Length));
            var cellWidth = Math.Max(artWidth, cells.Max(c => c.Description.Length)) + CellGap;
            var perRow = Math.Max(1, _terminalInfo.Width / cellWidth);

            foreach (var row in cells.Chunk(perRow))
            {
                _console.WriteLine();

                // Skip the blank padding rows below the shortest-needed height.
                var height = row.Max(c => LastInkedRow(c.Lines) + 1);
                for (var lineIdx = 0; lineIdx < height; lineIdx++)
                {
                    var line = string.Concat(row.Select(c =>
                    {
                        var raw = lineIdx < c.Lines.Length ? c.Lines[lineIdx] : string.Empty;
                        var padding = new string(' ', cellWidth - raw.Length);
                        return _artColouriser.Colourise(raw, c.ArtKey, colourEnabled) + padding;
                    }));
                    _console.WriteLine(line.TrimEnd());
                }

                _console.WriteLine(string.Concat(row.Select(c => c.Description.PadRight(cellWidth))).TrimEnd());
            }
        }

        _console.WriteLine();
    }

    private static string[] SplitArt(string art) =>
        string.IsNullOrEmpty(art) ? [] : art.Split('\n');

    private static int LastInkedRow(string[] lines)
    {
        for (var i = lines.Length - 1; i >= 0; i--)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                return i;
            }
        }

        return -1;
    }
}
