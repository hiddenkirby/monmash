# Unity iOS Pipeline

Follow this before spending time on gameplay.

1. Install Unity 6.3 LTS with iOS Build Support.
2. Install/update Xcode.
3. Open this repo in Unity Hub.
4. Create or open `Assets/Scenes/Boot.unity`.
5. Put one visible sprite or UI label in the scene.
6. Open Player Settings:
   - Company name: `RKirby`
   - Product name: `Tidepool`
   - Bundle ID: `com.rkirby.tidepool`
   - Minimum iOS version: `15.0`
   - Orientation: Landscape Left and Landscape Right only
   - Scripting backend: IL2CPP
   - Architecture: ARM64
7. Switch platform to iOS.
8. Build to a local Xcode project.
9. In Xcode, set Signing & Capabilities to the personal Apple ID team.
10. Run on the iPad by cable.
11. If the app will not launch, trust the developer certificate on the iPad:
    `Settings -> General -> VPN & Device Management`.

Gate: a blank Unity scene must launch on device before implementing more gameplay.

