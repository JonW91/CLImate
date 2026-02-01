# CLImate

CLImate is a terminal weather app. Type a location, and it prints a 7-day forecast with ASCII art and plain-text summaries that work across common terminals.

## Features

- Location lookup by name (international)
- 7-day forecast with highs/lows, wind, and precipitation
- Today-only view with morning/afternoon/evening splits when available
- ASCII art for conditions (no special terminal requirements)
- Metric or imperial units

## Usage

```bash
dotnet run --project CLImate.App -- "London, UK"
dotnet run --project CLImate.App -- --units imperial "New York, NY"
dotnet run --project CLImate.App -- --country GB "Edinburgh, Scotland"
dotnet run --project CLImate.App -- --today "Seattle, WA"
```

If multiple locations match, CLImate will prompt you to pick one. You can also pass a 2-letter country code to narrow results.

CLImate also ships with a configurable country-name mapping file. You can add or edit entries in:
`CLImate.App/Assets/country-codes.json`

## Data sources

- Open-Meteo Geocoding + Forecast APIs

## API details (current usage)

CLImate calls Open-Meteo with the following parameters:

Geocoding:
- Endpoint: https://geocoding-api.open-meteo.com/v1/search
- Parameters: `name`, `count`, `language`, `format`
- Docs: https://open-meteo.com/en/docs/geocoding-api

Forecast:
- Endpoint: https://api.open-meteo.com/v1/forecast
- Parameters: `latitude`, `longitude`, `daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_sum,wind_speed_10m_max,wind_gusts_10m_max`, `hourly=weather_code,temperature_2m,precipitation,wind_speed_10m,wind_gusts_10m`, `timezone=auto`, plus unit parameters when needed.
- Docs: https://open-meteo.com/en/docs

## Notes

Weather warnings are displayed when the `METEOBLUE_API_KEY` environment variable is set with a valid MeteoBlue API key. Without an API key, all days will show "Warning: none".

## Development

Run tests:
```bash
dotnet test CLImate.Tests/CLImate.Tests.csproj
```

Build the solution:
```bash
dotnet build
```

## Install (no runtime required)

You can install a platform-specific, self-contained binary that does not require .NET to be installed.

```bash
curl -fsSL https://raw.githubusercontent.com/JonW91/CLImate/main/scripts/install.sh | sh
```

The installer auto-detects OS/architecture and downloads the matching release asset.

For Windows PowerShell:

```powershell
irm https://raw.githubusercontent.com/JonW91/CLImate/main/scripts/install.ps1 | iex
```

### Release assets naming

```
climate-linux-x64.tar.gz
climate-linux-arm64.tar.gz
climate-macos-x64.tar.gz
climate-macos-arm64.tar.gz
climate-windows-x64.zip
```

## Publishing binaries

Use the helper script to build self-contained, single-file binaries:

```bash
./scripts/publish.sh
```

## Releases

Push a tag like `v0.1.0` to trigger a GitHub Actions build and attach binaries.

Create a release tag with:

```bash
./scripts/tag-release.sh 0.1.0
git push origin v0.1.0
```
