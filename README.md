# CLImate 🌤️

A cross-platform command-line weather forecast application built with .NET 10. Get ASCII art weather forecasts directly in your terminal.

![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)
![License](https://img.shields.io/badge/license-MIT-blue)
![Beta](https://img.shields.io/badge/status-beta-orange)

## Features

- 🌍 **Global location search** - Search by city, region, or address worldwide
- 🎨 **ASCII art weather** - Weather conditions displayed with ANSI colours
- 📊 **7-day forecasts** - Daily high/low temperatures, precipitation, wind speeds
- 🕐 **Today view** - Morning/afternoon/evening/night table with ASCII art per time period
- ⚠️ **Weather warnings** - Severe weather alerts (US via NWS, EU via Meteoalarm; other regions show "no warnings available")
- 🌡️ **Metric/Imperial units** - Switch between measurement systems
- 🖥️ **Cross-platform** - Works on Windows, macOS, and Linux terminals
- 📦 **Self-contained** - No .NET runtime required (standalone binaries available)
- 📐 **Adaptive layout** - Automatically chooses horizontal table or vertical list based on terminal width

## Quick Start

```bash
# Get weather for a location
climate London

# Use imperial units
climate --units imperial "New York, NY"

# Today's forecast only with time segments
climate --today Paris

# 24-hour hourly forecast
climate --hourly London

# Filter by country code (avoid ambiguity)
climate --country US Portland
```

## Installation

### One-Line Install (Recommended)

Download and install the self-contained binary - **no .NET runtime required**:

**Linux/macOS:**
```bash
curl -fsSL https://raw.githubusercontent.com/JonW91/CLImate/main/scripts/install.sh | sh
```

**Windows (PowerShell):**
```powershell
irm https://raw.githubusercontent.com/JonW91/CLImate/main/scripts/install.ps1 | iex
```

### Manual Download

Download the binary for your platform from the [latest release](https://github.com/JonW91/CLImate/releases/latest):

- `climate-linux-x64.tar.gz` - Linux (Intel/AMD 64-bit)
- `climate-linux-arm64.tar.gz` - Linux (ARM 64-bit, e.g., Raspberry Pi)
- `climate-macos-x64.tar.gz` - macOS (Intel)
- `climate-macos-arm64.tar.gz` - macOS (Apple Silicon)
- `climate-windows-x64.zip` - Windows (64-bit)

Extract and add to your PATH:

```bash
# Linux/macOS
tar -xzf climate-*.tar.gz
sudo mv climate /usr/local/bin/

# Windows (PowerShell)
Expand-Archive climate-windows-x64.zip -DestinationPath $env:LOCALAPPDATA\CLImate
# Add to PATH via System Settings
```

### Build from Source

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```bash
git clone https://github.com/JonW91/CLImate.git
cd CLImate/CLImate.App
dotnet run -- London
```

## Usage Examples

### Basic Weather

```bash
# Current weather and 7-day forecast
climate London

# Specific region to avoid ambiguity
climate "London, Ontario" --country CA
climate Portland --country US
```

### Different Views

```bash
# Today's weather broken down by time periods
climate --today "San Francisco, CA"

# 24-hour hourly forecast
climate --hourly Tokyo

# Force horizontal table layout (if terminal is wide enough)
climate --horizontal Paris

# Force vertical list layout
climate --vertical "New York"
```

### Units and Formatting

```bash
# Use imperial units (Fahrenheit, mph, inches)
climate --units imperial Chicago

# Use metric units (Celsius, km/h, mm) - default
climate --units metric Berlin

# Enable colours (default if terminal supports it)
climate --colour London

# Disable colours
climate --no-colour London

# Disable ASCII art (show text labels instead)
climate --no-art London
```

### Advanced Options

```bash
# Specify country code to filter location search
climate --country DE Frankfurt  # Frankfurt, Germany
climate --country US Frankfurt  # Frankfurt, Kentucky

# Show help
climate --help

# Show version
climate --version
```

### Output Examples

**7-Day Forecast (Vertical Layout):**
```
CLImate - Forecast for London, England, United Kingdom
----------------------------------------------------

7-Day Forecast
────────────────────────────────────────

┌─ TODAY · Tue 10 Mar ─────────────────────────
│  Drizzle
│        .--.    
│     .-(    ).  
│    (__.__)__)  
│     ' ' ' '    
│     ' ' ' '    
│     ' ' ' '    
│
│  Temp: 5.4°C -> 9°C
│  Rain: 1mm
│  Wind: 32km/h (gusts 61km/h)
└──────────────────────────────────────
```

**Today's Forecast (Horizontal Table):**
```
Today's Forecast · Monday 10 March
───────────────────────────────────────────────────────────────────────────
│         │ Morning    │ Afternoon  │ Evening    │ Night      │
│         │ 6-12       │ 12-18      │ 18-24      │ 0-6        │
├─────────┼────────────┼────────────┼────────────┼────────────┤
│ Weather │ Drizzle    │ Overcast   │ Clear sky  │ Clear sky  │
│         │    .--.    │    .--.    │  \ | /     │  \ | /     │
│         │ .-(    ).  │ .-(    ).  │ -{(@)}--   │ -{(@)}--   │
│         │(__.__)__)  │(__.__)__)  │  / | \     │  / | \     │
│         │ ' ' ' '    │            │            │            │
├─────────┼────────────┼────────────┼────────────┼────────────┤
│ Temp    │ 6°C        │ 8°C        │ 7°C        │ 5°C        │
│ Rain    │ 0.2mm      │ 0mm        │ 0mm        │ 0mm        │
│ Wind    │ 12km/h     │ 15km/h     │ 8km/h      │ 6km/h      │
└─────────┴────────────┴────────────┴────────────┴────────────┘
```

## Configuration

CLImate reads configuration from:
- **Linux/macOS:** `~/.config/climate/config.json`
- **Windows:** `%APPDATA%\climate\config.json`

### Example Configuration

```json
{
  "defaultUnits": "metric",
  "defaultCountry": "GB",
  "showArt": true,
  "useColour": true,
  "favouriteLocations": [
    { "name": "Home", "lat": 51.5074, "lon": -0.1278 },
    { "name": "Work", "lat": 51.5155, "lon": -0.0922 }
  ]
}
```

## Command Reference

```
USAGE:
    climate [OPTIONS] [LOCATION]

ARGUMENTS:
    <LOCATION>    Location to get weather for (city, region, address)

OPTIONS:
    -u, --units <UNITS>             Units: metric, imperial [default: metric]
    -c, --country <CODE>            Two-letter country code (GB, US, DE, etc.)
    -t, --today                     Show today's forecast only
        --hourly                    Show 24-hour forecast
    -H, --horizontal                Force horizontal table layout
    -V, --vertical                  Force vertical list layout
        --colour                    Force ANSI colours on
        --no-colour                 Disable ANSI colours
        --no-art                    Disable ASCII art (use text labels)
    -h, --help                      Show help message
        --version                   Show version information
```

## Data Sources

- **Weather Data:** [Open-Meteo](https://open-meteo.com/) - Free, no API key required
- **Location Search:** [Open-Meteo Geocoding API](https://open-meteo.com/en/docs/geocoding-api)
- **Weather Warnings:** 
  - **US:** [National Weather Service](https://www.weather.gov/)
  - **Europe:** [Meteoalarm](https://www.meteoalarm.eu/)
  - **Other regions:** "No warnings available for this region"

## Technical Details

- **Framework:** .NET 10
- **Platforms:** Linux (x64, ARM64), macOS (x64, ARM64), Windows (x64)
- **Dependencies:** None (self-contained binaries)
- **Terminal Requirements:** 
  - Minimum 45 columns for ASCII art
  - ANSI color support recommended
  - Unicode support for weather symbols

## Development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Linux, macOS, or Windows

### Build and Test

```bash
# Clone and build
git clone https://github.com/JonW91/CLImate.git
cd CLImate

# Run tests
dotnet test

# Run locally
cd CLImate.App
dotnet run -- London

# Publish self-contained binary
./scripts/publish.sh
```

### Project Structure

```
CLImate/
├── CLImate.App/           # Main application
│   ├── Assets/            # ASCII art and data files
│   ├── Cli/              # Command-line parsing
│   ├── Models/           # Data models
│   ├── Rendering/        # Display logic and ASCII art
│   └── Services/         # API clients and business logic
├── CLImate.Tests/         # Unit tests
├── scripts/              # Build and installation scripts
└── packaging/            # Package manager configurations
```

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Priority Areas

- **Platform testing** - Test on different terminals and operating systems
- **Accessibility** - Screen reader compatibility, high contrast modes
- **Localization** - Translations for weather descriptions
- **Performance** - Caching, request optimization

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Open-Meteo](https://open-meteo.com/) for providing free weather data
- ASCII art inspired by [wttr.in](https://wttr.in/)
- Community contributors and testers