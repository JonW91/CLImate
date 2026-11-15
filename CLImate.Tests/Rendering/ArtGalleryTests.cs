using System.Text.Json;
using CLImate.App.Cli;
using CLImate.App.Rendering;
using FakeItEasy;

namespace CLImate.Tests.Rendering;

public sealed class ArtGalleryTests
{
    private readonly List<string> _lines = [];
    private readonly ITerminalInfo _terminalInfo = A.Fake<ITerminalInfo>();
    private readonly IArtColouriser _artColouriser = A.Fake<IArtColouriser>();

    private ArtGallery CreateGallery(int terminalWidth)
    {
        var console = A.Fake<IConsoleIO>();
        A.CallTo(() => console.WriteLine(A<string>._)).Invokes((string line) => _lines.Add(line));
        A.CallTo(() => console.WriteLine()).Invokes(() => _lines.Add(string.Empty));

        var ansi = A.Fake<IAnsiColouriser>();
        A.CallTo(() => ansi.ShouldUseColour(A<bool>._)).ReturnsLazily((bool enabled) => enabled);
        A.CallTo(() => _artColouriser.Colourise(A<string>._, A<string>._, A<bool>._))
            .ReturnsLazily((string art, string _, bool _) => art);
        A.CallTo(() => _terminalInfo.Width).Returns(terminalWidth);

        // Real catalogues: the gallery is only useful if it shows the shipped art.
        var catalogue = new AsciiArtCatalogue(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return new ArtGallery(console, catalogue, _artColouriser, ansi, new WeatherCodeCatalogue(), _terminalInfo);
    }

    [Fact]
    public void Render_ShowsEveryConditionAtEverySize()
    {
        CreateGallery(terminalWidth: 120).Render(useColour: false);

        var output = string.Join('\n', _lines);
        string[] labels =
        [
            "Clear sky", "Partly cloudy", "Overcast", "Fog", "Drizzle", "Freezing drizzle", "Rain",
            "Freezing rain", "Rain showers", "Snow", "Snow grains", "Snow showers", "Thunderstorm", "Hailstorm"
        ];

        Assert.All(labels, label => Assert.Equal(3, CountLabel(label)));
        Assert.Contains("LARGE", output);
        Assert.Contains("MEDIUM", output);
        Assert.Contains("SMALL", output);
        Assert.Contains("----88888----", output);   // large sun
        Assert.Contains("/*/*/*/*/", output);       // large hail between bolts

        int CountLabel(string label) =>
            _lines.Count(l => l.Split("  ", StringSplitOptions.RemoveEmptyEntries).Any(part => part.Trim() == label));
    }

    [Theory]
    [InlineData(60)]
    [InlineData(80)]
    [InlineData(160)]
    public void Render_NeverPrintsALineWiderThanTheTerminal(int terminalWidth)
    {
        CreateGallery(terminalWidth).Render(useColour: false);

        Assert.All(_lines, line => Assert.True(line.Length <= terminalWidth, $"{line.Length} > {terminalWidth}: {line}"));
    }

    [Fact]
    public void Render_PassesColourChoiceAndArtKeyToColouriser()
    {
        CreateGallery(terminalWidth: 120).Render(useColour: true);

        A.CallTo(() => _artColouriser.Colourise(A<string>._, "thunderstorm_hail", true)).MustHaveHappened();
        A.CallTo(() => _artColouriser.Colourise(A<string>._, A<string>._, false)).MustNotHaveHappened();
    }
}
