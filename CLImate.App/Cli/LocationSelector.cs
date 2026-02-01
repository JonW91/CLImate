using CLImate.App.Models;
using CLImate.App.Rendering;

namespace CLImate.App.Cli;

public interface ILocationSelector
{
    GeoResult? SelectLocation(IReadOnlyList<GeoResult> results);
}

public sealed class LocationSelector : ILocationSelector
{
    private readonly IConsoleIO _console;
    private readonly ILocationFormatter _formatter;

    public LocationSelector(IConsoleIO console, ILocationFormatter formatter)
    {
        _console = console;
        _formatter = formatter;
    }

    public GeoResult? SelectLocation(IReadOnlyList<GeoResult> results)
    {
        if (results.Count == 0)
        {
            return null;
        }

        if (results.Count == 1)
        {
            return results[0];
        }

        _console.WriteLine("Select a location:");
        for (var i = 0; i < results.Count; i++)
        {
            _console.WriteLine($"  {i + 1}. {_formatter.Format(results[i])}");
        }

        var input = Prompt($"Choose 1-{results.Count} (default 1): ");
        if (string.IsNullOrWhiteSpace(input))
        {
            return results[0];
        }

        if (int.TryParse(input, out var index) && index >= 1 && index <= results.Count)
        {
            return results[index - 1];
        }

        _console.WriteLine("Invalid selection.");
        return null;
    }

    private string Prompt(string prompt)
    {
        _console.Write(prompt);
        return _console.ReadLine() ?? string.Empty;
    }
}
