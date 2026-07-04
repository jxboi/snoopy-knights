using System.IO;
using SnoopyKnights.Buildings;
using SnoopyKnights.Core;
using SnoopyKnights.Grid;
using SnoopyKnights.Units;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SnoopyKnights.EditorTools
{
    /// <summary>
    /// Headless visual check: boots the game, builds a small demo settlement
    /// with roads and a few enemies, then renders the camera to a PNG so the
    /// art/layout can be reviewed without a device.
    /// Run: -batchmode -executeMethod SnoopyKnights.EditorTools.ScreenshotTool.Run
    /// Output path via -screenshotOut, default Temp/screenshot.png.
    /// </summary>
    public static class ScreenshotTool
    {
        const string ActiveKey = "SnoopyKnights.Screenshot.Active";
        const int Width = 1280, Height = 720;

        static int frames;
        static bool built;

        public static void Run()
        {
            SessionState.SetBool(ActiveKey, true);
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            EditorApplication.EnterPlaymode();
        }

        [InitializeOnLoadMethod]
        static void Rearm()
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;
            EditorApplication.update += Tick;
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            var game = Game.Instance;
            if (game == null) return;

            if (!built)
            {
                built = true;
                Time.timeScale = 6f;
                BuildDemo(game);
                return;
            }

            frames++;
            if (frames < 120) return; // let workers spawn and start moving

            Capture(game);
            SessionState.SetBool(ActiveKey, false);
            EditorApplication.Exit(0);
        }

        static void BuildDemo(Game game)
        {
            var origin = GameConfig.TownCenterOrigin;

            void Road(int x, int y)
            {
                if (game.Map.InBounds(x, y) && game.Map.Get(x, y).CanPlaceRoad)
                {
                    game.Map.Get(x, y).HasRoad = true;
                    game.Map.NotifyChanged(x, y);
                }
            }

            Place(game, BuildingType.House, origin.x - 4, origin.y);
            Place(game, BuildingType.Storehouse, origin.x + 4, origin.y);
            Place(game, BuildingType.Woodcutter, origin.x - 5, origin.y + 5);
            Place(game, BuildingType.Quarry, origin.x + 5, origin.y + 5);
            Place(game, BuildingType.Farm, origin.x - 1, origin.y + 6);
            Place(game, BuildingType.Barracks, origin.x + 5, origin.y - 4);
            Place(game, BuildingType.Kitchen, origin.x - 4, origin.y + 3);
            Place(game, BuildingType.Watchtower, origin.x + 1, origin.y - 3);

            for (int x = origin.x - 4; x <= origin.x + 5; x++) Road(x, origin.y - 1);
            for (int y = origin.y - 1; y <= origin.y + 5; y++) Road(origin.x - 4, y);

            // A couple of enemies marching in, to show hostiles.
            game.Units.Spawn(UnitType.Raider, GridMap.TileCenter(new Vector2Int(origin.x + 8, origin.y - 3)));
            game.Units.Spawn(UnitType.Brute, GridMap.TileCenter(new Vector2Int(origin.x + 9, origin.y - 2)));

            // A guard right behind the Town Center, to check y-sorted occlusion.
            game.Units.Spawn(UnitType.Guard, GridMap.TileCenter(new Vector2Int(origin.x + 1, origin.y + 3)));

            // Frame the base.
            game.Cam.SetView(new Vector2(origin.x + 1.5f, origin.y + 2f), 8.5f);
        }

        static void Place(Game game, BuildingType type, int x, int y)
        {
            var b = game.Buildings.Place(type, new Vector2Int(x, y), instant: true, free: true);
            if (b == null) Debug.LogWarning($"[Screenshot] Could not place {type} at {x},{y}");
        }

        static void Capture(Game game)
        {
            string outPath = "Temp/screenshot.png";
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-screenshotOut") outPath = args[i + 1];

            var cam = game.Cam.Camera;
            var rt = new RenderTexture(Width, Height, 24);
            var prev = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            tex.Apply();

            cam.targetTexture = prev;
            RenderTexture.active = null;

            File.WriteAllBytes(outPath, tex.EncodeToPNG());
            int fx = Object.FindObjectsByType<Rendering.FadeOutSprite>(FindObjectsSortMode.None).Length;
            Debug.Log($"[Screenshot] Wrote {outPath} ({Width}x{Height}). Live fx sprites: {fx}.");
        }
    }
}
