#!/usr/bin/env sh
set -euf

REPO="${CLIMATE_REPO:-JonW91/CLImate}"
VERSION="${CLIMATE_VERSION:-latest}"

# Handle latest vs specific version
if [ "$VERSION" = "latest" ]; then
  # Try latest release first, fall back to latest pre-release
  BASE_URL="https://github.com/${REPO}/releases/latest/download"
  FALLBACK_URL="https://api.github.com/repos/${REPO}/releases"
else
  BASE_URL="https://github.com/${REPO}/releases/download/${VERSION}"
fi

OS=""
ARCH=""

# Detect OS
uname_s=$(uname -s)
case "$uname_s" in
  Linux) 
    # Check if running in Termux (Android)
    if [ -n "${TERMUX_VERSION:-}" ] || [ -d "/data/data/com.termux" ]; then
      echo "⚠️  Termux detected: CLImate may not work properly on Android." >&2
      echo "   Consider using a Linux desktop/server environment." >&2
      echo "   Attempting to install anyway..." >&2
    fi
    OS="linux" 
    ;;
  Darwin) OS="macos" ;;
  MINGW*|MSYS*|CYGWIN*) OS="windows" ;;
  *)
    echo "❌ Unsupported OS: $uname_s" >&2
    echo "   Supported: Linux, macOS, Windows" >&2
    exit 1
    ;;
esac

# Detect architecture
uname_m=$(uname -m)
case "$uname_m" in
  x86_64|amd64) ARCH="x64" ;;
  aarch64|arm64) ARCH="arm64" ;;
  *)
    echo "❌ Unsupported architecture: $uname_m" >&2
    echo "   Supported: x86_64 (x64), aarch64 (arm64)" >&2
    exit 1
    ;;
esac

# Determine file extension and binary name
EXT="tar.gz"
BIN_NAME="climate"
if [ "$OS" = "windows" ]; then
  EXT="zip"
  BIN_NAME="climate.exe"
fi

ASSET="climate-${OS}-${ARCH}.${EXT}"
URL="${BASE_URL}/${ASSET}"

echo "📡 Downloading CLImate for ${OS}-${ARCH}..."

# Create temporary directory
TMP_DIR=$(mktemp -d)
cleanup() { rm -rf "$TMP_DIR"; }
trap cleanup EXIT

ARCHIVE="$TMP_DIR/$ASSET"

# Download with error handling for 404s
if command -v curl >/dev/null 2>&1; then
  if ! curl -fsSL "$URL" -o "$ARCHIVE" 2>/dev/null; then
    if [ "$VERSION" = "latest" ]; then
      echo "⚠️  Latest release not found, trying latest pre-release..." >&2
      # Try to get the actual latest release (including pre-releases)
      if command -v jq >/dev/null 2>&1; then
        LATEST_TAG=$(curl -s "$FALLBACK_URL" | jq -r '.[0].tag_name')
        if [ "$LATEST_TAG" != "null" ] && [ -n "$LATEST_TAG" ]; then
          NEW_URL="https://github.com/${REPO}/releases/download/${LATEST_TAG}/${ASSET}"
          echo "📡 Downloading from: $LATEST_TAG" >&2
          curl -fsSL "$NEW_URL" -o "$ARCHIVE"
        else
          echo "❌ Failed to find any release. Please check: https://github.com/${REPO}/releases" >&2
          exit 1
        fi
      else
        echo "❌ Failed to download. Try installing a specific version:" >&2
        echo "   CLIMATE_VERSION=v0.1.0-beta curl -fsSL ... | sh" >&2
        exit 1
      fi
    else
      echo "❌ Failed to download $ASSET" >&2
      echo "   Available releases: https://github.com/${REPO}/releases" >&2
      exit 1
    fi
  fi
elif command -v wget >/dev/null 2>&1; then
  if ! wget -qO "$ARCHIVE" "$URL" 2>/dev/null; then
    echo "❌ Failed to download. Please install curl or check the URL manually:" >&2
    echo "   $URL" >&2
    exit 1
  fi
else
  echo "❌ Please install curl or wget to continue." >&2
  exit 1
fi

echo "📦 Extracting archive..."

# Extract archive
if [ "$EXT" = "zip" ]; then
  if ! command -v unzip >/dev/null 2>&1; then
    echo "❌ Please install unzip to continue." >&2
    exit 1
  fi
  unzip -q "$ARCHIVE" -d "$TMP_DIR"
else
  tar -xzf "$ARCHIVE" -C "$TMP_DIR"
fi

# Determine install directory
if [ "$(id -u)" -eq 0 ]; then
  INSTALL_DIR="/usr/local/bin"
elif [ -n "${TERMUX_VERSION:-}" ]; then
  # Termux uses different paths
  INSTALL_DIR="$PREFIX/bin"
else
  INSTALL_DIR="${HOME}/.local/bin"
fi

# Create directories
mkdir -p "$INSTALL_DIR"

# Verify binary exists
if [ ! -f "$TMP_DIR/$BIN_NAME" ]; then
  echo "❌ Expected binary '$BIN_NAME' not found in archive." >&2
  ls -la "$TMP_DIR" >&2
  exit 1
fi

# Install binary
echo "🚀 Installing CLImate to $INSTALL_DIR..."
install -m 0755 "$TMP_DIR/$BIN_NAME" "$INSTALL_DIR/$BIN_NAME"

# Check if directory is in PATH
if ! echo "$PATH" | tr ':' '\n' | grep -qx "$INSTALL_DIR"; then
  echo ""
  echo "⚠️  $INSTALL_DIR is not in your PATH."
  echo "   Add this to your shell config (~/.bashrc, ~/.zshrc, etc.):"
  echo "   export PATH=\"$INSTALL_DIR:\$PATH\""
  echo ""
fi

echo "✅ CLImate installed successfully!"
echo ""
echo "🌤️  Try it out:"
echo "   climate London"
echo "   climate --help"
