using System.Collections.Generic;
using SnoopyKnights.Buildings;
using SnoopyKnights.Grid;
using SnoopyKnights.Pathfinding;
using SnoopyKnights.Rendering;
using SnoopyKnights.Res;
using UnityEngine;

namespace SnoopyKnights.Units
{
    /// <summary>Shared services units need. Passed in by UnitManager.</summary>
    public struct UnitContext
    {
        public GridMap Map;
        public BuildingManager Buildings;
        public ResourceStock Stock;
        public UnitManager Units;
    }

    /// <summary>
    /// Base unit: movement along A* paths, health, and procedural visuals.
    /// Subclasses implement behaviour in Tick().
    /// </summary>
    public class Unit : MonoBehaviour, Combat.IDamageable
    {
        public UnitDef Def { get; private set; }
        public int Health { get; private set; }
        public bool IsDead { get; private set; }
        /// <summary>Shown in the unit info panel.</summary>
        public string Status { get; protected set; } = "Idle";

        public event System.Action<Unit> Died;

        protected UnitContext Ctx;
        protected GridMap Map => Ctx.Map;

        List<Vector2Int> path;
        int pathIndex;
        SpriteRenderer body;
        SpriteRenderer carryDot;
        Transform icon;
        WorldBar healthBar;
        bool working;
        float workAnimT;

        public bool IsMoving => path != null;
        public Vector2Int CurrentTile => GridMap.WorldToTile(transform.position);
        public Vector2 Pos => transform.position;

        public virtual void Init(UnitContext ctx, UnitDef def, Vector2 pos)
        {
            Ctx = ctx;
            Def = def;
            Health = def.MaxHealth;
            transform.position = new Vector3(pos.x, pos.y, 0f);
            BuildVisuals();
        }

        void BuildVisuals()
        {
            var artSprite = SpriteBank.Unit(Def.Type);
            if (artSprite != null)
            {
                body = SpriteFactory.NewRenderer(transform, "Body", artSprite, Color.white,
                    SortLayer.Unit + 1, Vector2.zero, 1f);
                // Enemies get a subtle red wash to read as hostile at a glance.
                if (Def.IsEnemy) body.color = new Color(1f, 0.82f, 0.82f);
                icon = body.transform; // the work-bob animates the whole character
            }
            else
            {
                SpriteFactory.NewRenderer(transform, "Border", SpriteFactory.Circle,
                    new Color(0.1f, 0.09f, 0.07f), SortLayer.Unit, Vector2.zero, 0.6f);
                body = SpriteFactory.NewRenderer(transform, "Body", SpriteFactory.Circle,
                    Def.Color, SortLayer.Unit + 1, Vector2.zero, 0.5f);

                var iconSprite = Def.Icon switch
                {
                    IconShape.Square => SpriteFactory.Square,
                    IconShape.Diamond => SpriteFactory.Diamond,
                    IconShape.Triangle => SpriteFactory.Triangle,
                    _ => SpriteFactory.Circle
                };
                icon = SpriteFactory.NewRenderer(transform, "Icon", iconSprite,
                    new Color(1f, 1f, 1f, 0.85f), SortLayer.Unit + 2, Vector2.zero, 0.24f).transform;
            }

            carryDot = SpriteFactory.NewRenderer(transform, "Carry", SpriteFactory.Square,
                Color.white, SortLayer.Unit + 3, new Vector2(0f, 0.45f), 0.24f);
            carryDot.gameObject.SetActive(false);

            healthBar = WorldBar.Create(transform, new Vector2(0f, 0.55f), 0.7f,
                Def.IsEnemy ? new Color(0.9f, 0.3f, 0.2f) : new Color(0.3f, 0.85f, 0.3f));
            healthBar.Show(false);
        }

        // ---- Movement --------------------------------------------------------

        /// <summary>Paths to a walkable tile. Returns false if unreachable.</summary>
        public bool MoveTo(Vector2Int goal)
        {
            var p = Pathfinder.ToTile(Map, CurrentTile, goal);
            if (p == null) return false;
            SetPath(p);
            return true;
        }

        /// <summary>Paths next to a (possibly blocked) target tile.</summary>
        public bool MoveAdjacentTo(Vector2Int target)
        {
            var p = Pathfinder.ToAdjacent(Map, CurrentTile, target);
            if (p == null) return false;
            SetPath(p);
            return true;
        }

        public void AbortPath() => path = null;

        void SetPath(List<Vector2Int> p)
        {
            path = p.Count > 0 ? p : null;
            pathIndex = 0;
        }

        public bool NearTile(Vector2Int t, float maxDist = 0.6f) =>
            ((Vector2)transform.position - GridMap.TileCenter(t)).magnitude <= maxDist;

        public bool AdjacentToTile(Vector2Int t)
        {
            var c = CurrentTile;
            return Mathf.Abs(c.x - t.x) + Mathf.Abs(c.y - t.y) <= 1;
        }

        void Update()
        {
            if (IsDead) return;
            UpdateMovement(Time.deltaTime);
            UpdateWorkAnim(Time.deltaTime);
            Tick(Time.deltaTime);
        }

        void UpdateMovement(float dt)
        {
            if (path == null) return;
            if (pathIndex >= path.Count)
            {
                path = null;
                return;
            }
            Vector2 target = GridMap.TileCenter(path[pathIndex]);
            float speed = Def.MoveSpeed * Map.SpeedMultiplier(CurrentTile);
            Vector2 next = Vector2.MoveTowards(transform.position, target, speed * dt);
            transform.position = next;
            if ((next - target).sqrMagnitude < 0.003f)
                pathIndex++;
        }

        protected virtual void Tick(float dt) { }

        // ---- Working / carrying visuals ---------------------------------------

        protected void SetWorking(bool on)
        {
            working = on;
            if (!on && icon != null) icon.localPosition = Vector3.zero;
        }

        void UpdateWorkAnim(float dt)
        {
            if (!working || icon == null) return;
            workAnimT += dt * 6f;
            icon.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(workAnimT)) * 0.12f, 0f);
        }

        public void SetCarrying(ResourceType? res)
        {
            carryDot.gameObject.SetActive(res.HasValue);
            if (res.HasValue) carryDot.color = ResourceInfo.Color(res.Value);
        }

        /// <summary>Idle units gravitate toward the nearest storage so they're easy to find.</summary>
        protected void IdleDrift()
        {
            if (IsMoving) return;
            var storage = Ctx.Buildings.FindNearestStorage(Pos);
            if (storage == null) return;
            var e = storage.EntranceTile;
            if (((Vector2)transform.position - GridMap.TileCenter(e)).magnitude <= 3.5f) return;
            var t = e + new Vector2Int(Random.Range(-2, 3), Random.Range(-1, 2));
            if (!MoveTo(t)) MoveTo(e);
        }

        // ---- Health -----------------------------------------------------------

        public bool IsAlive => !IsDead;
        public Vector2 AimPoint => transform.position;

        public void TakeDamage(int amount) => TakeDamage(amount, null);

        public void TakeDamage(int amount, Unit attacker)
        {
            if (IsDead) return;
            Audio.AudioManager.Play(Audio.Sfx.Hit);
            Health = Mathf.Max(0, Health - amount);
            healthBar.Show(Health < Def.MaxHealth);
            healthBar.Set((float)Health / Def.MaxHealth);
            if (Health == 0) Die();
            else OnDamaged(attacker);
        }

        /// <summary>Lets subclasses react (fight back, retarget).</summary>
        protected virtual void OnDamaged(Unit attacker) { }

        /// <summary>Used by save/load: set health without triggering damage reactions.</summary>
        public void RestoreHealth(int value)
        {
            Health = Mathf.Clamp(value, 1, Def.MaxHealth);
            healthBar.Show(Health < Def.MaxHealth);
            healthBar.Set((float)Health / Def.MaxHealth);
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            Health = Mathf.Min(Def.MaxHealth, Health + amount);
            healthBar.Set((float)Health / Def.MaxHealth);
            if (Health >= Def.MaxHealth) healthBar.Show(false);
        }

        protected virtual void OnDie() { }

        void Die()
        {
            IsDead = true;
            OnDie();
            Died?.Invoke(this);
        }
    }
}
