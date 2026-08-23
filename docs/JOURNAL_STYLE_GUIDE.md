# Journal Style Guide

Issue: #118

This guide defines the v0.5 journal visual language before the journal screen is rebuilt.
It keeps the collection screen warm, readable, and clearly distinct from protected creature
collection UI trade dress.

## Design Goals

- Make the journal feel like a field notebook from a sunny tidepool walk.
- Keep every core state readable at a glance for a 7-year-old.
- Preserve the v0.1 reward loop: finding, naming, and revisiting Tidelings.
- Avoid color-only meaning by pairing rarity color with text and, where practical, an icon.
- Keep touch targets at least 88pt and anchor all controls inside the iPad safe area.

## Color Palette

| Role | Hex | Use |
|---|---:|---|
| Tidepaper | `#F7EBCB` | Main journal background, warm paper tone |
| Shell Panel | `#FFF7E4` | Detail panel and card interiors |
| Driftwood Ink | `#2F3A35` | Primary text |
| Soft Kelp | `#3E6F5A` | Primary accents, selected tabs, success fills |
| Tide Blue | `#3B82A0` | Links, interactive focus, Current accents |
| Coral Pink | `#D97972` | Warm callouts and Coral accents |
| Glow Amber | `#E7A83A` | Highlights, progress shine, Secret trim support |
| Stone Gray | `#6F756E` | Disabled states, silhouette frames |
| Shallow Wash | `#D8EEF0` | Subtle section backgrounds |
| Deep Shadow | `#1E2826` | High-contrast silhouettes and modal scrims |

Use Tidepaper as the dominant surface, Shell Panel for contained information, and no large
single-color gradients. Interactive controls should use Soft Kelp or Tide Blue with Driftwood
Ink text unless a rarity state is the primary message.

## Rarity Colors

| Rarity | Hex | Secondary cue | Journal use |
|---|---:|---|---|
| Common | `#4F9D69` | Small leaf/seagrass mark | Slot trim and rarity chip |
| Uncommon | `#3F8FB5` | Ripple mark | Slot trim and rarity chip |
| Rare | `#8661B4` | Starfish mark | Slot trim and rarity chip |
| Secret | `#D8A536` | Shell mark | Slot trim, discovery flare, progress milestone |

Rarity color never stands alone. Slot labels, detail rows, or tooltips must spell out the
rarity name so the state remains accessible without color perception.

## Typography

Use the default Unity UI font until a project font is selected. The hierarchy below assumes
the existing Canvas Scaler reference resolution of `1024 x 768` with match `0.5`.

| Text role | Size | Weight | Use |
|---|---:|---|---|
| Screen title | 40pt | Bold | `Journal`, detail creature name |
| Section header | 28pt | Bold | Identity, Habitat, Stats, Field Notes |
| Body | 22pt | Regular | Field notes, habitat text, caught date |
| Caption | 16pt | Regular | Seen count, rarity chip, helper copy |

Keep line length short in the detail panel. Field notes should wrap at roughly 48 characters
per line on the reference layout.

## Layout

The journal has two primary regions:

| Region | Reference size | Behavior |
|---|---:|---|
| Collection grid | 520 x 560 | Scrolls only if future rosters exceed the visible grid |
| Detail panel | 420 x 560 | Shows the selected caught Tideling or an undiscovered hint |

On narrower layouts, stack the grid above the detail panel rather than shrinking touch targets.
The found-count progress bar belongs above the grid and should stay visible while browsing.

## Slot Card Spec

| Property | Value |
|---|---|
| Reference size | 144 x 164 |
| Corner radius | 8px equivalent |
| Border | 4px rarity trim for discovered slots, 3px Stone Gray for undiscovered slots |
| Interior | Shell Panel with a subtle Shallow Wash lower band for captions |
| Touch target | Full card, minimum 88pt in both dimensions |
| Image area | 112 x 104, centered |
| Text area | 128 x 36, bottom aligned |

### Slot States

| State | Visual treatment | Text |
|---|---|---|
| Caught | Full-color sprite, rarity trim, display name, optional nickname indicator | Tideling name or nickname |
| Seen but uncaught | Dimmed sprite or silhouette, dashed rarity trim if known | `Seen nearby` |
| Undiscovered | Deep Shadow silhouette, `?`, Stone Gray border | `?` |
| Selected | 4px Soft Kelp outer focus ring outside the rarity trim | Same as base state |

If a sprite is missing during prototype work, show a Shell Panel placeholder with the Tideling
name and do not crash the journal.

## Detail Panel Sections

The detail panel uses grouped sections. Each section has a 28pt header, 12px vertical gap, and
one or more 22pt body rows.

| Section | Contents |
|---|---|
| Identity | Large sprite, display name, nickname field, Current, rarity |
| Habitat | Found zone, habitat zones, caught date |
| Stats | Times seen, level or growth state when available, contest move summary when available |
| Field Notes | One warm paragraph from the species data |

Nickname editing remains a first-class action. Keep its input row large, obvious, and inside
the safe area. Do not hide it behind a long-press or nested modal.

## Progress Bar

The progress bar reads `N of 13 found` and uses a shell shape:

- Overall reference size: 420 x 36.
- Track: rounded shell silhouette in Stone Gray at 30% opacity.
- Fill: Soft Kelp base with a Glow Amber leading highlight.
- Segment marks: 13 subtle shell ridges, one per Tideling.
- Fill animation: 0.35 seconds ease-out after a new species is caught.
- Milestones: at 10 found, briefly pulse the Secret gold color to support Old Barnaby.

The text label is always visible above or centered in the bar. Do not rely on fill length
alone to communicate progress.

## Copy And Tone

- Use warm field-notebook language: `Found in the meadow`, `Seen 3 times`.
- Avoid scolding, failure language, scores, ratings, stars, timers, or streaks.
- Keep undiscovered hints gentle: `Something lives here` is better than `Locked`.
- Do not use protected franchise terms, near-miss terms, or copied UI naming patterns.

## Implementation Notes

- Keep new UI prefabs scene-safe and safe-area aware.
- Use rarity color values from this guide in code or serialized data rather than inventing
  near-duplicates per component.
- Pair icons/text with color for Current and rarity states.
- Verify final UI in Unity Play Mode and on iPad before closing scene-assembly or polish work.
