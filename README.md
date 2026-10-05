# Smart Glass Spatial Pin

A minimal XREAL/NRSDK Unity starter. In the Unity Editor, press **Space** to create a world-space note 1.5 metres in front of the camera. The note remains fixed when the camera moves.

## Local setup

1. Install **Unity Hub** and install **Unity 2022.3 LTS** with **Android Build Support**, **Android SDK & NDK Tools**, and **OpenJDK** selected.
2. In Unity Hub, add this repository folder as a project and open it with Unity 2022.3 LTS.
3. Download `NRSDKForUnity_2.4.1.unitypackage` from the [official XREAL download page](https://developer.xreal.com/download/), accepting XREAL's terms where prompted.
4. In Unity, choose **Assets → Import Package → Custom Package**, select the downloaded package, then import all assets.
5. Open `Assets/Scenes/SpatialPinDemo.unity` and press Play. Hold the right mouse button to look around in the XREAL simulator; press Space to pin a note.

## Android build settings

After importing NRSDK, open **Edit → Project Settings → Player → Android** and confirm:

- Color Space: `Linear`
- Scripting Backend: `IL2CPP`
- Target Architectures: `ARM64` only
- Minimum API Level: Android 10.0 / API 29 or above

Then choose **File → Build Settings**, switch to Android, and select **Build and Run** with an Android host connected through USB debugging.

## Notes

- The project intentionally does not commit the XREAL `.unitypackage`: it is a vendor download governed by XREAL's terms. Importing it creates the SDK assets locally.
- `SpatialPinManager` uses reflection for optional NRSDK controller input, so this repository remains playable in Unity before the SDK package is imported. It uses the XREAL `CameraCenter` and controller trigger automatically when the SDK is present.
