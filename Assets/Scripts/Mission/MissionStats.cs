using SnoopyKnights.Buildings;
using SnoopyKnights.Units;
using UnityEngine;

namespace SnoopyKnights.Mission
{
    /// <summary>Mission tallies for the end screen. Survives save/load.</summary>
    public sealed class MissionStats : MonoBehaviour
    {
        float baseSeconds; // play time accumulated before a save/load round-trip

        public int EnemiesSlain { get; private set; }
        public int VillagersLost { get; private set; }
        public int BuildingsLost { get; private set; }

        /// <summary>Sim seconds played (pauses excluded, fast-forward counts double).</summary>
        public float PlaySeconds => baseSeconds + Time.timeSinceLevelLoad;

        public void Init(UnitManager units, BuildingManager buildings)
        {
            units.UnitDied += u =>
            {
                if (u.Def.IsEnemy) EnemiesSlain++;
                else VillagersLost++;
            };
            // Only combat losses arrive here; demolitions bypass this event.
            buildings.BuildingDestroyed += _ => BuildingsLost++;
        }

        public void Restore(int enemiesSlain, int villagersLost, int buildingsLost, float playSeconds)
        {
            EnemiesSlain = enemiesSlain;
            VillagersLost = villagersLost;
            BuildingsLost = buildingsLost;
            baseSeconds = playSeconds;
        }
    }
}
