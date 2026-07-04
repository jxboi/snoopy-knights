using UnityEngine;
using UnityEngine.SceneManagement;

namespace SnoopyKnights.Core
{
    /// <summary>
    /// Entry point. The scene is empty; everything is created from code so the
    /// project needs no hand-authored assets. Re-runs on scene (re)load so the
    /// restart button works.
    /// </summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            CreateGame();
            SceneManager.sceneLoaded += (_, _) => CreateGame();
        }

        static void CreateGame()
        {
            if (Object.FindFirstObjectByType<Game>() != null)
                return;
            new GameObject("Game").AddComponent<Game>();
        }
    }
}
