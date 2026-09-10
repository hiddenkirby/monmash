# v0.8 Landmark Vistas

Issue: #204

Status: source-side concept and layout sheet. Production art, Unity scene wiring, generated
scene output, LFS validation, and iPad review remain pending.

## Shared Layout Contract

Each vista is an authored place the player can read during ordinary movement. It should make
the zone feel larger without changing the existing grid, catch, journal, or save rules.

Every vista uses four visual bands:

| Band | Purpose | Collision rule |
|---|---|---|
| Foreground | Near shells, grass, kelp, foam, or frame elements at the screen edge | Decorative only; never blocks touch targets or the player route |
| Gameplay | Walkable tiles, obstacles, encounter grass, gates, and interactable markers | Source of truth for movement and encounters |
| Midground | Landmark body and readable depth cues | May align with obstacles, but cannot hide their shape |
| Background | Distant coast, water, sky tint, glows, or silhouettes | Decorative only; no gameplay state |

Gameplay readability wins over spectacle. A missing foreground, midground, or background layer
must leave the route traversable and the objective understandable. Landmark art can be absent;
colliders and saved route state cannot depend on it loading.

## Sorting And Camera Rules

- Keep the player, encounter markers, gates, and UI above decorative layers.
- Foreground elements may overlap the bottom edge only where they do not obscure tap targets or
  the next reachable tile.
- Midground landmarks should be visible at the standard overworld camera size without requiring
  panning.
- Background silhouettes should stay far enough from creature proportions that they read as
  environmental shape, not a catchable Tideling.
- Day/night variants reuse the same geometry where possible and change tint, glow, and small
  highlight layers instead of duplicating entire scenes.
- Reduced-motion mode disables continuous sway, parallax, particles, and camera emphasis while
  preserving static landmarks and route-open state.

## Tidepool Shallows

Landmark: tide-carved stone arch framing the wider coast.

| Band | Layout |
|---|---|
| Foreground | Rounded shells and shallow-water ripples along the lower-left screen edge |
| Gameplay | Warm sand path passes cleanly through the arch opening toward the Meadow gate |
| Midground | Soft stone arch with a broad opening; shell glint marker near the base |
| Background | Pale distant coast shape visible through the arch, low turquoise water line |

Day look: warm sand, clear turquoise pools, small white glints.

Night look: cooler pool tint, fewer glints, soft moon edge on the arch.

Missing-art fallback: keep the existing Shallows tiles and use a small sign or marker named
`Landmark_ShallowArch` so discovery and route state can still be verified.

## Seagrass Meadow

Landmark: sweeping grass basin that moves in broad underwater waves.

| Band | Layout |
|---|---|
| Foreground | Sparse grass blades at the bottom edge, short enough to preserve path visibility |
| Gameplay | Seagrass encounter tiles remain visibly distinct from decorative grass bands |
| Midground | Bowl-shaped grass basin with wave bands pointing toward the Kelp route |
| Background | Soft surface glints and distant reed-like shapes |

Day look: blue-green water, brighter surface glints, readable grass edges.

Night look: darker grass bands, low cyan highlights, no hidden obstacle silhouettes.

Missing-art fallback: retain normal seagrass tiles and place a static bent-grass marker named
`Landmark_MeadowWave`.

## Kelp Curtain

Landmark: towering layered kelp gallery with distant lights and silhouettes.

| Band | Layout |
|---|---|
| Foreground | Tall kelp ribbons at far left/right edges that fade near the player path |
| Gameplay | Walkable route remains a clear corridor; blocked route visuals never cover collider truth |
| Midground | Kelp columns forming a gallery and parting near the Rocky Shelf gate |
| Background | Sparse gold/cyan glows and distant soft shapes behind the kelp |

Day look: deep teal water, warm green kelp, restrained highlights.

Night look: stronger distant lights, darker kelp columns, readable corridor edges.

Missing-art fallback: show a simple dark-water corridor and a static glow marker named
`Landmark_KelpLights`.

## Rocky Shelf

Landmark: monumental barnacled old-stone formation that foreshadows Old Barnaby.

| Band | Layout |
|---|---|
| Foreground | Cream foam and small barnacles framing the lower edge |
| Gameplay | Rocky path stays flat, clear, and separated from the Barnaby approach marker |
| Midground | Large friendly stone formation with barnacle clusters and a broad shelf silhouette |
| Background | Wide surf and late-light horizon, with shell resonance cues left to audio profiles |

Day look: slate stone, coral accents, cream foam, warm late-light edges.

Night look: moonlit foam and soft blue stone planes without hard threat shadows.

Missing-art fallback: keep Rocky Shelf tiles and a safe marker named `Landmark_RockyOldStones`.

## Asset List To Produce Later

| Asset family | Suggested paths | Notes |
|---|---|---|
| Shallows arch layers | `Assets/Art/Vistas/Shallows/` | Foreground shells, arch midground, distant coast |
| Meadow wave layers | `Assets/Art/Vistas/Meadow/` | Grass foreground, basin midground, glint overlays |
| Kelp gallery layers | `Assets/Art/Vistas/Kelp/` | Edge ribbons, columns, glow overlays, silhouettes |
| Rocky shelf layers | `Assets/Art/Vistas/Rocky/` | Foam foreground, stone formation, late-light horizon |

All new binary art must use Git LFS and be logged in `Assets/ASSET_MANIFEST.md` with source,
license, date, path, and AI prompt/model/review details where applicable. Do not import these
assets until `git lfs install --local` and `scripts/verify-lfs.sh` can run in this checkout.

## Generator Handoff

When Unity is available, update `Tools/Tidepool/Create v0.1 Overworld Scene` or a v0.8
successor generator so each zone gets:

- Stable GameObject names matching the fallback markers above.
- Separate child roots for foreground, gameplay-adjacent landmark, midground, and background.
- A missing-art fallback object that can be left active when optional sprites are absent.
- Day/night tint hooks that do not replace or rewrite gameplay colliders.
- Reduced-motion toggles for any continuous layer movement added later.

The generated result should be reproducible from source assets and editor code. Avoid manual
scene-only wiring unless the exception is documented next to the scene assembly instructions.

## Verification Still Required

- Unity compile after the generator and assets are added.
- Scene Play Mode checks for every zone route, gate, and camera framing.
- iPad landscape review for safe areas, 88pt UI clearance, touch readability, and performance.
- LFS and asset-manifest checks for all new art.
- IP-safety review for accidental resemblance and protected-expression concerns.
