namespace CLImate.App.Rendering;

public interface ITemperatureColourScale
{
    AnsiColour GetColour(double value);
}

public sealed class TemperatureColourScale : ITemperatureColourScale
{
    private const double ColdMax = 5;
    private const double WarmMax = 20;

    public AnsiColour GetColour(double value)
    {
        if (double.IsNaN(value))
        {
            return AnsiColour.Default;
        }

        if (value <= ColdMax)
        {
            return AnsiColour.Blue;
        }

        if (value <= WarmMax)
        {
            return AnsiColour.Yellow;
        }

        return AnsiColour.Red;
    }
}
