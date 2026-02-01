# Project Plan

## Goals
- Maintain a clean single-project CLI with clear separation of concerns.
- Keep DTOs isolated from rendering and CLI logic.
- Provide readable output with optional ANSI coloring.
- Improve ASCII art legibility with weather-appropriate color cues.

## Completed
- Composition root for DI registration.
- Folder structure split by responsibility.
- API mapper layer introduced.
- Domain forecast models added.
- ANSI color support with `--no-color` and `--color` flags.
- Per-character colorization for weather art (clouds/rain/snow/lightning).
- Optional warnings integration with per-day warning lines.
- Weather warnings service (MeteoBlue integration) implemented.
- Forecast model extended to include warnings by date.
- CLI updated to fetch and display warnings.

## In Progress (Uncommitted Changes)
- Weather warnings feature fully integrated (needs commit).
- Placeholder "Weather warnings: Not available yet" message still showing (should be removed).

## Next Steps
- [ ] Clean up and commit weather warnings feature.
- [ ] Remove placeholder "not available yet" message from CLI output.
- [ ] Add unit tests for `ApiMapper`, `TemperatureColorScale`, and CLI parsing.
- [ ] Add unit tests for `WeatherWarningsService` and `MeteoBlueWarningsClient`.
- [ ] Make temperature thresholds configurable (config file or CLI flags).
- [ ] Add a diagnostics mode for capturing raw API responses when needed.
