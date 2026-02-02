param(
  [string]$Repo = $env:CLIMATE_REPO,
  [string]$Version = $env:CLIMATE_VERSION
)

if (-not $Repo) { $Repo = "JonW91/CLImate" }
if (-not $Version) { $Version = "latest" }

if ($Version -eq "latest") {
  $baseUrl = "https://github.com/$Repo/releases/latest/download"
} else {
  $baseUrl = "https://github.com/$Repo/releases/download/$Version"
}

$os = if ($IsWindows) { "windows" } elseif ($IsMacOS) { "macos" } else { "linux" }
$arch = switch ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture) {
  "X64" { "x64" }
  "Arm64" { "arm64" }
  default { throw "Unsupported architecture: $($_)" }
}

$ext = if ($os -eq "windows") { "zip" } else { "tar.gz" }
$binName = if ($os -eq "windows") { "climate.exe" } else { "climate" }

$asset = "climate-$os-$arch.$ext"
$url = "$baseUrl/$asset"

$tempDir = New-Item -ItemType Directory -Path ([System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [System.Guid]::NewGuid().ToString()))
$archive = Join-Path $tempDir.FullName $asset

try {
  Invoke-WebRequest -Uri $url -OutFile $archive -UseBasicParsing

  if ($ext -eq "zip") {
    Expand-Archive -Path $archive -DestinationPath $tempDir.FullName -Force
  } else {
    tar -xzf $archive -C $tempDir.FullName
  }

  if ($IsWindows) {
    $installDir = Join-Path $env:LOCALAPPDATA "Programs\CLImate"
  } else {
    $installDir = Join-Path $HOME ".local/bin"
  }

  New-Item -ItemType Directory -Force -Path $installDir | Out-Null

  $src = Join-Path $tempDir.FullName $binName
  if (-not (Test-Path $src)) {
    throw "Expected binary '$binName' not found in archive."
  }

  $dest = Join-Path $installDir $binName
  Copy-Item $src $dest -Force

  # Copy Assets folder if present
  $assetsSrc = Join-Path $tempDir.FullName "Assets"
  if (Test-Path $assetsSrc) {
    $assetsDest = Join-Path $installDir "Assets"
    Copy-Item $assetsSrc $assetsDest -Recurse -Force
  }

  Write-Host "Installed CLImate to $dest"
  if ($IsWindows) {
    Write-Host "Add '$installDir' to PATH to run 'climate' from anywhere."
  } else {
    Write-Host "Ensure '$installDir' is on PATH to run 'climate' from anywhere."
  }
}
finally {
  Remove-Item -Recurse -Force $tempDir.FullName
}
