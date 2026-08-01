# Unity iOS Pipeline

Follow this before spending time on gameplay.

1. Install Unity 6.3 LTS with iOS Build Support.
2. Install/update Xcode.
3. Open this repo in Unity Hub.
4. Create or open `Assets/Scenes/Boot.unity`.
5. Put one visible sprite or UI label in the scene.
6. Run `Tools -> Tidepool -> Apply iPad iOS Player Settings`, then run
   `Tools -> Tidepool -> Validate iPad iOS Player Settings`.
7. If setting values manually, open Player Settings:
   - Company name: `RKirby`
   - Product name: `Tidepool`
   - Bundle ID: `com.rkirby.tidepool`
   - Minimum iOS version: `15.0`
   - Target device: iPad only
   - Orientation: Landscape Left and Landscape Right only
   - Scripting backend: IL2CPP
   - Architecture: ARM64
8. Switch platform to iOS.
9. Build to a local Xcode project.
10. In Xcode, set Signing & Capabilities to the personal Apple ID team.
11. Run on the iPad by cable.
12. If the app will not launch, trust the developer certificate on the iPad:
    `Settings -> General -> VPN & Device Management`.

Gate: a blank Unity scene must launch on device before implementing more gameplay.

## Verification Notes

- Xcode must report a usable version with `xcodebuild -version`.
- Unity must open the project without script compilation errors.
- `Validate iPad iOS Player Settings` must pass before building.
- Device verification is not complete until the blank scene launches on the target iPad.
