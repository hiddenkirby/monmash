# Tidepool

Tidepool is a calm, offline creature-collecting game prototype for iPad. The v0.1 goal is the loop from the PRD:

```text
Explore -> Encounter -> Catch mini-game -> Journal entry -> Explore
```

This repository is initialized for Unity 6.5.6 (6000.5.6f1), 2D URP, iOS 15+, landscape iPad builds.

## Current Setup

- Git repo initialized on `main`
- Remote: `git@github.com:hiddenkirby/monmash.git`
- Unity-style project layout under `Assets/`, `Packages/`, and `ProjectSettings/`
- Git LFS tracking patterns for art/audio/video assets in `.gitattributes`
- Runtime C# scaffolding for:
  - Tideling species data
  - save/load JSON
  - grid pathfinding and tap-to-move
  - seagrass encounter rolls
  - catch mini-game controller
  - journal UI controller
  - safe-area UI fitting
- Editor helper: `Tools/Tidepool/Create Starter Species Assets`

## First Unity Steps

1. Install Unity 6.5.6 (6000.5.6f1) with iOS Build Support.
2. Open this folder as a Unity project.
3. Let Unity generate missing `.meta`, `.csproj`, and scene metadata.
4. Run `Tools/Tidepool/Create Starter Species Assets` to create the 13 ScriptableObject species rows from the PRD.
5. Create these scenes:
   - `Assets/Scenes/Boot.unity`
   - `Assets/Scenes/Overworld.unity`
   - `Assets/Scenes/CatchEncounter.unity`
   - `Assets/Scenes/Journal.unity`
6. Build a blank scene to iPad before adding more gameplay.

## Important Local Tooling Note

Binary art, audio, and video assets must go through Git LFS. Run this before committing binary assets:

```sh
brew install git-lfs
git lfs install --local
scripts/verify-lfs.sh
```

See `docs/ASSET_PIPELINE.md` for the full import checklist.

## Hard Requirements

- No network SDKs, analytics, ads, IAP, accounts, or notifications.
- Do not use protected franchise names in code, assets, prompts, or store copy.
- Log every asset source in `Assets/ASSET_MANIFEST.md`.
- Keep iPad UI touch targets at least 88pt.

Before release or package changes, run:

```sh
scripts/verify-no-network-guardrails.sh
```

See `docs/NO_NETWORK_GUARDRAILS.md` and `docs/PRE_RELEASE_CHECKLIST.md` for the full offline/privacy checklist.
