using SnoopyKnights.Combat;
using SnoopyKnights.Grid;
using UnityEngine;

namespace SnoopyKnights.Units
{
    /// <summary>
    /// Guard (melee) or Archer (ranged). Holds a post, auto-engages enemies
    /// that come near, and returns to the post afterwards. The player moves
    /// the post with a single tap while the soldier is selected.
    /// </summary>
    public sealed class SoldierUnit : Unit
    {
        Vector2 post;
        Unit target;
        float attackT, scanT, repathT;

        float LeashRange => Def.AggroRange + 3f;

        public override void Init(UnitContext ctx, UnitDef def, Vector2 pos)
        {
            base.Init(ctx, def, pos);
            post = pos;
            Status = "Guarding";
        }

        /// <summary>Player order: move the guard post.</summary>
        public void OrderMove(Vector2Int tile)
        {
            target = null;
            post = GridMap.TileCenter(tile);
            MoveTo(tile);
            Status = "Moving";
        }

        protected override void Tick(float dt)
        {
            attackT -= dt;
            scanT -= dt;
            if (scanT <= 0f)
            {
                scanT = 0.3f;
                AcquireTarget();
            }

            if (target != null)
            {
                EngageTarget(dt);
                return;
            }

            if (!IsMoving)
            {
                if (((Vector2)transform.position - post).magnitude > 1.2f)
                {
                    if (MoveTo(GridMap.WorldToTile(post)))
                        Status = "Returning to post";
                }
                else
                {
                    Status = "Guarding";
                }
            }
        }

        void EngageTarget(float dt)
        {
            if (target == null || target.IsDead)
            {
                target = null;
                return;
            }

            // Don't chase forever; stay near the post.
            if ((target.Pos - post).magnitude > LeashRange)
            {
                target = null;
                return;
            }

            float dist = Vector2.Distance(Pos, target.Pos);
            if (dist <= Def.AttackRange)
            {
                AbortPath();
                Status = "Fighting";
                if (attackT <= 0f)
                {
                    attackT = Def.AttackInterval;
                    PlayAttackAnim();
                    if (Def.Ranged) Projectile.Spawn(Pos, target, Def.Damage);
                    else target.TakeDamage(Def.Damage, this);
                }
            }
            else
            {
                repathT -= dt;
                if (repathT <= 0f || !IsMoving)
                {
                    repathT = 0.4f;
                    if (!MoveTo(target.CurrentTile))
                        MoveAdjacentTo(target.CurrentTile);
                    Status = "Chasing";
                }
            }
        }

        void AcquireTarget()
        {
            if (target != null && !target.IsDead &&
                (target.Pos - post).magnitude <= LeashRange)
                return; // stick with the current fight

            target = Ctx.Units.FindNearest(Pos, Def.AggroRange, u => u.Def.IsEnemy);
        }

        protected override void OnDamaged(Unit attacker)
        {
            if (attacker != null && !attacker.IsDead && attacker.Def.IsEnemy && target == null)
                target = attacker;
        }
    }
}
