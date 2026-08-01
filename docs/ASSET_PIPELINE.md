# Asset Pipeline

Tidepool keeps binary art, audio, and video assets in Git LFS so normal Git history stays small.

## Before Importing Assets

1. Install Git LFS if needed:
   ```sh
   brew install git-lfs
   ```
2. Initialize LFS for this repo:
   ```sh
   git lfs install --local
   ```
3. Verify the tracked patterns:
   ```sh
   git lfs track --list
   scripts/verify-lfs.sh
   ```

## Required LFS Patterns

The root `.gitattributes` must keep these asset families on LFS:

- Raster and source art: `*.png`, `*.jpg`, `*.jpeg`, `*.psd`, `*.aseprite`
- Audio: `*.wav`, `*.mp3`, `*.ogg`, `*.aif`, `*.aiff`
- Video: `*.mp4`, `*.mov`

## Import Checklist

- Add the asset under `Assets/Art`, `Assets/Audio`, or another Unity asset folder.
- Keep the generated `.meta` file with the asset.
- Run `git check-attr filter -- path/to/asset` and confirm `filter: lfs`.
- After staging, run `git lfs status` and confirm the asset appears as a Git LFS object.
- Log provenance in `Assets/ASSET_MANIFEST.md` before the asset can ship.
