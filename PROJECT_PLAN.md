# Project Plan

## Goals
- Maintain a clean single-project CLI with clear separation of concerns.
- Keep DTOs isolated from rendering and CLI logic.
- Provide readable output with optional ANSI coloring.
- Improve ASCII art legibility with weather-appropriate color cues.
- Maintain test coverage for core functionality.

## Completed
- Composition root for DI registration.
- Folder structure split by responsibility.
- API mapper layer introduced.
- Domain forecast models added.
- ANSI color support with `--no-color` and `--color` flags.
- Per-character colorization for weather art (clouds/rain/snow/lightning).
- Weather warnings integration with per-day warning lines.
- Weather warnings service (MeteoBlue integration) implemented.
- Forecast model extended to include warnings by date.
- CLI updated to fetch and display warnings.
- xUnit test project with FakeItEasy.
- Tests for WeatherWarningsService (7 tests).
- Tests for domain models: Forecast, DailyForecast, ForecastUnits, WeatherWarning (11 tests).

## Next Steps
- [ ] Add tests for MeteoBlueWarningsClient.
- [ ] Add tests for ApiMapper and TemperatureColorScale.
- [ ] Add tests for CLI argument parsing.
- [ ] Make temperature thresholds configurable (config file or CLI flags).
- [ ] Add a diagnostics mode for capturing raw API responses when needed.

