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
    private readonly ITerminalInfo _terminalInfo;

    public LocationSelector(IConsoleIO console, ILocationFormatter formatter, ITerminalInfo terminalInfo)
    {
        _console = console;
        _formatter = formatter;
        _terminalInfo = terminalInfo;
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

        // In non-interactive (piped) mode, silently return the top result.
        if (_terminalInfo.IsInputRedirected)
        {
            return results[0];
        }

        _console.WriteLine("Select a location:");
        for (var i = 0; i < results.Count; i++)
        {
            _console.WriteLine($"  {i + 1}. {_formatter.Format(results[i])}");
        }

        if (TryReadSelection(results, out var first))
            return first;

        // One re-prompt on invalid input.
        _console.WriteLine($"Please enter a number between 1 and {results.Count}.");
        if (TryReadSelection(results, out var second))
            return second;

        _console.WriteLine("No valid selection made.");
        return null;
    }

    private bool TryReadSelection(IReadOnlyList<GeoResult> results, out GeoResult? selected)
    {
        var input = Prompt($"Choose 1-{results.Count} (default 1): ");
        if (string.IsNullOrWhiteSpace(input))
        {
            selected = results[0];
            return true;
        }

        if (int.TryParse(input, out var index) && index >= 1 && index <= results.Count)
        {
            selected = results[index - 1];
            return true;
        }

        selected = null;
        return false;
    }

    private string Prompt(string prompt)
    {
        _console.Write(prompt);
        return _console.ReadLine() ?? string.Empty;
    }
}
