#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Breachpoint.App;

namespace Breachpoint.EditorTools
{
    // Creates the runtime scene and builds the WebGL player from the command line.
    public static class BuildScript
    {
        const string ScenePath = "Assets/Scenes/Main.unity";

        // Create Main.unity containing only a GameBootstrap object (UI is built in code).
        public static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("Bootstrap");
            go.AddComponent<GameBootstrap>();
            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("[Build] created scene " + ScenePath);
        }

        public static void BuildWebGL()
        {
            if (!File.Exists(ScenePath)) CreateScene();

            PlayerSettings.productName = "BREACHPOINT";
            PlayerSettings.companyName = "Phantom Studios";
            PlayerSettings.runInBackground = true;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.template = "APPLICATION:Default";

            var opts = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "WebGLBuild",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(opts);
            Debug.Log("[Build] WebGL result: " + report.summary.result + " size=" + report.summary.totalSize);
            EditorApplication.Exit(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
        }
    }
}
#endif
