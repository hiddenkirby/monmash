# v0.8 Old Barnaby Finale Assembly

Issue: #212

The finale is a gentle authored approach to the existing Steady-the-Jar encounter. The catch
controller and `SaveData.caught` remain authoritative: the finale never grants or duplicates a
catch, and choosing **Let it go** keeps the invitation available.

## Generate The Definition

In Unity, run `Tools/Tidepool/Create Old Barnaby Finale Definition`. This creates
`Assets/Data/Finale/OldBarnabyFinale.asset` and links the existing `old-barnaby` species asset.
Run `Tools/Tidepool/Verify Old Barnaby Finale` to check gating, retry, catch-authoritative
completion, advanced-save backfill, and memory idempotency.

## Rocky Shelf Wiring

1. Place a `BarnabyFinaleController` at the authored Rocky Shelf approach and assign the
   generated definition plus the existing player mover.
2. Wire the approach trigger or invitation button to `TryBeginFinale`. Keep its collider off
   the walkable route and require `chapter.kelp` plus `landmark.rocky.old-stones`.
3. Bind an invitation root, a skippable standard presentation root, a static reduced-motion
   root, and a persistent celebration root. Missing optional roots must still launch the catch.
4. Keep the skip control visible from the first presentation frame. If it is UI, place it
   inside `SafeAreaFitter` with an 88pt minimum target.
5. Bind `FinaleStarted` to the local landmark/camera/audio presentation only. Bind
   `FinaleCompleted` to coast celebration presentation; persistent chapter, set-piece, and
   field-station facts are already reconciled from the catch.
6. Expose `OpenMemory` from the Journal or return card. It acknowledges
   `expedition.memory_seen` without changing catches or replaying the full encounter.

Verify in Play Mode that skip and missing-content paths launch the same encounter, **Let it
go** and a missed catch restore movement and preserve the invitation, a catch creates only one
Old Barnaby entry, existing completed saves backfill without replay, and all scene exits restore
input. On iPad, also verify landscape safe areas, 88pt controls, reduced motion, mute behavior,
force-quit/relaunch, and the full journal/field-station celebration.
