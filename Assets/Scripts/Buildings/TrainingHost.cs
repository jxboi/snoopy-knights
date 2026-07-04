using System.Collections.Generic;
using SnoopyKnights.Economy;
using SnoopyKnights.Grid;
using SnoopyKnights.Res;
using SnoopyKnights.Units;
using UnityEngine;

namespace SnoopyKnights.Buildings
{
    /// <summary>
    /// Attached to buildings that train units (Town Center, Barracks).
    /// Costs are paid on enqueue; finished units appear at the entrance.
    /// </summary>
    public sealed class TrainingHost : MonoBehaviour
    {
        public const int MaxQueue = 5;

        Building building;
        ResourceStock stock;
        UnitManager units;
        EconomySystem economy;

        readonly List<UnitType> queue = new List<UnitType>();
        float progress;

        public IReadOnlyList<UnitType> Queue => queue;
        public float Progress01 => queue.Count == 0
            ? 0f
            : Mathf.Clamp01(progress / UnitDefs.Get(queue[0]).TrainSeconds);

        public void Init(Building building, ResourceStock stock, UnitManager units, EconomySystem economy)
        {
            this.building = building;
            this.stock = stock;
            this.units = units;
            this.economy = economy;
        }

        public bool CanEnqueue(UnitType type) =>
            building.IsOperational &&
            queue.Count < MaxQueue &&
            stock.CanAfford(UnitDefs.Get(type).TrainCost) &&
            economy.Population + queue.Count < economy.PopulationCap;

        public bool Enqueue(UnitType type)
        {
            if (!CanEnqueue(type)) return false;
            if (!stock.TrySpend(UnitDefs.Get(type).TrainCost)) return false;
            queue.Add(type);
            return true;
        }

        /// <summary>Used by save/load: restore an in-progress queue (already paid for).</summary>
        public void RestoreQueue(IEnumerable<UnitType> types, float progress01)
        {
            queue.Clear();
            queue.AddRange(types);
            progress = queue.Count > 0
                ? UnitDefs.Get(queue[0]).TrainSeconds * Mathf.Clamp01(progress01)
                : 0f;
        }

        void Update()
        {
            if (queue.Count == 0 || !building.IsOperational) return;

            var def = UnitDefs.Get(queue[0]);
            progress += Time.deltaTime;
            if (progress < def.TrainSeconds) return;

            // Hold a finished unit if the cap filled up in the meantime.
            if (economy.Population >= economy.PopulationCap) return;

            progress = 0f;
            var type = queue[0];
            queue.RemoveAt(0);
            Vector2 door = GridMap.TileCenter(building.EntranceTile);
            units.Spawn(type, door + Random.insideUnitCircle * 0.3f);
        }
    }
}
