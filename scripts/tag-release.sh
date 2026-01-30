#!/usr/bin/env sh
set -euf

if [ $# -ne 1 ]; then
  echo "Usage: $0 <version> (e.g., 0.1.0)" >&2
  exit 1
fi

VERSION="$1"
TAG="v$VERSION"

git tag -a "$TAG" -m "Release $TAG"

echo "Created tag $TAG"

echo "Push with: git push origin $TAG"
