namespace CLImate.App.Cli;

public interface ICliHelp
{
    void Print();
}

public sealed class CliHelp : ICliHelp
{
    private readonly IConsoleIO _console;

    public CliHelp(IConsoleIO console)
    {
        _console = console;
    }

    public void Print()
    {
        _console.WriteLine("CLImate - CLI weather forecasts");
        _console.WriteLine();
        _console.WriteLine("Usage:");
        _console.WriteLine("  climate [options] <location>");
        _console.WriteLine();
        _console.WriteLine("Examples:");
        _console.WriteLine("  climate London");
        _console.WriteLine("  climate --units imperial \"New York, NY\"");
        _console.WriteLine("  climate --horizontal London    # Force table layout");
        _console.WriteLine();
        _console.WriteLine("Options:");
        _console.WriteLine("  -u, --units <metric|imperial>   Units for output (default: metric)");
        _console.WriteLine("  -c, --country <code>            2-letter country code filter (e.g., GB, US)");
        _console.WriteLine("  -t, --today                     Show only today's forecast");
        _console.WriteLine("  --hourly                        Show 24-hour forecast for today");
        _console.WriteLine("  -H, --horizontal                Force horizontal table layout");
        _console.WriteLine("  -V, --vertical                  Force vertical list layout");
        _console.WriteLine("  --no-art                        Disable ASCII art (use text labels)");
        _console.WriteLine("  --no-colour                     Disable ANSI colours");
        _console.WriteLine("  --colour                        Force ANSI colours on");
        _console.WriteLine("  -h, --help                      Show this help message");
        _console.WriteLine("  -v, --version                   Show version information");
    }
}
