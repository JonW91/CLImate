#!/usr/bin/env sh
set -euf

PROJECT="${1:-CLImate.App}"
DIST_DIR="${DIST_DIR:-dist}"

RUNTIMES="linux-x64 linux-arm64 macos-x64 macos-arm64 win-x64"

rm -rf "$DIST_DIR"
mkdir -p "$DIST_DIR"

for RID in $RUNTIMES; do
  OUT_DIR="$DIST_DIR/$RID"
  dotnet publish "$PROJECT" -c Release -r "$RID" \
    -p:PublishSingleFile=true \
    -p:SelfContained=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -o "$OUT_DIR"

  BIN_NAME="climate"
  EXT="tar.gz"
  if echo "$RID" | grep -q "win-"; then
    BIN_NAME="climate.exe"
    EXT="zip"
  fi

  if [ ! -f "$OUT_DIR/$BIN_NAME" ]; then
    echo "Missing binary for $RID" >&2
    exit 1
  fi

  if [ "$EXT" = "zip" ]; then
    (cd "$OUT_DIR" && zip -q "../climate-${RID%%-*}-${RID#*-}.zip" "$BIN_NAME")
  else
    (cd "$OUT_DIR" && tar -czf "../climate-${RID%%-*}-${RID#*-}.tar.gz" "$BIN_NAME")
  fi

done

echo "Artifacts in $DIST_DIR"
