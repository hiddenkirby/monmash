# Narrative Framework

Issue: #135

This document is the v0.7 source of truth for adding light story guidance and map progression
to Tidepool. The story should make discovery feel personal without turning the game into a
quest log or blocking the calm collection loop.

## Goals

- Add warm, short story beats at natural collection milestones.
- Give zone unlocks clear in-world meaning through signs, mentor comments, and map changes.
- Keep gates gentle: never punish, scold, or strand the player.
- Preserve offline play, no-network requirements, and the existing save/load model.
- Avoid protected franchise references, copied trade dress, or genre-specific catch slogans.

## Mentor NPC Concept

The mentor is a friendly tidepool guide who appears at key moments with one or two short
sentences. The mentor is not a trainer, rival, shopkeeper, quest giver, or authority figure.
They are a nearby helper who notices what the player found and points toward the next
interesting place.

Working concept:

- Name: Mira
- Role: local tidepool guide and notebook keeper
- Visual direction: rain boots, sun hat, small field notebook, shell pin, gentle posture
- Voice: curious, warm, specific, never scolding

Mira should speak in compact lines that fit a 7-year-old's attention span. If a beat needs more
than two sentences, split it into a later beat or a sign.

## Tone Guide

- Use invitation language: `Care to look further?`, `Want to see what lives there?`
- Use discovery language: `Something moves in the kelp.`
- Avoid failure language: do not say `wrong`, `locked`, `blocked`, or `not enough`.
- Avoid obligation language: no daily tasks, scores, grades, stars, streaks, or timers.
- Keep copy concrete and place-based.

## Story Beats

Each story beat has an id, trigger, copy, and one optional map effect. A beat should fire once
and then be remembered in save data so it does not repeat on every scene load.

| Order | Beat id | Trigger | Mentor copy | Map effect |
|---:|---|---|---|---|
| 1 | `first_catch_intro` | First caught Tideling | `What did you find? Show me!` | None |
| 2 | `meadow_pointer` | 3 total caught species | `The meadow is just through here. Look for the grass waving in the water.` | Highlight Meadow sign |
| 3 | `kelp_clue` | 5 total caught species | `Something moves in the kelp, but it is still too thick to push through.` | Show Kelp Curtain rustle |
| 4 | `kelp_unlock` | 8 total caught species | `The kelp has thinned. Care to look further?` | Open Kelp Curtain path |
| 5 | `old_barnaby_omen` | 10 total caught species | `The oldest shells are waking up. Keep your eyes on the shallows.` | Emphasize Shallows return path |
| 6 | `all_found_celebration` | 13 total caught species | `You found them all! The tidepools are full.` | Gentle journal celebration |

Old Barnaby keeps the existing encounter rule: it appears in Tidepool Shallows after 10 caught
species excluding Old Barnaby. The narrative beat gives that moment weight without changing the
encounter into a forced capture or a punishment.

## Zone Progression Gates

Zone gates are in-world nudges. They should look like tidepool geography, not menus.

| Gate | Locked rule | Unlock rule | Locked copy | Unlock visual |
|---|---|---|---|---|
| Kelp Curtain | Kelp path cannot be entered yet | 5 species caught in Seagrass Meadow | `The kelp is woven tight here. Maybe the meadow can teach us more first.` | Kelp parts, path tile appears, sign brightens |
| Rocky Shelf | Rocky path cannot be entered yet | 3 species caught in Kelp Curtain | `The rocks are still slick. Let's learn the kelp path first.` | Tide lowers, stepping stones appear |

Gate copy should appear from a sign or Mira, not as an error popup. The player should always be
able to turn around, keep exploring, and catch more Tidelings.

## Trigger Rules

Use caught-species counts, not seen counts, for story progression. Seen species can happen from
an encounter that slips away; gates should reward collection progress without punishing failed
catches.

Recommended helper rules:

- `totalCaughtSpecies`: count unique caught species in `SaveData.caught`.
- `caughtInZone(zone)`: count unique caught species whose `caughtInZone` matches the zone.
- `hasSeenBeat(id)`: checks story progress state.
- `markBeatSeen(id)`: records a beat and saves immediately.

The current `CaughtTideling` model already stores `caughtInZone`, so zone-specific counts can
be derived without changing existing catch records. A future implementation still needs a
story progress field on `SaveData`, such as `List<string> seenStoryBeatIds`.

## Map Progression Mechanics

Map changes should feel physical:

- Kelp Curtain: tall kelp tiles sway while locked, then part to reveal a narrow safe path.
- Rocky Shelf: water recedes from stepping stones when unlocked.
- Meadow sign: brightens or gains a small shell marker when Mira points the player there.
- Shallows return: a subtle shell glint near the Shallows path after the Old Barnaby omen.

These are Editor-facing visual changes. Source-side work can define trigger components,
serialized references, and save rules, but tile edits, colliders, animation previews, and final
safe-area/touch QA require Unity.

## UI Behavior

Story delivery should be lightweight:

- Use a small safe-area-aware dialogue panel.
- Show mentor art or icon only when it helps recognition.
- Keep the continue button at least 88pt.
- Allow tap to advance; do not require drag, double-tap, or timed input.
- Never block saving.
- Do not replay an already-seen beat unless a debug tool explicitly resets story progress.

The story panel should be able to queue behind catch, journal, or zone-transition events rather
than interrupting a catch mini-game.

## Implementation Notes

- Add story progress to `SaveData` with a migration-safe default list.
- Keep story state local and offline; no analytics or remote config.
- Prefer a `StoryBeat` ScriptableObject or serializable table so copy and triggers are editable
  without hardcoding every line in a controller.
- Let `ZoneTransitionTrigger` ask a gate rule before applying a transition.
- Keep `EncounterDirector` responsible for encounter selection; story logic should not bypass
  the existing Old Barnaby rule.
- Use `GameSaveService.Save()` after recording a story beat or gate unlock.
- Add Editor scene wiring only in Unity and keep generated `.meta` files from Unity.

## Acceptance Boundaries

This framework defines the rules and copy. It does not by itself:

- create Mira art;
- paint locked or unlocked map states;
- wire dialogue panels into scenes;
- validate Play Mode behavior;
- validate iPad safe area and touch targets.

Those remain implementation and Editor verification tasks for follow-up issues.
