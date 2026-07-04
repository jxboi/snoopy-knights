using System.Collections.Generic;
using SnoopyKnights.Buildings;
using SnoopyKnights.Grid;
using SnoopyKnights.Units;
using UnityEngine;

namespace SnoopyKnights.Rendering
{
    /// <summary>
    /// Loads pixel-art sprites from Resources/Art by name and maps game enums
    /// to them. Everything returns null gracefully if the art is missing, so
    /// the procedural fallback still works. (Art is CC0 Kenney; see CREDITS.)
    /// </summary>
    public static class SpriteBank
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Sprite Load(string path)
        {
            if (cache.TryGetValue(path, out var s)) return s;
            s = Resources.Load<Sprite>("Art/" + path);
            cache[path] = s;
            return s;
        }

        public static Sprite Building(BuildingType t) => Load("buildings/" + t.ToString().ToLowerInvariant());
        public static Sprite Unit(UnitType t) => Load("units/" + t.ToString().ToLowerInvariant());

        public static Sprite GroundGrass(bool alt) => Load(alt ? "tiles/grass2" : "tiles/grass");
        public static Sprite GroundCobble => Load("tiles/cobble");
        public static Sprite GroundWater => Load("tiles/water");
        public static Sprite Road => Load("tiles/dirt");
        public static Sprite Tree => Load("tiles/tree");
        public static Sprite TreeAutumn => Load("tiles/tree_autumn");
        public static Sprite Rock => Load("tiles/rock");
        public static Sprite Bush => Load("tiles/bush");

        /// <summary>A small tool icon that identifies a building at a glance, or null.</summary>
        public static Sprite BuildingIcon(BuildingType t)
        {
            string name = t switch
            {
                BuildingType.Woodcutter => "axe",
                BuildingType.Quarry => "pickaxe",
                BuildingType.Farm => "pitchfork",
                BuildingType.Barracks => "sword",
                BuildingType.Storehouse => "barrel",
                BuildingType.TownCenter => "flag",
                _ => null
            };
            return name == null ? null : Load("icons/" + name);
        }

        public static bool HasArt => Load("tiles/grass") != null;
    }
}
