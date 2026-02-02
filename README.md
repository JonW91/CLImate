# CLImate 🌤️

A cross-platform command-line weather forecast application built with .NET 10. Get beautiful ASCII art weather forecasts directly in your terminal.

![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)
![License](https://img.shields.io/badge/license-MIT-blue)
![Beta](https://img.shields.io/badge/status-beta-orange)

## Features

- 🌍 **Global location search** - Search by city, region, or address worldwide
- 🎨 **Colourful ASCII art** - Weather conditions displayed with ANSI colours
- 📊 **7-day forecasts** - Daily high/low temperatures, precipitation, wind speeds
- 🕐 **Today view** - Morning/afternoon/evening/night table with ASCII art per time period
- ⚠️ **Weather warnings** - Integrated severe weather alerts (requires MeteoBlue API key)
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

> **Note:** CLImate is currently in **beta** (v0.1.0-beta). Package manager support is coming soon.

### Option 1: One-Line Install Scripts (Recommended)

Download and install the self-contained binary - **no .NET runtime required**:

**Linux/macOS:**
```bash
curl -fsSL https://raw.githubusercontent.com/JonW91/CLImate/main/scripts/install.sh | sh
```

**Windows PowerShell:**
```powershell
irm https://raw.githubusercontent.com/JonW91/CLImate/main/scripts/install.ps1 | iex
```

### Option 2: Download Binary Manually

Download from the [Releases](https://github.com/JonW91/CLImate/releases) page:

| Platform | Download |
|----------|----------|
| **Linux x64** | `climate-linux-x64.tar.gz` |
| **Linux ARM64** | `climate-linux-arm64.tar.gz` |
| **macOS Intel** | `climate-macos-x64.tar.gz` |
| **macOS Apple Silicon** | `climate-macos-arm64.tar.gz` |
| **Windows x64** | `climate-windows-x64.zip` |

Extract and add to your PATH:
```bash
# Linux/macOS
tar -xzf climate-*.tar.gz
sudo mv climate /usr/local/bin/

# Windows (PowerShell)
Expand-Archive climate-windows-x64.zip -DestinationPath $env:LOCALAPPDATA\CLImate
# Add to PATH via System Settings or:
$env:PATH += ";$env:LOCALAPPDATA\CLImate"
```

### Option 3: Package Managers (Coming Soon)

We're working on getting CLImate into popular package managers:

<details>
<summary><strong>🍺 Homebrew (macOS/Linux)</strong> - Planned</summary>

```bash
# Coming soon!
brew install jonw91/tap/climate
```
</details>

<details>
<summary><strong>🪟 Scoop (Windows)</strong> - Planned</summary>

```powershell
# Coming soon!
scoop bucket add climate https://github.com/JonW91/scoop-climate
scoop install climate
```
</details>

<details>
<summary><strong>🍫 Chocolatey (Windows)</strong> - Planned</summary>

```powershell
# Coming soon!
choco install climate
```
</details>

<details>
<summary><strong>📦 WinGet (Windows)</strong> - Planned</summary>

```powershell
# Coming soon!
winget install JonW91.CLImate
```
</details>

### Option 4: .NET Global Tool

If you have the [.NET 10 SDK](https://dotnet.microsoft.com/download) installed:

```bash
dotnet tool install --global CLImate --prerelease
```

Then run with:
```bash
climate London
```

### Option 5: Build from Source

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
| `--version` | `-v` | Show version information |
| `--units <metric\|imperial>` | `-u` | Set temperature/wind units (default: metric) |
| `--country <code>` | `-c` | Filter by 2-letter country code (e.g., GB, US) |
| `--today` | `-t` | Show today's forecast with time segments |
| `--hourly` | | Show 24-hour hourly forecast for today |
| `--horizontal` | `-H` | Force horizontal table layout |
| `--vertical` | `-V` | Force vertical list layout |
| `--no-art` | | Disable ASCII art, use text labels |
| `--no-colour` | | Disable ANSI colour output |
| `--colour` | | Force ANSI colours on |

## Adaptive Layout

CLImate automatically detects your terminal size and chooses the best layout:

- **Wide terminals (140+ columns)**: Displays a compact horizontal table with ASCII art for all 7 days side-by-side
- **Medium terminals (100-139 columns)**: Horizontal table with compact weather icons
- **Narrow terminals (<100 columns)**: Uses a vertical list with detailed ASCII art per day

The same applies to today's forecast (`-t`):
- **Wide terminals**: Shows Morning/Afternoon/Evening/Night in a horizontal table with ASCII art
- **Narrow terminals**: Vertical list with each time period stacked

You can override this with `--horizontal` or `--vertical` flags.

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
- ✅ .NET global tool (`dotnet tool install -g CLImate`)
- ✅ Adaptive terminal layout (auto horizontal/vertical)
- ✅ Hourly forecast mode (`--hourly`)
- 🔲 Package manager distribution (Homebrew, Chocolatey, Scoop)
- 🔲 Configuration file for user preferences
- 🔲 Caching for reduced API calls

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

Made with ☀️ and ☔ by [JonW91](https://github.com/JonW91)
