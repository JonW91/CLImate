# Copilot Instructions for CLImate

## Build, Test, and Run

```bash
# Build
dotnet build

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~WeatherWarningsServiceTests"

# Run the app locally
dotnet run --project CLImate.App -- London

# Publish self-contained binary
dotnet publish CLImate.App -c Release --self-contained -p:PublishSingleFile=true
```

### Constrained Environments

For memory-limited environments (e.g., Termux on Android), use GC limits:

```bash
DOTNET_GCServer=0 DOTNET_GCHeapHardLimitPercent=30 dotnet build
```

## Architecture

CLImate is a .NET 10 CLI weather app using constructor-based dependency injection via `Microsoft.Extensions.DependencyInjection`.

### Dependency Flow

```
Program.cs → AppComposition.BuildServiceProvider() → CliApplication.RunAsync()
```

All services are registered as singletons in `CLImate.App/Composition/AppComposition.cs`. The composition root wires everything together—add new services there.

### Layer Responsibilities

| Layer | Purpose |
|-------|---------|
| `Cli/` | Command parsing (`CliOptionsParser`), user interaction (`IConsoleIO`), terminal detection (`ITerminalInfo`) |
| `Services/` | External API clients (`ForecastService`, `GeocodingService`), business logic, data mapping |
| `Rendering/` | Console output formatting, ASCII art (`AsciiArtCatalogue`), ANSI colours, table layouts |
| `Models/` | DTOs and domain types (`Forecast`, `Units`, `CliOptions`) |
| `Assets/` | JSON data files (`ascii-art.json`, `country-codes.json`) copied to output |

### External APIs

- **Open-Meteo** (free, no key): Weather forecasts and geocoding
- **MeteoBlue** (optional, requires `METEOBLUE_API_KEY` env var): Weather warnings

## Key Conventions

### Interface Pattern

Every non-trivial class has an interface for testability:
- **New interfaces**: Place in a separate `Interfaces/` folder within each layer (e.g., `Services/Interfaces/IForecastService.cs`)
- All dependencies injected via constructor
- Prefix interfaces with `I`, suffix async methods with `Async`

### Testing

- Framework: xUnit with FakeItEasy for mocking
- Test structure mirrors `CLImate.App/` (e.g., `CLImate.Tests/Services/` tests `CLImate.App/Services/`)
- Pattern: Arrange-Act-Assert
- Mock external dependencies; test through interfaces

### Terminal Output

- All console I/O goes through `IConsoleIO` (never use `Console` directly in services)
- ASCII art is loaded from `Assets/ascii-art.json` via `IAsciiArtCatalogue`
- Colour is applied through `IAnsiColouriser`; respect `--no-colour` flag
- Layout adapts to terminal width via `ITerminalInfo`
- Supports `NO_COLOR` env var and `--no-colour` / `--colour` flags

### Adding a New CLI Option

1. Add property to `CliOptions.cs`
2. Parse in `CliOptionsParser.cs`
3. Update `CliHelp.cs` with usage text
4. Handle in `CliApplication.RunAsync()`

### Adding a New Weather Feature

1. Add model types in `Models/`
2. Create interface in `Services/Interfaces/`
3. Create implementation in `Services/`
4. Register in `AppComposition.cs`
5. Inject into `CliApplication` or relevant renderer
6. Add tests in corresponding `CLImate.Tests/` folder

## Project Status

See `ROADMAP.md` for current development priorities. Key completed items:
- Self-contained publishing for all platforms
- Adaptive terminal layout (horizontal/vertical based on width)
- CI/CD with GitHub Actions (build/test on Ubuntu, Windows, macOS)
- Extensive unit tests with xUnit/FakeItEasy

Planned: Homebrew tap fixes, configuration file support, caching
