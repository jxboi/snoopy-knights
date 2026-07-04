using UnityEngine;

namespace SnoopyKnights.Grid
{
    /// <summary>The logical tile map. Pure data + queries, no rendering.</summary>
    public sealed class GridMap
    {
        public readonly int Width;
        public readonly int Height;
        readonly Tile[] tiles;

        /// <summary>Raised when a tile's content changes (road, resource, occupant, type).</summary>
        public event System.Action<int, int> TileChanged;

        public GridMap(int width, int height)
        {
            Width = width;
            Height = height;
            tiles = new Tile[width * height];
            for (int i = 0; i < tiles.Length; i++)
                tiles[i] = new Tile();
        }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
        public bool InBounds(Vector2Int t) => InBounds(t.x, t.y);

        public Tile Get(int x, int y) => tiles[y * Width + x];
        public Tile Get(Vector2Int t) => Get(t.x, t.y);

        public void NotifyChanged(int x, int y) => TileChanged?.Invoke(x, y);

        public bool IsWalkable(int x, int y) => InBounds(x, y) && Get(x, y).IsWalkable;
        public bool IsWalkable(Vector2Int t) => IsWalkable(t.x, t.y);

        /// <summary>Cost of stepping onto a tile. Roads are cheaper, so paths prefer them.</summary>
        public float MoveCost(int x, int y) => Get(x, y).HasRoad ? 0.5f : 1f;

        /// <summary>Speed multiplier while standing on a tile.</summary>
        public float SpeedMultiplier(Vector2Int t) =>
            InBounds(t) && Get(t).HasRoad ? 1.5f : 1f;

        public static Vector2 TileCenter(int x, int y) => new Vector2(x + 0.5f, y + 0.5f);
        public static Vector2 TileCenter(Vector2Int t) => TileCenter(t.x, t.y);

        public static Vector2Int WorldToTile(Vector2 world) =>
            new Vector2Int(Mathf.FloorToInt(world.x), Mathf.FloorToInt(world.y));

        public static readonly Vector2Int[] CardinalDirs =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };
    }
}
