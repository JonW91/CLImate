# Release test checklist: vX.Y.Z

<!--
How to use this file
1. Before a release, copy this file to docs/release-testing/vX.Y.Z.md.
2. Fill in the Status block.
3. Work down the sections. Tick an item by changing "- [ ]" to "- [x]".
   If an item fails or looks wrong, leave it unticked and add a row to Findings
   using the item's ID (for example C4).
4. Tag the release only when every item is ticked or its finding is marked
   "accepted" or "fixed".
Items marked (after tag) can only be done once the release workflow has run.
-->

## Status

| Field | Value |
| --- | --- |
| Version | vX.Y.Z |
| Started | YYYY-MM-DD |
| Tested by | |
| Machine / terminal | |
| Verdict | not started / in progress / ready to tag / released |

## What changed in this release

- 

## A. Build and automated tests

- [ ] **A1** `dotnet build CLImate.App -c Release` finishes with 0 warnings and 0 errors
- [ ] **A2** `dotnet test CLImate.Tests -c Release` passes (note the count in Findings if it changed)
- [ ] **A3** `<Version>` in `CLImate.App/CLImate.App.csproj` matches the version above
- [ ] **A4** CI is green on `main` for the commit being tagged

## B. Core commands

Run these from the source build (`dotnet run --project CLImate.App -c Release -- <args>`) or an installed binary.

- [ ] **B1** `climate --version` prints the version above
- [ ] **B2** `climate --help` lists every option and the config commands
- [ ] **B3** `climate London` shows a 7-day forecast
- [ ] **B4** `climate --today Paris` shows night / morning / afternoon / evening
- [ ] **B5** `climate --hourly London` shows the 24-hour view
- [ ] **B6** `climate --units imperial "New York, NY"` shows °F, mph and inches
- [ ] **B7** `climate --country US Portland` only offers US matches
- [ ] **B8** `climate Springfield` prompts for a location; an invalid choice re-prompts
- [ ] **B9** `echo 1 | climate Springfield` picks a location without hanging
- [ ] **B10** `climate zzzzqqqq` gives a clear "not found" message and a non-zero exit code
- [ ] **B11** An unknown flag (`climate --nope London`) prints usage rather than a stack trace

## C. Layout and ASCII art

The 7-day table picks 18-, 16- or 14-column cells to fit the terminal: 134, 120 and 106 columns wide in total.
Re-run the command after resizing the window. Output already on screen is re-wrapped by the terminal, not by CLImate.

- [ ] **C1** `climate -V Phoenix` (vertical list): art shows, every block is 9 rows tall
- [ ] **C2** `climate -H Phoenix` at 134+ columns: multi-row art in every cell, 18-column cells
- [ ] **C3** `climate -H Phoenix` at about 125 and about 110 columns: still multi-row art, narrower cells, no wrapped or broken borders
- [ ] **C3b** `climate -H Phoenix` under 106 columns: a "too narrow" note, then the list view
- [ ] **C3c** `climate Phoenix` with no flag: table at 134+ columns, list view below that
- [ ] **C4** Sun (clear sky): top and bottom `|`, the diagonal rays and the `88888` disk share one vertical axis, in both `-V` and `-H`
- [ ] **C5** Partly cloudy: hyphen side rays present and aligned with the cloud
- [ ] **C6** Day labels are not cut off with `…` in table cells (check "Mainly clear", "Partly cloudy", "Hailstorm", "Freezing drizzle")
- [ ] **C7** Resize the terminal to under 45 columns: output degrades cleanly with no art
- [ ] **C8** `climate --no-art London` shows text labels instead of art
- [ ] **C9** Table borders line up on every row (no cell pushed left or right)

## D. Colour

`climate --gallery` prints every icon at each size, with no location or network needed. Use it for conditions nowhere is forecasting.

- [ ] **D1** The whole sun is the same bold yellow, including the horizontal hyphen rays, on clear sky and partly cloudy
- [ ] **D2** Clouds are bold dark grey for rain, freezing rain, rain showers, thunderstorm and hailstorm, and light grey for everything else; rain `/` and drizzle `'` are blue
- [ ] **D3** Snow `*` is white with grey cloud (`climate --gallery`)
- [ ] **D4** Thunderstorm bolts are bold yellow; hail `*` between the bolts is white (`climate --gallery`)
- [ ] **D7** `climate --gallery`: all 14 icons show at large, medium and small, every one is aligned, and none has the wrong colours
- [ ] **D5** `climate --no-colour London` prints no colour codes
- [ ] **D6** `climate London | cat` prints no stray escape characters

## E. Config

- [ ] **E1** `climate config show` prints the current config
- [ ] **E2** `climate config set units imperial`, then `climate London` uses imperial; set it back to metric
- [ ] **E3** `climate config set country GB` filters results; clear or reset it afterwards
- [ ] **E4** `climate config set art off` and `colour off` take effect; turn both back on
- [ ] **E5** `climate config add-favourite London` saves the location

## F. Weather warnings

- [ ] **F1** A US location with an active alert shows the warning line (NWS)
- [ ] **F2** A European location shows warnings or none, without errors (Meteoalarm)
- [ ] **F3** A location outside both regions shows "no warnings available" rather than an error

## G. Release and install (after tag)

- [ ] **G1** Release workflow succeeded for the tag
- [ ] **G2** Release page has 10 assets: 5 archives (linux-x64, linux-arm64, osx-x64, osx-arm64, win-x64) and 5 `.sha256` files
- [ ] **G3** `curl -fsSL https://raw.githubusercontent.com/JonW91/CLImate/main/scripts/install.sh | sh` installs the new version
- [ ] **G4** After G3, `~/.local/bin/Assets/ascii-art.json` has a fresh timestamp
- [ ] **G5** `which -a climate` shows only the copy you expect; `climate --version` is the new version
- [ ] **G6** Installed binary shows real art, not `[ CLEAR ]` placeholders
- [ ] **G7** Homebrew formula in `JonW91/homebrew-climate` updated: version and all four sha256 values
- [ ] **G8** `brew install jonw91/climate/climate` (or `brew upgrade climate`) works and shows art
- [ ] **G9** Windows: `install.ps1` installs the new version (when a Windows machine is available)

## H. Docs

- [ ] **H1** README examples and option list match `climate --help`
- [ ] **H2** ROADMAP.md reflects what shipped

## Findings

Severity: blocker (do not release), major, minor, cosmetic.
Status: open, fixed, accepted (releasing anyway), deferred.

| # | Item | What happened | Severity | Status | Notes / commit |
| --- | --- | --- | --- | --- | --- |
| 1 | | | | | |

## Sign-off

- [ ] Every item above is ticked, or has a finding marked fixed, accepted or deferred
- [ ] Tag pushed: `git tag vX.Y.Z && git push origin vX.Y.Z`
