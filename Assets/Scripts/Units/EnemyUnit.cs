using SnoopyKnights.Buildings;
using SnoopyKnights.Res;
using UnityEngine;

namespace SnoopyKnights.Units
{
    /// <summary>
    /// Wave enemy: attacks nearby defenders and workers, otherwise marches on
    /// the settlement's buildings (watchtowers are preferred targets).
    /// Drops gold when killed.
    /// </summary>
    public sealed class EnemyUnit : Unit
    {
        Unit targetUnit;
        Building targetBuilding;
        float attackT, scanT, repathT;

        protected override void Tick(float dt)
        {
            attackT -= dt;
            scanT -= dt;
            if (scanT <= 0f)
            {
                scanT = 0.5f;
                Retarget();
            }

            if (targetUnit != null && !targetUnit.IsDead)
            {
                ChaseUnit(dt);
            }
            else
            {
                targetUnit = null;
                AttackBuilding(dt);
            }
        }

        void ChaseUnit(float dt)
        {
            float dist = Vector2.Distance(Pos, targetUnit.Pos);
            if (dist <= Def.AttackRange)
            {
                AbortPath();
                Status = "Fighting";
                if (attackT <= 0f)
                {
                    attackT = Def.AttackInterval;
                    targetUnit.TakeDamage(Def.Damage, this);
                }
            }
            else if (dist > Def.AggroRange * 1.6f)
            {
                targetUnit = null; // lost them; back to razing buildings
            }
            else
            {
                repathT -= dt;
                if (repathT <= 0f || !IsMoving)
                {
                    repathT = 0.5f;
                    if (!MoveTo(targetUnit.CurrentTile))
                        MoveAdjacentTo(targetUnit.CurrentTile);
                    Status = "Chasing";
                }
            }
        }

        void AttackBuilding(float dt)
        {
            if (targetBuilding == null)
            {
                Status = "Prowling";
                return;
            }

            float dist = targetBuilding.DistanceTo(Pos);
            if (dist <= Mathf.Max(Def.AttackRange, 0.4f))
            {
                AbortPath();
                Status = "Razing";
                if (attackT <= 0f)
                {
                    attackT = Def.AttackInterval;
                    targetBuilding.TakeDamage(Def.Damage);
                }
            }
            else
            {
                repathT -= dt;
                if (repathT <= 0f || !IsMoving)
                {
                    repathT = 1f;
                    if (!MoveAdjacentTo(targetBuilding.ClosestFootprintTile(Pos)))
                        targetBuilding = null; // unreachable; pick another next scan
                    else
                        Status = "Marching";
                }
            }
        }

        void Retarget()
        {
            // Fresh prey nearby beats a distant building.
            var prey = Ctx.Units.FindNearest(Pos, Def.AggroRange, u => !u.Def.IsEnemy);
            if (prey != null)
            {
                targetUnit = prey;
                return;
            }

            if (targetBuilding != null) return;

            // Nearest building, with watchtowers weighted as juicier targets.
            Building best = null;
            float bestScore = float.MaxValue;
            foreach (var b in Ctx.Buildings.All)
            {
                float score = b.DistanceTo(Pos) *
                    (b.Def.Type == BuildingType.Watchtower ? 0.55f : 1f);
                if (score < bestScore) { bestScore = score; best = b; }
            }
            targetBuilding = best;
        }

        protected override void OnDamaged(Unit attacker)
        {
            if (attacker == null || attacker.IsDead || attacker.Def.IsEnemy) return;
            if (targetUnit == null ||
                (attacker.Pos - Pos).sqrMagnitude < (targetUnit.Pos - Pos).sqrMagnitude)
                targetUnit = attacker;
        }

        protected override void OnDie()
        {
            if (Def.GoldReward > 0)
            {
                Ctx.Stock.Add(ResourceType.Gold, Def.GoldReward);
                Rendering.FloatingText.Spawn(new Vector2(Pos.x, Pos.y + 0.5f),
                    $"+{Def.GoldReward}g", new Color(1f, 0.85f, 0.2f), 1f);
            }
        }
    }
}
