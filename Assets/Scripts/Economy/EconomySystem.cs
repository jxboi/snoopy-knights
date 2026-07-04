using SnoopyKnights.Buildings;
using SnoopyKnights.Res;
using SnoopyKnights.Units;
using UnityEngine;

namespace SnoopyKnights.Economy
{
    /// <summary>
    /// Settlement-level economy rules: population cap from housing, food
    /// consumption (starvation hurts), and training hosts on buildings.
    /// </summary>
    public sealed class EconomySystem : MonoBehaviour
    {
        /// <summary>Every unit eats 1 food per this many seconds.</summary>
        const float FoodSecondsPerUnit = 45f;
        const float StarveDamageInterval = 3f;

        ResourceStock stock;
        BuildingManager buildings;
        UnitManager units;

        float eatAccumulator;
        float starveTimer;

        public int PopulationCap { get; private set; }
        public int Population => units.Population;
        public bool Starving { get; private set; }

        public void Init(ResourceStock stock, BuildingManager buildings, UnitManager units)
        {
            this.stock = stock;
            this.buildings = buildings;
            this.units = units;

            buildings.BuildingCompleted += OnBuildingCompleted;
            buildings.Removed += _ => RecomputeCap();
            RecomputeCap();
        }

        void OnBuildingCompleted(Building b)
        {
            if (b.Def.Trains.Length > 0)
            {
                var host = b.gameObject.AddComponent<TrainingHost>();
                host.Init(b, stock, units, this);
            }
            RecomputeCap();
        }

        void RecomputeCap()
        {
            int cap = 0;
            foreach (var b in buildings.All)
                if (b.IsOperational)
                    cap += b.Def.PopulationBonus;
            PopulationCap = cap;
        }

        void Update()
        {
            int pop = units.Population;
            if (pop == 0) return;

            // The whole settlement eats pop food per FoodSecondsPerUnit.
            eatAccumulator += Time.deltaTime * pop / FoodSecondsPerUnit;
            while (eatAccumulator >= 1f)
            {
                eatAccumulator -= 1f;
                Starving = !stock.TrySpend(ResourceType.Food, 1);
            }

            if (Starving && stock.Get(ResourceType.Food) > 0)
                Starving = false;

            if (Starving)
            {
                starveTimer += Time.deltaTime;
                if (starveTimer >= StarveDamageInterval)
                {
                    starveTimer = 0f;
                    // Copy: starvation deaths modify the unit list.
                    var snapshot = new System.Collections.Generic.List<Unit>(units.All);
                    foreach (var u in snapshot)
                        if (!u.Def.IsEnemy)
                            u.TakeDamage(1);
                }
            }
            else
            {
                starveTimer = 0f;
            }
        }
    }
}
