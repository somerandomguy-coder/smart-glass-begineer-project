# Smart Glass Spatial Pin

A minimal Unity XR/XREAL SDK 3.1 starter, targeting Unity 6.6. In the Unity Editor, press **Space** to create a world-space note 1.5 metres in front of the camera. The note remains fixed when the camera moves.

## Local setup

1. Install **Unity Hub** and Unity **6.6 (6000.6.4f1)** with **Android Build Support**, **Android SDK & NDK Tools**, and **OpenJDK** selected.
2. In Unity Hub, add this repository folder as a project and open it with Unity 6.6.
3. Download **XREAL SDK for Unity 3.1.0** from the [official XREAL download page](https://developer.xreal.com/download/), accepting XREAL's terms where prompted. Rename the downloaded tarball to `com.xreal.xr.tgz` if necessary.
4. Place that file at `Packages/ThirdParty/com.xreal.xr.tgz`, then reopen Unity. The manifest already references this local XREAL package. The tarball is deliberately ignored by Git because it is a vendor download governed by XREAL's terms.
5. In **Edit → Project Settings → XR Plug-in Management**, enable the XREAL provider for Android. Add an XR Origin/XR Camera from the SDK's sample or setup flow, tagged `MainCamera`.
6. Open `Assets/Scenes/SpatialPinDemo.unity` and press Play. Press Space to pin a note; a connected XREAL controller trigger also pins a note through Unity XR input.

The project includes Unity's Input System package for Unity 6.6. If Unity asks, allow it to enable the new Input System and restart the editor.

## Android build settings

After importing XREAL SDK 3.1, open **Edit → Project Settings → Player → Android** and confirm:

- Color Space: `Linear`
- Scripting Backend: `IL2CPP`
- Target Architectures: `ARM64` only
- Minimum API Level: Android 10.0 / API 29 or above

Then choose **File → Build Settings**, switch to Android, and select **Build and Run** with an Android host connected through USB debugging.

## Notes

- The project intentionally does not commit the XREAL `.unitypackage`: it is a vendor download governed by XREAL's terms. Importing it creates the SDK assets locally.
- XREAL SDK 3.x uses Unity XR, not the older `NRKernal`/`NRInput` API. `SpatialPinManager` uses the standard XR controller trigger, avoiding a vendor-specific input dependency.
