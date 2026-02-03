# typed: false
# frozen_string_literal: true

# Homebrew formula for CLImate
# To use: brew tap jonw91/climate && brew install climate
class Climate < Formula
  desc "Cross-platform CLI weather forecast with ASCII art"
  homepage "https://github.com/JonW91/CLImate"
  version "0.1.0-beta"
  license "MIT"

  on_macos do
    on_arm do
      url "https://github.com/JonW91/CLImate/releases/download/v#{version}/climate-macos-arm64.tar.gz"
      sha256 "1d509a15ae1b803e7faa8f6378bcf6a38ff9a7ceffbc3b0131449badcce822b4"
    end
    on_intel do
      url "https://github.com/JonW91/CLImate/releases/download/v#{version}/climate-macos-x64.tar.gz"
      sha256 "133d64ec34640c4aa5018cf0908ffc4c03d7ef4c1f485ab260fc7dd00b7e8897"
    end
  end

  on_linux do
    on_arm do
      url "https://github.com/JonW91/CLImate/releases/download/v#{version}/climate-linux-arm64.tar.gz"
      sha256 "a77d4bffd0526ea224ae3cc7c7991e8afd9521c0d50fd1b7de33dcb3e48085ea"
    end
    on_intel do
      url "https://github.com/JonW91/CLImate/releases/download/v#{version}/climate-linux-x64.tar.gz"
      sha256 "15fa614c9e027b74399d312139246e86cc22a11676d47fc73e22c47365122d95"
    end
  end

  def install
    bin.install "climate"
    # Install Assets folder alongside binary
    (libexec/"Assets").install Dir["Assets/*"] if Dir.exist?("Assets")
  end

  def caveats
    <<~EOS
      CLImate has been installed!

      Usage:
        climate London
        climate --today Paris
        climate --hourly "New York"

      For weather warnings, set your MeteoBlue API key:
        export METEOBLUE_API_KEY=your_api_key
    EOS
  end

  test do
    assert_match "CLImate", shell_output("#{bin}/climate --version")
  end
end
