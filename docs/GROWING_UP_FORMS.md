# Growing-Up Forms

This document scopes the v0.4 reversible growing-up form model. Growing-up forms are optional
presentation states for caught Tidelings, not permanent transformations.

## Save Memory

Each caught Tideling keeps:

- `rememberedGrowthFormIds`: form memories the journal can offer for that Tideling.
- `activeGrowthFormId`: the currently selected memory, or empty for the original form.

The original form is always available and does not need an entry in `rememberedGrowthFormIds`.
Loaded save data normalizes missing lists, duplicate memories, blank memories, and stale active
forms that no longer have a remembered entry.

## Runtime Rules

Use `GameSaveService.RememberGrowthForm` when gameplay unlocks a new memory. Use
`GameSaveService.SelectGrowthForm` when the player chooses a remembered form in the journal.
Use `GameSaveService.SelectOriginalGrowthForm` to return to the original form.

Remembering a form does not automatically replace the original form. The player chooses which
memory is active, and choosing the original form is always allowed.

## Asset And UI Work Still Required

This source-side foundation does not add grown-form art or scene wiring. Before any form ships:

- Add form artwork through Git LFS.
- Log every form asset in `Assets/ASSET_MANIFEST.md`.
- Wire journal UI that makes original and remembered forms viewable.
- Verify Unity compile, Play Mode journal behavior, and iPad readability.

Avoid the words and structure of franchise evolution systems in player-facing copy. Use
Tidepool language such as `growing up`, `memory`, and `remembered form`.
