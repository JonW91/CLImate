#!/bin/bash
# Build RPM package for CLImate
# Usage: ./build-rpm.sh <version> <architecture>
# Example: ./build-rpm.sh 0.1.0-beta x86_64

set -e

VERSION="${1:-0.1.0-beta}"
ARCH="${2:-x86_64}"
PKG_NAME="climate"
RPM_VERSION="${VERSION//-/\~}"  # Replace - with ~ for RPM versioning

echo "Building RPM package for CLImate v${VERSION} (${ARCH})..."

# Setup rpmbuild directory structure
RPMBUILD_DIR="${HOME}/rpmbuild"
mkdir -p "${RPMBUILD_DIR}"/{BUILD,RPMS,SOURCES,SPECS,SRPMS}

# Download source tarball
DOWNLOAD_ARCH="x64"
[ "$ARCH" = "aarch64" ] && DOWNLOAD_ARCH="arm64"

curl -fsSL "https://github.com/JonW91/CLImate/releases/download/v${VERSION}/climate-linux-${DOWNLOAD_ARCH}.tar.gz" \
    -o "${RPMBUILD_DIR}/SOURCES/climate-linux-${DOWNLOAD_ARCH}.tar.gz"

# Create spec file
cat > "${RPMBUILD_DIR}/SPECS/climate.spec" << EOF
Name:           climate
Version:        ${RPM_VERSION}
Release:        1%{?dist}
Summary:        Cross-platform CLI weather forecast with ASCII art
License:        MIT
URL:            https://github.com/JonW91/CLImate
Source0:        climate-linux-${DOWNLOAD_ARCH}.tar.gz

AutoReqProv:    no
BuildArch:      ${ARCH}

%description
CLImate is a cross-platform command-line weather forecast application 
built with .NET 10. Get beautiful ASCII art weather forecasts directly 
in your terminal.

%prep
%setup -q -c

%install
mkdir -p %{buildroot}%{_bindir}
mkdir -p %{buildroot}%{_datadir}/%{name}/Assets
install -m 755 climate %{buildroot}%{_bindir}/climate
cp -r Assets/* %{buildroot}%{_datadir}/%{name}/Assets/ 2>/dev/null || true

%files
%{_bindir}/climate
%{_datadir}/%{name}

%changelog
* $(date "+%a %b %d %Y") JonW91 <jonw91@users.noreply.github.com> - ${RPM_VERSION}-1
- Package release for v${VERSION}
EOF

# Build RPM
rpmbuild -bb "${RPMBUILD_DIR}/SPECS/climate.spec"

# Copy result
find "${RPMBUILD_DIR}/RPMS" -name "*.rpm" -exec cp {} . \;

echo "RPM package built successfully!"
