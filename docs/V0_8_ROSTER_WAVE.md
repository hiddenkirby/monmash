# v0.8 Roster Wave

Issue: #215

Status: source-side content specification. ScriptableObject rows, sprites, database wiring,
journal layout validation, encounter testing, contest testing, LFS checks, and iPad review
remain pending.

## Design Goals

The roster grows from 13 to 21 Tidelings by adding two species per zone. The new wave should
make the wider coast feel more alive without turning discovery into filler or invalidating the
original 13-species accomplishment.

Rules for this wave:

- Keep names short, original, pronounceable, and safe for player-facing copy.
- Use only existing runtime availability values: `Always` and `LastHourOfDaylight`.
- Keep every species discoverable through reachable, non-frustrating routes.
- Use at least four new species in authored clues or chapter beats.
- Do not require real-world time, network state, daily play, currency, items, or loss.
- Missing sprites must fall back to existing journal/encounter missing-art behavior.

## Balance Summary

Current distribution after this wave:

| Current | Original count | New count | Total |
|---|---:|---:|---:|
| Current | 3 | 1 | 4 |
| Stone | 3 | 1 | 4 |
| Coral | 3 | 1 | 4 |
| Glow | 2 | 3 | 5 |
| Tide | 2 | 2 | 4 |

Rarity distribution in the new wave:

| Rarity | New count | Use |
|---|---:|---|
| Common | 2 | Early confidence and wider-zone baseline encounters |
| Uncommon | 4 | Main discovery texture across all four zones |
| Rare | 2 | Authored clue payoff and chapter-scale anticipation |

## Species Rows To Author

| ID | Name | Current | Rarity | Habitat | Availability | Field note |
|---|---|---|---|---|---|---|
| `shellwink` | Shellwink | Glow | Common | Tidepool Shallows | Always | Its shell blinks softly whenever a wave uncovers something shiny. |
| `pebblepip` | Pebblepip | Tide | Uncommon | Tidepool Shallows | Always | It taps tiny stones into neat circles, then forgets which one it started with. |
| `reedray` | Reedray | Current | Uncommon | Seagrass Meadow | Always | A flat little ray that follows grass shadows like paths on a map. |
| `duskbubble` | Duskbubble | Glow | Rare | Seagrass Meadow | LastHourOfDaylight | It rises only when the meadow turns golden and leaves a trail of quiet bubbles. |
| `kelpcurl` | Kelpcurl | Coral | Common | Kelp Curtain | Always | It wraps itself in one kelp ribbon and peeks out like a shy bookmark. |
| `lanternly` | Lanternly | Glow | Uncommon | Kelp Curtain | Always | Its tiny light turns warmer when another Tideling swims nearby. |
| `foamlet` | Foamlet | Tide | Uncommon | Rocky Shelf | Always | It rides little foam rings in circles and always drifts back to the same stone. |
| `barnaclap` | Barnaclap | Stone | Rare | Rocky Shelf | LastHourOfDaylight | It opens and closes with a soft click, answering the oldest stones at sunset. |

## Encounter And Unlock Rules

| Species | Unlock posture | Non-frustration guardrail |
|---|---|---|
| Shellwink | Available from the first Shallows visit | Common rarity keeps the first new discovery easy to see |
| Pebblepip | Available after the Shallows chapter starts | Always available; clue copy should mention stone circles rather than a hidden counter |
| Reedray | Available when Meadow is reachable | Uncommon only; no special condition beyond the existing habitat |
| Duskbubble | Available in the final minute of the compressed daylight cycle | Journal hint must explain golden meadow timing after first sighting |
| Kelpcurl | Available when Kelp Curtain is reachable | Common rarity gives the new zone an immediate friendly find |
| Lanternly | Available when Kelp Curtain is reachable | Always available; can be used by glow-trail authored clues |
| Foamlet | Available when Rocky Shelf is reachable | Always available; useful as a gentle first Rocky discovery |
| Barnaclap | Available in the final minute of the compressed daylight cycle after Rocky is reachable | Rare and conditional; pair with a deterministic old-stone clue so it never feels impossible |

The 25-step pity timer should continue to choose among currently available species only. Do not
add a new availability enum unless a later issue also updates encounter filtering, journal copy,
and tests.

## Authored Clue And Chapter Usage

Use these four species as named authored-discovery anchors:

| Species | Chapter use | Clue shape |
|---|---|---|
| Shellwink | Chapter 1 teaching clue | A shell glint near the Shallows arch points toward the first deterministic discovery |
| Reedray | Chapter 2 Meadow movement clue | A soft grass-shadow path crosses the basin without changing encounter grass semantics |
| Lanternly | Chapter 3 Kelp light clue | A warm glow trail leads to the Kelp landmark and remains as a static marker if motion is reduced |
| Barnaclap | Chapter 4 Old Stones lead-in | Soft clicking answers from the shelf, foreshadowing Old Barnaby without making Barnaclap a required catch |

Optional supporting beats:

- Duskbubble can appear as the Meadow rare-discovery payoff if the timing rule tests well.
- Foamlet can be a visible ambient actor at the Rocky Shelf after the route opens.
- Kelpcurl can teach that Kelp Curtain has friendly creatures before any rare clue appears.
- Pebblepip can decorate the field station with a small stone-circle drawing after discovery.

## Contest Posture

Assign two existing contest move slots per species after the move asset generator is available.
Until then, use these current-aligned placeholders in design review:

| Species | Visiting AI | First move | Second move unlock |
|---|---|---|---|
| Shellwink | Tricky | Glow peek | Level 3: Soft shimmer |
| Pebblepip | Defensive | Tide nudge | Level 4: Circle drift |
| Reedray | Aggressive | Current glide | Level 4: Grassline dash |
| Duskbubble | Tricky | Glow rise | Level 5: Bubble veil |
| Kelpcurl | Defensive | Coral wrap | Level 3: Ribbon rest |
| Lanternly | Tricky | Glow wink | Level 4: Warm beacon |
| Foamlet | Defensive | Tide bob | Level 3: Foam turn |
| Barnaclap | Aggressive | Stone click | Level 5: Shelf answer |

Contest copy stays gentle. A lost contest never removes a catch, memory, route, or field-station
display.

## Growth-Form Posture

The first implementation should not require unique grown-form art. Use the existing reversible
growth-memory IDs until form metadata and assets exist.

| Species | Growth direction |
|---|---|
| Shellwink | Shell glow becomes a wider soft halo |
| Pebblepip | Stone circles become tidier and more elaborate |
| Reedray | Fins grow longer and more ribbon-like |
| Duskbubble | Bubble trail gains a second color at dusk |
| Kelpcurl | Kelp ribbon becomes a small leafy cloak |
| Lanternly | Lantern light gains a warm center and cool edge |
| Foamlet | Foam ring becomes a small crown of bubbles |
| Barnaclap | Barnacle shell gains old-stone markings |

Remembered forms remain optional, reversible, and journal-selected. Do not use permanent
transformation language in player-facing copy.

## Completion And Save Compatibility

The original 13-species completion should stay meaningful for old saves. A save that already
found all original species should keep any original reward or celebration state. The 21-species
total should be treated as the v0.8 wider-coast roster, not a retroactive loss of completion.

Implementation should avoid hard-coded `13` assumptions in:

- Journal progress text and grid layout.
- Old Barnaby trigger checks.
- Atlas and goals completion copy.
- Any all-species celebration or release checklist.

Recommended copy pattern:

- Original milestone: `13 original Tidelings found`
- Wider-coast progress: `18 of 21 wider-coast Tidelings found`

## Asset And Data Handoff

Future implementation needs:

- Transparent illustrated gameplay sprite for each new species at the established target size.
- Unity-generated `.meta` files for every imported asset.
- Git LFS initialized locally before committing binary art.
- `Assets/ASSET_MANIFEST.md` entries with source/license/date/path and AI prompt/model/review
  details where applicable.
- `TidelingSpecies` ScriptableObject rows and `SpeciesDatabase` registration.
- Encounter, journal, atlas, ambient behavior, goals, contest, save, and missing-sprite checks.

Do not close #215 until Unity compile, encounter/journal/contest/save validation, LFS checks,
asset provenance, IP-safety review, and iPad layout checks pass.
