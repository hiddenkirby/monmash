# Roadmap

## v0.1 - It Catches (SHIPPED — tagged `v0.1`)

- Two zones: Tidepool Shallows and Seagrass Meadow.
- Tap-to-move on a grid.
- Seagrass random encounters with grace and pity counters.
- Steady-the-Jar catch mini-game.
- Journal grid with silhouettes, nicknames, and found count.
- JSON save/load in `Application.persistentDataPath`.
- iPad landscape build.

## v0.2 - Contests

- Friendly turn-based contests.
- Two moves per Tideling.
- No fainting; Tidelings get tuckered out and nap.

## v0.3 - Growing

- Levels 1-20.
- Currents affect outcomes.
- New moves at thresholds.

## v0.4 - Wider Coast

- Kelp Curtain and Rocky Shelf.
- Growing-up forms.
- Day/night rules.

## v0.5 - Journal Visual Polish

- Rarity-colored card frames and styled grid slots.
- Shell-shaped progress bar (PRD section 5.4).
- Sectioned, scrollable detail panel.
- Slot and panel animations.
- Sort/filter bar.
- Themed journal background and card frame art.

## v0.6 - Contest Depth & Strategy

- Move categories: Attack / Focus / Defend (rock-paper-scissors loop).
- Multi-round contests (best-of-3).
- Move telegraphing — the visiting Tideling shows its move category before resolving.
- Current advantage ring display.
- Party swap in contests.
- AI behavior patterns (Aggressive / Defensive / Tricky).

## v0.7 - Narrative & Map Progression

- Mentor NPC who appears at key moments with warm, short text.
- StoryBeat system (ScriptableObject) tied to catch milestones.
- Zone progression gates (Kelp blocked until 5 Meadow species, Rocky blocked until 3 Kelp species).
- Zone welcome signage.
- Simple quest/goals panel.
- Old Barnaby as narrative climax.
- Map visual progression (gates part on unlock).

## v0.8 - The Great Low Tide (INTEGRATION / DEVICE QA)

- Four connected expedition chapters across Shallows, Meadow, Kelp Curtain, and Rocky Shelf.
- Persistent route reveals, landmarks, ambient Tidelings, authored discoveries, and Coast Atlas.
- A Field Station that projects existing progress into permanent keepsakes.
- A coast-festival wrapper around the existing friendly contest and an Old Barnaby finale.
- Source foundations and Field Station/festival/finale desktop-Editor wiring are present on
  `main`; remaining authored clue placement is tracked by the integration checklist.
- Release remains gated on clean generated-setup coverage, full new/legacy-save journeys, and
  target-iPad performance, safe-area, touch, accessibility, offline, and force-quit checks.

See `docs/GREAT_LOW_TIDE.md` for the experience contract and
`docs/V0_8_INTEGRATION_CHECKLIST.md` for the reproducible integration and release gates.
