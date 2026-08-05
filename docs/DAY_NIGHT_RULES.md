# Day And Night Rules

Day/night encounter conditions use a compressed in-game daylight cycle, not the
device wall clock.

## v0.4 Decision

- Tidepool should not require playing at a real-world time of day to find a
  Tideling.
- The default daylight cycle is eight minutes of scene time.
- The final minute of that cycle counts as the last hour of daylight.
- Save data does not store clock progress in v0.4. Restarting the app simply
  restarts the lightweight cycle, which avoids migration and stale timestamp
  edge cases.
- Conditional species should include a journal hint so the rule is visible or
  inferable after discovery.

## First Conditional Species

Gullwing uses `EncounterAvailability.LastHourOfDaylight`. It remains a rare
Seagrass Meadow encounter, but only during the final minute of the compressed
daylight cycle.

If playtesting makes this feel too hard to find, tune
`EncounterDirector.lastHourOfDaylightSeconds` in the scene before changing the
species data.
