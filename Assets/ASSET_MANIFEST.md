# Tidepool Asset Manifest

Every non-code asset that ships with the game belongs here before it is committed.

## Current Committed Asset Inventory

As of 2026-08-03, the repository includes a small CC0 prototype tile/UI subset, a small original procedural audio set, and one original app icon. No creature art, video, font, or other runtime media assets are committed.

| Asset | Type | Source | License | Tool/Model | Date | Prompt or Notes | Reviewed |
|---|---|---|---|---|---|---|---|
| `Assets/Art/Tiles/KenneyRpgBase/*` | Tile and obstacle sprites | Kenney RPG Base, https://kenney.nl/assets/rpg-base | Creative Commons CC0, license file included at `Assets/Art/Tiles/KenneyRpgBase/License.txt` | N/A | 2026-08-02 | Curated subset for grass/seagrass-like terrain, sand, shallow water edges, shrubs, fence, and crate obstacles. | Yes - reviewed for protected visual trade dress; selected neutral environment props only. |
| `Assets/Art/UI/KenneyUiPack/*` | UI sprites | Kenney UI Pack, https://kenney.nl/assets/ui-pack | Creative Commons CC0, license file included at `Assets/Art/UI/KenneyUiPack/License.txt` | N/A | 2026-08-02 | Curated green controls for buttons, square icon buttons, calm-bar slider parts, input field, and divider. | Yes - reviewed for protected visual trade dress; no red/white capture sphere or genre-specific copied UI. |
| `Assets/Audio/ambient_loop.wav` | Ambient audio loop | Original procedural synthesis generated in-repo | Original Tidepool project asset | `scripts/generate-minimal-audio-assets.py`, Python `wave`/`math` synthesis | 2026-08-03 | Eight-second soft periodic water-tone bed intended to loop cleanly; no external samples. | Yes - reviewed for absence of recognizable protected motifs or borrowed commercial sounds. |
| `Assets/Audio/catch_chime.wav` | Catch chime | Original procedural synthesis generated in-repo | Original Tidepool project asset | `scripts/generate-minimal-audio-assets.py`, Python `wave`/`math` synthesis | 2026-08-03 | Short ascending bell-like triad for successful catches; no external samples. | Yes - reviewed for absence of recognizable protected motifs or borrowed commercial sounds. |
| `Assets/Audio/escape_note.wav` | Escape note | Original procedural synthesis generated in-repo | Original Tidepool project asset | `scripts/generate-minimal-audio-assets.py`, Python `wave`/`math` synthesis | 2026-08-03 | Short warm descending tone for the friendly `It slipped away!` outcome; no external samples. | Yes - reviewed for absence of recognizable protected motifs or borrowed commercial sounds. |
| `Assets/Audio/ui_tap.wav` | UI tap sound | Original procedural synthesis generated in-repo | Original Tidepool project asset | `scripts/generate-minimal-audio-assets.py`, Python `wave`/`math` synthesis | 2026-08-03 | Quiet percussive UI tap; no external samples. | Yes - reviewed for absence of recognizable protected motifs or borrowed commercial sounds. |
| `Assets/Art/UI/app_icon_glass_jar.png` | App icon | Original procedural drawing generated in-repo | Original Tidepool project asset | `scripts/generate-app-icon.py`, Python PNG drawing | 2026-08-03 | Glass collecting jar concept on a calm teal background; no external image input. | Yes - reviewed for protected visual trade dress; no red/white sphere, copied character, or protected UI iconography. |

## Shared Creature Style Prompt

Children's book illustration of a small friendly sea creature, soft watercolor and ink style, warm daylight, gentle rounded shapes, front three-quarter view, centered, full body, transparent background, no text, no border.

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
- Future AI prompts must not include protected franchise names, existing character names, living artist names, or near-miss references. Log exact prompts in the table above when assets are added.
