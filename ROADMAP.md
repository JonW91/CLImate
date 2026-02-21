# CLImate Roadmap

This document outlines the development plan for making CLImate a production-ready, universally accessible CLI weather application.

---

## Current State Assessment

### ✅ What Works Well
- Clean architecture with dependency injection
- Cross-platform .NET 10 codebase (Linux, macOS, Windows)
- ASCII art weather visualisation with ANSI colours
- 7-day, today-only, and hourly forecast modes
- Location search with country code filtering
- Metric/Imperial unit support
- Weather warnings integration (US: NWS, EU: Meteoalarm; fallback message elsewhere)
- Robust error handling for network failures ✅
- Input validation with ISO 3166-1 country codes ✅
- Adaptive terminal layout (horizontal/vertical) ✅
- Graceful degradation for missing services ✅
- CI/CD pipeline with GitHub Actions ✅
- Comprehensive test coverage (121 tests) ✅

### ⚠️ Remaining Areas for Improvement

| Area | Issue | Priority |
|------|-------|----------|
| **Configuration** | No persistent user preferences | Medium |
| **Caching** | Repeated API calls for same location | Medium |
| **Logging** | No diagnostic logging | Low |
| **Accessibility** | Screen reader compatibility unknown | Low |

---

## Phase 1: Production Readiness (Priority: High) ✅

### 1.1 Self-Contained Publishing ✅
Already configured in publish scripts. Produces single-file executables for:
- `linux-x64`, `linux-arm64`
- `osx-x64`, `osx-arm64`  
- `win-x64`

**Action**: Verify publish scripts work and add to CI/CD.

### 1.2 Robust Error Handling ✅
Add graceful handling for:
- [x] Network timeout/connectivity issues
- [ ] API rate limiting (Open-Meteo is generous but has limits)
- [x] Invalid API responses
- [ ] Missing/corrupt asset files

```csharp
// Implemented in CliApplication.cs - wraps RunCoreAsync with try/catch
catch (HttpRequestException)
{
    _console.WriteLine("Unable to reach weather service. Check your internet connection and try again.");
    return 1;
}
catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
{
    _console.WriteLine("Request timed out. The weather service may be unavailable. Please try again.");
    return 1;
}
```

### 1.3 Input Validation ✅
- [x] Sanitise location input (prevent injection in URLs) - Uses Uri.EscapeDataString in GeocodingService
- [x] Validate country codes against ISO 3166-1 catalogue - Added IsValidCode() with full country list
- [x] Handle special characters in location names - Uri.EscapeDataString handles encoding

### 1.4 Graceful Degradation ✅
- [x] Work without MeteoBlue API key (warnings show "none", service returns empty)
- [x] Fallback if ASCII art file is missing/corrupt (shows text labels instead)
- [x] Handle terminals without ANSI support (checks NO_COLOR env, IsOutputRedirected)

---

## Phase 2: Distribution (Priority: High)

### 2.1 GitHub Releases (Current)
Self-contained binaries attached to tagged releases.

**Installation:**
```bash
# Linux/macOS
curl -fsSL https://raw.githubusercontent.com/JonW91/CLImate/main/scripts/install.sh | sh

# Windows PowerShell
irm https://raw.githubusercontent.com/JonW91/CLImate/main/scripts/install.ps1 | iex
```

### 2.2 Package Managers

| Package Manager | Platform | Status | Notes |
|----------------|----------|--------|-------|
| **NuGet (.NET Tool)** | Cross-platform | 🔲 Ready | Automated in release workflow (requires `NUGET_API_KEY`) |
| **Chocolatey** | Windows | 🔄 In Review | Submitted to community repository |
| **Winget** | Windows | 🔄 In Review | PR submitted to winget-pkgs |
| **Scoop** | Windows | ✅ Available | `scoop bucket add climate https://github.com/JonW91/scoop-climate` |
| **Homebrew** | macOS/Linux | ⚠️ Issues | Tap exists but installation failing - needs debugging |
| **APT/Debian** | Linux | 🔲 Ready | `.deb` package scripts in `packaging/deb/` |
| **DNF/RPM** | Fedora/RHEL | 🔲 Ready | `.rpm` spec in `packaging/rpm/` |
| **AUR** | Arch Linux | 🔲 Future | Community maintained |
| **Snap** | Linux | 🔲 Future | Universal Linux package |

### 2.3 .NET Global Tool ✅
```bash
dotnet tool install --global CLImate
```
**Status: Configured** - Added to CLImate.App.csproj
- [x] Add `<PackAsTool>true</PackAsTool>` to csproj
- [x] Configure NuGet package metadata
- [ ] Publish to NuGet.org (automated in release workflow; requires `NUGET_API_KEY`)

```xml
<!-- Already added to CLImate.App.csproj -->
<PropertyGroup>
  <PackAsTool>true</PackAsTool>
  <ToolCommandName>climate</ToolCommandName>
  <PackageId>CLImate</PackageId>
  <Version>0.1.0</Version>
  <Authors>JonW91</Authors>
  <Description>CLI weather forecasts with beautiful ASCII art. Get 7-day forecasts directly in your terminal.</Description>
  <PackageProjectUrl>https://github.com/JonW91/CLImate</PackageProjectUrl>
  <RepositoryUrl>https://github.com/JonW91/CLImate</RepositoryUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageTags>weather;cli;terminal;forecast;ascii-art;dotnet-tool</PackageTags>
  <PackageReadmeFile>README.md</PackageReadmeFile>
</PropertyGroup>
```

### 2.4 Docker Image
```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine
COPY publish/ /app/
ENTRYPOINT ["/app/climate"]
```

```bash
docker run --rm ghcr.io/jonw91/climate London
```

---

## Phase 3: User Experience (Priority: Medium)

### 3.0 Adaptive Terminal Layout ✅
Automatically adjusts output based on terminal dimensions:

- [x] Detect terminal width/height at runtime
- [x] Horizontal table layout for wide terminals (140+ columns with ASCII art, 100+ with compact icons)
- [x] Vertical list layout for narrow terminals
- [x] `--horizontal` / `-H` flag to force table layout
- [x] `--vertical` / `-V` flag to force list layout
- [x] Today view (`-t`) also uses table layout with time periods as columns
- [x] Hourly view (`--hourly`) shows 24-hour forecast grouped by time blocks
- [x] ASCII art displayed in both horizontal and vertical layouts
- [x] Universal ASCII characters (no emojis) for cross-terminal compatibility

**Usage:**
```bash
climate London              # Auto-detect best layout
climate -H London           # Force horizontal table
climate -V London           # Force vertical list
climate -t London           # Today's forecast with time period table
climate --hourly London     # 24-hour hourly forecast
```

### 3.1 Configuration File
Support `~/.config/climate/config.json` or `%APPDATA%\climate\config.json`:

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

**Commands:**
```bash
climate config set units imperial
climate config set country US
climate @home              # Use saved location
climate --save home        # Save current location
```

### 3.2 Caching
- Cache geocoding results (location → coordinates)
- Cache forecasts for short period (5-15 minutes)
- Reduce API calls for repeated queries

### 3.3 Output Formats
```bash
climate London --format json    # Machine-readable
climate London --format csv     # Spreadsheet import
climate London --format brief   # Single line summary
```

### 3.4 Shell Integration

**Bash/Zsh prompt integration:**
```bash
# Add to .bashrc/.zshrc
climate_prompt() {
  climate --format brief --no-art "$(cat ~/.config/climate/default-location)" 2>/dev/null
}
```

**Tab completion:**
```bash
# Bash completion
complete -W "$(climate --list-countries)" climate
```

---

## Phase 4: Extended Features (Priority: Low)

### 4.1 Forecast Options
- [ ] Hourly forecasts (`climate --hourly London`)
- [ ] Extended range (14-day where available)
- [ ] Historical weather lookup
- [ ] Multiple locations comparison

### 4.2 Alert/Notification System
```bash
climate alert --location London --condition "temp > 30"
climate alert --location London --condition "weather = rain"
```

### 4.3 Internationalisation
- [ ] Localised weather descriptions
- [ ] Right-to-left terminal support
- [ ] Locale-aware date/time formatting

### 4.4 Accessibility
- [ ] Screen reader friendly output mode
- [ ] High contrast colour schemes
- [ ] Verbose descriptions option

---

## Phase 5: Infrastructure (Ongoing)

### 5.1 CI/CD Pipeline
```yaml
# .github/workflows/release.yml
on:
  push:
    tags: ['v*']

jobs:
  build:
    strategy:
      matrix:
        include:
          - os: ubuntu-latest
            rid: linux-x64
          - os: ubuntu-latest
            rid: linux-arm64
          - os: macos-latest
            rid: osx-x64
          - os: macos-latest
            rid: osx-arm64
          - os: windows-latest
            rid: win-x64
    
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet publish CLImate.App -c Release -r ${{ matrix.rid }} --self-contained -p:PublishSingleFile=true
      - uses: actions/upload-artifact@v4
```

**Status: Implemented** ✅
- `.github/workflows/ci.yml` - Build and test on push/PR (Ubuntu, Windows, macOS)
- `.github/workflows/release.yml` - Create releases on version tags

### 5.2 Testing Strategy ✅
- [x] Unit tests for all services
- [x] CLI options parser tests
- [x] Country code catalogue tests
- [x] Table renderer tests
- [x] Terminal info tests
- [ ] Integration tests with mocked HTTP
- [ ] E2E tests with actual CLI invocation
- [x] Cross-platform CI testing

### 5.3 Documentation
- [x] README with quick start
- [x] CONTRIBUTING.md
- [ ] API documentation
- [ ] Architecture decision records (ADRs)

---

## Distribution Comparison

| Method | Pros | Cons | Best For |
|--------|------|------|----------|
| **GitHub Release** | Simple, universal | Manual download | Power users |
| **.NET Tool** | Easy install, auto-update | Requires .NET SDK | .NET developers |
| **Homebrew** | Mac-native, trusted | Mac/Linux only | macOS users |
| **Chocolatey** | Windows-native | Windows only | Windows users |
| **Docker** | Isolated, reproducible | Overhead | CI/CD, servers |
| **Self-contained binary** | No dependencies | Larger file size | Everyone |

---

## Recommended Priority Order

1. ~~**Immediate**: Verify self-contained publishing works~~ ✅
2. ~~**Short-term**: Add error handling, create GitHub Actions workflow~~ ✅
3. **Medium-term**: NuGet tool package, Homebrew tap
4. **Long-term**: Configuration file, caching, package managers

---

## Version Milestones

| Version | Target | Features |
|---------|--------|----------|
| **0.1.0** | Current | Basic forecasts, ASCII art |
| **0.2.0** | +2 weeks | Error handling, CI/CD, tested publishing |
| **0.3.0** | +1 month | NuGet tool, Homebrew tap |
| **0.4.0** | +2 months | Configuration file, caching |
| **1.0.0** | +3 months | Production ready, stable API |

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for how to help with this roadmap.

Priority areas for contributions:
- Package manager manifests (Homebrew, Chocolatey, Scoop)
- CI/CD workflows
- Test coverage
- Documentation
