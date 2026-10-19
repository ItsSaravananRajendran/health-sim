#!/usr/bin/env bash
# Publish Builds/WebGL to the gh-pages branch (GitHub Pages).
# Usage: tools/publish-pages.sh   (run after the WebGL build)
set -euo pipefail

root="$(git rev-parse --show-toplevel)"
build="$root/Builds/WebGL"
[ -f "$build/index.html" ] || { echo "No build at $build. Run the WebGL build first." >&2; exit 1; }

remote_url="$(git -C "$root" remote get-url origin)"
work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT

# Fresh single-commit history each time keeps the branch small.
cp -r "$build"/. "$work"/
rm -f "$work/unity-build.provenance.json"
touch "$work/.nojekyll"

cd "$work"
git init -q -b gh-pages
git add -A
git commit -q -m "Publish WebGL build $(date -u +%Y-%m-%dT%H:%MZ)"
git push -f "$remote_url" gh-pages
echo "Pushed gh-pages. Enable Pages: Settings > Pages > Deploy from branch > gh-pages / (root)."
