# Tidepool — Weekend Execution Plan

**Now:** Saturday, August 1, 2026, ~11:45am ET
**Target:** v0.1 running on her iPad by Sunday evening
**Working budget:** ~8 hours Saturday, ~6 hours Sunday

This is aggressive but achievable *only* if the build pipeline is proven before any game code exists. Everything below is ordered around that.

---

## Block 0 — Do this in the next five minutes (11:45am)

Two downloads, both in the background, both before you do anything else:

1. **Unity Hub** → install → sign in → install **Unity 6.5.6 (6000.5.6f1)** with these modules checked:
   - **iOS Build Support** ← the one that matters
   - Visual Studio / your editor of choice
   - *Skip* Android, WebGL, Windows, Linux. They're gigabytes you don't need today.
2. **Xcode** from the Mac App Store (if not already current). This is the bigger download — start it first.

While those run, do §Block 1 by hand.

---

## Block 1 — Art batch, while Unity downloads (11:45am – 12:45pm)

The art is the long pole and it's fully parallel with the install. Generate **all 13 creature sprites in one session** with a shared style prefix so the roster looks like a set.

**Style prefix to reuse verbatim on every prompt:**

> *Children's book illustration of a small friendly sea creature, soft watercolor and ink style, warm daylight, gentle rounded shapes, front three-quarter view, centered, full body, transparent background, no text, no border*

Then append the creature-specific line from the PRD roster table (§4.3). For example:

> *…a tiny hermit crab wearing a smooth grey pebble as a shell, slightly too big for it, curious expression*

**Rules:**

- **Never** include an existing franchise, creature, or artist name in a prompt. Not once. It's the only thing that turns a legally clean project into a messy one.
- Generate all 13 before evaluating any of them. Judge them as a set.
- Regenerate outliers with the same prompt rather than hand-fixing them.
- **Log every prompt** into `assets/ASSET_MANIFEST.md` as you go. Ten minutes now, saves a bad afternoon later.

**Also grab (5 min):** Kenney's CC0 top-down tile pack and UI pack from kenney.nl for the tilemap, buttons, and journal frame. AI is bad at seamless tiles; don't fight it.

**Deliverable:** `assets/creatures/*.png` (13 files, transparent, ~512px) + a manifest.

---

## Block 2 — Prove the pipeline (12:45pm – 2:30pm) ⚠️ THE CRITICAL BLOCK

**Do not write game code until an empty Unity scene is running on her iPad.** This is where first-time Unity/iOS projects die, and finding out at 9pm Sunday is the failure mode this whole plan exists to prevent.

1. New Unity project, **2D (URP)** template, named `Tidepool`.
2. Put a single sprite in the scene. That's the whole "game" for now.
3. `git init`, add Unity's official `.gitignore`, enable Git LFS for `*.png`. **Commit before importing art.**
4. **Player Settings:**
   - Company/Product name set
   - Bundle ID: `com.rkirby.tidepool` (must be globally unique)
   - Target device: iPad (or iPhone+iPad)
   - Orientation: **Landscape Left + Landscape Right only**
   - Target minimum iOS: 15.0
   - Scripting backend: IL2CPP, ARM64
5. `File → Build Settings → iOS → Switch Platform` (this takes a while the first time)
6. **Build** → produces an Xcode project folder
7. Open in Xcode → **Signing & Capabilities** → Team = your personal Apple ID → let it auto-manage signing
8. Plug in the iPad → trust the Mac → select it as the run destination → **Run**
9. On the iPad: **Settings → General → VPN & Device Management → trust your developer certificate.** The app will refuse to launch until you do this, and the error message doesn't tell you that.

**Gate:** a blank Unity scene is on her iPad. If it's 2:30pm and this isn't done, stop everything else and fix it — nothing downstream matters without it.

**Common failures:** bundle ID collision (change it), "no signing certificate" (sign into Xcode → Settings → Accounts), Xcode too old for the Unity version (update Xcode), device not trusted (step 9).

---

## Block 3 — Movement and the world (2:30pm – 5:00pm)

1. **Tilemap:** one scene, one map, two visually distinct regions (pools + seagrass). Paint a small map — roughly 40×25 tiles. Small is fine; she'll explore all of it.
2. Two tilemap layers: `Ground` (walkable) and `Obstacles` (with a Tilemap Collider 2D). A separate `Seagrass` layer, or a tile marker, flags encounter terrain.
3. **BFS pathfinder** over walkable cells — ~80 lines, no packages. Input: start cell + target cell. Output: `List<Vector2Int>`.
4. **Tap-to-move:** raycast the tap to a world position → `WorldToCell` → BFS → hand the path to a walk coroutine that lerps between cell centers at ~4 tiles/sec. Tapping mid-walk re-routes.
5. Camera follow with light smoothing (or Cinemachine if it's already there — don't install it just for this).
6. **Canvas Scaler:** Scale With Screen Size, 1024×768, match 0.5. Anchor UI inside `Screen.safeArea`.

**Gate (5:00pm):** she could walk around the map on the iPad. Deploy and check it on device *now*, not later — touch input behaves differently than mouse.

---

## Block 4 — Data + encounters (5:00pm – 7:00pm)

1. `TidelingSpecies` ScriptableObject per the PRD (§6.3). Create the assets — 13 of them, or as many as have art.
2. A `SpeciesDatabase` ScriptableObject holding the list, so nothing has to search the project at runtime.
3. **Encounter roll** on each step onto a seagrass tile: 12%, plus a 3-step grace counter after any encounter, plus a 25-step pity counter.
4. Weighted rarity pick: Common 60 / Uncommon 32 / Rare 8, filtered to the current zone.
5. On encounter: pause the overworld, load `CatchEncounter` additively.

**Gate:** walking through seagrass pops an empty encounter screen with the right creature's name on it.

---

## Break — dinner, bedtime, be a dad (7:00pm – 8:30pm)

---

## Block 5 — The catch mini-game (8:30pm – 10:30pm)

The heart of v0.1. Budget the most care here.

1. Encounter scene: creature sprite centered, big and appealing. Its name below.
2. **Calm bar:** a horizontal track, a highlighted steady zone, a marker ping-ponging across it.
3. Tap anywhere → check if the marker is inside the zone.
   - **Hit:** fill one of three jar pips. Three pips = caught.
   - **Miss:** marker speed +15%, zone shrinks ~10%. Three misses = it escapes.
4. **Tune it soft.** Start with a zone covering ~35% of the bar for Commons. It should feel almost automatic to you — that's roughly right for a 7-year-old.
5. **"Let it go"** button, always visible, always instant.
6. Caught → a short celebration → journal entry created → return to overworld with the 3-step grace period active.
7. Escaped → "It slipped away!" → back to the overworld. Nothing lost.

**Gate (10:30pm):** she could catch a creature on the iPad. **If you hit this, Sunday is comfortable.** Stop here for the night.

---

## Sunday

### Block 6 — Journal + save (9:00am – 11:30am)

1. **Journal screen:** grid of 13 slots, undiscovered shown as a dark silhouette with a `?`. Derive the silhouette at runtime by tinting the sprite black — don't author 13 more images.
2. Detail page: art, name, Current icon, where and when caught, field note, times seen.
3. **Nickname field**, 12 chars. *Do not cut this.* It's the cheapest emotional payoff in the project.
4. "**7 of 13 found**" progress readout.
5. **Save/load:** `JsonUtility` → `Application.persistentDataPath/save.json`. Include `schemaVersion = 1`. Autosave on catch, on zone change, and in `OnApplicationPause`.
6. **Test the kill case:** catch something, force-quit the app, relaunch. Is it still there? If not, nothing else this weekend matters.

### Block 7 — Polish (11:30am – 2:00pm)

In priority order — take them top-down and stop when you run out of time:

1. Ambient loop, catch chime, escape note, UI tap *(four sounds, that's it)*
2. Verify every touch target is ≥88pt, on device, with her hands in mind
3. First-run: two lines of text — *"Tap to walk. Look in the seagrass."*
4. A title screen with **Continue**
5. Old Barnaby's trigger at 10 species
6. Currents shown as an icon on journal pages (display only)

### Block 8 — Ship it (2:00pm – 3:00pm)

1. Rebuild → Xcode → install on the iPad
2. **Play it yourself for a full ten minutes on the device.** Not in the editor.
3. Set a nice app icon — a glass jar. This is the first thing she sees and it costs five minutes.
4. Delete the dev build and reinstall clean, to verify a first-run experience with no save file
5. Commit and tag `v0.1`

### 3:00pm — Give her the iPad

Say as little as possible. Watch what she does before you explain anything — the first two minutes of a kid playing something cold is the best design feedback that exists, and you only get it once.

Then tell her: *"This one only lasts a week while I'm still building it."* (See PRD §9 — the 7-day profile expiry.)

**Take notes. Don't fix anything today.**

---

## Descope ladder

If you're behind, cut in this exact order. Everything above the line still produces something she'll enjoy.

| Cut # | Drop this | Cost |
|---|---|---|
| 1 | Title screen, Old Barnaby, Current icons | Nothing she'll notice |
| 2 | Audio beyond the catch chime | Minor |
| 3 | Roster from 13 → 8 → 6 | She'll find them faster; still feels like a collection |
| 4 | Second zone; one map, one terrain type | Minor |
| 5 | Journal detail pages — grid + nicknames only | Real loss, still fine |
| 6 | Escape-on-three-misses — make every catch succeed | Loses the challenge, keeps the joy |
| ⛔ | **Never cut:** tap-to-move, encounters, catching, the journal grid, nicknames, save/load | This is the game |

If it's Sunday at 2pm and the catch mini-game isn't working, **make catching a single tap that always succeeds.** A game where she walks around, finds creatures, catches them, names them, and sees them in a journal is a complete and satisfying thing. The timing bar is a refinement, not the product.

---

## Things that will bite you

- **The developer-certificate trust step on the iPad** (Block 2, step 9). The app just won't launch and Xcode won't tell you why.
- **Bundle ID collisions.** Make it unique on the first try.
- **Touch ≠ mouse.** Test on device at every gate, not at the end.
- **`Screen.safeArea`.** The home indicator will swallow a bottom-anchored button and you won't see it in the editor.
- **Git LFS before the first art commit**, not after.
- **Scope creep into battles.** It will feel very tempting around 10pm Saturday. It is a v0.2 feature. It is written down as v0.2 in the PRD precisely so that starting it tonight is a decision you make consciously rather than one you drift into.
