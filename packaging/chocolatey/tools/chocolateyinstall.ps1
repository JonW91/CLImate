$ErrorActionPreference = 'Stop'

$packageName = 'climate'
$toolsDir = "$(Split-Path -Parent $MyInvocation.MyCommand.Definition)"

$url64 = 'https://github.com/JonW91/CLImate/releases/download/v0.1.0-beta/climate-windows-x64.zip'
$checksum64 = 'PLACEHOLDER_SHA256_WINDOWS_X64'

$packageArgs = @{
    packageName    = $packageName
    unzipLocation  = $toolsDir
    url64bit       = $url64
    checksum64     = $checksum64
    checksumType64 = 'sha256'
}

Install-ChocolateyZipPackage @packageArgs

# Create shim for climate.exe
$climatePath = Join-Path $toolsDir 'climate.exe'
Install-BinFile -Name 'climate' -Path $climatePath
