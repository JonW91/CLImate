# WinGet Submission Helper Script
# Run this script to prepare and submit CLImate to WinGet

param(
    [string]$Version = "0.1.2"
)

$ErrorActionPreference = "Stop"

Write-Host "=== CLImate WinGet Submission Helper ===" -ForegroundColor Cyan

# 1. Download the release and compute hash
Write-Host "`n1. Downloading Windows release..." -ForegroundColor Yellow
$zipPath = "$env:TEMP\climate-win-x64.zip"
$releaseUrl = "https://github.com/JonW91/CLImate/releases/download/v$Version/climate-win-x64.zip"

try {
    Invoke-WebRequest -Uri $releaseUrl -OutFile $zipPath -UseBasicParsing
    Write-Host "   Downloaded: $zipPath" -ForegroundColor Green
} catch {
    Write-Host "   Error: Release not found or still building. Check GitHub Actions." -ForegroundColor Red
    Write-Host "   URL: $releaseUrl" -ForegroundColor Gray
    exit 1
}

# 2. Compute SHA256
Write-Host "`n2. Computing SHA256 hash..." -ForegroundColor Yellow
$hash = (Get-FileHash $zipPath -Algorithm SHA256).Hash
Write-Host "   Hash: $hash" -ForegroundColor Green

# 3. Clone winget-pkgs fork
Write-Host "`n3. Setting up winget-pkgs fork..." -ForegroundColor Yellow
$wingetPath = "$env:TEMP\winget-pkgs-submit"
if (Test-Path $wingetPath) {
    Remove-Item $wingetPath -Recurse -Force
}
git clone --depth 1 https://github.com/JonW91/winget-pkgs.git $wingetPath
Set-Location $wingetPath

# 4. Create manifest directory
$manifestDir = "manifests\j\JonW91\CLImate\$Version"
New-Item -ItemType Directory -Path $manifestDir -Force | Out-Null

# 5. Create version manifest
@"
# yaml-language-server: `$schema=https://aka.ms/winget-manifest.version.1.6.0.schema.json
PackageIdentifier: JonW91.CLImate
PackageVersion: $Version
DefaultLocale: en-US
ManifestType: version
ManifestVersion: 1.6.0
"@ | Set-Content "$manifestDir\JonW91.CLImate.yaml" -Encoding UTF8

# 6. Create locale manifest
@"
# yaml-language-server: `$schema=https://aka.ms/winget-manifest.defaultLocale.1.6.0.schema.json
PackageIdentifier: JonW91.CLImate
PackageVersion: $Version
PackageLocale: en-US
Publisher: JonW91
PublisherUrl: https://github.com/JonW91
Author: JonW91
PackageName: CLImate
PackageUrl: https://github.com/JonW91/CLImate
License: MIT
LicenseUrl: https://github.com/JonW91/CLImate/blob/main/LICENSE
ShortDescription: Cross-platform CLI weather forecast with ASCII art
Description: |-
  CLImate is a cross-platform command-line weather forecast application built with .NET 10.
  Get beautiful ASCII art weather forecasts directly in your terminal.
  
  Features include global location search, colorful ASCII art displays, 7-day forecasts,
  hourly forecasts, today view with time segments, metric/imperial units, and weather warnings.
Tags:
  - weather
  - forecast
  - cli
  - terminal
  - ascii-art
  - dotnet
  - cross-platform
ReleaseNotesUrl: https://github.com/JonW91/CLImate/releases/tag/v$Version
ManifestType: defaultLocale
ManifestVersion: 1.6.0
"@ | Set-Content "$manifestDir\JonW91.CLImate.locale.en-US.yaml" -Encoding UTF8

# 7. Create installer manifest
@"
# yaml-language-server: `$schema=https://aka.ms/winget-manifest.installer.1.6.0.schema.json
PackageIdentifier: JonW91.CLImate
PackageVersion: $Version
InstallerType: zip
NestedInstallerType: portable
NestedInstallerFiles:
  - RelativeFilePath: climate.exe
    PortableCommandAlias: climate
Installers:
  - Architecture: x64
    InstallerUrl: https://github.com/JonW91/CLImate/releases/download/v$Version/climate-win-x64.zip
    InstallerSha256: $hash
ManifestType: installer
ManifestVersion: 1.6.0
"@ | Set-Content "$manifestDir\JonW91.CLImate.installer.yaml" -Encoding UTF8

Write-Host "`n4. Manifest files created:" -ForegroundColor Yellow
Get-ChildItem $manifestDir | ForEach-Object { Write-Host "   - $($_.Name)" -ForegroundColor Green }

# 8. Validate manifests (if winget is installed)
Write-Host "`n5. Validating manifests..." -ForegroundColor Yellow
try {
    winget validate --manifest $manifestDir
    Write-Host "   Validation passed!" -ForegroundColor Green
} catch {
    Write-Host "   winget not available for validation (install Windows App Installer)" -ForegroundColor Yellow
}

# 9. Commit and push
Write-Host "`n6. Committing changes..." -ForegroundColor Yellow
git checkout -b "climate-$Version"
git add .
git commit -m "Add JonW91.CLImate version $Version"
git push origin "climate-$Version"

Write-Host "`n=== Success! ===" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Go to: https://github.com/JonW91/winget-pkgs" -ForegroundColor White
Write-Host "2. Click 'Compare & pull request' for branch 'climate-$Version'" -ForegroundColor White
Write-Host "3. Target: microsoft/winget-pkgs main branch" -ForegroundColor White
Write-Host "4. Submit the PR and wait for automated validation" -ForegroundColor White
Write-Host "`nSHA256 Hash: $hash" -ForegroundColor Gray
