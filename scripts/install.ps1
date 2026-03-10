param(
  [string]$Repo = $env:CLIMATE_REPO,
  [string]$Version = $env:CLIMATE_VERSION
)

if (-not $Repo) { $Repo = "JonW91/CLImate" }
if (-not $Version) { $Version = "latest" }

Write-Host "📡 Installing CLImate..." -ForegroundColor Cyan

# Handle latest vs specific version
if ($Version -eq "latest") {
  $baseUrl = "https://github.com/$Repo/releases/latest/download"
  $fallbackUrl = "https://api.github.com/repos/$Repo/releases"
} else {
  $baseUrl = "https://github.com/$Repo/releases/download/$Version"
}

# Detect OS - PowerShell runs on Windows, macOS, and Linux
$os = "win"  # Default assumption for PowerShell
if ($PSVersionTable.Platform -eq "Unix") {
  if ($IsMacOS -or [System.Runtime.InteropServices.RuntimeInformation]::OSDescription -match "Darwin") {
    $os = "osx"  # Match release asset naming
  } else {
    $os = "linux"
  }
}

# Detect architecture  
$arch = switch ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture) {
  "X64" { "x64" }
  "Arm64" { "arm64" }
  default { 
    Write-Error "❌ Unsupported architecture: $($_)"
    Write-Host "   Supported: X64, ARM64" -ForegroundColor Yellow
    exit 1 
  }
}

$ext = if ($os -eq "win") { "zip" } else { "tar.gz" }
$binName = if ($os -eq "win") { "climate.exe" } else { "climate" }

$asset = "climate-$os-$arch.$ext"
$url = "$baseUrl/$asset"

Write-Host "📦 Downloading CLImate for $os-$arch..." -ForegroundColor Green

# Create temporary directory
$tempDir = New-Item -ItemType Directory -Path ([System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [System.Guid]::NewGuid().ToString()))
$archive = Join-Path $tempDir.FullName $asset

try {
  # Download with error handling
  try {
    Invoke-WebRequest -Uri $url -OutFile $archive -UseBasicParsing
  }
  catch {
    if ($Version -eq "latest") {
      Write-Warning "⚠️ Latest release not found, trying latest pre-release..."
      try {
        $releases = Invoke-RestMethod -Uri $fallbackUrl -UseBasicParsing
        $latestTag = $releases[0].tag_name
        $newUrl = "https://github.com/$Repo/releases/download/$latestTag/$asset"
        Write-Host "📡 Downloading from: $latestTag" -ForegroundColor Yellow
        Invoke-WebRequest -Uri $newUrl -OutFile $archive -UseBasicParsing
      }
      catch {
        Write-Error "❌ Failed to find any release. Please check: https://github.com/$Repo/releases"
        exit 1
      }
    } else {
      Write-Error "❌ Failed to download $asset"
      Write-Host "   Available releases: https://github.com/$Repo/releases" -ForegroundColor Yellow
      exit 1
    }
  }

  Write-Host "📁 Extracting archive..." -ForegroundColor Green

  # Extract archive
  if ($ext -eq "zip") {
    Expand-Archive -Path $archive -DestinationPath $tempDir.FullName -Force
  } else {
    # Use tar on Unix platforms (macOS/Linux with PowerShell)
    & tar -xzf $archive -C $tempDir.FullName
  }

  # Determine install directory
  if ($os -eq "win") {
    $installDir = Join-Path $env:LOCALAPPDATA "Programs\CLImate"
  } else {
    $installDir = Join-Path $HOME ".local/bin"
  }

  # Create install directory
  New-Item -ItemType Directory -Force -Path $installDir | Out-Null

  # Verify binary exists
  $src = Join-Path $tempDir.FullName $binName
  if (-not (Test-Path $src)) {
    Write-Error "❌ Expected binary '$binName' not found in archive."
    Get-ChildItem $tempDir.FullName | ForEach-Object { Write-Host "   Found: $($_.Name)" }
    exit 1
  }

  # Install binary
  Write-Host "🚀 Installing CLImate to $installDir..." -ForegroundColor Green
  $dest = Join-Path $installDir $binName
  Copy-Item $src $dest -Force

  # Copy Assets folder if present
  $assetsSrc = Join-Path $tempDir.FullName "Assets"
  if (Test-Path $assetsSrc) {
    $assetsDest = Join-Path $installDir "Assets"
    Copy-Item $assetsSrc $assetsDest -Recurse -Force
  }

  Write-Host ""
  Write-Host "✅ CLImate installed successfully!" -ForegroundColor Green
  Write-Host ""
  
  if ($os -eq "win") {
    # Check if directory is in PATH
    $pathDirs = $env:PATH -split ";"
    if ($installDir -notin $pathDirs) {
      Write-Host "⚠️  $installDir is not in your PATH." -ForegroundColor Yellow
      Write-Host "   Add it manually via System Settings > Environment Variables" -ForegroundColor Yellow
      Write-Host "   Or run this in an elevated PowerShell to add to PATH:" -ForegroundColor Yellow
      Write-Host "   [Environment]::SetEnvironmentVariable('Path', `$env:Path + ';$($installDir)', 'User')" -ForegroundColor Cyan
      Write-Host ""
    }
  } else {
    Write-Host "⚠️  Ensure '$installDir' is in your PATH." -ForegroundColor Yellow
    Write-Host "   Add this to your shell profile (~/.bashrc, ~/.zshrc, etc.):" -ForegroundColor Yellow  
    Write-Host "   export PATH=`"$($installDir):`$PATH`"" -ForegroundColor Cyan
    Write-Host ""
  }

  Write-Host "🌤️  Try it out:" -ForegroundColor Cyan
  Write-Host "   climate London" -ForegroundColor White
  Write-Host "   climate --help" -ForegroundColor White
}
finally {
  Remove-Item -Recurse -Force $tempDir.FullName -ErrorAction SilentlyContinue
}
