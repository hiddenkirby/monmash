# v0.8 Field Station Assembly

Issue: #211

The field station is a bounded, non-blocking area in the Tidepool Shallows Overworld. It is
available from the first chapter and projects existing save facts into visible keepsakes. It
does not own catches, growth, contests, chapters, goals, settings, or navigation state.

## Generate The Definition

In Unity, run `Tools/Tidepool/Create Field Station Definition`. This creates or updates
`Assets/Data/FieldStation/GreatLowTideFieldStation.asset` with stable upgrade IDs for:

- the starting atlas table;
- a discovery board after five distinct catches;
- Meadow, Kelp, and Rocky chapter keepsakes;
- a growing-up memory album;
- the coast-festival ribbon; and
- the finale celebration, backfilled from the authoritative Old Barnaby catch.

`FieldStationController` reconciles those displays from their authoritative save facts whenever
the station becomes active. Reconciliation is monotonic: an earned decoration remains earned,
unknown saved IDs are left alone, and no catch, form, contest result, or chapter is granted.

## Overworld Wiring

1. Create a `FieldStation` root in the Shallows beside, not on, the walkable route.
2. Add `FieldStationController` and assign the generated definition.
3. Add one decoration binding per definition stage. A standard root may contain gentle motion;
   its optional reduced-motion root must show the same earned fact without continuous motion.
4. Add optional visiting Tideling roots keyed by caught species ID. Visitor roots are
   presentation only; the controller disables any child colliders and never writes collection
   state.
5. Wire the four controller events to the existing Journal, Coast Atlas, Goals, and Settings
   open methods. Those systems remain the source of truth and must return to the same Overworld
   scene and player position.
6. Keep every route button inside `SafeAreaFitter`, at least 88pt, with a text or icon-plus-text
   label. Use the project Canvas Scaler baseline of 1024x768 and match 0.5.
7. Use the existing adaptive soundscape controller for the quiet field-station role. Missing
   audio or art must leave navigation and every route usable.

Run `Tools/Tidepool/Verify Field Station Projection`, then verify in Play Mode that new and
advanced saves reconstruct displays, visitors never affect movement, all four UI routes return
correctly, reduced motion swaps presentation roots, and save/reload preserves earned displays.
Target-iPad verification must cover safe areas, 88pt touch targets, landscape readability, and
force-quit/relaunch.
