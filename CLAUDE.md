# CLAUDE.md

Instructions for Claude and other assistant agents working on Tidepool.

Use `AGENTS.md` as the source of truth. This file is a shorter operational checklist for day-to-day work.

## First Read

Before editing, read:

1. `AGENTS.md`
2. `PRD.md`
3. `WEEKEND-PLAN.md`
4. Relevant docs in `docs/`

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

- Target Unity 6.3 LTS, 2D URP, iOS 15+, landscape iPad.
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

