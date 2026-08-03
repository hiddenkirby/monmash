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
cycle, then advances that rest when the player taps retry.

Use gentle copy such as:

- `Try another round?`
- `They need a little rest.`
- `Back to the shallows.`

Do not add timers, currency costs, item costs, or progress penalties to v0.2 contests.
