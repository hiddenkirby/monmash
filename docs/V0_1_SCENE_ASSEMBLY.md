# v0.1 Scene Assembly

## Boot

- Add a `GameSaveService` object.
- Load or route to `Overworld`.

## Overworld

- Add a `Grid` with Tilemaps:
  - `Ground`
  - `Obstacles`
  - `Seagrass`
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
  - catch scene name: `CatchEncounter`

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
  - detail image/name/current/habitat/field-note/times-seen/nickname fields

