using System.Collections.Generic;
using SnoopyKnights.Buildings;
using SnoopyKnights.Grid;
using SnoopyKnights.Res;
using UnityEngine;

namespace SnoopyKnights.Units
{
    /// <summary>Spawns and tracks all units (friendly and enemy).</summary>
    public sealed class UnitManager : MonoBehaviour
    {
        readonly List<Unit> units = new List<Unit>();
        UnitContext ctx;

        public IReadOnlyList<Unit> All => units;

        public event System.Action<Unit> Spawned;
        public event System.Action<Unit> UnitDied;

        public void Init(GridMap map, BuildingManager buildings, ResourceStock stock)
        {
            ctx = new UnitContext { Map = map, Buildings = buildings, Stock = stock, Units = this };
            buildings.Added += NudgeUnitsOffFootprint;
        }

        public Unit Spawn(UnitType type, Vector2 pos)
        {
            var def = UnitDefs.Get(type);
            var go = new GameObject(def.Name);
            go.transform.SetParent(transform, false);

            Unit unit = type switch
            {
                UnitType.Builder => go.AddComponent<BuilderUnit>(),
                UnitType.Carrier => go.AddComponent<CarrierUnit>(),
                UnitType.Farmer => go.AddComponent<WorkerUnit>(),
                _ => go.AddComponent<Unit>()
            };
            unit.Init(ctx, def, pos);
            unit.Died += HandleDied;
            units.Add(unit);
            Spawned?.Invoke(unit);
            return unit;
        }

        void HandleDied(Unit u)
        {
            units.Remove(u);
            UnitDied?.Invoke(u);
            Destroy(u.gameObject);
        }

        /// <summary>Friendly units currently alive (counts toward the population cap).</summary>
        public int Population
        {
            get
            {
                int n = 0;
                foreach (var u in units)
                    if (!u.Def.IsEnemy)
                        n++;
                return n;
            }
        }

        public Unit FindNearest(Vector2 from, float maxDist, System.Func<Unit, bool> filter)
        {
            Unit best = null;
            float bestSqr = maxDist * maxDist;
            foreach (var u in units)
            {
                if (u.IsDead || (filter != null && !filter(u))) continue;
                float d = (u.Pos - from).sqrMagnitude;
                if (d <= bestSqr) { bestSqr = d; best = u; }
            }
            return best;
        }

        /// <summary>Units standing where a building was just placed step aside to its entrance.</summary>
        void NudgeUnitsOffFootprint(Building b)
        {
            foreach (var u in units)
            {
                var t = u.CurrentTile;
                bool inside = t.x >= b.Origin.x && t.x < b.Origin.x + b.Def.Width &&
                              t.y >= b.Origin.y && t.y < b.Origin.y + b.Def.Height;
                if (!inside) continue;
                u.AbortPath();
                Vector2 spot = GridMap.TileCenter(b.EntranceTile);
                u.transform.position = spot + Random.insideUnitCircle * 0.25f;
            }
        }
    }
}
