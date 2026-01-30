# CLImate

CLImate is a terminal weather app. Type a location, and it prints a 7-day forecast with ASCII art and plain-text summaries that work across common terminals.

## Features

- Location lookup by name (international)
- 7-day forecast with highs/lows, wind, and precipitation
- ASCII art for conditions (no special terminal requirements)
- Metric or imperial units

## Usage

```bash
dotnet run --project CLImate.App -- "London, UK"
dotnet run --project CLImate.App -- --units imperial "New York, NY"
```

If multiple locations match, CLImate will prompt you to pick one.

## Data sources

- Open-Meteo Geocoding + Forecast APIs

## Notes

- Weather warnings are not implemented yet (planned for a future release).
