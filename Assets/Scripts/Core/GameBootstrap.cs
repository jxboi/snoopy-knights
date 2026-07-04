using UnityEngine;

namespace SnoopyKnights.Core
{
    /// <summary>
    /// Entry point. The scene is empty; everything is created from code so the
    /// project needs no hand-authored assets.
    /// </summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Object.FindFirstObjectByType<Game>() != null)
                return;
            new GameObject("Game").AddComponent<Game>();
        }
    }
}
