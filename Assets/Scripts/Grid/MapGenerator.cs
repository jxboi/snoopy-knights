using UnityEngine;

namespace SnoopyKnights.Grid
{
    /// <summary>
    /// Generates the fixed mission map: a grass field with forest and rock
    /// clusters, a small lake, and a clear area for the starting base.
    /// </summary>
    public static class MapGenerator
    {
        public const int MapWidth = 36;
        public const int MapHeight = 26;

        public const int WoodPerForestTile = 6;
        public const int StonePerRockTile = 10;

        public static GridMap Generate(int seed)
        {
            var map = new GridMap(MapWidth, MapHeight);
            var rng = new System.Random(seed);

            // Resource clusters around the edges of the buildable middle.
            Blob(map, rng, 6, 19, 42, TileType.Forest);
            Blob(map, rng, 4, 9, 26, TileType.Forest);
            Blob(map, rng, 17, 22, 30, TileType.Forest);
            Blob(map, rng, 29, 20, 22, TileType.Rock);
            Blob(map, rng, 31, 6, 14, TileType.Water);

            // Keep the starting base area clear.
            ClearRect(map, 12, 3, 25, 12);

            // Keep the map border walkable so enemy waves can enter anywhere.
            for (int x = 0; x < MapWidth; x++)
            {
                map.Get(x, 0).Type = TileType.Grass;
                map.Get(x, MapHeight - 1).Type = TileType.Grass;
            }
            for (int y = 0; y < MapHeight; y++)
            {
                map.Get(0, y).Type = TileType.Grass;
                map.Get(MapWidth - 1, y).Type = TileType.Grass;
            }

            // Stock resource nodes.
            for (int y = 0; y < MapHeight; y++)
                for (int x = 0; x < MapWidth; x++)
                {
                    var tile = map.Get(x, y);
                    if (tile.Type == TileType.Forest) tile.ResourceLeft = WoodPerForestTile;
                    else if (tile.Type == TileType.Rock) tile.ResourceLeft = StonePerRockTile;
                }

            return map;
        }

        static void Blob(GridMap map, System.Random rng, int cx, int cy, int count, TileType type)
        {
            int x = cx, y = cy;
            for (int i = 0; i < count; i++)
            {
                if (map.InBounds(x, y) && map.Get(x, y).Type == TileType.Grass)
                    map.Get(x, y).Type = type;

                // Random walk biased back toward the blob centre so it stays clumped.
                x += rng.Next(-1, 2);
                y += rng.Next(-1, 2);
                if (rng.Next(3) == 0) { x += System.Math.Sign(cx - x); y += System.Math.Sign(cy - y); }
                x = Mathf.Clamp(x, 1, map.Width - 2);
                y = Mathf.Clamp(y, 1, map.Height - 2);
            }
        }

        static void ClearRect(GridMap map, int x0, int y0, int x1, int y1)
        {
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    if (map.InBounds(x, y))
                        map.Get(x, y).Type = TileType.Grass;
        }
    }
}
