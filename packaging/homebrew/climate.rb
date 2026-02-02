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
      sha256 "PLACEHOLDER_SHA256_MACOS_ARM64"
    end
    on_intel do
      url "https://github.com/JonW91/CLImate/releases/download/v#{version}/climate-macos-x64.tar.gz"
      sha256 "PLACEHOLDER_SHA256_MACOS_X64"
    end
  end

  on_linux do
    on_arm do
      url "https://github.com/JonW91/CLImate/releases/download/v#{version}/climate-linux-arm64.tar.gz"
      sha256 "PLACEHOLDER_SHA256_LINUX_ARM64"
    end
    on_intel do
      url "https://github.com/JonW91/CLImate/releases/download/v#{version}/climate-linux-x64.tar.gz"
      sha256 "PLACEHOLDER_SHA256_LINUX_X64"
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
