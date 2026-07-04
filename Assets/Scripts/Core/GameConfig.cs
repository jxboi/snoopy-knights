using UnityEngine;

namespace SnoopyKnights.Core
{
    /// <summary>Mission tuning values in one place.</summary>
    public static class GameConfig
    {
        public const int StartWood = 45;
        public const int StartStone = 30;
        public const int StartFood = 25;
        public const int StartGold = 30;

        public const int RoadStoneCost = 1;

        /// <summary>Bottom-left tile of the 3x3 Town Center footprint.</summary>
        public static readonly Vector2Int TownCenterOrigin = new Vector2Int(16, 5);
    }
}
