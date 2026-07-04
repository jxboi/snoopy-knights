using SnoopyKnights.Core;
using SnoopyKnights.Grid;
using SnoopyKnights.Res;
using UnityEngine;

namespace SnoopyKnights.Buildings
{
    /// <summary>
    /// Input mode for drawing roads: tap places one tile, dragging paints a
    /// continuous path. Each tile costs stone immediately.
    /// </summary>
    public sealed class RoadPaintMode : IInputMode
    {
        public bool UsesDrag => true;

        readonly GridMap map;
        readonly ResourceStock stock;
        readonly int stoneCostPerTile;
        Vector2Int? lastTile;

        public RoadPaintMode(GridMap map, ResourceStock stock, int stoneCostPerTile)
        {
            this.map = map;
            this.stock = stock;
            this.stoneCostPerTile = stoneCostPerTile;
        }

        public void OnTap(Vector2 world) => TryPlace(GridMap.WorldToTile(world));

        public void OnDragStart(Vector2 world)
        {
            lastTile = GridMap.WorldToTile(world);
            TryPlace(lastTile.Value);
        }

        public void OnDrag(Vector2 world)
        {
            var target = GridMap.WorldToTile(world);
            if (lastTile == null)
            {
                lastTile = target;
                TryPlace(target);
                return;
            }

            // Step tile-by-tile so fast drags leave no gaps.
            var cur = lastTile.Value;
            int guard = 64;
            while (cur != target && guard-- > 0)
            {
                int dx = target.x - cur.x, dy = target.y - cur.y;
                if (Mathf.Abs(dx) >= Mathf.Abs(dy)) cur.x += System.Math.Sign(dx);
                else cur.y += System.Math.Sign(dy);
                TryPlace(cur);
            }
            lastTile = target;
        }

        public void OnDragEnd(Vector2 world) => lastTile = null;

        void TryPlace(Vector2Int t)
        {
            if (!map.InBounds(t)) return;
            var tile = map.Get(t);
            if (!tile.CanPlaceRoad) return;
            if (!stock.TrySpend(ResourceType.Stone, stoneCostPerTile)) return;
            tile.HasRoad = true;
            map.NotifyChanged(t.x, t.y);
        }
    }
}
