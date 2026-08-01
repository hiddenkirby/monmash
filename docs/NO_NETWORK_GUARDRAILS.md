# No-Network Guardrails

Tidepool must remain fully offline. Do not add analytics, ads, IAP, accounts, notifications, crash SDKs, telemetry, multiplayer, online services, or any other outbound network behavior.

## Package Review

Before adding or upgrading packages, review `Packages/manifest.json` for SDKs or services that imply network behavior. Reject packages related to:

- Analytics or telemetry
- Ads or attribution
- In-app purchasing
- Accounts, authentication, or identity
- Cloud save, remote config, backend services, or live ops
- Crash reporting
- Push or local notification SDKs
- Multiplayer, relay, lobbies, voice, or netcode

Unity's built-in `com.unity.modules.unitywebrequest` module can exist as part of the engine baseline, but gameplay and editor code must not use it unless a future issue explicitly changes the no-network requirement.

## Code Review

Runtime and editor code must not introduce outbound network APIs such as:

- `UnityWebRequest`
- `System.Net` HTTP or socket clients
- Analytics, ads, purchasing, auth, cloud, crash, notification, or multiplayer service clients

Run this before release and after package changes:

```sh
scripts/verify-no-network-guardrails.sh
```

## Device Verification

Before calling a release candidate ready:

1. Install a fresh build on the iPad.
2. Enable airplane mode.
3. Launch Tidepool.
4. Walk, trigger an encounter, catch or let a Tideling go, open the journal, rename a caught Tideling, force-quit, relaunch, and confirm save data remains.
5. Confirm no sign-in, warning, retry, spinner, network permission, or notification prompt appears.
