#!/usr/bin/env sh
set -euf

REPO="${CLIMATE_REPO:-JonW91/CLImate}"
VERSION="${CLIMATE_VERSION:-latest}"

if [ "$VERSION" = "latest" ]; then
  BASE_URL="https://github.com/${REPO}/releases/latest/download"
else
  BASE_URL="https://github.com/${REPO}/releases/download/${VERSION}"
fi

OS=""
ARCH=""

uname_s=$(uname -s)
case "$uname_s" in
  Linux) OS="linux" ;;
  Darwin) OS="macos" ;;
  MINGW*|MSYS*|CYGWIN*) OS="windows" ;;
  *)
    echo "Unsupported OS: $uname_s" >&2
    exit 1
    ;;
esac

uname_m=$(uname -m)
case "$uname_m" in
  x86_64|amd64) ARCH="x64" ;;
  aarch64|arm64) ARCH="arm64" ;;
  *)
    echo "Unsupported architecture: $uname_m" >&2
    exit 1
    ;;
esac

EXT="tar.gz"
BIN_NAME="climate"
if [ "$OS" = "windows" ]; then
  EXT="zip"
  BIN_NAME="climate.exe"
fi

ASSET="climate-${OS}-${ARCH}.${EXT}"
URL="${BASE_URL}/${ASSET}"

TMP_DIR=$(mktemp -d)
cleanup() { rm -rf "$TMP_DIR"; }
trap cleanup EXIT

ARCHIVE="$TMP_DIR/$ASSET"

if command -v curl >/dev/null 2>&1; then
  curl -fsSL "$URL" -o "$ARCHIVE"
elif command -v wget >/dev/null 2>&1; then
  wget -qO "$ARCHIVE" "$URL"
else
  echo "Please install curl or wget to continue." >&2
  exit 1
fi

if [ "$EXT" = "zip" ]; then
  if ! command -v unzip >/dev/null 2>&1; then
    echo "Please install unzip to continue." >&2
    exit 1
  fi
  unzip -q "$ARCHIVE" -d "$TMP_DIR"
else
  tar -xzf "$ARCHIVE" -C "$TMP_DIR"
fi

if [ "$(id -u)" -eq 0 ]; then
  INSTALL_DIR="/usr/local/bin"
else
  INSTALL_DIR="${HOME}/.local/bin"
fi

CLIMATE_DATA_DIR="${HOME}/.local/share/climate"

mkdir -p "$INSTALL_DIR"
mkdir -p "$CLIMATE_DATA_DIR"

if [ ! -f "$TMP_DIR/$BIN_NAME" ]; then
  echo "Expected binary '$BIN_NAME' not found in archive." >&2
  exit 1
fi

install -m 0755 "$TMP_DIR/$BIN_NAME" "$INSTALL_DIR/$BIN_NAME"

# Copy Assets folder if present
if [ -d "$TMP_DIR/Assets" ]; then
  cp -r "$TMP_DIR/Assets" "$INSTALL_DIR/"
fi

echo "Installed CLImate to $INSTALL_DIR/$BIN_NAME"
if ! echo "$PATH" | tr ':' '\n' | grep -qx "$INSTALL_DIR"; then
  echo "Note: add $INSTALL_DIR to your PATH to run 'climate' from anywhere."
fi
