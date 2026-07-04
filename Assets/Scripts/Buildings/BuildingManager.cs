using System.Collections.Generic;
using SnoopyKnights.Grid;
using SnoopyKnights.Res;
using UnityEngine;

namespace SnoopyKnights.Buildings
{
    /// <summary>
    /// Owns all placed buildings: placement validation, payment, footprint
    /// registration on the grid, demolition.
    /// </summary>
    public sealed class BuildingManager : MonoBehaviour
    {
        GridMap map;
        ResourceStock stock;
        readonly List<Building> buildings = new List<Building>();

        public IReadOnlyList<Building> All => buildings;

        public event System.Action<Building> Added;
        public event System.Action<Building> Removed;
        public event System.Action<Building> BuildingCompleted;
        public event System.Action<Building> BuildingDestroyed;

        /// <summary>M2 fallback: sites build themselves. Replaced by Builder units in M3.</summary>
        public bool AutoConstruct = true;

        public void Init(GridMap gridMap, ResourceStock resourceStock)
        {
            map = gridMap;
            stock = resourceStock;
        }

        void Update()
        {
            if (!AutoConstruct) return;
            for (int i = 0; i < buildings.Count; i++)
                buildings[i].AdvanceConstruction(Time.deltaTime);
        }

        // ---- Placement ------------------------------------------------------

        public bool CanPlace(BuildingDef def, Vector2Int origin)
        {
            // Stay one tile off the map border so the surroundings stay walkable.
            if (origin.x < 1 || origin.y < 2 ||
                origin.x + def.Width > map.Width - 1 || origin.y + def.Height > map.Height - 1)
                return false;

            for (int y = 0; y < def.Height; y++)
                for (int x = 0; x < def.Width; x++)
                    if (!map.Get(origin.x + x, origin.y + y).CanBuildOn)
                        return false;

            // The entrance tile must be usable ground (roads there are fine).
            var e = new Vector2Int(origin.x + def.Width / 2, origin.y - 1);
            var entrance = map.Get(e);
            return entrance.Type == TileType.Grass && entrance.Occupant == null;
        }

        public bool CanAfford(BuildingDef def) => stock.CanAfford(def.Cost);

        public Building Place(BuildingType type, Vector2Int origin, bool instant = false, bool free = false)
        {
            var def = BuildingDefs.Get(type);
            if (!CanPlace(def, origin)) return null;
            if (!free && !stock.TrySpend(def.Cost)) return null;

            var go = new GameObject($"{def.Name} {origin.x},{origin.y}");
            go.transform.SetParent(transform, false);
            var b = go.AddComponent<Building>();
            b.Init(def, origin, instant);
            b.Completed += OnBuildingCompleted;
            b.Destroyed += OnBuildingDestroyed;

            SetFootprint(b, b);
            buildings.Add(b);
            Added?.Invoke(b);
            if (b.IsOperational)
                BuildingCompleted?.Invoke(b); // instant placements complete inside Init
            return b;
        }

        public void Demolish(Building b)
        {
            // Refund half the cost.
            foreach (var c in b.Def.Cost)
                stock.Add(c.Type, c.Amount / 2);
            RemoveBuilding(b);
        }

        void OnBuildingCompleted(Building b) => BuildingCompleted?.Invoke(b);

        void OnBuildingDestroyed(Building b)
        {
            BuildingDestroyed?.Invoke(b);
            RemoveBuilding(b);
        }

        void RemoveBuilding(Building b)
        {
            SetFootprint(b, null);
            buildings.Remove(b);
            Removed?.Invoke(b);
            Destroy(b.gameObject);
        }

        void SetFootprint(Building b, ITileOccupant occupant)
        {
            for (int y = 0; y < b.Def.Height; y++)
                for (int x = 0; x < b.Def.Width; x++)
                {
                    map.Get(b.Origin.x + x, b.Origin.y + y).Occupant = occupant;
                    map.NotifyChanged(b.Origin.x + x, b.Origin.y + y);
                }
        }

        // ---- Queries ----------------------------------------------------------

        public Building GetAt(Vector2Int tile) =>
            map.InBounds(tile) ? map.Get(tile).Occupant as Building : null;

        public Building FindNearest(Vector2 from, System.Func<Building, bool> filter)
        {
            Building best = null;
            float bestDist = float.MaxValue;
            foreach (var b in buildings)
            {
                if (filter != null && !filter(b)) continue;
                float d = (b.CenterWorld - from).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = b; }
            }
            return best;
        }

        public Building FindNearestStorage(Vector2 from) =>
            FindNearest(from, b => b.Def.IsStorage && b.IsOperational);

        /// <summary>A staffed kitchen makes farms yield double food per cycle.</summary>
        public bool AnyStaffedKitchen()
        {
            foreach (var b in buildings)
                if (b.Def.Type == BuildingType.Kitchen && b.IsOperational && b.AssignedWorker != null)
                    return true;
            return false;
        }

        public bool AnyStorageAlive()
        {
            foreach (var b in buildings)
                if (b.Def.IsStorage && b.IsOperational)
                    return true;
            return false;
        }
    }
}
