# Pre-Release Checklist

Use this checklist before tagging a playable build.

## Offline And Privacy

- `scripts/verify-no-network-guardrails.sh` passes.
- `scripts/verify-ip-safety-guardrails.sh` passes.
- No analytics, ads, IAP, accounts, notifications, crash SDKs, telemetry, multiplayer, or online services are present.
- `Packages/manifest.json` contains only Unity engine modules — no third-party SDKs.
- `docs/PRIVACY_RELEASE_NOTES.md` matches the final build and package manifest.
- A clean iPad build works in airplane mode.
- Airplane-mode play covers walking, encounters, catching or letting go, journal open, nickname edit, force-quit, relaunch, and save persistence.
- Platform privacy disclosure answers are copied from the verified privacy notes, not from an earlier draft.

## iPad Build

- `Tools -> Tidepool -> Validate iPad iOS Player Settings` passes in Unity.
- Xcode builds and signs the generated iOS project.
- The app launches on the target iPad after trusting the developer certificate if needed.

## Asset Provenance

- Every shipping or candidate asset is listed in `Assets/ASSET_MANIFEST.md`.
- AI-generated assets include tool, model, date, prompt, and review status.
- Third-party assets include source, license, date, and path.
- `scripts/verify-lfs.sh` passes before committing binary assets.

## IP Safety

- `scripts/verify-ip-safety-guardrails.sh` passes.
- `docs/IP_SAFETY_CHECKLIST.md` has been reviewed for store copy, screenshots, and release-facing text.
- Store copy uses Tidepool's own fiction and vocabulary.
- Screenshots avoid protected visual trade dress, copied UI layouts, and excluded capture/riding mechanics.
