using UnityEngine;

namespace SnoopyKnights.Core
{
    /// <summary>
    /// Session game speed (1x / 2x fast-forward). Pausing and the end screen
    /// still zero Time.timeScale directly; everything that unfreezes goes
    /// through Apply() so the chosen speed survives a pause.
    /// </summary>
    public static class GameSpeed
    {
        public static int Multiplier { get; private set; } = 1;

        public static void Toggle()
        {
            Multiplier = Multiplier == 1 ? 2 : 1;
            if (Time.timeScale > 0f) // never unfreeze a pause/end screen
                Apply();
        }

        public static void Apply() => Time.timeScale = Multiplier;

        /// <summary>Scene (re)load: back to 1x, unfrozen.</summary>
        public static void Reset()
        {
            Multiplier = 1;
            Apply();
        }
    }
}
