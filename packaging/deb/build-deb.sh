#!/bin/bash
# Build DEB package for CLImate
# Usage: ./build-deb.sh <version> <architecture>
# Example: ./build-deb.sh 0.1.0-beta amd64

set -e

VERSION="${1:-0.1.0-beta}"
ARCH="${2:-amd64}"
PKG_NAME="climate"
PKG_VERSION="${VERSION//-/\~}"  # Replace - with ~ for Debian versioning

echo "Building DEB package for CLImate v${VERSION} (${ARCH})..."

# Create build directory
BUILD_DIR="$(mktemp -d)"
PKG_DIR="${BUILD_DIR}/${PKG_NAME}_${PKG_VERSION}_${ARCH}"

mkdir -p "${PKG_DIR}/DEBIAN"
mkdir -p "${PKG_DIR}/usr/bin"
mkdir -p "${PKG_DIR}/usr/share/${PKG_NAME}/Assets"

# Download or copy binary
if [ -f "climate" ]; then
    cp climate "${PKG_DIR}/usr/bin/climate"
else
    DOWNLOAD_ARCH="x64"
    [ "$ARCH" = "arm64" ] && DOWNLOAD_ARCH="arm64"
    
    curl -fsSL "https://github.com/JonW91/CLImate/releases/download/v${VERSION}/climate-linux-${DOWNLOAD_ARCH}.tar.gz" | \
        tar -xz -C "${PKG_DIR}/usr/bin/"
fi

chmod 755 "${PKG_DIR}/usr/bin/climate"

# Copy Assets if available
if [ -d "Assets" ]; then
    cp -r Assets/* "${PKG_DIR}/usr/share/${PKG_NAME}/Assets/"
fi

# Create control file
cat > "${PKG_DIR}/DEBIAN/control" << EOF
Package: ${PKG_NAME}
Version: ${PKG_VERSION}-1
Section: utils
Priority: optional
Architecture: ${ARCH}
Maintainer: JonW91 <jonw91@users.noreply.github.com>
Homepage: https://github.com/JonW91/CLImate
Description: Cross-platform CLI weather forecast with ASCII art
 CLImate is a command-line weather forecast application built with .NET 10.
 Get beautiful ASCII art weather forecasts directly in your terminal.
 Features include global location search, colorful ASCII art displays,
 7-day and hourly forecasts, and adaptive terminal layout.
EOF

# Build package
dpkg-deb --build "${PKG_DIR}"

# Move to current directory
mv "${BUILD_DIR}/${PKG_NAME}_${PKG_VERSION}_${ARCH}.deb" .

echo "Package built: ${PKG_NAME}_${PKG_VERSION}_${ARCH}.deb"

# Cleanup
rm -rf "${BUILD_DIR}"
