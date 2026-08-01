# AGENTS.md

Guidance for AI coding agents working in this repository.

## Project Context

This repo is `monmash`, the Unity project for **Tidepool**. Tidepool is a calm, offline creature-collecting game for iPad. The v0.1 target is the loop:

```text
Explore -> Encounter -> Catch mini-game -> Journal entry -> Explore
```

Read these files before changing behavior:

- `PRD.md` for product requirements, tone, IP guardrails, data model, and roadmap.
- `WEEKEND-PLAN.md` for the immediate build order and cut lines.
- `README.md` for current setup notes.
- `docs/UNITY_IOS_PIPELINE.md` before touching build or player settings.
- `docs/V0_1_SCENE_ASSEMBLY.md` before wiring Unity scenes.

## Non-Negotiable Product Constraints

- No network behavior: no analytics, ads, IAP, accounts, notifications, crash SDKs, telemetry, login, multiplayer, or online services.
- No protected franchise references in code, assets, prompts, docs intended for release, UI text, or store copy.
- Do not implement aiming, throwing, projectile capture, capture probability displays, or creature riding.
- Nothing is ever lost. Failed catches cost time only, never creatures, progress, currency, or items.
- Keep the tone warm and non-scolding. Use lines like `It slipped away!`, not failure language.
- v0.1 excludes battles, levels, growing-up forms, day/night, multiplayer, trading, crafting, and economy systems.

## Technical Baseline

- Engine: Unity 6.3 LTS, 2D URP.
- Target: iOS 15+, iPad, landscape only.
- Language: C#.
- Build path: Unity -> Xcode -> iPad by cable.
- Runtime save: JSON through `Application.persistentDataPath/save.json`, not `PlayerPrefs`.
- Core data should be designer-editable through ScriptableObjects where practical.

## Repository Practices

- Keep Unity-generated caches out of Git: `Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, and user settings are ignored.
- Use Git LFS for binary art/audio/video assets. Do not commit large binaries before `git lfs install --local` has been run.
- Keep `.meta` files when Unity generates them. They are part of Unity asset identity.
- Do not reorganize Unity folders casually; scene and asset references are path/GUID sensitive.
- Prefer small, scoped commits and PRs mapped to GitHub issues.

## Asset Rules

- Every non-code asset that ships or may ship must be logged in `Assets/ASSET_MANIFEST.md`.
- For AI-generated assets, log tool, model, date, prompt, and review status.
- For CC0 or third-party assets, log source, license, date, and path.
- Never prompt with protected names, existing characters, protected franchise terms, or living artist names.
- Review creature art for accidental resemblance before committing it as a shipping asset.
- Prefer illustrated transparent PNG creature sprites around 512px. Avoid fake pixel-art output unless the art pipeline changes deliberately.

## Unity Implementation Guidance

- Prove iPad build pipeline before expanding gameplay.
- Keep scene responsibilities aligned with the PRD:
  - `Boot`: load save and route onward.
  - `Overworld`: tilemap, player movement, encounters.
  - `CatchEncounter`: additive catch mini-game scene.
  - `Journal`: additive journal overlay.
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

If Unity is not available in the environment, state that clearly in the handoff and list what was checked locally.

## Scope Control

The weekend build is intentionally narrow. If time is tight, cut in this order:

1. Title screen, Old Barnaby, Current icons.
2. Audio beyond the catch chime.
3. Roster from 13 to 8 to 6.
4. Second zone.
5. Journal detail pages, while preserving grid and nicknames.
6. Escape-on-three-misses, making catch always succeed if needed.

Never cut tap-to-move, encounters, catching, the journal grid, nicknames, or save/load.

## Handoff Expectations

When finishing work, report:

- Issue number or task addressed.
- Files changed.
- What was verified.
- What could not be verified, especially Unity editor or iPad build steps.
- Any asset provenance entries that still need completion.

