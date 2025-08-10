using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class CloudBuildSettings : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        // Only enforce for Android builds
        if (report.summary.platform != BuildTarget.Android) return;

        // Scripting backend: IL2CPP (required for 64-bit on modern Android)
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

        // Architectures: ARMv7 + ARM64 (covers almost every device)
        PlayerSettings.Android.targetArchitectures =
            AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

        // Min/Target API
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24; // Android 7.0
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

        // Build APK (not AAB) so you can install directly
        EditorUserBuildSettings.buildAppBundle = false;

        // Graphics API: OpenGLES3 only (more compatible than Vulkan for now)
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });

        // Strip debug/dev flags
        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.connectProfiler = false;

        Debug.Log("[CloudBuildSettings] Applied Android build settings: IL2CPP + ARM64/ARMv7 + APK + API24 + GLES3");
    }
}
