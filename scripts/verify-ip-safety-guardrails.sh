#!/usr/bin/env bash
set -euo pipefail

scan_paths=(
  "Assets"
  "Packages"
  "ProjectSettings"
  "README.md"
  "docs"
)

patterns=(
  "pok.mon"
  "pocket[[:space:]_-]*monster"
  "pok.?[[:space:]_-]*ball"
  "pok.?dex"
  "gym[[:space:]_-]*leader"
  "gotta[[:space:]_-]*catch"
)

rg_args=(
  "--line-number"
  "--ignore-case"
  "--glob" "!Assets/ASSET_MANIFEST.md"
  "--glob" "!docs/IP_SAFETY_CHECKLIST.md"
  "--glob" "!docs/PRE_RELEASE_CHECKLIST.md"
  "--glob" "!*.png"
  "--glob" "!*.jpg"
  "--glob" "!*.jpeg"
  "--glob" "!*.psd"
  "--glob" "!*.aseprite"
  "--glob" "!*.wav"
  "--glob" "!*.mp3"
  "--glob" "!*.ogg"
  "--glob" "!*.aif"
  "--glob" "!*.aiff"
  "--glob" "!*.mp4"
  "--glob" "!*.mov"
)

for pattern in "${patterns[@]}"; do
  rg_args+=("--regexp" "$pattern")
done

if rg "${rg_args[@]}" "${scan_paths[@]}"; then
  echo "Release-facing files contain protected-franchise terms or near-miss terms that need IP review." >&2
  exit 1
fi

echo "No protected-franchise terms found in release-facing scan paths."
