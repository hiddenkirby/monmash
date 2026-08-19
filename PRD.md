# Tidepool — Product Requirements Document

**Working title:** Tidepool *(alternates: Tidewild, Shellwater, The Collecting Jar)*
**Repo:** `monmash`
**Author:** Ryan
**Date:** August 1, 2026
**Status:** Draft v1 — scoped for a first playable build by Sunday, August 2

---

## 1. Vision

A calm, ad-free creature-collecting game for one specific 7-year-old, set in tidepools and seagrass meadows instead of a fantasy overworld. She explores, discovers small sea creatures called **Tidelings**, catches them with a collecting jar, and fills a journal. Later versions add training and friendly contests.

The game is built to be legally distinct from Pokémon from day one, so that a Steam release remains an open option rather than a rewrite.

**The one-sentence pitch:** *Wade through the shallows, catch what you find, fill your journal.*

### Design pillars

1. **Discovery over grinding.** The reward is seeing a creature you haven't seen before, not a number going up.
2. **Nothing is ever lost.** No death, no permanent failure, no progress that can be undone by a mistake.
3. **Readable in five seconds.** She's a strong reader for 7, but every core action should be understandable from the picture alone.
4. **Quiet by default.** No ads, no purchases, no notifications, no accounts, no network. Ever.

### Non-goals

- Multiplayer, trading, or online anything (v1–v4)
- Open-ended crafting, base building, or economy systems
- Difficulty tuning for adults
- Voice acting or cutscenes

---

## 2. Target player

**Primary:** One 7-year-old girl, strong reader for her age, plays on an iPad, already fluent with the genre's vocabulary from watching/playing Pokémon-likes.

**Implications:**

| Trait | Design consequence |
|---|---|
| Strong reader, but 7 | Full sentences OK in the journal and tutorials; core buttons must still be icon-first |
| Short, interruptible sessions | Autosave on every meaningful event; no "are you sure?" dialogs to lose progress in |
| Low frustration tolerance for unfair loss | Catch failures cost ~8 seconds, never a creature or an item |
| Fine motor still developing | Minimum 88pt touch targets; no drag-precision, no double-taps, no gestures |
| Genre-literate | She will notice if it feels shallow. Depth should come from roster variety, not systems |

**Secondary player (you):** the point is also to learn the Unity/iOS pipeline end to end. The architecture below deliberately favors "understand every line" over "install a framework."

---

## 3. IP safety — the guardrails

Game *mechanics* are not copyrightable. Turn-based creature collection is a well-populated genre (Temtem, Coromon, Cassette Beasts, Monster Sanctuary, Palworld all ship commercially). What creates risk is **specific expression** — creature designs, names, art, music, UI trade dress — plus **a small number of Japanese software patents** Nintendo has asserted.

### Hard bans (never in the codebase, assets, or store copy)

- The words *Pokémon*, *pocket monster*, *Poké Ball*, *Pokédex*, *gym leader*, *gotta catch 'em all*, or any near-miss of them
- Any creature that reads as a recolor or silhouette-match of an existing Pokémon
- Copying the 18-type effectiveness chart, the 151/386/etc. roster counts, or the six-slot party + PC box structure verbatim
- The red/white sphere, the blue-and-white battle UI layout, the Center/Mart iconography
- Any Pokémon music motif, sound effect, or font

### Patent-aware design decisions

Nintendo and The Pokémon Company sued Pocketpair in September 2024 over three Japanese software patents covering (a) capturing/summoning a creature by **aiming and throwing** an item at it, (b) a **real-time capture-probability indicator** shown while aiming, and (c) **mounting a creature to fly or glide**. Pocketpair removed the throw-to-summon mechanic in a patch, and the Japan Patent Office has since provisionally rejected a related Nintendo application for lack of novelty. The case is still live.

These are Japanese patents and a hobby project is not a realistic target — but the cheapest possible time to design around them is now, before any code exists. So:

| Patented pattern | What Tidepool does instead |
|---|---|
| Aim and throw a capture device | **Tap-and-hold a jar** over a stationary creature. No aiming, no projectile, no trajectory. |
| Live capture-probability meter while aiming | **No probability display at all.** The catch is a timing mini-game (§5.3), not a dice roll with odds shown. |
| Mount a creature to fly/glide/traverse | **No riding.** Traversal is on foot only. If a swim or boat mechanic is ever added, it is a vehicle, not a creature. |

### Deliberate genre distance

| Pokémon convention | Tidepool |
|---|---|
| "Monsters," fantasy overworld | **Tidelings** — small sea creatures in real tidepool/coast habitats |
| Thrown capture sphere | A **glass collecting jar**, held over the creature |
| 18-type chart | **Five Currents** ring: Current, Coral, Tide, Glow, Stone (§4.2) |
| Faint / HP zero | **"Tuckered out"** — the creature naps and is fine in a minute |
| Trainer battles for badges | **Tidepool contests** — friendly, judged, no stakes (v0.3+) |
| Evolution via level | **Growing up** — a creature changes when it's been with you long enough (v0.4+) |

### Asset provenance

Because both AI-generated art and CC0 packs are in scope:

- **Keep a manifest.** `assets/ASSET_MANIFEST.md` logs every asset: source, license, and for AI assets the tool, model, date, and prompt. This is 10 minutes of work now and the difference between a smooth and a painful Steam submission later.
- **Never prompt with protected names.** No "like Pikachu," no "Pokémon style," no existing creature or artist names in any prompt. Prompt from the tidepool fiction only. Log the prompts so this is auditable.
- **Review every generated sprite** for accidental resemblance before it ships.
- **Steam disclosure:** Valve rewrote its AI rules in January 2026 to focus on player-facing content. AI-generated art that ships in the game **is** disclosable (a store-page checkbox plus a short description of which categories used AI). AI used only as a dev tool — code assistants, debugging — is explicitly out of scope. Plan to check the box; it's now common enough that roughly a fifth of new Steam releases carry the tag.
- **CC0 sources:** Kenney.nl and itch.io CC0 packs are safe and require no attribution, but log them anyway.

---

## 4. The world and the creatures

### 4.1 Setting

A stretch of quiet coastline at low tide. The player is a kid with a bucket, a net, and a glass jar, cataloguing what lives in the pools. Warm light, shallow water, no threat anywhere in the fiction.

**Zones** (v0.1 ships the first two):

| Zone | Encounter terrain | Feel |
|---|---|---|
| **Tidepool Shallows** | Shallow pools | Starter area. Common Tidelings only. |
| **Seagrass Meadow** | Seagrass beds — the encounter terrain | The main hunting ground. Walking through seagrass triggers encounters. |
| *Kelp Curtain* (v0.4) | Kelp fronds | Darker, rarer creatures |
| *Rocky Shelf* (v0.4) | Barnacle flats | Stone-current creatures, the secret |

### 4.2 The Five Currents

A five-element ring, each beating the next: **Current → Coral → Stone → Glow → Tide → Current**.

Five is deliberate: a 7-year-old can hold a five-ring in her head. It's also structurally different from the Pokémon chart (no dual types, no immunities, no 4× multipliers). Effectiveness is a flat 1.5× / 1.0× / 0.75×.

Currents are **collected and displayed in v0.1**; they only affect outcomes once battles land in v0.2.

### 4.3 Roster — 13 Tidelings

Twelve findable plus one secret. Every one of these is a **data row plus a sprite** — the code cost of 13 vs. 4 is essentially zero once the ScriptableObject pipeline exists (§6.3). The art is the only real cost.

| # | Name | Current | Rarity | Zone | Concept |
|---|---|---|---|---|---|
| 1 | **Blip** | Current | Common | Shallows | Thumb-sized darting fish, always in a hurry |
| 2 | **Nubbin** | Stone | Common | Shallows | Hermit crab wearing a pebble too big for it |
| 3 | **Frillick** | Coral | Common | Shallows | Ruffled sea slug, moves like a dropped ribbon |
| 4 | **Sputter** | Glow | Common | Shallows | Cluster of blinking plankton that travels as one |
| 5 | **Wobbet** | Tide | Common | Meadow | Small round jelly, drifts, bumps into things |
| 6 | **Clackaw** | Stone | Uncommon | Meadow | Pistol shrimp with one enormous snapping claw |
| 7 | **Sweepfin** | Current | Uncommon | Meadow | Palm-sized ray, glides just under the surface |
| 8 | **Mossback** | Coral | Uncommon | Meadow | Tiny turtle with a garden growing on its shell |
| 9 | **Lumen** | Glow | Uncommon | Meadow | Lanternfish; its light dims when it's shy |
| 10 | **Thistlecoat** | Coral | Uncommon | Meadow | Urchin whose spines lie flat when it trusts you |
| 11 | **Gullwing** | Current | Rare | Meadow | Flying fish; only appears in the last hour of daylight |
| 12 | **Tanglemaw** | Tide | Rare | Meadow | Small octopus, curious, unlatches your jar |
| 13 | **Old Barnaby** | Stone | Secret | Shallows | An ancient barnacled shape. Appears once, after 10 species are logged. |

**Rarity weights:** Common 60%, Uncommon 32%, Rare 8%. Old Barnaby is scripted, not rolled.

---

## 5. Core loops

### 5.1 The minute-to-minute loop (v0.1 — the weekend build)

```
Explore  →  Encounter  →  Catch mini-game  →  Journal entry  →  Explore
```

That's it. **No battles in v0.1.** Battle is v0.2. This is the deliberate cut line.

### 5.2 Exploration

- **Tap-to-move.** Tap a reachable tile; the character walks there via a grid path. Tapping again re-routes.
- **Grid-based**, 1 tile per step, ~4 tiles/second. Feels responsive without being twitchy.
- **Encounters** roll on each step *onto a seagrass tile*: 12% chance, with a **guaranteed 3-step grace period** after any encounter ends so she is never chain-ambushed.
- **Encounter pity:** if 25 seagrass steps pass with no encounter, force one. Prevents a frustrating dry spell.
- Camera follows with light smoothing. No manual camera control.

> **Note on the encounter choice.** Invisible random encounters in seagrass are what she knows and asked-for behavior, but they are the single most likely source of frustration for a 7-year-old, because they take control away. The grace period, the pity timer, and the always-succeeds "let it go" button are the mitigations. If she gets annoyed in playtesting, the fallback is visible wandering creatures — treat that as a known v0.2 lever, not a failure.

### 5.3 The catch mini-game — "Steady the Jar"

A Tideling appears, centered and stationary. It is never hostile.

1. A horizontal **calm bar** appears with a highlighted **steady zone**. A marker sweeps left↔right.
2. Tap when the marker is inside the zone. **Three successful taps** = caught.
3. A miss makes the creature wigglier: the marker speeds up ~15% and the zone shrinks slightly.
4. **Three misses** = the Tideling swims off. A short friendly line ("It slipped away!"), then straight back to exploring. Nothing is lost. The same species can be found again.
5. A **"Let it go"** button is always visible and always works instantly.

**Tuning by rarity:** Common = wide zone, slow marker. Rare = narrower zone, faster marker. This is where "gentle challenge" lives — the only place in v0.1 where she can fail, and the cost is eight seconds.

**Explicitly not implemented:** aiming, throwing, a catch-probability percentage, or any odds display. See §3.

### 5.4 The Journal

The collection screen and the actual reward.

- Grid of 13 slots. Undiscovered = a silhouette with a `?`.
- Tapping a caught Tideling opens its page: big art, name, Current, where it was found, the date she caught it, a one-paragraph field note, and **how many she's seen**.
- **She can rename any Tideling she catches.** Nickname field, 12 characters, on-screen keyboard. This is the highest-value/lowest-cost feature in the whole document — it converts a sprite into *her* creature. Do not cut this.
- Progress reads "**7 of 13 found**" with a shell-shaped progress bar.
- Catching the 10th species triggers Old Barnaby's appearance in the Shallows.

### 5.5 Later loops (not this weekend)

- **v0.2 — Contests.** Turn-based, 2 moves per Tideling, no fainting: a tuckered-out Tideling naps and swaps out. Losing costs nothing but a retry.
- **v0.3 — Growing.** Levels 1–20, Currents become mechanically real, moves learned at thresholds.
- **v0.4 — Growing up.** Some Tidelings change form after enough time together. Never forced, always reversible in the journal's memory.

---

## 6. Technical design

### 6.1 Stack

| Layer | Choice | Why |
|---|---|---|
| Engine | **Unity 6.5.6 (6000.5.6f1)** | Project-standard editor version for the current prototype. Keep the repository on one Unity patch version to avoid package and ProjectSettings churn. |
| Template | **2D (URP)** | Correct 2D defaults out of the box |
| Language | C# | |
| Target | **iOS 15+, iPad, landscape** | Unity supports iOS 13+; 15 is a safe floor |
| Scripting backend | IL2CPP, ARM64 | Required for device builds |
| Build path | Unity → Xcode project → device via cable | |
| Version control | Git + Unity's `.gitignore`, Git LFS for art | Set up *before* the first asset import |

### 6.2 Art direction — a decision that matters for the deadline

**Use illustrated sprites at ~512px, not pixel art.**

AI image tools are consistently poor at *true* pixel art — they produce pixel-art-*looking* images with inconsistent grids that fight a Pixel Perfect Camera and look wrong when scaled. Illustrated sprites with transparent backgrounds sidestep this entirely, downscale cleanly on a Retina iPad, and are far faster to iterate.

- Creature sprites: transparent PNG, ~512×512, consistent lighting direction, consistent framing (creature fills ~80% of frame).
- Generate all 13 in **one batch with one shared style prompt** — style consistency across a roster matters more than any individual sprite's quality.
- Tiles and UI: CC0 packs (Kenney's top-down/UI packs) rather than generated — AI is bad at seamless tiling.
- Log everything in `assets/ASSET_MANIFEST.md`.

### 6.3 Data architecture

The whole point of the ScriptableObject layer: **adding creature #14 must be a designer action, not a code change.**

```
TidelingSpecies : ScriptableObject
    id (string, stable)         displayName
    current (enum)              rarity (enum)
    sprite                      silhouette (auto-derived at runtime)
    fieldNote (string)          habitatZones (Zone[])
    catchZoneWidth (float)      catchMarkerSpeed (float)
```

```
CaughtTideling  (serializable, runtime)
    speciesId       nickname        caughtAtUtc
    caughtInZone    timesSeen
```

```
SaveData
    caught: List<CaughtTideling>
    seenSpeciesIds: List<string>
    playerTile: Vector2Int
    currentZone: string
    schemaVersion: int          // set to 1 now; makes v0.2 migration painless
```

### 6.4 Systems

| System | Approach | Est. lines |
|---|---|---|
| Grid pathfinding | **BFS over the Tilemap's walkable cells.** No packages, no NavMesh, fully deterministic, easy to debug. | ~80 |
| Player movement | Coroutine walking a tile queue, lerped between cell centers | ~60 |
| Encounter roll | Per-step check on seagrass tiles + grace counter + pity counter | ~50 |
| Catch mini-game | Own additive scene, ping-pong marker, tap window check | ~120 |
| Journal UI | Canvas + scroll grid, populated from the species asset list | ~150 |
| Save/load | `JsonUtility` → `Application.persistentDataPath`. **Not PlayerPrefs** — the collection is real data. | ~50 |
| Input | Legacy `Input.touches` / `Input.mousePosition`, or Input System. Either is fine; pick one and don't mix. | — |

### 6.5 Scenes

- `Boot` — loads save, routes onward
- `Overworld` — tilemap, player, encounter logic
- `CatchEncounter` — loaded **additively** over a paused overworld
- `Journal` — additive overlay

### 6.6 iPad-specific requirements

- **Landscape only.** Lock orientation in Player Settings.
- **Safe area.** Anchor all UI inside `Screen.safeArea` — the iPad home indicator will otherwise eat a button.
- **Canvas Scaler:** Scale With Screen Size, reference 1024×768, match 0.5.
- **Touch targets ≥ 88pt.** Bigger than Apple's 44pt minimum, because she's 7.
- Target 60fps — trivially achievable; don't spend time optimizing.

---

## 7. Content, audio, and tone

- **Audio:** one ambient loop (waves + gulls), a catch chime, a soft "it got away" note, and a UI tap. CC0 from Kenney or Freesound. Four sounds is enough; silence is worse than four sounds.
- **Copy voice:** warm, short, never scolding. "It slipped away!" not "You failed."
- **No timers, no scores, no stars, no daily streaks.** Nothing that creates obligation.

---

## 8. Non-functional requirements

| Requirement | Spec |
|---|---|
| **No network** | The app makes zero outbound requests. No analytics SDK, no crash reporting, no ads, no IAP. This makes COPPA a non-issue by construction, and is a hard requirement, not a preference. |
| Offline | Fully playable in airplane mode |
| Save integrity | Autosave after every catch, every zone change, and on `OnApplicationPause` |
| Launch time | Under 5 seconds cold |
| Build size | Under 300MB |
| Accessibility | No color-only information (Currents get icon + color + name); no timing element outside the catch bar |

---

## 9. Distribution

### v0.1 — free Apple ID ("personal team")

This is what ships this weekend, and it comes with real constraints worth knowing **before** Sunday:

- The provisioning profile **expires 7 days after issue.** The app stops launching and must be rebuilt from Xcode with the iPad connected.
- Roughly **3 devices** and **~10 new app IDs per rolling week** per free account.
- **You cannot send the build to anyone else.** Grandma cannot install it.

**Tell her this up front** — "this one only lasts a week while I'm still building it" lands much better on Saturday than a mysteriously dead app icon on Thursday.

### Upgrade path

A **$99/yr Apple Developer Program** membership buys 1-year certificates and TestFlight — she installs over the air and gets updates without cabling. Recommended once the game survives its second weekend. Enrollment can take from hours to a couple of days, so start it before you need it.

### Steam (someday)

$100 per app one-time. Requires: the AI disclosure checkbox (§3), an asset manifest you can stand behind, original creature designs, and no borrowed audio. Everything in this document is written so that becoming a Steam build is a *decision*, not a rewrite.

---

## 10. Scope and roadmap

### v0.1 — "It catches" (target: Sunday, Aug 2)

**Must ship:**

- Two zones (Shallows + Meadow), tap-to-move
- Random encounters in seagrass
- Catch mini-game with the gentle-failure loop
- Journal with silhouettes, nicknames, and "N of 13 found"
- Save/load
- Running on her iPad

**Roster floor: 6 species.** The design is 13; ship as many as the art batch produces. The code doesn't care.

**Explicitly out of v0.1:** battles, levels, Currents having any effect, Old Barnaby, day/night (Gullwing's condition), audio beyond the catch chime, title screen polish.

### Beyond

| Version | Theme | Headline |
|---|---|---|
| v0.2 | Contests | Turn-based friendly battles, 2 moves, no fainting |
| v0.3 | Growing | Levels 1–20, Currents matter, move learning |
| v0.4 | The wider coast | Kelp Curtain + Rocky Shelf, growing-up forms, day/night |
| v0.5 | Her game | Whatever she asks for after playing v0.4. This is the most important row in the table. |
| v1.0 | Public | Steam page, disclosure, full audio, settings, localization if anyone cares |

---

## 11. Success criteria

The only metric that counts: **does she ask to play it again, unprompted, on Monday?**

Supporting signals worth watching during her first session:

- Does she figure out tap-to-move without being told?
- Does she nickname anything?
- Does a failed catch make her try again, or put the iPad down?
- Does she ask "what else is out there?"
- Does she try to show it to someone?

Anti-goals: session length, number of catches, retention. Do not build toward those.

---

## 12. Risks

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| 1 | **The iOS build pipeline eats the weekend.** Unity iOS module download, Xcode version mismatch, signing errors — this is where first-time Unity/iOS projects reliably die. | High | Fatal to the deadline | **Deploy an empty scene to the iPad before writing a single line of game code.** This is the highest-leverage instruction in this document. |
| 2 | Unity Hub + Editor + iOS Build Support is a multi-GB download | Certain | 1–2 hours | Start it *right now*, in the background, before reading the rest of this |
| 3 | 13 creature sprites take longer than expected | Medium | Cosmetic | Roster floor is 6; ship the rest as they land. Code is roster-agnostic. |
| 4 | AI art comes out stylistically inconsistent across the roster | Medium | Cosmetic | One batch, one style prompt, generated together. Regenerate outliers, don't fix them individually. |
| 5 | Random encounters frustrate her | Medium | Real | Grace period + pity timer + instant "let it go". Visible-creature fallback is a known v0.2 lever. |
| 6 | 7-day profile expiry surprises her mid-week | Certain | Emotional | Tell her Saturday. Rebuild Thursday, or upgrade to the paid account. |
| 7 | Catch mini-game is tuned for an adult and she can't win | Medium | Real | Tune it soft, then watch her play. Err generous — a too-easy v0.1 is fixable, a discouraging one may not get a second session. |
| 8 | Scope creep into battles on Saturday night | High | Fatal to the deadline | Battles are v0.2. Written down here so it's a decision to break, not a drift. |

---

## 13. Open questions

1. **Zone transition** — one continuous map with a seam, or two scenes with a shore path between? (One map is simpler for v0.1.)
2. **Does she keep multiples of a species,** or does the journal track one entry per species with a "seen 4 times" counter? (Recommend: one entry per species in v0.1, real party management in v0.2.)
3. **Title screen or straight into the world?** (Recommend: straight in, with a Continue built in from the start.)
4. **What's her character?** A sprite she picks, or a fixed kid with a bucket? (Fixed for v0.1; a character picker is a great v0.2 surprise.)
5. **Does Gullwing's "last hour of daylight" condition use real device time or in-game time?** (Defer to v0.4 with the day/night system.)

---

*Sources for the constraint and policy claims in §3, §6, and §9 are listed at the end of the delivery message.*
