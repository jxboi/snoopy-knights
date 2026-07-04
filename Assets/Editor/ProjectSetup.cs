using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SnoopyKnights.EditorTools
{
    /// <summary>
    /// One-shot project configuration: creates the (empty) main scene, adds it
    /// to build settings, and applies iPhone-first player settings.
    /// Run from the menu or via -executeMethod in batch mode.
    /// </summary>
    public static class ProjectSetup
    {
        const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("Tools/Snoopy Knights/Configure Project")]
        public static void Configure()
        {
            EnsureMainScene();
            ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("[ProjectSetup] Project configured.");
        }

        static void EnsureMainScene()
        {
            if (!System.IO.File.Exists(ScenePath))
            {
                System.IO.Directory.CreateDirectory("Assets/Scenes");
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "JX";
            PlayerSettings.productName = "Snoopy Knights";

            // Landscape only.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.jx.snoopyknights");
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
        }
    }
}
