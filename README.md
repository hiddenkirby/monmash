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
- Runtime C# systems for:
  - Tideling species data
  - save/load JSON
  - grid pathfinding and tap-to-move
  - seagrass encounter rolls
  - catch mini-game controller
  - journal UI controller
  - safe-area UI fitting
- Great Low Tide v0.8 foundations for four expedition chapters, ambient Tidelings,
  authored discoveries, persistent route reveals, the Coast Atlas, the Field Station,
  the coast festival, and the Old Barnaby finale
- Committed Boot, Overworld, CatchEncounter, Journal, Contest, CharacterSelect, and
  PartySelect scenes
- Editor generators and validators under `Tools/Tidepool/`

The v0.8 source foundations and Field Station/festival/finale desktop-Editor wiring are in
place, but v0.8 is not release-verified.
Target-iPad performance, touch, safe-area, force-quit/relaunch, and complete new/legacy-save
journeys remain required. See [`docs/V0_8_INTEGRATION_CHECKLIST.md`](docs/V0_8_INTEGRATION_CHECKLIST.md)
for the reproducible scene order and the exact remaining gates.

## First Unity Steps

1. Install Unity 6.5.6 (6000.5.6f1) with iOS Build Support.
2. Open this folder as a Unity project.
3. Let Unity import the project and confirm the Console has no compile errors.
4. Use the committed scenes for normal development. To prove a clean generated setup, follow
   the ordered generator runbook in `docs/V0_8_INTEGRATION_CHECKLIST.md` instead of running
   scene generators in an arbitrary order.
5. Run the relevant `Tools/Tidepool/Verify ...` menu checks.
6. Build to iPad and complete the device checks before calling a milestone release-ready.

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
