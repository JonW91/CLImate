using CLImate.App.Cli;

namespace CLImate.Tests.Cli;

public sealed class TerminalInfoTests
{
    [Fact]
    public void Width_ReturnsPositiveValue()
    {
        var terminalInfo = new TerminalInfo();

        var width = terminalInfo.Width;

        Assert.True(width > 0);
    }

    [Fact]
    public void Height_ReturnsPositiveValue()
    {
        var terminalInfo = new TerminalInfo();

        var height = terminalInfo.Height;

        Assert.True(height > 0);
    }

    [Fact]
    public void IsRedirected_ReturnsBooleanValue()
    {
        var terminalInfo = new TerminalInfo();

        // Just verify it doesn't throw
        _ = terminalInfo.IsRedirected;
    }
}
