# Privacy Release Notes

Use these notes when preparing Apple or other platform privacy disclosures for Tidepool.
They describe the intended release posture; verify them against the final build before
submission.

## Intended Privacy Posture

- Tidepool is fully offline.
- The game does not collect, transmit, sell, share, or track player data.
- The game does not include accounts, sign-in, analytics, ads, in-app purchases,
  notifications, crash reporting SDKs, telemetry, multiplayer, cloud save, remote
  config, or online services.
- Save data is local JSON under Unity's `Application.persistentDataPath`.
- Gameplay does not make outbound HTTP, socket, web request, or service SDK calls.

## Current Package Posture

`Packages/manifest.json` contains only Unity engine, 2D, URP, UI, tilemap, audio,
image, JSON, and physics modules. No third-party SDKs, no analytics, no ads, no
crash reporting, no authentication, no cloud services.

Unity includes `com.unity.modules.unitywebrequest` in the engine baseline. Tidepool
must not call `UnityWebRequest` or any other outbound network API unless a future
issue explicitly changes the no-network requirement. Verified: no game or editor
code calls `UnityWebRequest`, `System.Net`, `HttpClient`, or `WebClient`.

## Guardrail Verification Results

Run these before release and after package changes:

```sh
scripts/verify-no-network-guardrails.sh
scripts/verify-ip-safety-guardrails.sh
scripts/verify-lfs.sh
```

Last verified results (current main branch):

| Script | Result |
|---|---|
| verify-no-network-guardrails.sh | No banned network SDK packages or source API usage found |
| verify-ip-safety-guardrails.sh | No protected-franchise terms found in release-facing scan paths |
| verify-lfs.sh | Git LFS is installed, initialized, and tracking Tidepool binary asset patterns |

## Apple Privacy Disclosure Draft

Use this as the starting point for App Store Connect if the final build still matches
the posture above.

| Disclosure area | Draft answer |
|---|---|
| Data collection | No data collected |
| Tracking | No tracking |
| Third-party advertising | Not present |
| Analytics | Not present |
| Developer advertising or marketing | Not present |
| Diagnostics collection | Not present |
| Contact information | Not collected |
| Identifiers | Not collected |
| User content | Not collected |
| Usage data | Not collected |
| Location | Not collected |
| Purchases | Not present |
| Search or browsing history | Not collected |
| Sensitive information | Not collected |

Do not submit this disclosure until the device checklist below has passed on the exact
candidate build.

## Airplane-Mode Device Verification

Before release, install a clean build on the target iPad and verify in airplane mode:

- Fresh launch reaches the playable flow without sign-in, permission, spinner, retry,
  network warning, notification prompt, or external browser flow.
- Player can walk, trigger an encounter, catch or let a Tideling go, open the journal,
  rename a caught Tideling, force-quit, relaunch, and keep save progress.
- No system privacy prompts appear during normal play.
- The result is recorded with date, build identifier, device, tester, and any observed
  exceptions.

Suggested result log format:

```text
Date:
Build/commit:
Device/iPadOS:
Tester:
Airplane-mode flow:
Save persistence:
Unexpected prompts:
Result:
```
