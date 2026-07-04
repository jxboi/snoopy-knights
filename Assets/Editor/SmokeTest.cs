using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SnoopyKnights.EditorTools
{
    /// <summary>
    /// Headless play-mode smoke test for CI/batch use: boots the game, runs a
    /// few minutes of accelerated simulation (through the first enemy wave),
    /// and exits non-zero if any error or exception was logged.
    /// Run with: -batchmode -executeMethod SnoopyKnights.EditorTools.SmokeTest.Run
    ///
    /// Entering play mode reloads the script domain and wipes static state, so
    /// the monitoring side re-arms itself via SessionState + InitializeOnLoad.
    /// </summary>
    public static class SmokeTest
    {
        const string ActiveKey = "SnoopyKnights.SmokeTest.Active";
        const float TargetSimSeconds = 200f; // past the first wave (spawns at 150s)
        const int MaxFrames = 60000;         // hard safety cap
        const float TimeScale = 50f;

        static int frames;
        static Core.Game game;
        static readonly List<string> errors = new List<string>();

        public static void Run()
        {
            SessionState.SetBool(ActiveKey, true);
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            EditorApplication.EnterPlaymode();
        }

        [InitializeOnLoadMethod]
        static void RearmAfterDomainReload()
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;
            Application.logMessageReceived += OnLog;
            EditorApplication.update += Tick;
        }

        static void OnLog(string message, string stack, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error && type != LogType.Assert)
                return;
            // Editor-internal noise (e.g. QuickSearch indexing) isn't a game failure.
            if (stack != null && stack.Contains("UnityEditor."))
                return;
            errors.Add($"{type}: {message}");
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying) return;

            if (frames == 0)
                Time.timeScale = TimeScale;
            frames++;

            if (game == null)
                game = Object.FindFirstObjectByType<Core.Game>();
            bool missionEnded = game != null && game.Mission != null &&
                game.Mission.State != Mission.MissionState.Playing;

            bool done = Time.time >= TargetSimSeconds || missionEnded || frames >= MaxFrames;
            if (!done && errors.Count == 0) return;

            AssertSimulationProgressed();

            string outcome = game == null ? "no game" : game.Mission.State.ToString();
            Debug.Log($"[SmokeTest] Ran {frames} frames, {Time.time:0}s simulated, mission={outcome}, {errors.Count} errors.");
            foreach (var e in errors.GetRange(0, Mathf.Min(5, errors.Count)))
                Debug.Log($"[SmokeTest] {e}");
            SessionState.SetBool(ActiveKey, false);
            EditorApplication.Exit(errors.Count == 0 ? 0 : 1);
        }

        /// <summary>The game must have actually run: booted, spawned units, reached wave 1.</summary>
        static void AssertSimulationProgressed()
        {
            if (game == null)
            {
                errors.Add("Assert: Game never booted.");
                return;
            }
            if (game.TownCenter == null && game.Mission.State == Mission.MissionState.Playing)
                errors.Add("Assert: no Town Center while still playing.");
            if (game.Units.All.Count == 0 && game.Mission.State == Mission.MissionState.Playing)
                errors.Add("Assert: no units alive while still playing.");
            if (game.Waves.WaveNumber < 1)
                errors.Add($"Assert: wave 1 never started ({Time.time:0}s simulated).");
        }
    }
}
