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
        static readonly Dictionary<string, Sprite[]> clipCache = new Dictionary<string, Sprite[]>();

        public static Sprite Load(string path)
        {
            if (cache.TryGetValue(path, out var s)) return s;
            s = Resources.Load<Sprite>("Art/" + path);
            cache[path] = s;
            return s;
        }

        public static Sprite Building(BuildingType t) => Load("buildings/" + t.ToString().ToLowerInvariant());
        public static Sprite Unit(UnitType t) => Load("units/" + t.ToString().ToLowerInvariant());

        // ---- Animated unit clips --------------------------------------------
        //
        // A clip lives at Resources/Art/units/<folder>/<action>.png, where the
        // folder is an art name for the unit type (see UnitArtFolder) and the
        // png is either a horizontal strip (auto-sliced by ArtImporter) or a set
        // of numbered frames <action>_0.png, <action>_1.png ... Both are loaded
        // transparently here. Returns null when no such art is present, which is
        // what keeps the procedural/static fallback working.

        /// <summary>Maps a game unit type to the art folder that holds its clips.</summary>
        public static string UnitArtFolder(UnitType t) => t switch
        {
            UnitType.Guard => "warrior",
            UnitType.Archer => "archer",
            UnitType.Raider => "goblin",
            UnitType.Brute => "orc",
            _ => "worker" // Builder, Carrier, Farmer share the villager art
        };

        /// <summary>Frames for one animation clip (idle/walk/work/attack/die), or null.</summary>
        public static Sprite[] Clip(UnitType t, string action) =>
            Clip("units/" + UnitArtFolder(t) + "/" + action);

        public static Sprite[] Clip(string path)
        {
            if (clipCache.TryGetValue(path, out var c)) return c;

            // A sliced strip imports as a multi-sprite texture: LoadAll returns
            // every frame. A single-frame png returns one sprite (a 1-frame clip).
            var all = Resources.LoadAll<Sprite>("Art/" + path);
            Sprite[] frames = null;
            if (all != null && all.Length > 0)
            {
                System.Array.Sort(all, (a, b) => FrameIndex(a.name).CompareTo(FrameIndex(b.name)));
                frames = all;
            }
            else
            {
                // Fallback: numbered individual frames path_0, path_1, ...
                var list = new List<Sprite>();
                for (int i = 0; ; i++)
                {
                    var s = Resources.Load<Sprite>("Art/" + path + "_" + i);
                    if (s == null) break;
                    list.Add(s);
                }
                if (list.Count > 0) frames = list.ToArray();
            }

            clipCache[path] = frames;
            return frames;
        }

        /// <summary>True once animated art exists for this unit (idle or walk).</summary>
        public static bool HasClips(UnitType t) =>
            Clip(t, "idle") != null || Clip(t, "walk") != null;

        /// <summary>Trailing integer in a sprite name, for ordering sliced frames.</summary>
        static int FrameIndex(string name)
        {
            int i = name.Length;
            while (i > 0 && char.IsDigit(name[i - 1])) i--;
            return i < name.Length && int.TryParse(name.Substring(i), out int n) ? n : 0;
        }

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
