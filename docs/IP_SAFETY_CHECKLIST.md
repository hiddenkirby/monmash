# IP Safety Checklist

Run this before any public store page, trailer, screenshot set, or release-facing page goes
live. Tidepool should use its own coast, jar, journal, Current, and Tideling vocabulary.

## Protected-Term Scan

1. Run:
   ```sh
   scripts/verify-ip-safety-guardrails.sh
   ```
2. Review any hits manually. A hit is only acceptable inside internal guardrail material whose
   purpose is to name the banned term and prevent its use.
3. Check any new store-copy draft, screenshot caption, trailer text, press text, or README
   release section with the same banned-term list before publishing it.

## Store Copy

- Use Tidepool's own fiction: shallow coast, seagrass, glass collecting jar, journal, Currents,
  Tidelings, friendly contests, and growing-up forms.
- Do not use protected franchise names, near-miss names, borrowed catchphrases, or category
  labels that imply affiliation with another creature-collecting property.
- Describe the loop plainly: walk through the shallows, meet small sea creatures, steady the
  jar, and fill the journal.
- Keep copy warm and non-scolding. Do not imply battles, fainting, or permanent loss for v0.1.

## Screenshots And Video

- Verify screenshots do not show protected visual trade dress, copied UI layouts, protected
  iconography, or red/white capture-device imagery.
- Review creature silhouettes against the asset manifest notes before publishing screenshots.
- Avoid screenshots that could imply mechanics Tidepool deliberately excludes: aiming,
  throwing a capture device, live capture odds, riding creatures, or franchise-style battle
  presentation.

## Release Sign-Off

- `Assets/ASSET_MANIFEST.md` has provenance and review status for every visible asset.
- `docs/PRE_RELEASE_CHECKLIST.md` is complete for the candidate build.
- IP scan results and any manual review notes are recorded in the release notes or tag notes.
