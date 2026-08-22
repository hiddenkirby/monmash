# Contest Architecture

This document scopes the v0.2 friendly contest loop. Contests are separate from the v0.1
catch loop and should stay warm, readable, and low-stakes.

## Scene Responsibilities

`Contest` owns the temporary flow for one friendly matchup:

- Show the player's active Tideling, the visiting Tideling, and two move buttons.
- Let the player choose one available move.
- Resolve the visiting Tideling's move with simple, readable text.
- Show the outcome without removing journal progress or caught Tidelings.
- Offer immediate retry and exit routes.

The scene should receive already-selected Tidelings from the overworld or journal flow. It
should not search every scene object for save data or species data at runtime; callers provide
the matchup context and the scene reports only the result.

## Editor Assembly Helper

Run `Tools/Tidepool/Create v0.2 Contest Scene` in Unity to create `Assets/Scenes/Contest.unity`.
The generated scene includes:

- A safe-area-aware Canvas using `1024 x 768` reference resolution and match `0.5`.
- Player and visiting Tideling image/name views.
- Two 96pt move buttons wired to `ContestFlowController`.
- Retry and Back buttons wired to the controller's retry/exit flow.

After generation, inspect the scene in Unity, assign fallback species for prototype testing if
needed, then verify compile, Play Mode flow, iPad safe area, and touch targets before closing
contest issues.

## Contest Move Assets

Run `Tools/Tidepool/Create Contest Move Assets` to generate 10 `ContestMove` ScriptableObject
assets (two per Current) in `Assets/Data/ContestMoves/` and wire them to all 13 species. Each
species gets two moves themed by its Current with gentle power values of 2-3. The generator is
idempotent — re-running it updates existing assets in place.

Run this after `Create Starter Species Assets` and before testing the contest flow.

## Contest Trigger

`ContestTrigger` is a runtime component placed on a UI button in the Overworld scene. It:
- Populates `ContestContext` with player and visiting species from the `SpeciesDatabase`.
- Disables player movement via `PlayerGridMover.SetInputEnabled(false)`.
- Loads the Contest scene additively.

The overworld scene generator creates and wires the Contest button automatically. To trigger a
contest manually, call `ContestTrigger.StartContest()` from a button click or other input.

## Contest Return Flow

`ContestFlowController.ExitContest` raises `ContestEvents.RaiseContestFinished()` before
unloading the scene. `ContestTrigger` listens for this event, clears `ContestContext`, and
re-enables player movement. This mirrors the `EncounterEvents` pattern used by the catch
mini-game.

## Data Model

`ContestMove` is a ScriptableObject so move copy and tuning can be edited without changing
code. Each move has:

- stable id
- display name
- Current
- gentle power value
- short description

`TidelingSpecies` exposes up to two contest move slots. The v0.2 UI should render no more than
two move buttons for the active species. Missing move slots are allowed during prototype data
entry and should disable or hide the corresponding button.

## Resolution

The first implementation should be deterministic and inspectable:

- Player chooses a move.
- The visiting Tideling chooses from its available moves using a simple rule.
- Current advantage applies once issue #43 lands.
- The higher adjusted result wins the exchange.
- A contest can end in a win, a retry prompt, or an exit to the previous scene.

## Player Safety

Contests must never remove caught Tidelings, journal entries, nicknames, seen-species progress,
or saved player position. A lost contest is just a cue to try again or leave.

Use `ContestParticipantState` for temporary contest-only rest state. A tuckered-out Tideling
rests for a small number of contest rounds, then becomes available again; this state is not part
of `SaveData` and must not remove or rewrite collection progress.

The first controller integration marks the lower-scoring Tideling tuckered out for one retry
cycle, then advances that rest when the player taps retry. While a Tideling is tuckered out,
its creature image dims to 50% opacity and a `napping...` status label appears below its name.
Move buttons disable while the player's Tideling is resting. Retry clears the rest state and
restores the visuals.

Use gentle copy such as:

- `Try another round?`
- `They need a little rest.`
- `Back to the shallows.`

Do not add timers, currency costs, item costs, or progress penalties to v0.2 contests.
