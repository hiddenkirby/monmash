#!/usr/bin/env bash
set -euo pipefail

patterns=(
  "Assets/Art/Creatures/lfs-probe.png"
  "Assets/Art/Creatures/lfs-probe.jpg"
  "Assets/Art/Creatures/lfs-probe.psd"
  "Assets/Art/Creatures/lfs-probe.aseprite"
  "Assets/Audio/lfs-probe.wav"
  "Assets/Audio/lfs-probe.mp3"
  "Assets/Audio/lfs-probe.ogg"
  "Assets/Audio/lfs-probe.aif"
  "Assets/Audio/lfs-probe.aiff"
  "Assets/Art/lfs-probe.mp4"
  "Assets/Art/lfs-probe.mov"
)

git lfs version >/dev/null
git lfs env >/dev/null

for path in "${patterns[@]}"; do
  attr="$(git check-attr filter -- "$path")"
  if [[ "$attr" != "$path: filter: lfs" ]]; then
    echo "Expected $path to use the lfs filter, got: $attr" >&2
    exit 1
  fi
done

tmpdir="$(mktemp -d)"
trap 'rm -rf "$tmpdir"' EXIT
probe="$tmpdir/lfs-probe.png"
printf '\x89PNG\r\n\x1a\n' > "$probe"

if ! git lfs pointer --file="$probe" | grep -q "https://git-lfs.github.com/spec/v1"; then
  echo "Git LFS did not generate a pointer for the probe asset." >&2
  exit 1
fi

echo "Git LFS is installed, initialized, and tracking Tidepool binary asset patterns."
