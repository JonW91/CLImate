namespace CLImate.App.Cli;

public interface ITerminalInfo
{
    int Width { get; }
    int Height { get; }
    bool IsRedirected { get; }
}

public sealed class TerminalInfo : ITerminalInfo
{
    private const int DefaultWidth = 80;
    private const int DefaultHeight = 24;

    public int Width
    {
        get
        {
            try
            {
                return Console.IsOutputRedirected ? DefaultWidth : Console.WindowWidth;
            }
            catch
            {
                return DefaultWidth;
            }
        }
    }

    public int Height
    {
        get
        {
            try
            {
                return Console.IsOutputRedirected ? DefaultHeight : Console.WindowHeight;
            }
            catch
            {
                return DefaultHeight;
            }
        }
    }

    public bool IsRedirected
    {
        get
        {
            try
            {
                return Console.IsOutputRedirected;
            }
            catch
            {
                return false;
            }
        }
    }
}
