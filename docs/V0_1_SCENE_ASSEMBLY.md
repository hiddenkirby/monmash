# v0.1 Scene Assembly

## Boot

- Add a `GameSaveService` object.
- Load or route to `Overworld`.

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
- If Shallows and Meadow are built as one continuous scene, use one `EncounterDirector`
  per encounter zone until a later zone-volume system exists; each director should point
  at the seagrass tilemap for its own zone and set its `currentZone` accordingly.

## CatchEncounter

- Add a Canvas using Scale With Screen Size, reference `1024 x 768`, match `0.5`.
- Add `SafeAreaFitter` to the safe-area root RectTransform.
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

## Journal

- Add a Canvas with `SafeAreaFitter`.
- Add `JournalController`.
- Wire:
  - species database
  - slot prefab
  - grid root
  - detail image/name/current/habitat/caught date and location/field-note/times-seen/nickname fields
