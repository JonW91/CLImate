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
- Comprehensive test coverage (168 tests) ✅
- **Simple binary distribution via install scripts** ✅

### ⚠️ Remaining Areas for Improvement

| Area | Issue | Priority |
|------|-------|----------|
| **Configuration** | No persistent user preferences | Medium |
| **Caching** | Repeated API calls for same location | Medium |
| **Logging** | No diagnostic logging | Low |
| **Accessibility** | Screen reader compatibility unknown | Low |

---

## Distribution Strategy: Simple & Universal ✅

### ✅ Current Approach: Install Scripts + Manual Download
**Status: COMPLETE** - This approach works well and covers all use cases:

```bash
# One-line install (recommended)
curl -fsSL https://raw.githubusercontent.com/JonW91/CLImate/main/scripts/install.sh | sh

# Manual download from GitHub releases
# Build from source for developers
```

**Benefits:**
- ✅ Works on any platform
- ✅ No external dependencies 
- ✅ Single point of truth (GitHub releases)
- ✅ No maintenance overhead for package managers
- ✅ Users get latest version immediately
- ✅ Simple to support and troubleshoot

**Decision: Package managers removed from scope** - They add complexity and maintenance burden without significant user benefit.

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

## Simplified Development Focus

### ✅ Completed (Production Ready)
1. **Binary Distribution** - Install scripts work universally
2. **ASCII Art Display** - Fixed terminal width detection issues  
3. **Comprehensive Testing** - 168 tests passing
4. **Cross-Platform Support** - Linux, macOS, Windows
5. **CI/CD Pipeline** - Automated builds and releases

### 🎯 Remaining Priorities

| Priority | Feature | Benefit |
|----------|---------|---------|
| **Medium** | Configuration file support | User convenience for defaults |
| **Medium** | Basic caching (5-15 min) | Reduce API calls, faster responses |
| **Low** | Enhanced error handling | API rate limits, asset recovery |
| **Low** | Additional output formats | JSON, CSV, brief modes |

### 📦 Distribution Strategy: Keep It Simple

**Decision:** Focus on **binary distribution** rather than package managers:

✅ **What works:**
- One-line install scripts for all platforms
- Direct GitHub releases download
- Self-contained binaries (no dependencies)
- Build from source for developers

❌ **What to avoid:**
- Package manager maintenance overhead
- Multiple distribution channels to support
- Version synchronization across platforms
- Community repository approval processes

### 🚀 Version Milestones

| Version | Status | Focus |
|---------|--------|-------|
| **0.1.0** | ✅ Current | Core functionality, ASCII art fixes |
| **0.2.0** | 🎯 Next | Configuration file, basic caching |
| **0.3.0** | Future | Enhanced error handling, output formats |
| **1.0.0** | Future | Stable API, accessibility features |

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development guidelines.

**Current priorities for contributions:**
- Configuration system implementation
- Caching layer for API responses  
- Cross-platform testing
- Documentation improvements

The project is **production-ready** as-is - remaining features are enhancements for user convenience rather than critical fixes.
