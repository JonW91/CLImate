namespace CLImate.App.Rendering;

public interface ITemperatureColorScale
{
    AnsiColor GetColor(double value);
}

public sealed class TemperatureColorScale : ITemperatureColorScale
{
    private const double ColdMax = 5;
    private const double WarmMax = 20;

    public AnsiColor GetColor(double value)
    {
        if (double.IsNaN(value))
        {
            return AnsiColor.Default;
        }

        if (value <= ColdMax)
        {
            return AnsiColor.Blue;
        }

        if (value <= WarmMax)
        {
            return AnsiColor.Yellow;
        }

        return AnsiColor.Red;
    }
}
