# v0.4 Zone Transitions

Use this when Kelp Curtain and Rocky Shelf are assembled in Unity. The source-side support is
ready, but the zone scenes, terrain visuals, encounter pools, and asset manifest entries still
need Editor work.

## Runtime Wiring

`ZoneTransitionTrigger` is a trigger-collider component for low-friction zone boundaries.

For each transition:

1. Add a `Collider2D` with `isTrigger` enabled at the edge of the source zone.
2. Add `ZoneTransitionTrigger`.
3. Set `Destination Zone` to `KelpCurtain`, `RockyShelf`, `TidepoolShallows`, or
   `SeagrassMeadow`.
4. Wire `Player Root`, `Player Mover`, and the scene `Grid`.
5. Optionally wire `Destination Spawn` to move the player to a safe readable tile.

Unity trigger events require the player or trigger boundary to participate in 2D physics with
the appropriate `Rigidbody2D`/`Collider2D` setup. Verify that wiring in Play Mode before
device testing.

When the player enters the trigger, the component:

- stops grid input while transitioning;
- moves the player to the destination spawn when one is assigned;
- saves `GameSaveService.Data.currentZone`;
- saves the player tile when a `Grid` is assigned;
- invokes `Entered Zone` for scene-specific effects such as a label, sound, or camera cue.

## Remaining Unity Work

- Paint distinct Kelp Curtain and Rocky Shelf terrain in the Editor.
- Create encounter tilemaps or zone-specific encounter regions for the new habitats.
- Assign species habitat data for any new zone encounter pools. `EncounterDirector` already
  filters by `TidelingSpecies.HabitatZones` and its serialized `currentZone`.
- Log new art, tile, UI, or audio assets in `Assets/ASSET_MANIFEST.md`.
- Verify that Shallows and Meadow still load, transition, save, and encounter correctly.
- Run Play Mode and iPad checks before closing #49.
