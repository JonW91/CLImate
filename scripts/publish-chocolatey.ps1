# Chocolatey Publish Helper Script
# Run this after the release is published

param(
    [string]$Version = "0.1.0-beta",
    [string]$ApiKey = $env:CHOCOLATEY_API_KEY
)

$ErrorActionPreference = "Stop"

Write-Host "=== CLImate Chocolatey Publisher ===" -ForegroundColor Cyan

if (-not $ApiKey) {
    Write-Host "Error: CHOCOLATEY_API_KEY not set" -ForegroundColor Red
    Write-Host "Set it with: `$env:CHOCOLATEY_API_KEY = 'your-key'" -ForegroundColor Yellow
    exit 1
}

# 1. Download the release and compute hash
Write-Host "`n1. Downloading Windows release..." -ForegroundColor Yellow
$zipPath = "$env:TEMP\climate-windows-x64.zip"
$releaseUrl = "https://github.com/JonW91/CLImate/releases/download/v$Version/climate-windows-x64.zip"

Invoke-WebRequest -Uri $releaseUrl -OutFile $zipPath -UseBasicParsing
$hash = (Get-FileHash $zipPath -Algorithm SHA256).Hash
Write-Host "   Hash: $hash" -ForegroundColor Green

# 2. Create package directory
Write-Host "`n2. Creating package structure..." -ForegroundColor Yellow
$pkgDir = "$env:TEMP\choco-climate"
if (Test-Path $pkgDir) { Remove-Item $pkgDir -Recurse -Force }
New-Item -ItemType Directory -Path "$pkgDir\tools" -Force | Out-Null

# 3. Create nuspec
@"
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2015/06/nuspec.xsd">
  <metadata>
    <id>climate</id>
    <version>$Version</version>
    <title>CLImate - Terminal Weather Forecast</title>
    <authors>JonW91</authors>
    <owners>JonW91</owners>
    <projectUrl>https://github.com/JonW91/CLImate</projectUrl>
    <licenseUrl>https://github.com/JonW91/CLImate/blob/main/LICENSE</licenseUrl>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <projectSourceUrl>https://github.com/JonW91/CLImate</projectSourceUrl>
    <docsUrl>https://github.com/JonW91/CLImate#readme</docsUrl>
    <bugTrackerUrl>https://github.com/JonW91/CLImate/issues</bugTrackerUrl>
    <tags>weather forecast cli terminal ascii-art dotnet cross-platform</tags>
    <summary>Cross-platform CLI weather forecast with beautiful ASCII art</summary>
    <description>
CLImate is a cross-platform command-line weather forecast application built with .NET 10.

Features:
- Global location search by city, region, or address
- Colorful ASCII art weather displays
- 7-day forecasts with daily high/low temperatures
- Today view with morning/afternoon/evening/night segments
- 24-hour hourly forecasts
- Metric and Imperial unit support
- Adaptive terminal layout
- Weather warnings (with MeteoBlue API key)

Usage:
  climate London
  climate --today Paris
  climate --hourly "New York, NY"
  climate --units imperial Chicago
    </description>
    <releaseNotes>https://github.com/JonW91/CLImate/releases/tag/v$Version</releaseNotes>
  </metadata>
  <files>
    <file src="tools\**" target="tools" />
  </files>
</package>
"@ | Set-Content "$pkgDir\climate.nuspec" -Encoding UTF8

# 4. Create install script
@"
`$ErrorActionPreference = 'Stop'

`$packageName = 'climate'
`$toolsDir = "`$(Split-Path -Parent `$MyInvocation.MyCommand.Definition)"

`$url64 = 'https://github.com/JonW91/CLImate/releases/download/v$Version/climate-windows-x64.zip'
`$checksum64 = '$hash'

`$packageArgs = @{
    packageName    = `$packageName
    unzipLocation  = `$toolsDir
    url64bit       = `$url64
    checksum64     = `$checksum64
    checksumType64 = 'sha256'
}

Install-ChocolateyZipPackage @packageArgs

`$climatePath = Join-Path `$toolsDir 'climate.exe'
Install-BinFile -Name 'climate' -Path `$climatePath
"@ | Set-Content "$pkgDir\tools\chocolateyinstall.ps1" -Encoding UTF8

# 5. Create uninstall script
@"
`$ErrorActionPreference = 'Stop'
Uninstall-BinFile -Name 'climate'
"@ | Set-Content "$pkgDir\tools\chocolateyuninstall.ps1" -Encoding UTF8

# 6. Pack
Write-Host "`n3. Packing..." -ForegroundColor Yellow
Set-Location $pkgDir
choco pack climate.nuspec

# 7. Push
Write-Host "`n4. Pushing to Chocolatey..." -ForegroundColor Yellow
$nupkg = Get-ChildItem "*.nupkg" | Select-Object -First 1
choco push $nupkg.Name --source https://push.chocolatey.org/ --api-key $ApiKey

Write-Host "`n=== Success! ===" -ForegroundColor Green
Write-Host "Package submitted to Chocolatey for moderation." -ForegroundColor Cyan
Write-Host "Check status at: https://community.chocolatey.org/packages/climate" -ForegroundColor White
