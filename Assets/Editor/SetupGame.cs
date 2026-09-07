using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public static class SetupGame
{
    [MenuItem("60 Second Escape/Setup Game Scene")]
    public static void Setup()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var go = new GameObject("GameBootstrap");
        go.AddComponent<SixtySecondEscape>();
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Game.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true) };
        Debug.Log("60 Second Escape scene created. Press Play to test.");
    }

    [MenuItem("60 Second Escape/Build Android APK")]
    public static void BuildAPK()
    {
        CloudBuildSettings.EnsureScene();
        CloudBuildSettings.ConfigureAndroid();
        string dir = "Builds";
        Directory.CreateDirectory(dir);
        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Game.unity" },
            locationPathName = dir + "/60SecondEscape.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None
        };
        BuildPipeline.BuildPlayer(options);
    }
}
