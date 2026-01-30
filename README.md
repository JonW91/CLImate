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
dotnet run --project CLImate.App -- --country GB "Edinburgh, Scotland"
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
- Parameters: `latitude`, `longitude`, `daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_sum,wind_speed_10m_max,wind_gusts_10m_max`, `timezone=auto`, plus unit parameters when needed.
- Docs: https://open-meteo.com/en/docs

## Notes

- Weather warnings are not implemented yet (planned for a future release).
