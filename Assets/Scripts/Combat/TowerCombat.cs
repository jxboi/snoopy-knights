using SnoopyKnights.Buildings;
using SnoopyKnights.Units;
using UnityEngine;

namespace SnoopyKnights.Combat
{
    /// <summary>Watchtower behaviour: shoots arrows at the nearest enemy in range.</summary>
    public sealed class TowerCombat : MonoBehaviour
    {
        public const float Range = 5.5f;
        public const int Damage = 8;
        public const float Interval = 1.1f;

        Building building;
        UnitManager units;
        Unit target;
        float cooldown, scanT;

        public void Init(Building building, UnitManager units)
        {
            this.building = building;
            this.units = units;
        }

        void Update()
        {
            if (!building.IsOperational) return;

            cooldown -= Time.deltaTime;
            scanT -= Time.deltaTime;
            if (scanT <= 0f)
            {
                scanT = 0.25f;
                if (target == null || target.IsDead ||
                    (target.Pos - (Vector2)building.CenterWorld).magnitude > Range)
                    target = units.FindNearest(building.CenterWorld, Range, u => u.Def.IsEnemy);
            }

            if (target != null && !target.IsDead && cooldown <= 0f)
            {
                cooldown = Interval;
                Projectile.Spawn(building.CenterWorld, target, Damage);
            }
        }
    }
}
