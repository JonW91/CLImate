namespace CLImate.App.Cli;

public sealed class ConsoleIO : IConsoleIO
{
    public string? ReadLine() => Console.ReadLine();

    public void Write(string text) => Console.Write(text);

    public void WriteLine(string text = "") => Console.WriteLine(text);
}
