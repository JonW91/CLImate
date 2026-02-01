# Changes Since Last Commit

## Architecture and Structure
- Split the CLI into focused folders: `Cli`, `Models`, `Services`, `Rendering`, and `Composition`.
- Added a composition root to centralize dependency injection setup.
- Reduced the entry point to a minimal bootstrap (`Program.cs`).

## Services and Mapping
- Introduced `IJsonHttpClient` for HTTP + JSON responsibilities.
- Added `ApiMapper` to isolate API DTO -> domain mapping.
- Geocoding and forecast services now use the mapper layer.

## Domain Models
- Added domain forecast models (`Forecast`, `DailyForecast`, `ForecastUnits`) for renderer/CLI use.
- Kept API DTOs in `ForecastModels.cs` with JSON property mappings.

## Rendering and Output
- Renderer now consumes domain models only.
- Added ANSI color support with optional `--no-color` / `--color` flags.
- Temperature values are color-coded (blue/cool, yellow/medium, red/hot).
- ASCII art is tinted by weather type (sun = yellow, rain = blue, clouds = gray, snow/lightning = white).

## CLI
- Added `CliOptions` and a parser for validation and option handling.
- Added help text updates for color toggles.

## Dependencies
- Added `Microsoft.Extensions.DependencyInjection` for DI.
