using SnoopyKnights.Rendering;
using UnityEngine;

namespace SnoopyKnights.Grid
{
    /// <summary>
    /// Draws the tile map with procedural sprites and keeps it in sync with
    /// GridMap.TileChanged events. Roads render as connected strips.
    /// </summary>
    public sealed class GridRenderer : MonoBehaviour
    {
        static readonly Color GrassA = new Color(0.42f, 0.58f, 0.29f);
        static readonly Color GrassB = new Color(0.45f, 0.61f, 0.31f);
        static readonly Color WaterColor = new Color(0.27f, 0.47f, 0.71f);
        static readonly Color TreeColor = new Color(0.16f, 0.36f, 0.15f);
        static readonly Color RockColor = new Color(0.52f, 0.52f, 0.55f);
        static readonly Color RoadColor = new Color(0.62f, 0.51f, 0.34f);

        GridMap map;
        SpriteRenderer[] ground;
        SpriteRenderer[] overlay;   // tree/rock marker, created lazily
        GameObject[] roadGroups;    // per-tile road strip group, rebuilt on change

        public void Build(GridMap map)
        {
            this.map = map;
            ground = new SpriteRenderer[map.Width * map.Height];
            overlay = new SpriteRenderer[map.Width * map.Height];
            roadGroups = new GameObject[map.Width * map.Height];

            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    ground[Idx(x, y)] = SpriteFactory.NewRenderer(
                        transform, $"Tile {x},{y}", SpriteFactory.Square, Color.white,
                        SortLayer.Ground, GridMap.TileCenter(x, y));
                    RefreshTile(x, y);
                }

            map.TileChanged += OnTileChanged;
        }

        void OnTileChanged(int x, int y)
        {
            RefreshTile(x, y);
            // Road strips depend on neighbours, so refresh them too.
            foreach (var d in GridMap.CardinalDirs)
                if (map.InBounds(x + d.x, y + d.y))
                    RefreshRoad(x + d.x, y + d.y);
        }

        void RefreshTile(int x, int y)
        {
            var tile = map.Get(x, y);

            ground[Idx(x, y)].color = tile.Type == TileType.Water
                ? WaterColor
                : ((x + y) % 2 == 0 ? GrassA : GrassB);

            // Tree / rock overlay.
            bool wantsOverlay = tile.Type == TileType.Forest || tile.Type == TileType.Rock;
            var ov = overlay[Idx(x, y)];
            if (wantsOverlay)
            {
                if (ov == null)
                {
                    ov = SpriteFactory.NewRenderer(
                        transform, $"Node {x},{y}", SpriteFactory.Circle, Color.white,
                        SortLayer.NodeOverlay, GridMap.TileCenter(x, y));
                    overlay[Idx(x, y)] = ov;
                }
                ov.gameObject.SetActive(true);
                if (tile.Type == TileType.Forest)
                {
                    ov.sprite = SpriteFactory.Triangle; // pine-ish tree
                    ov.color = TreeColor;
                    ov.transform.localScale = new Vector3(0.8f, 0.9f, 1f);
                }
                else
                {
                    ov.sprite = SpriteFactory.Circle;
                    ov.color = RockColor;
                    ov.transform.localScale = new Vector3(0.75f, 0.6f, 1f);
                }
            }
            else if (ov != null)
            {
                ov.gameObject.SetActive(false);
            }

            RefreshRoad(x, y);
        }

        void RefreshRoad(int x, int y)
        {
            var group = roadGroups[Idx(x, y)];
            if (group != null)
                Destroy(group);
            roadGroups[Idx(x, y)] = null;

            if (!map.Get(x, y).HasRoad)
                return;

            group = new GameObject($"Road {x},{y}");
            group.transform.SetParent(transform, false);
            group.transform.localPosition = GridMap.TileCenter(x, y);
            roadGroups[Idx(x, y)] = group;

            // Centre pad plus an arm toward each connected neighbour.
            SpriteFactory.NewRenderer(group.transform, "Pad", SpriteFactory.Square,
                RoadColor, SortLayer.Road, Vector2.zero, 0.55f);

            foreach (var d in GridMap.CardinalDirs)
            {
                var n = new Vector2Int(x + d.x, y + d.y);
                if (!map.InBounds(n) || !ConnectsRoad(n))
                    continue;
                var arm = SpriteFactory.NewRenderer(group.transform, "Arm", SpriteFactory.Square,
                    RoadColor, SortLayer.Road, new Vector2(d.x * 0.25f, d.y * 0.25f));
                arm.transform.localScale = d.x != 0
                    ? new Vector3(0.5f, 0.55f, 1f)
                    : new Vector3(0.55f, 0.5f, 1f);
            }
        }

        bool ConnectsRoad(Vector2Int t)
        {
            var tile = map.Get(t);
            return tile.HasRoad || tile.Occupant != null; // roads visually attach to buildings
        }

        int Idx(int x, int y) => y * map.Width + x;
    }
}
