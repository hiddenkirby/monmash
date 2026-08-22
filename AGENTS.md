# AGENTS.md

Guidance for AI coding agents working in this repository.

## Project Context

This repo is `monmash`, the Unity project for **Tidepool**. Tidepool is a calm, offline creature-collecting game for iPad. The core loop is:

```text
Explore -> Encounter -> Catch mini-game -> Journal entry -> Explore
```

Read these files before changing behavior:

- `PRD.md` for product requirements, tone, IP guardrails, data model, and roadmap.
- `WEEKEND-PLAN.md` for the original v0.1 build order and cut-priority guidance (historical — v0.1 shipped; the descope ladder remains useful standing guidance).
- `README.md` for current setup notes.
- `docs/ROADMAP.md` for version-by-version scope.
- `docs/UNITY_IOS_PIPELINE.md` before touching build or player settings.
- `docs/V0_1_SCENE_ASSEMBLY.md` before wiring Unity scenes (Boot, Overworld, CatchEncounter, Journal).
- `docs/CONTEST_ARCHITECTURE.md` before touching the v0.2 contest loop or `ContestFlowController`.
- `docs/GROWING_UP_FORMS.md` before touching growth-form memory or journal form selection.
- `docs/DAY_NIGHT_RULES.md` before tuning conditional encounter timing.
- `docs/V0_4_ZONE_TRANSITIONS.md` before adding or modifying zone transitions.
- `docs/NO_NETWORK_GUARDRAILS.md` before adding or upgrading packages, and before release.
- `docs/ASSET_PIPELINE.md` before importing art, audio, or video assets.
- `docs/PRE_RELEASE_CHECKLIST.md` and `docs/IP_SAFETY_CHECKLIST.md` before tagging a playable or release build.
- `docs/PUBLIC_RELEASE_ASSET_AUDIT.md` and `docs/PRIVACY_RELEASE_NOTES.md` for release-facing asset and privacy posture.

## Current Project State

v0.1 ("It Catches") is built and merged: two zones, tap-to-move, seagrass encounters, the Steady-the-Jar catch mini-game, journal grid with nicknames, and JSON save/load are all in the codebase and have been through multiple PR cycles.

Work has progressed past v0.1 into v0.2 and v0.4 scaffolding:

- **v0.2 Contests:** `ContestFlowController`, `ContestContext`, `ContestMove` (ScriptableObject), `ContestParticipantState` (tuckered-out rest state), and a `Contest` scene generator are present. See `docs/CONTEST_ARCHITECTURE.md`.
- **v0.4 Growing-Up Forms:** `TidelingGrowthForms`, `TidelingLevelProgression`, growth-form save memory (`rememberedGrowthFormIds`, `activeGrowthFormId`), and journal form-selection hooks are present. See `docs/GROWING_UP_FORMS.md`.
- **Conditional encounters:** `EncounterAvailability` and day/night rules are wired into `EncounterDirector`. See `docs/DAY_NIGHT_RULES.md`.
- **Zone transitions:** `ZoneTransitionTrigger` exists. See `docs/V0_4_ZONE_TRANSITIONS.md`.
- **Settings:** `TidepoolSettingsService` and `SettingsController` handle audio mute/volume at runtime.

Before starting work, check `git log` and open GitHub issues to see what is already in progress. Do not duplicate or regress existing systems.

## Non-Negotiable Product Constraints

- No network behavior: no analytics, ads, IAP, accounts, notifications, crash SDKs, telemetry, login, multiplayer, or online services.
- No protected franchise references in code, assets, prompts, docs intended for release, UI text, or store copy.
- Do not implement aiming, throwing, projectile capture, capture probability displays, or creature riding.
- Nothing is ever lost. Failed catches cost time only, never creatures, progress, currency, or items.
- Keep the tone warm and non-scolding. Use lines like `It slipped away!`, not failure language.
- v0.1 excludes battles, levels, growing-up forms, day/night, multiplayer, trading, crafting, and economy systems — but note that v0.2 contest and v0.4 growth-form scaffolding has begun behind feature flags and editor tools. Check the issue before assuming a system is off-limits.

## Technical Baseline

- Engine: Unity 6.5.6 (6000.5.6f1), 2D URP.
- Target: iOS 15+, iPad, landscape only.
- Language: C#.
- Scripting backend: IL2CPP, ARM64.
- Build path: Unity -> Xcode -> iPad by cable.
- Runtime save: JSON through `Application.persistentDataPath/save.json`, not `PlayerPrefs`.
- Core data should be designer-editable through ScriptableObjects where practical.

## Repository Practices

- Keep Unity-generated caches out of Git: `Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, and user settings are ignored.
- Use Git LFS for binary art/audio/video assets. Do not commit large binaries before `git lfs install --local` has been run.
- Keep `.meta` files when Unity generates them. They are part of Unity asset identity.
- Do not reorganize Unity folders casually; scene and asset references are path/GUID sensitive.
- Prefer small, scoped commits and PRs mapped to GitHub issues.
- When picking up an issue, immediately mark it with the `status: in progress` label so another agent does not claim it.
- Keep GitHub issue state aligned with the work: link PRs with closing keywords when appropriate, close completed issues after merge, and leave explicit handoff comments for verification that could not be performed.
- Remove `status: in progress` when the issue is closed, released back to the backlog, or handed off.

## Verification Scripts

Run these from the repo root before release, after package changes, or before committing binary assets:

| Script | When to run |
|---|---|
| `scripts/verify-no-network-guardrails.sh` | After package changes; before release. Scans for outbound network APIs and SDKs. |
| `scripts/verify-ip-safety-guardrails.sh` | Before any store page, screenshot, trailer, or release-facing text. Scans for protected franchise terms. |
| `scripts/verify-lfs.sh` | Before committing binary art/audio/video assets. Confirms LFS tracking is active. |
| `scripts/generate-app-icon.py` | When regenerating the iOS app icon. |
| `scripts/generate-minimal-audio-assets.py` | When regenerating the CC0 placeholder audio set. |

## Editor Tools

Unity menu items under `Tools/Tidepool/`:

| Menu item | What it does |
|---|---|
| Create Starter Species Assets | Creates the 13 `TidelingSpecies` ScriptableObject rows from the PRD. |
| Create v0.1 Overworld Scene | Generates `Assets/Scenes/Overworld.unity` with tilemaps, player, and encounter wiring. |
| Create v0.1 CatchEncounter Scene | Generates `Assets/Scenes/CatchEncounter.unity` with the catch UI and safe-area layout. |
| Create v0.2 Contest Scene | Generates `Assets/Scenes/Contest.unity` with the contest flow UI. |
| Configure v0.1 Local Playtest | Sets up local playtest configuration. |
| Apply iPad iOS Player Settings | Applies all iPad/iOS player settings from `docs/UNITY_IOS_PIPELINE.md`. |
| Validate iPad iOS Player Settings | Validates that player settings match the required values. |

Prefer these generators over hand-building scenes. Inspect the generated scene in Unity before iterating.

## Asset Rules

- Every non-code asset that ships or may ship must be logged in `Assets/ASSET_MANIFEST.md`.
- For AI-generated assets, log tool, model, date, prompt, and review status.
- For CC0 or third-party assets, log source, license, date, and path.
- Never prompt with protected names, existing characters, protected franchise terms, or living artist names.
- Review creature art for accidental resemblance before committing it as a shipping asset.
- Prefer illustrated transparent PNG creature sprites around 512px. Avoid fake pixel-art output unless the art pipeline changes deliberately.
- See `docs/ASSET_PIPELINE.md` for the full import checklist.

## Unity Implementation Guidance

- Prove iPad build pipeline before expanding gameplay.
- Keep scene responsibilities aligned with the PRD and docs:
  - `Boot`: load save and route onward.
  - `Overworld`: tilemap, player movement, encounters.
  - `CatchEncounter`: additive catch mini-game scene.
  - `Journal`: additive journal overlay.
  - `Contest`: v0.2 friendly contest scene (additive). See `docs/CONTEST_ARCHITECTURE.md`.
- Use safe-area aware UI for every screen.
- Use Canvas Scaler with reference resolution `1024 x 768`, match `0.5`.
- Core touch targets must be at least 88pt.
- Support touch input first. Mouse input is acceptable for editor testing, but not as the only path.
- Do not mix input systems casually. The scaffold uses legacy `Input`.
- Keep gameplay code deterministic and easy to inspect. Avoid adding frameworks unless the benefit is clear.

## C# Style

- Keep namespaces under `Tidepool.Domain`, `Tidepool.Runtime`, `Tidepool.UI`, or `Tidepool.Editor`.
- Prefer explicit serialized fields over scene-wide lookups.
- Keep runtime data serializable with Unity `JsonUtility` unless there is a strong reason to change.
- Add comments only where they clarify non-obvious behavior or constraints.
- Do not introduce async/network libraries.
- Handle missing assets gracefully during the prototype. A missing sprite should not crash the game.

## Testing and Verification

For gameplay changes, verify at the lowest level available:

- Compile in Unity after C# changes.
- Run the relevant scene in the editor.
- For movement, catching, save/load, and UI work, test on iPad before calling the issue done.
- For save changes, test catch -> force-quit -> relaunch -> journal still contains progress.
- For UI changes, check safe area and 88pt touch targets on the target device.
- Run `scripts/verify-no-network-guardrails.sh` after package or code changes that could introduce network behavior.

If Unity is not available in the environment, state that clearly in the handoff and list what was checked locally.

## Scope Control

The original weekend build (see `WEEKEND-PLAN.md`) is complete. The descope ladder below remains useful standing guidance when time is tight on any milestone:

1. Title screen, Old Barnaby, Current icons.
2. Audio beyond the catch chime.
3. Roster from 13 to 8 to 6.
4. Second zone.
5. Journal detail pages, while preserving grid and nicknames.
6. Escape-on-three-misses, making catch always succeed if needed.

Never cut tap-to-move, encounters, catching, the journal grid, nicknames, or save/load.

Current active scope includes v0.2 contests and v0.4 growth forms per `docs/ROADMAP.md`. Do not start work outside the roadmap unless an issue explicitly asks for it.

## Handoff Expectations

When finishing work, report:

- Issue number or task addressed.
- Files changed.
- What was verified.
- What could not be verified, especially Unity editor or iPad build steps.
- Any asset provenance entries that still need completion.
