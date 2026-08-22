# CLAUDE.md

Instructions for Claude and other assistant agents working on Tidepool.

Use `AGENTS.md` as the source of truth. This file is a shorter operational checklist for day-to-day work.

## First Read

Before editing, read:

1. `AGENTS.md`
2. `PRD.md`
3. `WEEKEND-PLAN.md` (historical — v0.1 shipped; descope ladder still useful)
4. `docs/ROADMAP.md`
5. The relevant docs in `docs/` for your task:
   - `UNITY_IOS_PIPELINE.md` — build/player settings
   - `V0_1_SCENE_ASSEMBLY.md` — Boot/Overworld/CatchEncounter/Journal scene wiring
   - `CONTEST_ARCHITECTURE.md` — v0.2 contest loop and `ContestFlowController`
   - `GROWING_UP_FORMS.md` — v0.4 growth-form save memory and journal hooks
   - `DAY_NIGHT_RULES.md` — conditional encounter timing
   - `V0_4_ZONE_TRANSITIONS.md` — zone transitions
   - `NO_NETWORK_GUARDRAILS.md` — package review and release gate
   - `ASSET_PIPELINE.md` — art/audio/video import checklist
   - `PRE_RELEASE_CHECKLIST.md` and `IP_SAFETY_CHECKLIST.md` — release gates
   - `PUBLIC_RELEASE_ASSET_AUDIT.md` and `PRIVACY_RELEASE_NOTES.md` — release posture

## Core Rules

- Tidepool is an offline iPad Unity game for a 7-year-old.
- No network features of any kind.
- No ads, analytics, accounts, IAP, notifications, telemetry, or online services.
- No protected franchise names, near-miss names, copied visual trade dress, or lookalike creature designs.
- Do not add aim-and-throw capture, live capture odds, or creature riding.
- Preserve the v0.1 loop: explore, encounter, catch, journal, explore.
- Keep the experience gentle. Nothing is lost and failed catches are not punished.

## Implementation Priorities

For v0.1, favor working device-tested gameplay over breadth:

1. Unity/iOS pipeline.
2. Tap-to-move overworld.
3. Seagrass encounters.
4. Steady-the-Jar catch mini-game.
5. Journal grid and nicknames.
6. JSON save/load.
7. iPad polish and handoff.

Do not start battles, levels, growth systems, day/night, or public-release work unless an issue explicitly asks for that roadmap version.

## Unity Notes

- Target Unity 6.5.6 (6000.5.6f1), 2D URP, iOS 15+, landscape iPad.
- Keep scene structure aligned with `Boot`, `Overworld`, `CatchEncounter`, and `Journal`.
- Use safe-area fitting for UI.
- Keep touch targets at least 88pt.
- Prefer ScriptableObjects for species/content data.
- Keep save data in `Application.persistentDataPath/save.json`.
- Avoid `PlayerPrefs` for collection state.

## Asset Notes

- Log every non-code asset in `Assets/ASSET_MANIFEST.md`.
- Use Git LFS for binary assets.
- AI sprite prompts must not reference protected franchises, existing characters, or living artists.
- Review generated creature art for accidental resemblance before shipping.

## Coding Style

- Use existing namespaces and folder conventions.
- Keep changes small and issue-focused.
- Prefer simple, inspectable C# over new packages.
- Handle missing prototype assets gracefully.
- Do not add unrelated refactors while implementing a feature.

## Verification

When possible:

- Compile in Unity.
- Run the edited scene.
- Test touch-heavy flows on iPad.
- For save work, verify force-quit and relaunch.

If Unity or iPad verification was not possible, say so explicitly in the final response.

