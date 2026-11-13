# Android Build (APK)

> **Languages:** English | [Português (BR)](BUILD_ANDROID_pt.md)

Guide to build an APK of **meta-class-alpha** for testing on a Google Cardboard device.

## Current project configuration

| Item | Value |
|------|-------|
| Unity | 6000.3.6f1 (Unity 6.3) |
| Build scene | `Assets/Scenes/SolarSystem.unity` |
| Package Name | `com.EbonLabs.metaclassalpha` |
| Version | 0.1 (versionCode: 1) |
| Min API Level | 25 (Android 7.1) |
| Scripting Backend | IL2CPP |
| Architectures | ARMv7 + ARM64 |
| XR | Google Cardboard (Android) |
| Keystore | Not configured (uses debug keystore for test builds) |

## Prerequisites

In **Unity Hub**, install editor **6000.3.6f1** with the module:

- **Android Build Support**
  - Android SDK & NDK Tools
  - OpenJDK

If anything is missing, Unity usually prompts to install it when switching the platform to Android.

## Build APK from the Editor

1. Open the project in Unity.
2. Go to **File → Build Profiles** (Unity 6) or **File → Build Settings**.
3. Select **Android** and click **Switch Platform** (wait for reimport).
4. Confirm that `Assets/Scenes/SolarSystem.unity` is included in the build.
5. Review **Edit → Project Settings → Player → Android**:
   - **Package Name**: `com.EbonLabs.metaclassalpha`
   - **Minimum API Level**: 25
   - **Scripting Backend**: IL2CPP
   - **Target Architectures**: ARMv7 and ARM64
6. To generate an **APK** (not AAB):
   - In the build profile, uncheck **Build App Bundle (Google Play)** if enabled.
7. Click **Build** (or **Build And Run** with the phone connected via USB).

The APK will be saved to the path you choose, for example:

```
Builds/meta-class-alpha.apk
```

## App signing

The project has no release keystore configured. For local testing, Unity automatically uses the **debug keystore**.

To publish on Google Play Store:

1. Open **Project Settings → Player → Android → Publishing Settings**.
2. Create or select a **keystore**.
3. Set the **alias** and password.
4. Store the `.keystore` file securely — without it you cannot publish updates for the same app.

## Command-line build (optional)

Requires a build script in `Assets/Editor/`. Example invocation:

```bash
/path/to/Unity \
  -quit -batchmode -nographics \
  -projectPath "/path/to/meta-class-alpha" \
  -buildTarget Android \
  -executeMethod AndroidBuild.BuildApk
```

Replace `/path/to/Unity` with the actual Unity 6.3 executable path on your system.

## Test on device

1. Enable **Developer options** and **USB debugging** on the device.
2. Connect the phone to the computer or transfer the APK manually.
3. Install and open the app with Google Cardboard attached to the phone.

> Cardboard must be tested on a physical device. VR mode does not work fully in the Editor.

## Common issues

| Issue | Solution |
|-------|----------|
| SDK or NDK not found | Unity Hub → Installs → gear icon → Add Modules |
| Build outputs `.aab` instead of `.apk` | Disable **Build App Bundle** in the build profile |
| App won't install on phone | Enable USB debugging and accept the authorization prompt |
| Cardboard not responding | Test on a physical device; calibrate the headset through the app |
