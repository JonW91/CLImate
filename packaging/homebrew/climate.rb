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
      sha256 "be3d461e22e907aabaa4ef9a6bebda586912d56629083b773bf41962921afdef"
    end
    on_intel do
      url "https://github.com/JonW91/CLImate/releases/download/v#{version}/climate-macos-x64.tar.gz"
      sha256 "ed7571e011231834e705cfe62ef4b95c8f65ccda63115cbf8f9501493abfd615"
    end
  end

  on_linux do
    on_arm do
      url "https://github.com/JonW91/CLImate/releases/download/v#{version}/climate-linux-arm64.tar.gz"
      sha256 "e8773b1152a1424518473cfa02ac77f2469ea5690fa9a62d0f327392d339e910"
    end
    on_intel do
      url "https://github.com/JonW91/CLImate/releases/download/v#{version}/climate-linux-x64.tar.gz"
      sha256 "af2ba8f51d83c3e313ab403ff61d4fad0ebafc4236939b493d5488dca6a3b8d5"
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
