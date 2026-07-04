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

        /// <summary>
        /// Reimports Resources/Art so the ArtImporter's sprite settings apply,
        /// then verifies a representative sprite actually loads. Safe to run in
        /// batch mode; used to guarantee art is ready before play/build.
        /// </summary>
        [MenuItem("Tools/Snoopy Knights/Reimport Art")]
        public static void ReimportArt()
        {
            const string folder = "Assets/Resources/Art";
            if (AssetDatabase.IsValidFolder(folder))
                AssetDatabase.ImportAsset(folder, ImportAssetOptions.ImportRecursive);
            AssetDatabase.Refresh();

            int loaded = 0, total = 0;
            foreach (var rel in new[] { "tiles/grass", "buildings/towncenter", "units/guard" })
            {
                total++;
                if (Resources.Load<Sprite>("Art/" + rel) != null) loaded++;
                else Debug.LogWarning($"[Art] Missing sprite: Art/{rel}");
            }
            Debug.Log($"[Art] Verified {loaded}/{total} representative sprites load as Sprite.");
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
