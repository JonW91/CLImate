namespace CLImate.App.Cli;

public sealed class CliOptionsParseResult
{
    private CliOptionsParseResult(CliOptions options, bool isValid, string? errorMessage)
    {
        Options = options;
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public CliOptions Options { get; }
    public bool IsValid { get; }
    public string? ErrorMessage { get; }

    public static CliOptionsParseResult Success(CliOptions options) => new(options, true, null);

    public static CliOptionsParseResult Failure(string errorMessage)
        => new(new CliOptions(), false, errorMessage);
}
