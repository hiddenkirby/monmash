# Contest Depth Model

Issue: #126

This document is the v0.6 source of truth for making Tidepool contests more strategic while
keeping them warm, fair, and low-stakes. It builds on `docs/CONTEST_ARCHITECTURE.md` and the
current `ContestFlowController` foundation: ScriptableObject moves, Current multipliers,
tuckered-out rest state, and gentle progress that is never lost.

## Goals

- Give the player a readable choice every round without adding adult-level complexity.
- Teach Current matchups through play, not memorization.
- Make visiting Tidelings feel different through simple, visible behavior patterns.
- Preserve the core safety rules: no HP bars, no damage numbers, no currency, no item costs,
  no losing creatures, and no removed journal progress.

## Move Categories

Every `ContestMove` gains one category. Category interaction is a gentle rock-paper-scissors
loop:

```text
Attack beats Focus
Focus beats Defend
Defend beats Attack
```

The fiction should stay soft:

| Category | Meaning | Beats | Copy direction |
|---|---|---|---|
| Attack | A bright, direct contest flourish | Focus | `interrupts the setup` |
| Focus | A careful, patient build-up | Defend | `finds a path through` |
| Defend | A calm redirect or shelter | Attack | `softly turns it aside` |

Same-category ties go to the higher adjusted gentle power. Adjusted gentle power is:

```text
base gentle power * Current effectiveness multiplier
```

If both moves share category and adjusted gentle power, the round is a tie.

## Resolution Order

Resolve one round in this order:

1. Visiting Tideling telegraphs its intended category.
2. Player chooses one available move or swaps party member when allowed.
3. If the player chose a move, compare category advantage first.
4. If category advantage does not decide the round, compare adjusted gentle power.
5. Award the round result, show short warm copy, then advance to the next round.

Category advantage should be decisive because it is the new strategic layer. Current advantage
is the tie-breaker and teaching layer when categories match or neither category counters the
other.

## Round Structure

Contests are best-of-3:

- First to 2 round wins wins the contest.
- Tied rounds do not award a win to either side.
- If the contest reaches 3 resolved rounds without either side reaching 2 wins, treat the
  contest as a friendly tie.
- The UI always shows `Round 1 of 3`, `Round 2 of 3`, or `Round 3 of 3`.
- The UI shows player round wins and visiting round wins with simple shell markers.

### Win Condition

| Contest result | Progress award | Player-facing copy |
|---|---:|---|
| Win | 2 points | `You won the friendly contest!` |
| Tie | 1 point | `That was a close one. Everyone learned something.` |
| Loss | 1 point | `They need a little rest. Try again when you like.` |

Progress uses the existing gentle progression model and must remain deterministic and save-safe.
Losing or tying never removes progress, creatures, nicknames, saved position, or journal data.

## Telegraphing

Before the player chooses, the visiting Tideling shows its planned category for 1-2 seconds:

- Attack: ripple burst icon, warm red/coral accent.
- Focus: spiral shell icon, glow amber accent.
- Defend: smooth pebble shield icon, kelp green accent.

Telegraphing makes the contest fair. The player is not guessing blindly; she is responding to
a visible hint. The telegraph should include icon, color, and text such as `Wobbet is focusing`
so the meaning does not rely on color alone.

## Current Advantage Ring

Contests should display the five-current ring:

```text
Current -> Coral -> Stone -> Glow -> Tide -> Current
```

Use the ring as a small teaching aid, not a dense chart:

- Highlight the player's active Current and the visiting Tideling's Current.
- Draw a simple arrow from the advantaged Current to the disadvantaged Current.
- Pair Current colors with icons and names.
- Keep the multiplier hidden from the main UI; do not show probability or damage-style math.

The implementation can continue using `TidelingCurrentRules.GetEffectivenessMultiplier` for
the actual 1.5 / 1.0 / 0.75 adjustment.

## Party Swap

The player brings 2-3 caught Tidelings to a contest.

Rules:

- The active Tideling chooses moves for the current round.
- If the active Tideling is tuckered out, the player swaps to another available party member.
- Swapping uses the player's choice for that round and does not score a round by itself.
- A tuckered Tideling rests in party state and becomes available again after its rest counter
  reaches zero.
- Party state is contest-only. It must not remove or overwrite caught Tidelings in save data.

The party picker should show creature art, name or nickname, Current, and rest status. A
tuckered Tideling remains visible but disabled with copy like `Napping this round`.

## Visiting AI Patterns

Each visiting Tideling uses one simple behavior pattern. The pattern is visible in copy or icon
language so the player can learn it.

| Pattern | Bias | Behavior |
|---|---|---|
| Aggressive | Attack | Opens with Attack, favors Attack after ties, occasionally mixes in Focus |
| Defensive | Defend | Opens with Defend, repeats Defend after losing, occasionally uses Attack |
| Tricky | Mixed | Rotates categories, avoids repeating the same category more than twice |

AI choices remain deterministic enough to inspect and test. A seeded random helper is
acceptable later, but the first implementation should prefer clear weighted tables over opaque
behavior trees.

## UI States

The contest screen needs these visible states:

| State | Required UI |
|---|---|
| Telegraph | Visiting category icon/name, round counter, party state |
| Choose move | Two active move buttons, category labels, Current ring |
| Choose swap | Party list with available/resting Tidelings |
| Round result | Round winner, warm copy, updated shell markers |
| Contest result | Win/tie/loss copy, progress gained, retry and back buttons |

Move buttons should show:

- move display name;
- category icon and label;
- Current icon and label;
- short description;
- unavailable state when locked or resting.

## Explicit Exclusions

Do not add these to v0.6 contests:

- HP bars or health totals.
- Damage numbers.
- Currency costs, item costs, entry fees, or rewards that can be lost.
- Ranked mode, streaks, timers, stars, grades, or daily obligations.
- Removed or released creatures.
- Capture odds, aim-and-throw mechanics, projectiles, or riding.
- Network features, analytics, leaderboards, cloud saves, or accounts.

## Implementation Notes

- Extend `ContestMove` with a serialized category enum rather than encoding categories in move
  names or descriptions.
- Keep `ContestParticipantState` contest-local. If party-level rest data is needed, store it
  in the contest controller or a contest session object, not in `SaveData`.
- Add round counters and score markers to `ContestFlowController` before adding scene polish.
- Keep `ContestContext` as the handoff point, but expand it to include the selected party when
  party selection exists.
- Use existing Current display helpers for names, icons, and colors.
- Verify final behavior in Unity Play Mode and on iPad before closing implementation issues.
