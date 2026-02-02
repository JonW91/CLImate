Name:           climate
Version:        0.1.0~beta
Release:        1%{?dist}
Summary:        Cross-platform CLI weather forecast with ASCII art
License:        MIT
URL:            https://github.com/JonW91/CLImate
Source0:        https://github.com/JonW91/CLImate/releases/download/v0.1.0-beta/climate-linux-x64.tar.gz

# Disable automatic dependency scanning (self-contained binary)
AutoReqProv:    no

%description
CLImate is a cross-platform command-line weather forecast application 
built with .NET 10. Get beautiful ASCII art weather forecasts directly 
in your terminal.

Features:
- Global location search by city, region, or address
- Colorful ASCII art weather displays
- 7-day forecasts with daily high/low temperatures
- Today view with morning/afternoon/evening/night segments
- 24-hour hourly forecasts
- Metric and Imperial unit support
- Adaptive terminal layout

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
* %(date "+%a %b %d %Y") JonW91 <jonw91@users.noreply.github.com> - 0.1.0~beta-1
- Initial beta release
- ASCII art weather forecasts
- 7-day and hourly forecast modes
- Cross-platform support
