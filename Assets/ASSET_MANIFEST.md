# Tidepool Asset Manifest

Every non-code asset that ships with the game belongs here before it is committed.

## Current Committed Asset Inventory

As of 2026-08-03, the repository includes a small CC0 prototype tile/UI subset, a small original procedural audio set, one original app icon, and 13 AI-generated creature sprites. No video, font, or other runtime media assets are committed.

| Asset | Type | Source | License | Tool/Model | Date | Prompt or Notes | Reviewed |
|---|---|---|---|---|---|---|---|
| `Assets/Art/Tiles/KenneyRpgBase/*` | Tile and obstacle sprites | Kenney RPG Base, https://kenney.nl/assets/rpg-base | Creative Commons CC0, license file included at `Assets/Art/Tiles/KenneyRpgBase/License.txt` | N/A | 2026-08-02 | Curated subset for grass/seagrass-like terrain, sand, shallow water edges, shrubs, fence, and crate obstacles. | Yes - reviewed for protected visual trade dress; selected neutral environment props only. |
| `Assets/Art/UI/KenneyUiPack/*` | UI sprites | Kenney UI Pack, https://kenney.nl/assets/ui-pack | Creative Commons CC0, license file included at `Assets/Art/UI/KenneyUiPack/License.txt` | N/A | 2026-08-02 | Curated green controls for buttons, square icon buttons, calm-bar slider parts, input field, and divider. | Yes - reviewed for protected visual trade dress; no red/white capture sphere or genre-specific copied UI. |
| `Assets/Audio/ambient_loop.wav` | Ambient audio loop | Original procedural synthesis generated in-repo | Original Tidepool project asset | `scripts/generate-minimal-audio-assets.py`, Python `wave`/`math` synthesis | 2026-08-03 | Eight-second soft periodic water-tone bed intended to loop cleanly; no external samples. | Yes - reviewed for absence of recognizable protected motifs or borrowed commercial sounds. |
| `Assets/Audio/catch_chime.wav` | Catch chime | Original procedural synthesis generated in-repo | Original Tidepool project asset | `scripts/generate-minimal-audio-assets.py`, Python `wave`/`math` synthesis | 2026-08-03 | Short ascending bell-like triad for successful catches; no external samples. | Yes - reviewed for absence of recognizable protected motifs or borrowed commercial sounds. |
| `Assets/Audio/escape_note.wav` | Escape note | Original procedural synthesis generated in-repo | Original Tidepool project asset | `scripts/generate-minimal-audio-assets.py`, Python `wave`/`math` synthesis | 2026-08-03 | Short warm descending tone for the friendly `It slipped away!` outcome; no external samples. | Yes - reviewed for absence of recognizable protected motifs or borrowed commercial sounds. |
| `Assets/Audio/ui_tap.wav` | UI tap sound | Original procedural synthesis generated in-repo | Original Tidepool project asset | `scripts/generate-minimal-audio-assets.py`, Python `wave`/`math` synthesis | 2026-08-03 | Quiet percussive UI tap; no external samples. | Yes - reviewed for absence of recognizable protected motifs or borrowed commercial sounds. |
| `Assets/Art/UI/app_icon_glass_jar.png` | App icon | Original procedural drawing generated in-repo | Original Tidepool project asset | `scripts/generate-app-icon.py`, Python PNG drawing | 2026-08-03 | Glass collecting jar concept on a calm teal background; no external image input. | Yes - reviewed for protected visual trade dress; no red/white sphere, copied character, or protected UI iconography. |
| `Assets/Art/Creatures/*.png` | Creature sprites | AI-generated in Codex with local chroma-key background removal | Tidepool project AI-generated asset | OpenAI built-in image generation tool, local `remove_chroma_key.py`, Pillow resize | 2026-08-03 | 13 species-specific prompts logged below. Final PNGs are 512x512 transparent sprites. | Yes - reviewed as a set for accidental resemblance, protected trade dress, prompt compliance, and usable framing. |

## Shared Creature Style Prompt

Children's book illustration of a small friendly sea creature, soft watercolor and ink style, warm daylight, gentle rounded shapes, front three-quarter view, centered, full body, transparent background, no text, no border.

## Creature Sprite Prompt Log

All 13 creature sprites used the built-in OpenAI image generation tool. The generated chroma-key source images were processed with `/Users/rkirby/.codex/skills/.system/imagegen/scripts/remove_chroma_key.py`, then resized to 512x512 PNGs with alpha.

Shared prompt wrapper for Blip, Nubbin, Frillick, Sputter, Wobbet, Clackaw, Sweepfin, Mossback, Lumen, Gullwing, Tanglemaw, and Old Barnaby:

```text
Use case: illustration-story
Asset type: Tidepool game creature sprite
Primary request: Children's book illustration of a small friendly sea creature, soft watercolor and ink style, warm daylight, gentle rounded shapes, front three-quarter view, centered, full body. [species-specific request]
Scene/backdrop: perfectly flat solid #ff00ff chroma-key background for background removal.
Composition/framing: single creature only, centered, full body, generous padding, creature fills about 80% of the frame.
Lighting/mood: warm daylight, calm and friendly. [optional species-specific mood detail]
Constraints: no text, no border, no watermark, no cast shadow, no contact shadow, no reflection. Background must be one uniform #ff00ff color with no gradients, shadows, texture, floor plane, or lighting variation. Do not use #ff00ff anywhere in the creature. Avoid protected franchise references, existing character resemblance, red/white capture-device imagery, and living artist styles.
```

Shared prompt wrapper for Thistlecoat:

```text
Use case: illustration-story
Asset type: Tidepool game creature sprite
Primary request: Children's book illustration of a small friendly sea creature, soft watercolor and ink style, warm daylight, gentle rounded shapes, front three-quarter view, centered, full body. A tiny friendly purple-and-coral urchin whose spines lie flat when it trusts you, rounded body with soft flattened spines, gentle expression.
Scene/backdrop: perfectly flat solid #00ff00 chroma-key background for background removal.
Composition/framing: single creature only, centered, full body, generous padding, creature fills about 80% of the frame.
Lighting/mood: warm daylight, calm and friendly.
Constraints: no text, no border, no watermark, no cast shadow, no contact shadow, no reflection. Background must be one uniform #00ff00 color with no gradients, shadows, texture, floor plane, or lighting variation. Do not use #00ff00 anywhere in the creature. Avoid protected franchise references, existing character resemblance, red/white capture-device imagery, and living artist styles.
```

Species-specific request lines:

| Asset | Species-specific request |
|---|---|
| `Assets/Art/Creatures/blip.png` | A thumb-sized darting fish, always in a hurry, curious and friendly. |
| `Assets/Art/Creatures/nubbin.png` | A tiny hermit crab wearing a smooth grey pebble as a shell, the pebble slightly too big for it, curious expression. |
| `Assets/Art/Creatures/frillick.png` | A ruffled sea slug that moves like a dropped ribbon, friendly and gentle. |
| `Assets/Art/Creatures/sputter.png` | A tiny cluster of blinking plankton that travels as one, many small glowing dots forming a friendly little group shape. |
| `Assets/Art/Creatures/wobbet.png` | A small round jelly creature that drifts and bumps into things, translucent blue body, friendly expression. |
| `Assets/Art/Creatures/clackaw.png` | A pistol shrimp with one enormous snapping claw, friendly and non-threatening, small body with one oversized claw. |
| `Assets/Art/Creatures/sweepfin.png` | A palm-sized ray that glides just under the surface, soft diamond body, gentle smile, friendly and graceful. |
| `Assets/Art/Creatures/mossback.png` | A tiny turtle with a little garden of moss and small sea plants growing on its shell, friendly and calm. |
| `Assets/Art/Creatures/lumen.png` | A shy lanternfish with a small warm glowing light that dims when it feels shy, friendly expression. |
| `Assets/Art/Creatures/thistlecoat.png` | A tiny friendly purple-and-coral urchin whose spines lie flat when it trusts you, rounded body with soft flattened spines, gentle expression. |
| `Assets/Art/Creatures/gullwing.png` | A small flying fish with wing-like fins, friendly and graceful, posed as if gliding just above shallow water but without any water background. |
| `Assets/Art/Creatures/tanglemaw.png` | A small curious octopus with soft rounded arms, gently unlatching a glass jar lid with one arm, friendly and clever, no danger. |
| `Assets/Art/Creatures/old-barnaby.png` | An ancient barnacled tidepool creature shape, slow and kind, stone-like body with small barnacles and gentle eyes, mysterious but warm. |

Prompt-specific wrapper adjustments:

| Asset | Exact adjustment |
|---|---|
| `Assets/Art/Creatures/sputter.png` | Lighting/mood used: `warm daylight, calm and friendly, gentle glow from the plankton.` Composition/framing used: `single grouped creature only, centered, full body, generous padding, creature fills about 80% of the frame.` |
| `Assets/Art/Creatures/lumen.png` | Lighting/mood used: `warm daylight, calm and friendly, gentle warm glow from the lantern.` |
| `Assets/Art/Creatures/tanglemaw.png` | Composition/framing used: `single creature only with a tiny simple glass jar lid prop, centered, full body, generous padding, creature fills about 80% of the frame.` |
| `Assets/Art/Creatures/old-barnaby.png` | Lighting/mood used: `warm daylight, calm, friendly, quietly mysterious.` |

## Planned Asset Rows

Add rows here before committing any new shippable non-code asset.

| Asset | Type | Required provenance |
|---|---|---|
| `Assets/Art/Creatures/*.png` | Creature sprites | Tool, model, generation date, exact prompt, path, and review status for accidental resemblance. |
| `Assets/Art/Tiles/*` | Tiles | Source URL or package name, license, acquisition date, imported paths, and review status for protected visual trade dress. |
| `Assets/Art/UI/*` | UI art | Source URL or package name, license, acquisition date, imported paths, and review status for protected visual trade dress. |
| `Assets/Audio/*` | Audio | Source URL or package name, license, acquisition date, imported paths, and review status for recognizable protected motifs or borrowed commercial sounds. |

## IP Review Notes

- 2026-08-01: Reviewed committed repository contents for shippable non-code assets. No creature art, tile art, UI art, audio, video, fonts, or other runtime media assets are currently present, so there are no copied designs or protected trade dress to compare.
- 2026-08-01: Reviewed manifest and project documentation for banned protected-franchise terms or near-miss names intended for release UI/store copy. No release-facing asset prompts or store copy with banned terms or near-miss names are present in this manifest.
- 2026-08-02: Reviewed imported Kenney tile/UI subset. Assets are generic CC0 environmental tiles, foliage/wood obstacles, and green UI controls; no protected-franchise character art, capture-device trade dress, copied battle UI layout, or protected iconography was introduced.
- 2026-08-03: Ran the current public-release asset audit in `docs/PUBLIC_RELEASE_ASSET_AUDIT.md`. The committed candidate still contains only the Kenney CC0 tile/UI subsets plus placeholder `.gitkeep` files for creatures/audio; no AI-generated player-facing assets, audio, video, fonts, or unresolved provenance items are committed.
- 2026-08-03: Reviewed original procedural audio set. The files are generated from simple sine-wave synthesis with no external samples, no protected melodies, no borrowed commercial sounds, and no recognizable protected motifs.
- 2026-08-03: Reviewed original app icon. The icon depicts a glass collecting jar with water on a plain teal background and does not use protected capture-device trade dress or copied character imagery.
- 2026-08-03: Reviewed the generated creature sprite set as 512x512 transparent PNGs. The set uses original tidepool creature concepts, consistent children's-book watercolor styling, no text, no protected capture-device trade dress, and no obvious silhouette/recolor match to protected character designs. `Thistlecoat` was regenerated on a green chroma-key background after the first magenta-key matte was rejected for poor edge extraction.
- Future AI prompts must not include protected franchise names, existing character names, living artist names, or near-miss references. Log exact prompts in the table above when assets are added.
