# The Great Low Tide — Experience and Production Specification

Issue: #201

Milestone: v0.8
Status: implementation source of truth

## Player Promise

For one unusually wide low tide, the four familiar habitats become one connected expedition.
The player follows signs of a very old visitor, helps the coast open gentle paths, fills a field
station with memories, and ends at a welcoming celebration with Old Barnaby. The experience
adds occasion and continuity without adding urgency, loss, chores, or a second core loop.

The minute-to-minute loop remains:

```text
Explore -> Notice -> Encounter -> Catch or let go -> Journal -> Explore
```

The expedition layer gives that loop a readable next place and a visible effect on the coast.
It never replaces free exploration, the existing encounter grace and pity rules, the Steady-the-
Jar interaction, the journal, friendly contests, or reversible growing-up memories.

## Experience Principles

- **A journey, not a checklist.** One inviting next step is enough. The atlas and goals panel
  describe places and discoveries rather than exposing raw counters as chores.
- **Persistent wonder.** Open paths, landmark responses, field-station displays, and completed
  celebrations reconstruct from save data every time.
- **Nothing is missable.** Randomness may add surprise but cannot be the only route through a
  chapter. Skipped scenes, missed catches, missing assets, and interrupted presentations all
  return to a safe playable state.
- **Quiet spectacle.** Layered art, motion, and local audio make moments feel large. Every beat
  remains understandable without audio and playable with reduced motion.
- **One source of truth.** Existing catches, story beats, zone unlocks, contest progress, levels,
  and growth memories remain authoritative. v0.8 records only state that those systems cannot
  already answer.

## Session Rhythm

A useful session is five to fifteen minutes:

1. Boot shows either the skippable opening or a compact return card.
2. The player sees one current chapter objective.
3. Ordinary exploration provides ambient life, clues, catches, and journal progress.
4. One landmark or route change closes a chapter-sized thought.
5. The atlas and field station reflect progress before the player stops.

No tide expires. There are no daily tasks, countdowns, real-world time gates, streaks, energy,
currency, crafting requirements, or penalties for leaving mid-chapter.

## Beginning-to-End Arc

### Chapter 1 — The Coast Opens (Tidepool Shallows)

The opening panorama reveals that the water has drawn back farther than usual. Mira invites the
player to look, not hurry. A tide-carved stone arch frames the distant coast and field station.
Small ripple and shell clues teach the visual language for authored discoveries. The chapter
ends when ordinary early collection progress and a visit to the arch reveal the Meadow route.

The existing `first_catch_intro` and `meadow_pointer` beats remain valid. New saves encounter
them naturally; advanced saves backfill the chapter without replaying introductory dialogue.

### Chapter 2 — The Waving Path (Seagrass Meadow)

Broad waves move through the grass and ambient Tidelings make the habitat feel occupied. The
player follows two deterministic clue sites and any qualifying catch path toward the Meadow
landmark. The landmark response and existing Meadow catch progress part the Kelp Curtain. The
field station gains a Meadow drawing or pressed-grass display.

This chapter uses the existing five-Meadow-species gate as one completion path, not a random-
only requirement. A deterministic landmark discovery supplies an alternate completion token so
the chapter cannot stall on encounter rolls.

### Chapter 3 — Lights Behind the Kelp (Kelp Curtain)

Layered kelp, distant glows, and silhouettes create depth while leaving the grid readable. The
player follows glow trails, meets a visiting Tideling, and reaches a sheltered kelp landmark.
Completing any two of the authored discoveries, or satisfying the existing Kelp catch gate,
reveals the Rocky Shelf route. A friendly contest may be introduced as an optional celebration;
losing, leaving, or never entering it cannot stop progression.

The field station gains a soft light display and a place for a contest ribbon when earned.

### Chapter 4 — The Old Stones (Rocky Shelf)

Barnacled formations echo the shape of Old Barnaby without presenting danger. Clues from prior
chapters resolve into a clear approach. The player reaches the monumental shelf, completes the
coast-wide festival set piece when desired, and receives a repeatable invitation to meet Old
Barnaby. Letting him go or missing the catch preserves the invitation. Catching him completes
the expedition and changes the coast and field station into a lasting celebration.

Existing saves that already contain Old Barnaby receive the completion state and a short
welcome-back memory; they do not replay or duplicate the catch.

## Major Beat Contract

Every player-visible one-shot beat follows the same contract:

1. Evaluate eligibility from saved facts and current authored content.
2. Persist the durable result before an irreversible visual/collider change.
3. Present optional camera, motion, dialogue, and audio layers.
4. On skip, interruption, or missing content, apply the same final state immediately.
5. Return input and scene control through a guaranteed cleanup path.

| Beat ID | Trigger | Presentation | Persistent result | Missing/interrupted fallback |
|---|---|---|---|---|
| `expedition.opening_seen` | New save; no expedition state | Short coast panorama, title, Mira invitation | Opening acknowledged; chapter 1 active | Show text card, mark seen, continue to Overworld |
| `chapter.shallows.started` | First safe Overworld entry | Arch framing and first objective | Chapter 1 started | Objective appears without camera move |
| `discovery.shallows.arch` | Player enters authored arch area | Shell glint, restrained framing | Arch discovered | Record discovery and show a small toast |
| `chapter.shallows.completed` | Arch discovered plus first catch, or advanced-save backfill | Meadow route reveal | Chapter 1 complete; Meadow route state open | Reconstruct open route without replay |
| `chapter.meadow.started` | First Meadow entry after chapter 1 | Grass wave and welcome sign | Chapter 2 started | Show welcome text only |
| `discovery.meadow.wave` | Enter deterministic clue site | Broad grass wave, ambient peek | Meadow clue discovered | Static bent-grass marker remains |
| `chapter.meadow.completed` | Meadow landmark plus qualifying progress, or existing five-Meadow gate | Kelp Curtain parts | Chapter 2 complete; Kelp route open | Persist and enable traversable collider state first |
| `chapter.kelp.started` | First Kelp entry | Layered glow and Mira line | Chapter 3 started | Show objective and continue |
| `discovery.kelp.lights` | Enter glow-trail site | Local glow trail and silhouette | Kelp clue discovered | Static glow marker; no encounter grant |
| `chapter.kelp.completed` | Two authored Kelp discoveries or existing three-Kelp gate | Rocky Shelf reveal | Chapter 3 complete; Rocky route open | Apply open visuals/colliders immediately |
| `festival.completed` | Player wins the featured contest | Crowd response and ribbon handoff | Set piece complete; station ribbon earned | Compact result card; contest result remains authoritative |
| `chapter.rocky.started` | First Rocky entry | Shelf framing and distant response | Chapter 4 started | Welcome sign and objective only |
| `discovery.rocky.approach` | Reach the authored approach with required chapters complete | Landmark response, Barnaby invitation | Finale available | Keep approach traversable and show invitation text |
| `finale.barnaby.met` | Start finale encounter | Gentle interactive entrance | Finale attempted, not completed | Return safely; invitation remains available |
| `finale.barnaby.completed` | Existing catch result records Old Barnaby | Coast and station celebration | Chapter 4 and expedition complete | Backfill from caught species; never add a duplicate catch |
| `expedition.memory_seen` | Completed save opens return card or journal memory | Shortened panorama/memory | Memory acknowledged | Continue button always routes to normal play |

IDs are lowercase dotted identifiers and are immutable once shipped. Renamed assets retain the
same IDs. Unknown IDs in a save are preserved where practical and ignored by runtime queries.

## Trigger and Backfill Rules

- Catches remain authoritative in `SaveData.caught`; seen-only encounters do not satisfy catch
  requirements.
- `triggeredStoryBeatIds`, `completedQuestIds`, and `unlockedZoneIds` remain authoritative for
  existing v0.7 content. v0.8 must not mirror those facts into new lists.
- On load, derive missing chapter completion in order from existing zone unlocks, qualifying
  catches, triggered story beats, and Old Barnaby ownership.
- Backfill is monotonic and idempotent. It may grant already-earned presentation state but must
  not add catches, levels, forms, contest wins, or duplicate dialogue queues.
- Authored objectives have at least one deterministic location-based route. Random catches can
  accelerate progress but cannot indefinitely block it.
- Presentation-started state is not used as completion state. If interruption occurs after the
  durable result is written, the next load reconstructs the final state without replaying the
  whole sequence.

## Stable State Model

Issue #203 owns the concrete implementation. The intended additive model is:

```text
SaveData
  schemaVersion
  ...existing v0.1-v0.7 fields...
  activeExpeditionChapterId
  completedExpeditionChapterIds[]
  authoredDiscoveryIds[]
  landmarkStateIds[]
  fieldStationUpgradeIds[]
  completedSetPieceIds[]
```

Use stable IDs rather than serialized object references. APIs should be idempotent query/
remember operations and autosave only when a value changes. Collections are normalized for
nulls, blanks, whitespace, and duplicates. Unknown values never block loading.

Suggested canonical IDs:

| Kind | IDs |
|---|---|
| Chapters | `chapter.shallows`, `chapter.meadow`, `chapter.kelp`, `chapter.rocky` |
| Landmarks | `landmark.shallows.arch`, `landmark.meadow.wave`, `landmark.kelp.lights`, `landmark.rocky.old-stones` |
| Station stages | `station.shallows`, `station.meadow`, `station.kelp`, `station.rocky`, `station.finale` |
| Set pieces | `setpiece.opening`, `setpiece.meadow-unlock`, `setpiece.kelp-unlock`, `setpiece.rocky-unlock`, `setpiece.festival`, `setpiece.finale` |

## Reusable Systems vs. Authored Content

| Reusable system | Authored content |
|---|---|
| Expedition state/query service over `GameSaveService` | Four chapter definitions and copy |
| Skippable sequence runner with interruption cleanup | Opening, three route reveals, festival, finale timelines |
| Discovery sequence model and trigger | Clue sites for named species and zones |
| Atlas node/state binding | Coast illustration, node positions, goal text |
| Landmark state binding | Four landmark layer sets |
| Ambient motion components and bounded pools | Per-zone profiles and placed emitters |
| Field-station decoration slots | Drawings, shells, ribbon, lights, finale display |
| Layered local audio controller | Zone beds, stingers, calls, musical cues |
| Editor generators and validators | Scene placements, asset bindings, content tables |

Reusable systems must function with missing optional content. Authored content must not contain
logic that competes with save, encounter, contest, journal, growth, or zone services.

## Scene and Data Responsibilities

- **Boot:** owns save load, opening/return decision, skip-safe routing, and no expedition logic.
- **Overworld:** owns exploration, gates, landmarks, ambient actors, discovery triggers, and
  additive entry points. It remains the recovery destination after an interrupted overlay.
- **CatchEncounter:** remains the only catch interaction. Authored discoveries populate
  `EncounterContext`; they do not bypass catch results or remove `Let it go`.
- **Journal:** shows atlas/memory entry points and discovery context without owning progress.
- **Contest:** retains existing best-of-three, Current, swap, and tuckered-out rules. Festival
  presentation observes results and cannot change contest balance.
- **CoastAtlas (additive):** read-only projection of world state with close/back behavior. It is
  not fast travel unless a later approved revision explicitly adds it.
- **FieldStation:** may be a dedicated additive scene or a bounded Overworld area. It reads
  saved achievements into decoration slots and links existing UI destinations.
- **Finale (additive presentation):** orchestrates the approach and encounter handoff; the catch
  service remains authoritative for ownership.

New designer-editable assets should include `ExpeditionChapter`, `AuthoredDiscovery`,
`LandmarkPresentation`, `AmbientZoneProfile`, `FieldStationStage`, and `CoastAtlasDefinition`
ScriptableObjects where they reduce scene hardcoding. Stable IDs, validation, and soft defaults
are required for each.

## Zone Visual and Landmark Brief

| Zone | Landmark | Layering and palette | Readability rule |
|---|---|---|---|
| Shallows | Tide-carved arch framing the coast | Warm sand and turquoise water; shell foreground; pale distant coast | Arch opening frames, never covers, the walkable route |
| Meadow | Sweeping grass basin | Blue-green water; broad midground grass bands; small surface glints | Motion direction must not disguise obstacles or encounter grass |
| Kelp Curtain | Towering kelp gallery with distant lights | Deep teal layers, warm green ribbons, sparse gold/cyan glows | Foreground kelp fades or parts near player and gates |
| Rocky Shelf | Monumental barnacled old-stone formation | Slate, coral, cream foam, warm late-light accent | Barnaby-like silhouette stays environmental, kind, and distinct from a creature sprite |

Each vista has foreground, gameplay, midground, and background layers, day/night tints, reduced-
motion states, and a missing-decoration fallback. Gameplay colliders and walkable tiles never
depend on decorative art loading successfully.

## Audio Brief

All audio is bundled locally, honors `TidepoolSettingsService`, and has a silent fallback.

| Location/moment | Layers |
|---|---|
| Shallows | Small water laps, pebble ticks, sparse shore birds |
| Meadow | Soft grass wash, bubbles, light reed-like musical color |
| Kelp | Lower water bed, kelp creaks, distant gentle calls, small glow tones |
| Rocky Shelf | Wider surf, shell resonance, spacious low musical notes |
| Field station | Sheltered water, paper/shell details, quiet motif |
| Discovery | Brief ducking, one local cue, short non-urgent stinger |
| Route unlock | Rising but restrained local flourish; no fanfare pressure |
| Festival | Warm rhythmic variation using the same musical identity |
| Finale | Sparse coast-wide motif, then ordinary ambience returns |

Zone transitions crossfade without duplicate loops. Mute and volume updates apply immediately.
No runtime fetch, streaming URL, microphone, analytics, or remote configuration is permitted.

## Content Matrix

| Feature | Shallows | Meadow | Kelp Curtain | Rocky Shelf |
|---|---|---|---|---|
| Chapter | Coast Opens | Waving Path | Lights Behind Kelp | Old Stones |
| Landmark | Stone arch | Grass basin | Kelp gallery | Barnacled formation |
| Deterministic clues | Shell glint, ripples | Wave path, unusual shell | Glow trail, silhouette | Old shell marks, distant response |
| Ambient life | Peek/surface | Drift/school | Glow/hide | Rest/approach after befriending |
| Route event | Meadow reveal | Kelp parts | Shelf opens | Finale approach |
| Station reward | Atlas table | Meadow drawing | Hanging glow | Old-stone rubbing |
| Audio identity | Pebbles/laps | Grass/bubbles | Creaks/glows | Surf/resonance |
| Day/night change | Cooler pools/glints | Dark grass bands | Stronger distant lights | Moonlit foam edges |
| Authored catch use | Early teaching example | Gullwing or Meadow representative | Tanglemaw or new representative | New representative and Barnaby lead-in |

## Opening and Return Flow

New saves see a panorama lasting no more than roughly twenty seconds if untouched, with an
immediately available `Skip` button and a core `Continue` target of at least 88pt. Copy:

> The tide went farther out than anyone expected.
>
> A whole coast is waiting to be noticed.

Reduced motion uses static crossfades and tint changes. Missing layers use the title card alone.
The opening acknowledgement is saved before routing; force-quit resumes at the Overworld, not
inside an incomplete cinematic.

Returning saves see the active chapter name, current place, and one next-step sentence. Completed
saves receive `The coast remembers your Great Low Tide.` They can continue immediately; the full
opening never auto-replays.

## Accessibility, Input, and Safety

- Canvas Scaler reference is `1024 x 768`, match `0.5`; all screen UI lives inside
  `SafeAreaFitter`.
- Core touch targets are at least 88pt. No required drag precision, double-tap, hold timing,
  gesture, or audio-only cue is added.
- Every sequence has skip or continue visible from its first interactive frame.
- Reduced motion removes pans, parallax, continuous sway, large particles, and camera impulses;
  tint, static layering, copy, and final-state changes remain.
- Copy uses short warm sentences and avoids `failed`, `wrong`, `locked`, urgency, or threat.
- Existing `Let it go`, retry, back, and free-exploration exits remain reachable.
- No protected names, characters, visual trade dress, copied musical motifs, aiming, throwing,
  capture probability, or creature riding is introduced.

## Performance and Offline Budgets

- Target remains 60 fps on the supported iPad; no required gameplay depends on frame rate.
- Ambient populations and particles have serialized per-zone caps and reuse bounded pools.
- Offscreen continuous animation and audio layers disable or sleep outside a camera margin.
- Normal exploration must not allocate per-frame managed garbage from v0.8 ambience.
- Art and audio import settings are sized for iPad memory; shared layers are reused between
  day/night states where practical.
- The complete experience works in airplane mode. No new package or API may add outbound network
  behavior. Run `scripts/verify-no-network-guardrails.sh` after relevant code/package changes.

## Asset and Production Requirements

- Every shipping or potentially shipping art/audio asset is recorded in
  `Assets/ASSET_MANIFEST.md` with source, license, date, path, and required AI prompt/model/review
  details.
- Binary art/audio uses Git LFS and passes `scripts/verify-lfs.sh` before commit.
- Creature and environmental art receives accidental-resemblance and protected-expression
  review. Prompts never name protected franchises, characters, or living artists.
- Editor tools update existing `Tools/Tidepool/` generators. Scene wiring should be reproducible;
  any manual exception is documented next to the scene assembly instructions.
- Missing sprites, clips, materials, profiles, or unknown state IDs produce a neutral fallback,
  never a crash, invisible route, lost input, or blocked save.

## Required Verification by Feature

Automated/edit-mode coverage should include new-save defaults, schema migrations, idempotent
writes, JSON round trips, unknown IDs, backfill, skip equivalence, duplicate prevention, and
interrupted-state reconstruction. Unity Play Mode covers scene entry/exit, input restoration,
route colliders, standard encounters, `Let it go`, contest retry/exit, journal return, and
missing-asset paths.

Target-iPad verification covers safe areas, landscape layouts, 88pt targets, touch-first flows,
reduced motion, mute/volume, force-quit/relaunch, memory, frame pacing, and a clean offline
new-save journey. Release verification also runs the no-network, IP-safety, LFS, asset-manifest,
privacy, and pre-release checklists.

## Scope and Descope Order

The protected minimum is: opening invitation, one readable landmark per zone, a coherent four-
chapter path with deterministic completion, persistent route state, and the Old Barnaby finale.

If time or performance requires cuts, remove in this order:

1. Extra creature calls and musical variations.
2. Optional ambient behavior variants and dense particles.
3. Field-station visiting Tidelings; keep static earned displays.
4. Festival spectators and extended introductions; keep the existing contest and ribbon result.
5. Optional clue cinematography; keep deterministic clue triggers and static markers.
6. Atlas pan/parallax; keep a static safe-area map with state markers.
7. Extra authored clues beyond one deterministic route per chapter.

Do not cut the opening, one landmark per zone, the expedition arc, persistent route recovery,
the finale, existing core loop, journal/nicknames/save behavior, skip/reduced-motion safety, or
offline operation.

## Architecture References

This specification extends rather than replaces:

- `PRD.md` for product pillars, catch rules, tone, IP distance, and data principles.
- `docs/ROADMAP.md` for version scope.
- `docs/NARRATIVE_FRAMEWORK.md` and `docs/STORY_COPY.md` for Mira, v0.7 story IDs, gates, and voice.
- `docs/V0_4_ZONE_TRANSITIONS.md` for zone transition responsibilities and Editor wiring.
- `docs/DAY_NIGHT_RULES.md` for compressed in-game lighting time and encounter availability.
- `docs/CONTEST_ARCHITECTURE.md` and `docs/CONTEST_DEPTH_MODEL.md` for friendly contest rules.
- `docs/GROWING_UP_FORMS.md` for reversible form memories and save normalization.
- `docs/V0_1_SCENE_ASSEMBLY.md` and `docs/UNITY_IOS_PIPELINE.md` for scene and device baselines.
- `docs/ASSET_PIPELINE.md`, `docs/NO_NETWORK_GUARDRAILS.md`,
  `docs/PRE_RELEASE_CHECKLIST.md`, and `docs/IP_SAFETY_CHECKLIST.md` for shipping controls.

Later v0.8 issues may refine presentation details, but any change to persistence semantics,
player safety, chapter completion, protected minimum scope, or core-loop boundaries must update
this document first.
