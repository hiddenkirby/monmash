# v0.1 Scene Assembly

## Boot

- Add a `GameSaveService` object.
- Add `BootRouter`.
  - For no title screen, leave `loadOverworldOnStart` enabled.
  - For a title screen, disable `loadOverworldOnStart` and wire the Continue
    button to `BootRouter.ContinueToOverworld`.

## Overworld

- Add a `Grid` with Tilemaps:
  - `Ground`
  - `Obstacles`
  - `Seagrass`
- Paint every reachable walking tile on `Ground`.
- Paint blocking rocks, walls, or props on `Obstacles`; `PlayerGridMover` treats any
  occupied obstacle cell as non-walkable.
- Paint encounter grass only on `Seagrass`; `EncounterDirector` only rolls encounters
  after `PlayerGridMover` reports a completed step on a `Seagrass` cell.
- Add the player sprite.
- Add `PlayerGridMover` and wire:
  - `Grid`
  - player transform
  - ground tilemap
  - obstacle tilemap
- Add `EncounterDirector` and wire:
  - `PlayerGridMover`
  - seagrass tilemap
  - species database
  - current zone:
    - `TidepoolShallows` for the Shallows area
    - `SeagrassMeadow` for the Meadow area
  - catch scene name: `CatchEncounter`
  - in-game daylight cycle defaults unless a later playtest needs faster/slower
    conditional encounters
- If Shallows and Meadow are built as one continuous scene, use one `EncounterDirector`
  per encounter zone until a later zone-volume system exists; each director should point
  at the seagrass tilemap for its own zone and set its `currentZone` accordingly.
- Add `FirstRunGuidanceController` to a safe-area UI panel for the first-run line:
  `Tap to walk. Look in the seagrass.`
- Add a `ContestTrigger` to a safe-area UI button (e.g. "Contest") wired to:
  - `speciesDatabase`
  - `playerSpeciesId` (e.g. `blip`)
  - `visitingSpeciesId` (e.g. `wobbet`)
  - `contestSceneName` (`Contest`)
  - `playerMover` (the same `PlayerGridMover` used by `EncounterDirector`)
- The overworld scene generator (`Tools/Tidepool/Create v0.1 Overworld Scene`)
  creates the Contest button and wires it automatically.

## CatchEncounter

- Add a Canvas using Scale With Screen Size, reference `1024 x 768`, match `0.5`.
- Add `SafeAreaFitter` to the safe-area root RectTransform.
- Keep the `Let it go` button and audio settings controls fully inside the safe-area root,
  with at least 16pt of reference-resolution margin from the bottom/top/right edges.
- Add `CatchEncounterController`.
- Wire:
  - creature image
  - creature name text
  - calm bar track
  - steady zone RectTransform
  - marker RectTransform
  - three jar pip images
  - result text
  - let-go button
  - settings panel mute toggle, volume slider, and volume value text

## Journal

- Add a Canvas with `SafeAreaFitter`.
- Add `JournalController`.
- Wire:
  - species database
  - slot prefab
  - grid root
  - detail image/name/current icon/current text/habitat/caught date and location/level/growth/growth memory/moves/field-note/times-seen/nickname fields
  - optional growth-form dropdown to `JournalController.growthFormDropdown`
  - optional original-form button to `JournalController.selectOriginalGrowthFormButton`
- Wire the nickname submit button or input submit event to `JournalController.SaveNickname`.
- Wire the growth-form dropdown value-changed event to
  `JournalController.SelectGrowthFormFromDropdown` if not relying on the controller's source-side listener.
- Wire the original-form button click event to `JournalController.SelectOriginalGrowthForm` if not
  relying on the controller's source-side listener.
- The level field shows `Level X` (or `Level 20 — all grown up!` at max). The growth field
  shows `Y friendly moments until next growth` (or `Almost ready to grow` / `Fully grown`).
  The moves field lists known moves and shows `(unlocks at level N)` for locked moves.
  Uncaught entries show `?` for the name, a black-tinted silhouette, and `Unknown` /
  `Not found yet` / `Keep looking in the seagrass.` for detail fields.
- Conditional encounter hints are appended to the existing habitat text, so no
  separate Journal field is required for v0.4 availability rules.

See `docs/DAY_NIGHT_RULES.md` before tuning conditional encounter timing.
