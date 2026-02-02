# Contributing to CLImate

Thank you for your interest in contributing to CLImate! This document provides guidelines for contributing to the project.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Git
- A terminal that supports UTF-8 and ANSI colours (for testing output)

### Setting Up

```bash
# Clone the repository
git clone https://github.com/JonW91/CLImate.git
cd CLImate

# Build the solution
dotnet build

# Run tests
dotnet test

# Run the application
dotnet run --project CLImate.App -- London
```

## Development Workflow

### Branch Naming

- `feature/description` - New features
- `fix/description` - Bug fixes
- `docs/description` - Documentation updates
- `refactor/description` - Code refactoring

### Commit Messages

Use clear, descriptive commit messages:

```
feat: add hourly forecast option
fix: handle network timeout gracefully
docs: update installation instructions
test: add unit tests for LocationInputParser
refactor: extract weather code mapping to catalogue
```

### Pull Request Process

1. Fork the repository
2. Create a feature branch from `main`
3. Make your changes
4. Ensure tests pass: `dotnet test`
5. Ensure build succeeds: `dotnet build`
6. Submit a pull request

## Code Style

### General Guidelines

- Follow existing code patterns and naming conventions
- Use meaningful variable and method names
- Keep methods focused and small
- Add XML documentation for public APIs

### Specific Conventions

- **Interfaces**: Prefix with `I` (e.g., `IForecastService`)
- **Async methods**: Suffix with `Async` (e.g., `GetForecastAsync`)
- **Private fields**: Prefix with `_` (e.g., `_console`)
- **Constants**: Use `PascalCase`

### Example

```csharp
public interface IWeatherService
{
    Task<Forecast?> GetForecastAsync(double latitude, double longitude, CancellationToken cancellationToken);
}

public sealed class WeatherService : IWeatherService
{
    private readonly IHttpClient _client;

    public WeatherService(IHttpClient client)
    {
        _client = client;
    }

    public async Task<Forecast?> GetForecastAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

## Project Structure

```
CLImate/
├── CLImate.App/
│   ├── Assets/           # JSON data files (ASCII art, country codes)
│   ├── Cli/              # Command-line interface components
│   ├── Composition/      # Dependency injection setup
│   ├── Models/           # Data models and DTOs
│   ├── Rendering/        # Output formatting and display
│   └── Services/         # Business logic and API clients
├── CLImate.Tests/
│   ├── Models/           # Model tests
│   └── Services/         # Service tests
├── scripts/              # Build and publish scripts
└── README.md
```

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~WeatherWarningsServiceTests"
```

### Writing Tests

- Use xUnit for test framework
- Use FakeItEasy for mocking
- Follow Arrange-Act-Assert pattern
- Test edge cases and error conditions

```csharp
[Fact]
public async Task GetForecastAsync_ValidCoordinates_ReturnsForecast()
{
    // Arrange
    var client = A.Fake<IJsonHttpClient>();
    A.CallTo(() => client.GetAsync<ForecastResponse>(A<string>._, A<CancellationToken>._))
        .Returns(Task.FromResult(new ForecastResponse { /* ... */ }));
    
    var service = new ForecastService(client, new ApiMapper());

    // Act
    var result = await service.GetForecastAsync(51.5, -0.1, Units.Metric, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.Days);
}
```

## Areas for Contribution

### High Priority

- **Error handling**: Improve network error messages
- **CI/CD**: GitHub Actions workflow for releases
- **Package managers**: Homebrew formula, Chocolatey package
- **Tests**: Increase coverage, add integration tests

### Medium Priority

- **Configuration**: User preferences file support
- **Caching**: Reduce repeated API calls
- **Output formats**: JSON/CSV export

### Low Priority

- **Internationalisation**: Localised strings
- **New features**: Hourly forecasts, alerts

See [ROADMAP.md](ROADMAP.md) for the full development plan.

## Reporting Issues

When reporting bugs, please include:

- Operating system and version
- .NET version (`dotnet --version`)
- Terminal application
- Steps to reproduce
- Expected vs actual behaviour
- Error messages (if any)

## Questions?

Open an issue with the `question` label or start a discussion.

---

Thank you for contributing! 🌤️
