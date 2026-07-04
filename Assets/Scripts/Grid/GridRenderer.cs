using System.Collections.Generic;
using SnoopyKnights.Rendering;
using UnityEngine;

namespace SnoopyKnights.Grid
{
    /// <summary>
    /// Draws the tile map. Uses pixel-art sprites from SpriteBank when present,
    /// falling back to tinted procedural shapes. Roads are a dirt path overlay;
    /// adjacent dirt tiles read as a continuous, connected road.
    ///
    /// Everything decorative is deterministic per tile (hash of x,y), so the
    /// map looks identical across sessions and save/loads: grass variants are
    /// scattered rather than checkerboarded, bushes dot open grass, trees and
    /// rocks get a per-tile offset/scale/variant, and water gently shimmers.
    /// </summary>
    public sealed class GridRenderer : MonoBehaviour
    {
        static readonly Color GrassA = new Color(0.42f, 0.58f, 0.29f);
        static readonly Color GrassB = new Color(0.45f, 0.61f, 0.31f);
        static readonly Color WaterColor = new Color(0.27f, 0.47f, 0.71f);
        static readonly Color TreeColor = new Color(0.16f, 0.36f, 0.15f);
        static readonly Color RockColor = new Color(0.52f, 0.52f, 0.55f);
        static readonly Color CobbleColor = new Color(0.5f, 0.5f, 0.54f);
        // Solid dirt fill matched to the tileset's dirt palette. We render roads
        // as a full-cell square (not the tiles/dirt sprite, which is a grass/dirt
        // transition piece) so adjacent road tiles merge into a continuous path.
        static readonly Color RoadColor = new Color(0.918f, 0.647f, 0.424f);

        GridMap map;
        bool useArt;
        SpriteRenderer[] ground;
        SpriteRenderer[] overlay;   // tree/rock marker, created lazily
        SpriteRenderer[] overlay2;  // second background tree on dense forest tiles
        SpriteRenderer[] road;      // dirt path overlay, created lazily
        SpriteRenderer[] deco;      // bush clutter on open grass, created lazily

        readonly List<(SpriteRenderer sr, int x, int y)> waterTiles =
            new List<(SpriteRenderer, int, int)>();
        readonly HashSet<int> waterSeen = new HashSet<int>();

        public void Build(GridMap map)
        {
            this.map = map;
            useArt = SpriteBank.HasArt;
            int n = map.Width * map.Height;
            ground = new SpriteRenderer[n];
            overlay = new SpriteRenderer[n];
            overlay2 = new SpriteRenderer[n];
            road = new SpriteRenderer[n];
            deco = new SpriteRenderer[n];

            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    ground[Idx(x, y)] = SpriteFactory.NewRenderer(
                        transform, $"Tile {x},{y}",
                        useArt ? SpriteBank.GroundGrass(true) : SpriteFactory.Square,
                        Color.white, SortLayer.Ground, GridMap.TileCenter(x, y));
                    RefreshTile(x, y);
                }

            BuildSkirt();
            map.TileChanged += OnTileChanged;
        }

        /// <summary>Darkened grass rings just outside the map, so the world
        /// fades into the background instead of stopping at a hard edge.</summary>
        void BuildSkirt()
        {
            for (int y = -2; y < map.Height + 2; y++)
                for (int x = -2; x < map.Width + 2; x++)
                {
                    if (x >= 0 && x < map.Width && y >= 0 && y < map.Height) continue;
                    int ring = Mathf.Max(
                        x < 0 ? -x : x - (map.Width - 1),
                        y < 0 ? -y : y - (map.Height - 1));
                    float shade = ring <= 1 ? 0.6f : 0.38f;
                    var c = useArt
                        ? new Color(shade, shade, shade)
                        : GrassA * shade;
                    c.a = 1f;
                    SpriteFactory.NewRenderer(transform, "Skirt",
                        useArt ? SpriteBank.GroundGrass(Hash(x, y) < 0.5f) : SpriteFactory.Square,
                        c, SortLayer.Ground, GridMap.TileCenter(x, y));
                }
        }

        void OnTileChanged(int x, int y) => RefreshTile(x, y);

        void RefreshTile(int x, int y)
        {
            var tile = map.Get(x, y);
            SetGround(x, y, tile);
            SetOverlay(x, y, tile);
            SetRoad(x, y, tile);
            SetDeco(x, y, tile);
        }

        void SetGround(int x, int y, Tile tile)
        {
            var g = ground[Idx(x, y)];
            bool alt = Hash(x, y) < 0.42f; // scattered, not checkerboarded
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

            if (tile.Type == TileType.Water && waterSeen.Add(Idx(x, y)))
                waterTiles.Add((g, x, y)); // water tiles never change type
        }

        void SetOverlay(int x, int y, Tile tile)
        {
            bool wants = tile.Type == TileType.Forest || tile.Type == TileType.Rock;
            var ov = overlay[Idx(x, y)];
            var ov2 = overlay2[Idx(x, y)];
            if (!wants)
            {
                if (ov != null) ov.gameObject.SetActive(false);
                if (ov2 != null) ov2.gameObject.SetActive(false);
                return;
            }

            if (ov == null)
            {
                ov = SpriteFactory.NewRenderer(transform, $"Node {x},{y}",
                    SpriteFactory.Circle, Color.white, SortLayer.Decor, GridMap.TileCenter(x, y));
                overlay[Idx(x, y)] = ov;
            }
            ov.gameObject.SetActive(true);

            bool forest = tile.Type == TileType.Forest;
            // Per-tile jitter/scale so nodes don't sit on a rigid grid.
            float jx = (Hash(x + 51, y + 7) - 0.5f) * 0.3f;
            float jy = (Hash(x + 13, y + 77) - 0.5f) * 0.24f;
            float scale = 0.9f + 0.25f * Hash(x + 5, y + 31);

            if (useArt)
            {
                bool autumn = forest && Hash(x + 23, y + 41) < 0.22f
                    && SpriteBank.TreeAutumn != null;
                ov.sprite = forest
                    ? (autumn ? SpriteBank.TreeAutumn : SpriteBank.Tree)
                    : SpriteBank.Rock;
                ov.color = Color.white;
                ov.transform.localScale = new Vector3(scale, scale, 1f);
                ov.transform.localPosition = GridMap.TileCenter(x, y) + new Vector2(jx, jy);
                ov.sortingOrder = SortLayer.World(y + 0.1f + jy);

                // A second, smaller tree behind makes forests read denser.
                bool dense = forest && Hash(x + 3, y + 29) < 0.4f;
                if (dense)
                {
                    if (ov2 == null)
                    {
                        ov2 = SpriteFactory.NewRenderer(transform, $"Node2 {x},{y}",
                            SpriteFactory.Circle, Color.white, SortLayer.Decor, GridMap.TileCenter(x, y));
                        overlay2[Idx(x, y)] = ov2;
                    }
                    ov2.gameObject.SetActive(true);
                    bool autumn2 = Hash(x + 71, y + 3) < 0.22f && SpriteBank.TreeAutumn != null;
                    ov2.sprite = autumn2 ? SpriteBank.TreeAutumn : SpriteBank.Tree;
                    ov2.transform.localScale = new Vector3(scale * 0.8f, scale * 0.8f, 1f);
                    ov2.transform.localPosition = GridMap.TileCenter(x, y)
                        + new Vector2(-jx + 0.22f, 0.3f + jy * 0.5f);
                    ov2.sortingOrder = SortLayer.World(y + 0.4f + jy * 0.5f);
                }
                else if (ov2 != null) ov2.gameObject.SetActive(false);
            }
            else if (forest)
            {
                ov.sprite = SpriteFactory.Triangle;
                ov.color = TreeColor;
                ov.transform.localScale = new Vector3(0.8f, 0.9f, 1f);
                ov.sortingOrder = SortLayer.World(y);
            }
            else
            {
                ov.sprite = SpriteFactory.Circle;
                ov.color = RockColor;
                ov.transform.localScale = new Vector3(0.75f, 0.6f, 1f);
                ov.sortingOrder = SortLayer.World(y);
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
                // Slight per-tile tone variation keeps long roads from looking flat.
                var c = RoadColor * (0.95f + 0.08f * Hash(x + 201, y + 67));
                c.a = 1f;
                r = SpriteFactory.NewRenderer(transform, $"Road {x},{y}",
                    SpriteFactory.Square, c, SortLayer.Road, GridMap.TileCenter(x, y));
                road[Idx(x, y)] = r;
            }
            r.gameObject.SetActive(true);
        }

        void SetDeco(int x, int y, Tile tile)
        {
            bool wants = useArt && tile.Type == TileType.Grass && !tile.HasRoad
                && tile.Occupant == null && Hash(x + 91, y + 137) < 0.028f
                && SpriteBank.Bush != null;
            var d = deco[Idx(x, y)];
            if (!wants)
            {
                if (d != null) d.gameObject.SetActive(false);
                return;
            }

            if (d == null)
            {
                float jx = (Hash(x + 7, y + 19) - 0.5f) * 0.4f;
                float jy = (Hash(x + 43, y + 11) - 0.5f) * 0.4f;
                d = SpriteFactory.NewRenderer(transform, $"Bush {x},{y}",
                    SpriteBank.Bush, Color.white, SortLayer.Decor,
                    GridMap.TileCenter(x, y) + new Vector2(jx, jy),
                    0.4f + 0.22f * Hash(x + 3, y + 57));
                deco[Idx(x, y)] = d;
            }
            d.gameObject.SetActive(true);
        }

        void Update()
        {
            // Gentle water shimmer (deterministic phase per tile).
            for (int i = 0; i < waterTiles.Count; i++)
            {
                var (sr, x, y) = waterTiles[i];
                float b = 0.94f + 0.06f * Mathf.Sin(Time.time * 1.7f + x * 0.9f + y * 1.3f);
                sr.color = useArt ? new Color(b, b, b) : WaterColor * b;
            }
        }

        /// <summary>Deterministic per-tile noise in [0,1).</summary>
        static float Hash(int x, int y)
        {
            uint h = (uint)(x * 374761393 + y * 668265263);
            h = (h ^ (h >> 13)) * 1274126177u;
            return ((h ^ (h >> 16)) & 0xFFFFFF) / 16777216f;
        }

        int Idx(int x, int y) => y * map.Width + x;
    }
}
