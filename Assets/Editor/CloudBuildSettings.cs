using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CloudBuildSettings
{
    // Unity Build Automation calls this before exporting the player.
    // The UNITY_CLOUD_BUILD guard keeps the project compiling locally too.
#if UNITY_CLOUD_BUILD
    public static void PreExport()
    {
        EnsureScene();
        ConfigureAndroid();
    }
#endif

    public static void EnsureScene()
    {
        const string scenePath = "Assets/Scenes/Game.unity";
        Directory.CreateDirectory("Assets/Scenes");

        if (!File.Exists(scenePath))
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("GameBootstrap");
            go.AddComponent<SixtySecondEscape>();
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
    }

    public static void ConfigureAndroid()
    {
        PlayerSettings.companyName = "60SecondEscape Studio";
        PlayerSettings.productName = "60 Second Escape";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.applicationIdentifier = "com.sixtysecondescape.game";
        PlayerSettings.defaultIsNativeResolution = true;

        // Landscape is a good fit for the current prototype.
        PlayerSettings.defaultScreenOrientation = ScreenOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;

        // Keep the build portable; Unity's Android tooling selects the appropriate
        // target SDK installed on the cloud builder.
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = false;
    }
}
