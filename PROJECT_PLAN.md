# Project Plan

## Goals
- Maintain a clean single-project CLI with clear separation of concerns.
- Keep DTOs isolated from rendering and CLI logic.
- Provide readable output with optional ANSI coloring.

## Completed
- Composition root for DI registration.
- Folder structure split by responsibility.
- API mapper layer introduced.
- Domain forecast models added.
- ANSI color support with `--no-color` and `--color` flags.

## Next Steps
- Add unit tests for `ApiMapper`, `TemperatureColorScale`, and CLI parsing.
- Make temperature thresholds configurable (config file or CLI flags).
- Add a simple diagnostics mode for capturing raw API responses when needed.
- Consider a richer domain model for geocoding results if more UI logic is added.
