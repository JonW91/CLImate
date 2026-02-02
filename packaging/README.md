# CLImate Packaging

This directory contains packaging configurations for distributing CLImate through various package managers.

## Package Managers

### 🍺 Homebrew (macOS/Linux)

**Location:** `homebrew/climate.rb`

To create a Homebrew tap:

1. Create a new repository: `homebrew-climate`
2. Copy `climate.rb` to the repo root or `Formula/` directory
3. Update SHA256 hashes after each release
4. Users install with:
   ```bash
   brew tap jonw91/climate
   brew install climate
   ```

**Automating SHA256 updates:** The release workflow can compute and update hashes automatically.

---

### 🪟 Scoop (Windows)

**Location:** `scoop/climate.json`

To create a Scoop bucket:

1. Create a new repository: `scoop-climate`
2. Copy `climate.json` to the `bucket/` directory
3. Update SHA256 hash after each release
4. Users install with:
   ```powershell
   scoop bucket add climate https://github.com/JonW91/scoop-climate
   scoop install climate
   ```

---

### 🍫 Chocolatey (Windows)

**Location:** `chocolatey/`

To publish to Chocolatey:

1. Update `climate.nuspec` with correct version
2. Update SHA256 in `tools/chocolateyinstall.ps1`
3. Pack: `choco pack climate.nuspec`
4. Push: `choco push climate.0.1.0-beta.nupkg --source https://push.chocolatey.org/`

**Requirements:** Chocolatey account and API key.

---

### 📦 WinGet (Windows)

**Location:** `winget/JonW91.CLImate.yaml`

To submit to WinGet:

1. Fork [microsoft/winget-pkgs](https://github.com/microsoft/winget-pkgs)
2. Copy manifest to `manifests/j/JonW91/CLImate/0.1.0-beta/`
3. Update SHA256 hash
4. Submit pull request

**Validation:** Run `winget validate --manifest <path>` before submitting.

---

### 🎩 DNF/RPM (Fedora/RHEL/CentOS)

**Location:** `rpm/`

Build RPM package:

```bash
cd packaging/rpm
chmod +x build-rpm.sh
./build-rpm.sh 0.1.0-beta x86_64
```

**Install locally:**
```bash
sudo dnf install ./climate-0.1.0~beta-1.x86_64.rpm
```

**To create a COPR repository:**
1. Create account at [copr.fedorainfracloud.org](https://copr.fedorainfracloud.org)
2. Create new project
3. Upload SRPM or link to spec file

---

### 📦 APT/DEB (Debian/Ubuntu)

**Location:** `deb/`

Build DEB package:

```bash
cd packaging/deb
chmod +x build-deb.sh
./build-deb.sh 0.1.0-beta amd64
```

**Install locally:**
```bash
sudo dpkg -i climate_0.1.0~beta_amd64.deb
```

**To create a PPA (Ubuntu):**
1. Create account at [launchpad.net](https://launchpad.net)
2. Create a PPA
3. Upload source package with `dput`

---

### 📦 NuGet (.NET Global Tool)

Already configured in `CLImate.App.csproj`. To publish:

```bash
dotnet pack CLImate.App -c Release
dotnet nuget push bin/Release/CLImate.*.nupkg --api-key <key> --source https://api.nuget.org/v3/index.json
```

---

## Version Updates

When releasing a new version:

1. Update version in `CLImate.App.csproj`
2. Create and push git tag: `git tag v0.2.0 && git push origin v0.2.0`
3. Wait for release workflow to create binaries
4. Compute SHA256 hashes:
   ```bash
   sha256sum climate-*.tar.gz climate-*.zip
   ```
5. Update hashes in all package manifests
6. Commit and push packaging updates
7. Submit to respective package repositories

## SHA256 Hash Placeholders

All manifests contain `PLACEHOLDER_SHA256_*` values that must be replaced with actual hashes after the release binaries are built.

Use this script to compute hashes:
```bash
for f in climate-*.tar.gz climate-*.zip; do
    echo "$f: $(sha256sum "$f" | cut -d' ' -f1)"
done
```
