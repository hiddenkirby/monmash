# v0.8 Integration and Release Checklist

Issue: #213

Use this runbook to reproduce and validate **The Great Low Tide** from a clean checkout. The
committed scenes are the normal development baseline; regeneration is a release check and
should happen on a disposable verification branch so scene diffs can be reviewed before they
are kept.

## Current Repository State

The source foundations for the four chapters, persistent expedition save state, route reveals,
ambient Tidelings, authored discoveries, Coast Atlas, Field Station, coast festival, and Old
Barnaby finale are on `main`. The committed Overworld contains the Field Station and the
festival/finale wiring. These facts do not replace the release gates below: target-iPad and
complete new/legacy-save verification are still outstanding.

## Clean Generated Setup

1. Open the project in Unity 6.5.6 and wait for import and compilation to finish with no errors.
2. Run the data generators needed by the scene generators:
   - `Tools/Tidepool/Create Starter Species Assets`
   - `Tools/Tidepool/Create Contest Move Assets`
   - `Tools/Tidepool/Create Story Beat Assets`
   - `Tools/Tidepool/Create Great Low Tide Chapter Assets`
   - `Tools/Tidepool/Create Ambient Zone Profiles`
   - `Tools/Tidepool/Create Ambient Tideling Zone Profiles`
   - `Tools/Tidepool/Create Authored Discovery Sequence Assets`
   - `Tools/Tidepool/Create Route Unlock Sequence Assets`
   - `Tools/Tidepool/Create Coast Atlas Definition`
   - `Tools/Tidepool/Create Field Station Definition`
   - `Tools/Tidepool/Create Coast Festival Definition`
   - `Tools/Tidepool/Create Old Barnaby Finale Definition`
3. Regenerate scenes in dependency order:
   - `Tools/Tidepool/Create v0.8 Boot Scene`
   - `Tools/Tidepool/Create v0.1 Overworld Scene`
   - `Tools/Tidepool/Create v0.1 CatchEncounter Scene`
   - `Tools/Tidepool/Create v0.1 Journal Scene`
   - `Tools/Tidepool/Create v0.2 Contest Scene`
   - `Tools/Tidepool/Create Character Select Scene`
   - `Tools/Tidepool/Create Party Select Scene`
4. Apply the idempotent v0.8 Overworld additions after regenerating the base Overworld:
   - `Tools/Tidepool/Wire v0.8 Field Station Into Overworld`
   - `Tools/Tidepool/Wire v0.8 Festival and Finale Into Overworld`
5. Review every generated asset and scene diff. Do not accept unrelated `ProjectSettings`,
   package, or local playtest-save changes.

The authored discovery definitions are generated, but their world clue sites still require
the placement and presentation review described in `docs/GREAT_LOW_TIDE.md`. A generated asset
alone does not satisfy the discovery issue's scene-play acceptance criteria.

## Unity Verification Menus

Run every relevant source-side verifier under `Tools/Tidepool/` before Play Mode:

- `Verify Expedition Save Foundation`
- `Verify Great Low Tide Opening`
- `Verify Great Low Tide Chapters`
- `Verify Route Unlock Sequences`
- `Verify Authored Discoveries`
- `Verify Ambient Motion Layer`
- `Verify Ambient Tideling Actors`
- `Verify Soundscape Profiles`
- `Verify Coast Atlas Projection`
- `Verify Field Station Projection`
- `Verify Coast Festival Projection`
- `Verify Old Barnaby Finale`
- `Verify Contest Balance`
- `Validate iPad iOS Player Settings`

Record the Unity version, result, and any Console warning/error for each check. A source review
or shell-only check is not a substitute for these menus.

## Save-Journey Matrix

| Save | Required journey |
|---|---|
| Clean new save | Opening through all four chapters, festival, Barnaby encounter, journal payoff, and return flow. |
| Early legacy save | Existing catches survive; the current chapter and first reachable goal backfill without replay storms. |
| Mid legacy save | Open routes, atlas nodes, goals, and Field Station displays reconstruct without duplicate beats. |
| Late legacy save | Festival/finale prerequisites remain reachable and no already-earned decoration is lost. |
| Already complete | Existing Old Barnaby ownership backfills finale/chapter/station state without a duplicate catch. |

At each major route, festival, and finale transition, verify catch or progress write, autosave,
force-quit, relaunch, and reconstruction. Exercise skip, reduced-motion, muted, missing optional
assets, retry, **Let it go**, and ordinary-encounter paths. No branch may remove progress or trap
the player in an additive scene.

## Target-iPad Gate

- Confirm landscape safe areas and readable copy on every changed screen.
- Confirm every core control is at least 88pt and works with touch, not only a mouse.
- Record frame pacing and memory while ambience, Atlas, Field Station, festival, and finale are active.
- Verify mute/volume and reduced motion through scene changes and relaunch.
- Repeat a major-transition force-quit/relaunch on device.
- Complete an airplane-mode journey covering exploration, encounters, catch or **Let it go**,
  journal, festival/contest return, finale return, and save persistence.

## Repository Release Gates

Run from the repository root:

```sh
git diff --check
scripts/verify-no-network-guardrails.sh
scripts/verify-ip-safety-guardrails.sh
scripts/verify-lfs.sh
```

Then verify every shipping asset is represented in `Assets/ASSET_MANIFEST.md`, the privacy
notes still match the built app, and the full `docs/PRE_RELEASE_CHECKLIST.md` is complete.
Record any unrun item as an explicit limitation; do not infer a device result from desktop
Play Mode or a source-side verifier.
