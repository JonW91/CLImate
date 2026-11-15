using CLImate.App.Rendering;
using FakeItEasy;

namespace CLImate.Tests.Rendering;

public sealed class ArtColouriserTests
{
    private readonly ArtColouriser _colouriser;

    public ArtColouriserTests()
    {
        // Tag each coloured run so tests can read which colour every character got.
        var ansi = A.Fake<IAnsiColouriser>();
        A.CallTo(() => ansi.Apply(A<string>._, A<AnsiColour>._, A<bool>._))
            .ReturnsLazily((string text, AnsiColour colour, bool _) => $"<{colour}:{text}>");
        _colouriser = new ArtColouriser(ansi);
    }

    [Fact]
    public void Colourise_WhenDisabled_ReturnsArtUnchanged()
    {
        Assert.Equal("-(  ).-", _colouriser.Colourise("-(  ).-", "partly_cloudy", enabled: false));
    }

    [Theory]
    [InlineData("-(    ).- ", "<BoldYellow:-><Grey:(><Default:    ><Grey:).><BoldYellow:-><Default: >")]
    [InlineData("   --(   (    ).--", "<Default:   ><BoldYellow:--><Grey:(><Default:   ><Grey:(><Default:    ><Grey:).><BoldYellow:-->")]
    public void Colourise_PartlyCloudy_SideRaysAreYellow(string line, string expected)
    {
        Assert.Equal(expected, _colouriser.Colourise(line, "partly_cloudy", enabled: true));
    }

    [Fact]
    public void Colourise_PartlyCloudy_CloudOutlineHyphensStayGrey()
    {
        Assert.Equal("<Default:  ><Grey:.--.--.>", _colouriser.Colourise("  .--.--.", "partly_cloudy", enabled: true));
    }

    [Fact]
    public void Colourise_PartlyCloudy_HandlesMultiLineArt()
    {
        var result = _colouriser.Colourise(" .--.\n-(  ).-", "partly_cloudy", enabled: true);

        Assert.Equal("<Default: ><Grey:.--.>\n<BoldYellow:-><Grey:(><Default:  ><Grey:).><BoldYellow:->", result);
    }

    [Fact]
    public void Colourise_Clear_EveryCharacterIsYellow()
    {
        Assert.Equal("<Default:   ><BoldYellow:----88888---->", _colouriser.Colourise("   ----88888----", "clear", enabled: true));
    }

    [Fact]
    public void Colourise_PartlyCloudy_DiagonalRaysMatchSideRays()
    {
        Assert.Equal("<Default: ><BoldYellow:\\><Default: ><BoldYellow:|><Default: ><BoldYellow:/>", _colouriser.Colourise(" \\ | /", "partly_cloudy", enabled: true));
    }

    [Fact]
    public void Colourise_Overcast_LineEndHyphensAreCloudNotRays()
    {
        Assert.Equal("<Grey:-(><Default:  ><Grey:).->", _colouriser.Colourise("-(  ).-", "overcast", enabled: true));
    }

    [Fact]
    public void Colourise_Thunderstorm_BoltsBoldYellowCloudBoldDarkGrey()
    {
        Assert.Equal(
            "<BoldDarkGrey:(__.__)>\n<Default:  ><BoldYellow:/><Default: ><BoldYellow:\\>",
            _colouriser.Colourise("(__.__)\n  / \\", "thunderstorm", enabled: true));
    }

    // Thick rain-bearing cloud is dark; everything else is the lighter grey.
    [Theory]
    [InlineData("rain", "BoldDarkGrey")]
    [InlineData("freezing_rain", "BoldDarkGrey")]
    [InlineData("rain_showers", "BoldDarkGrey")]
    [InlineData("thunderstorm", "BoldDarkGrey")]
    [InlineData("thunderstorm_hail", "BoldDarkGrey")]
    [InlineData("drizzle", "Grey")]
    [InlineData("freezing_drizzle", "Grey")]
    [InlineData("overcast", "Grey")]
    [InlineData("partly_cloudy", "Grey")]
    [InlineData("snow", "Grey")]
    [InlineData("snow_grains", "Grey")]
    [InlineData("snow_showers", "Grey")]
    [InlineData("unknown", "Grey")]
    public void Colourise_CloudColourFollowsCondition(string key, string expectedColour)
    {
        Assert.Equal($"<{expectedColour}:(__.__)__)>", _colouriser.Colourise("(__.__)__)", key, enabled: true));
    }

    [Fact]
    public void Colourise_ThunderstormHail_BoltsBoldYellowHailWhite()
    {
        Assert.Equal(
            "<Default:  ><BoldYellow:/><White:*><BoldYellow:/><White:*><BoldYellow:/>",
            _colouriser.Colourise("  /*/*/", "thunderstorm_hail", enabled: true));
    }
}
