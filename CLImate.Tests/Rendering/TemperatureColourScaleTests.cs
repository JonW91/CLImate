using CLImate.App.Rendering;

namespace CLImate.Tests.Rendering;

public sealed class TemperatureColourScaleTests
{
    private readonly TemperatureColourScale _temperatureColourScale;

    public TemperatureColourScaleTests()
    {
        _temperatureColourScale = new TemperatureColourScale();
    }

    [Theory]
    [InlineData(-10.0)]
    [InlineData(-5.0)]
    [InlineData(0.0)]
    [InlineData(5.0)]
    public void GetColour_WithColdTemperatures_ReturnsBlue(double temperature)
    {
        // Act
        var result = _temperatureColourScale.GetColour(temperature);

        // Assert
        Assert.Equal(AnsiColour.Blue, result);
    }

    [Theory]
    [InlineData(5.1)]
    [InlineData(10.0)]
    [InlineData(15.0)]
    [InlineData(20.0)]
    public void GetColour_WithWarmTemperatures_ReturnsYellow(double temperature)
    {
        // Act
        var result = _temperatureColourScale.GetColour(temperature);

        // Assert
        Assert.Equal(AnsiColour.Yellow, result);
    }

    [Theory]
    [InlineData(20.1)]
    [InlineData(25.0)]
    [InlineData(35.0)]
    [InlineData(50.0)]
    public void GetColour_WithHotTemperatures_ReturnsRed(double temperature)
    {
        // Act
        var result = _temperatureColourScale.GetColour(temperature);

        // Assert
        Assert.Equal(AnsiColour.Red, result);
    }

    [Fact]
    public void GetColour_WithNanValue_ReturnsDefault()
    {
        // Act
        var result = _temperatureColourScale.GetColour(double.NaN);

        // Assert
        Assert.Equal(AnsiColour.Default, result);
    }

    [Fact]
    public void GetColour_WithColdMaxBoundary_ReturnsBlue()
    {
        // The constant ColdMax is 5, so exactly 5.0 should return Blue
        // Act
        var result = _temperatureColourScale.GetColour(5.0);

        // Assert
        Assert.Equal(AnsiColour.Blue, result);
    }

    [Fact]
    public void GetColour_WithWarmMaxBoundary_ReturnsYellow()
    {
        // The constant WarmMax is 20, so exactly 20.0 should return Yellow
        // Act
        var result = _temperatureColourScale.GetColour(20.0);

        // Assert
        Assert.Equal(AnsiColour.Yellow, result);
    }

    [Fact]
    public void GetColour_WithJustAboveColdMax_ReturnsYellow()
    {
        // Just above 5.0 should transition to Yellow
        // Act
        var result = _temperatureColourScale.GetColour(5.01);

        // Assert
        Assert.Equal(AnsiColour.Yellow, result);
    }

    [Fact]
    public void GetColour_WithJustAboveWarmMax_ReturnsRed()
    {
        // Just above 20.0 should transition to Red
        // Act
        var result = _temperatureColourScale.GetColour(20.01);

        // Assert
        Assert.Equal(AnsiColour.Red, result);
    }

    [Theory]
    [InlineData(double.MinValue)]
    [InlineData(-273.15)] // Absolute zero
    [InlineData(-100.0)]
    public void GetColour_WithExtremelyLowTemperatures_ReturnsBlue(double temperature)
    {
        // Act
        var result = _temperatureColourScale.GetColour(temperature);

        // Assert
        Assert.Equal(AnsiColour.Blue, result);
    }

    [Theory]
    [InlineData(100.0)]
    [InlineData(500.0)]
    [InlineData(double.MaxValue)]
    public void GetColour_WithExtremelyHighTemperatures_ReturnsRed(double temperature)
    {
        // Act
        var result = _temperatureColourScale.GetColour(temperature);

        // Assert
        Assert.Equal(AnsiColour.Red, result);
    }

    [Theory]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void GetColour_WithInfinityValues_ReturnsCorrectColours(double temperature)
    {
        // Act
        var result = _temperatureColourScale.GetColour(temperature);

        // Assert
        if (double.IsPositiveInfinity(temperature))
        {
            Assert.Equal(AnsiColour.Red, result);
        }
        else if (double.IsNegativeInfinity(temperature))
        {
            Assert.Equal(AnsiColour.Blue, result);
        }
    }

    [Fact]
    public void GetColour_ImplementsCorrectInterface()
    {
        // Assert
        Assert.IsAssignableFrom<ITemperatureColourScale>(_temperatureColourScale);
    }
}