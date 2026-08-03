# Public Release Asset Audit

Audit date: 2026-08-03

Scope: current committed repository contents under `Assets/Art` and `Assets/Audio`. No final
public build exists yet, so this audit covers the current candidate asset set and must be
re-run for any future release build.

## Committed Asset Inventory

| Category | Current contents | Provenance status | Release decision |
|---|---|---|---|
| Creature art | No committed creature sprites. `Assets/Art/Creatures/.gitkeep` only. | No player-facing creature art to disclose or review. | Not present in current build. |
| Tile art | Kenney RPG Base subset under `Assets/Art/Tiles/KenneyRpgBase/`. | Listed in `Assets/ASSET_MANIFEST.md`; CC0 license file committed in the asset folder. | Approved for prototype/public candidate use. |
| UI art | Kenney UI Pack subset under `Assets/Art/UI/KenneyUiPack/`. | Listed in `Assets/ASSET_MANIFEST.md`; CC0 license file committed in the asset folder. | Approved for prototype/public candidate use. |
| Audio | No committed audio files. `Assets/Audio/.gitkeep` only. | No player-facing audio to disclose or review. | Not present in current build. |
| Video/fonts/other media | None found. | No provenance needed for absent assets. | Not present in current build. |

## AI Disclosure Categories

- Current committed build candidate: no AI-generated player-facing content is committed.
- If future creature sprites are AI-generated, disclose player-facing creature art and keep
  tool, model, date, exact prompt, path, and review status in `Assets/ASSET_MANIFEST.md`.
- AI used only for code assistance or internal planning is not a player-facing asset category
  in this audit.

## Unresolved Provenance

No unresolved provenance items were found in the current committed asset set. No assets need to
be replaced or removed for provenance reasons in the current candidate.

## Re-Run Triggers

Re-run this audit before public release whenever any of these change:

- New files are added under `Assets/Art`, `Assets/Audio`, or another runtime media folder.
- Store screenshots, trailer captures, or public page imagery are prepared.
- A Unity-generated build is selected as a public candidate.
- `Assets/ASSET_MANIFEST.md` changes.
