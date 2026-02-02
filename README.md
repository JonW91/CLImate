# CLImate 🌤️

A cross-platform command-line weather forecast application built with .NET 10. Get beautiful ASCII art weather forecasts directly in your terminal.

![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)
![License](https://img.shields.io/badge/license-MIT-blue)

## Features

- 🌍 **Global location search** - Search by city, region, or address worldwide
- 🎨 **Colourful ASCII art** - Weather conditions displayed with ANSI colours
- 📊 **7-day forecasts** - Daily high/low temperatures, precipitation, wind speeds
- 🕐 **Today view** - Morning/afternoon/evening splits when available
- ⚠️ **Weather warnings** - Integrated severe weather alerts (requires MeteoBlue API key)
- 🌡️ **Metric/Imperial units** - Switch between measurement systems
- 🖥️ **Cross-platform** - Works on Windows, macOS, and Linux terminals
- 📦 **Self-contained** - No .NET runtime required (standalone binaries available)

## Quick Start

```bash
# Get weather for a location
climate London

# Use imperial units
climate --units imperial "New York, NY"

# Today's forecast only with time segments
climate --today Paris

# Filter by country code (avoid ambiguity)
climate --country US Portland
```

## Installation

### Option 1: Download Self-Contained Binary (Recommended)

Download the latest release for your platform - **no .NET runtime required**:

**Linux/macOS:**
```bash
curl -fsSL https://raw.githubusercontent.com/JonW91/CLImate/main/scripts/install.sh | sh
```

**Windows PowerShell:**
```powershell
irm https://raw.githubusercontent.com/JonW91/CLImate/main/scripts/install.ps1 | iex
```

Or download directly from the [Releases](https://github.com/JonW91/CLImate/releases) page.

### Option 2: .NET Global Tool

If you have the [.NET 10 SDK](https://dotnet.microsoft.com/download) installed:

```bash
dotnet tool install --global CLImate
```

### Option 3: Build from Source

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/JonW91/CLImate.git
cd CLImate
dotnet build
dotnet run --project CLImate.App -- London
```


## Command-Line Options

| Option | Short | Description |
|--------|-------|-------------|
| `--help` | `-h` | Show help information |
| `--units <metric\|imperial>` | `-u` | Set temperature/wind units (default: metric) |
| `--country <code>` | `-c` | Filter by 2-letter country code (e.g., GB, US) |
| `--today` | `-t` | Show today's forecast with time segments |
| `--no-art` | | Disable ASCII art, use text labels |
| `--no-colour` | | Disable ANSI colour output |
| `--colour` | | Force ANSI colours on |

## Usage Examples

```bash
# Basic usage
climate Manchester

# Disambiguate locations with country code
climate -c GB Manchester
climate -c US Portland

# Imperial units for US users
climate -u imperial Chicago

# Minimal output for scripts/piping
climate --no-art --no-colour Seattle

# Detailed today forecast with time periods
climate --today "San Francisco, CA"
```

## How It Works

If multiple locations match your query, CLImate will prompt you to pick one. Use the `--country` flag with a 2-letter code to narrow results.

CLImate also ships with a configurable country-name mapping file at `CLImate.App/Assets/country-codes.json`.

## Data Sources

| Data | Provider | API Key Required |
|------|----------|------------------|
| Weather forecasts | [Open-Meteo](https://open-meteo.com/) | No (free) |
| Geocoding | [Open-Meteo Geocoding](https://open-meteo.com/en/docs/geocoding-api) | No (free) |
| Weather warnings | [MeteoBlue](https://www.meteoblue.com/) | Yes (optional) |

### Weather Warnings

Set the `METEOBLUE_API_KEY` environment variable for weather warning support:

```bash
export METEOBLUE_API_KEY=your_api_key_here
```

Without an API key, warnings will display as "none".

## Development

```bash
# Run tests
dotnet test

# Build the solution
dotnet build

# Publish self-contained binary for current platform
dotnet publish CLImate.App -c Release --self-contained -p:PublishSingleFile=true
```

See [CONTRIBUTING.md](CONTRIBUTING.md) for development guidelines.

## Publishing & Releases

Use the helper script to build self-contained, single-file binaries for all platforms:

```bash
./scripts/publish.sh
```

### Release Asset Naming

| Platform | Filename |
|----------|----------|
| Linux x64 | `climate-linux-x64.tar.gz` |
| Linux ARM64 | `climate-linux-arm64.tar.gz` |
| macOS x64 | `climate-macos-x64.tar.gz` |
| macOS ARM64 (Apple Silicon) | `climate-macos-arm64.tar.gz` |
| Windows x64 | `climate-windows-x64.zip` |

### Creating a Release

Push a tag to trigger GitHub Actions:

```bash
./scripts/tag-release.sh 0.1.0
git push origin v0.1.0
```

## Roadmap

See [ROADMAP.md](ROADMAP.md) for the full development plan, including:

- ✅ Self-contained binaries (no runtime required)
- 🔲 Package manager distribution (Homebrew, Chocolatey, Scoop)
- 🔲 .NET global tool (`dotnet tool install -g CLImate`)
- 🔲 Configuration file for user preferences
- 🔲 Output format options (JSON, CSV)
- 🔲 Caching for reduced API calls

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

Made with ☀️ and ☔ by [JonW91](https://github.com/JonW91)
