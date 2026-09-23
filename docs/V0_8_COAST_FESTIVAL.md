# v0.8 Coast Festival Assembly

Issue: #214

The coast festival wraps one existing friendly contest with celebratory presentation. It does
not change move selection, Current/category scoring, best-of-three rules, party swaps,
tuckered-out behavior, or gentle progression. A loss or tie removes nothing and offers another
try; only the existing contest controller's player-win result earns the persistent ribbon.

## Generate The Definition

In Unity, run `Tools/Tidepool/Create Coast Festival Definition`. This creates
`Assets/Data/Festival/GreatLowTideFestival.asset` with expedition prerequisites and warm copy.
Run `Tools/Tidepool/Verify Coast Festival Projection` to check entry gating, replay, explicit
contest outcomes, win-only completion, ribbon backfill, and collection safety.

## Venue Wiring

1. Place the festival venue after `chapter.kelp` and `setpiece.rocky-unlock` are complete.
2. Add `CoastFestivalController`, assign the generated definition, and assign a dedicated
   `ContestTrigger` configured for the named visiting participant.
3. Bind invitation, skippable introduction, static reduced-motion, and celebration roots.
   Missing optional presentation must still open the existing party/contest flow.
4. Bind `FestivalStarted` to introductions and local spectator/audio cues only.
5. Bind `FestivalWon` to the ribbon handoff and coast response. Persistence is already written
   from the contest win before optional presentation runs.
6. Bind `FestivalRetryOffered` to encouraging result presentation. Retry/leave must use the
   existing contest controls and must not change saves beyond normal gentle progress.
7. Keep every route/skip control inside `SafeAreaFitter`, at least 88pt, and readable without
   audio or color alone. Spectator objects must have no gameplay colliders or contest logic.

Verify in Play Mode that win, tie, loss, retry, repeat, exit, reduced-motion, muted, and
missing-presentation paths all return correctly. Confirm that only a win earns
`setpiece.festival` and `station.festival-ribbon`, while contest balance and collection data
remain unchanged. Target-iPad checks must cover safe areas, touch targets, landscape
readability, save/reload, and spectator performance.
