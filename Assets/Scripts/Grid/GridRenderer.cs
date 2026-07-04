using SnoopyKnights.Rendering;
using UnityEngine;

namespace SnoopyKnights.Grid
{
    /// <summary>
    /// Draws the tile map. Uses pixel-art sprites from SpriteBank when present,
    /// falling back to tinted procedural shapes. Roads are a dirt path overlay;
    /// adjacent dirt tiles read as a continuous, connected road.
    /// </summary>
    public sealed class GridRenderer : MonoBehaviour
    {
        static readonly Color GrassA = new Color(0.42f, 0.58f, 0.29f);
        static readonly Color GrassB = new Color(0.45f, 0.61f, 0.31f);
        static readonly Color WaterColor = new Color(0.27f, 0.47f, 0.71f);
        static readonly Color TreeColor = new Color(0.16f, 0.36f, 0.15f);
        static readonly Color RockColor = new Color(0.52f, 0.52f, 0.55f);
        static readonly Color CobbleColor = new Color(0.5f, 0.5f, 0.54f);
        static readonly Color RoadColor = new Color(0.62f, 0.51f, 0.34f);

        GridMap map;
        bool useArt;
        SpriteRenderer[] ground;
        SpriteRenderer[] overlay;   // tree/rock marker, created lazily
        SpriteRenderer[] road;      // dirt path overlay, created lazily

        public void Build(GridMap map)
        {
            this.map = map;
            useArt = SpriteBank.HasArt;
            ground = new SpriteRenderer[map.Width * map.Height];
            overlay = new SpriteRenderer[map.Width * map.Height];
            road = new SpriteRenderer[map.Width * map.Height];

            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    ground[Idx(x, y)] = SpriteFactory.NewRenderer(
                        transform, $"Tile {x},{y}",
                        useArt ? SpriteBank.GroundGrass((x + y) % 2 == 0) : SpriteFactory.Square,
                        Color.white, SortLayer.Ground, GridMap.TileCenter(x, y));
                    RefreshTile(x, y);
                }

            map.TileChanged += OnTileChanged;
        }

        void OnTileChanged(int x, int y) => RefreshTile(x, y);

        void RefreshTile(int x, int y)
        {
            var tile = map.Get(x, y);
            SetGround(x, y, tile);
            SetOverlay(x, y, tile);
            SetRoad(x, y, tile);
        }

        void SetGround(int x, int y, Tile tile)
        {
            var g = ground[Idx(x, y)];
            bool alt = (x + y) % 2 == 0;
            if (useArt)
            {
                g.color = Color.white;
                g.sprite = tile.Type == TileType.Water ? SpriteBank.GroundWater
                    : tile.Type == TileType.Rock ? SpriteBank.GroundCobble
                    : SpriteBank.GroundGrass(alt);
            }
            else
            {
                g.color = tile.Type == TileType.Water ? WaterColor
                    : tile.Type == TileType.Rock ? CobbleColor
                    : (alt ? GrassA : GrassB);
            }
        }

        void SetOverlay(int x, int y, Tile tile)
        {
            bool wants = tile.Type == TileType.Forest || tile.Type == TileType.Rock;
            var ov = overlay[Idx(x, y)];
            if (!wants)
            {
                if (ov != null) ov.gameObject.SetActive(false);
                return;
            }

            if (ov == null)
            {
                ov = SpriteFactory.NewRenderer(transform, $"Node {x},{y}",
                    SpriteFactory.Circle, Color.white, SortLayer.NodeOverlay, GridMap.TileCenter(x, y));
                overlay[Idx(x, y)] = ov;
            }
            ov.gameObject.SetActive(true);

            bool forest = tile.Type == TileType.Forest;
            if (useArt)
            {
                ov.sprite = forest ? SpriteBank.Tree : SpriteBank.Rock;
                ov.color = Color.white;
                ov.transform.localScale = Vector3.one;
            }
            else if (forest)
            {
                ov.sprite = SpriteFactory.Triangle;
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

        void SetRoad(int x, int y, Tile tile)
        {
            var r = road[Idx(x, y)];
            if (!tile.HasRoad)
            {
                if (r != null) r.gameObject.SetActive(false);
                return;
            }

            if (r == null)
            {
                r = SpriteFactory.NewRenderer(transform, $"Road {x},{y}",
                    useArt ? SpriteBank.Road : SpriteFactory.Square,
                    useArt ? Color.white : RoadColor, SortLayer.Road, GridMap.TileCenter(x, y));
                road[Idx(x, y)] = r;
            }
            r.gameObject.SetActive(true);
        }

        int Idx(int x, int y) => y * map.Width + x;
    }
}
