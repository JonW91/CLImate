namespace CLImate.App.Cli;

public interface IConsoleIO
{
    string? ReadLine();
    void Write(string text);
    void WriteLine(string text = "");
}
