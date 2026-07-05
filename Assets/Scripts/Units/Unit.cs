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
        SpriteRenderer flash;
        Transform icon;
        Rendering.SpriteAnimator animator; // non-null only when animated art exists
        string animAction = "";
        float oneShotT;
        WorldBar healthBar;
        UnityEngine.Rendering.SortingGroup sortGroup;
        Vector3 bodyRestPos; // billboard lift; the work-bob adds on top of it
        bool working;
        float workAnimT;
        float flashT;

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
            // The composite y-sorts against buildings and trees by the feet baseline.
            sortGroup = gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
            UpdateSortOrder();

            var s = SpriteFactory.NewRenderer(transform, "Shadow", SpriteFactory.SoftCircle,
                new Color(0f, 0f, 0f, 0.34f), SortLayer.Unit - 5, new Vector2(0f, -0.48f));
            s.transform.localScale = new Vector3(0.68f, 0.28f, 1f);

            var artSprite = SpriteBank.Unit(Def.Type);
            if (SpriteBank.HasClips(Def.Type))
            {
                // Animated art present: frame clips (idle/walk/work/attack) drive
                // the look. Frames use a bottom-center pivot so the feet sit on
                // the tile with no lift; locomotion replaces the work-bob.
                bodyRestPos = Vector3.zero;
                body = SpriteFactory.NewRenderer(transform, "Body", null, Color.white,
                    SortLayer.Unit + 1, bodyRestPos, 1f);
                body.transform.localRotation = ViewTilt.Upright;
                if (Def.IsEnemy) body.color = new Color(1f, 0.72f, 0.72f);
                animator = body.gameObject.AddComponent<Rendering.SpriteAnimator>();
                PlayLoop("idle"); // sets frame 0 so the flash silhouette below has a sprite
                icon = null;
            }
            else if (artSprite != null)
            {
                // Stand upright out of the tilted ground; lift so the feet still
                // touch the tile (the sprite pivot is at its center).
                bodyRestPos = new Vector3(0f, ViewTilt.PivotLift(0.5f), 0f);
                body = SpriteFactory.NewRenderer(transform, "Body", artSprite, Color.white,
                    SortLayer.Unit + 1, bodyRestPos, 1f);
                body.transform.localRotation = ViewTilt.Upright;
                // Enemies get a red wash to read as hostile at a glance.
                if (Def.IsEnemy) body.color = new Color(1f, 0.72f, 0.72f);
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

            // White silhouette for the hit-flash, matching the body sprite.
            flash = SpriteFactory.NewRenderer(body.transform, "Flash", body.sprite,
                new Color(1f, 1f, 1f, 0f), SortLayer.Unit + 5);

            carryDot = SpriteFactory.NewRenderer(transform, "Carry", SpriteFactory.Square,
                Color.white, SortLayer.Unit + 3, new Vector2(0f, ViewTilt.MarkerY(0.45f)), 0.24f);
            carryDot.transform.localRotation = ViewTilt.Upright;
            carryDot.gameObject.SetActive(false);

            healthBar = WorldBar.Create(transform, new Vector2(0f, ViewTilt.MarkerY(0.55f)), 0.7f,
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
            UpdateFlash(Time.deltaTime);
            Tick(Time.deltaTime);
            UpdateAnimState(Time.deltaTime);
            UpdateSortOrder();
        }

        void UpdateSortOrder()
        {
            // +1: at an equal baseline the unit stands in front of the building.
            sortGroup.sortingOrder = SortLayer.World(transform.position.y - 0.38f) + 1;
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
            Vector2 pos = transform.position;
            Vector2 next = Vector2.MoveTowards(pos, target, speed * dt);
            transform.position = next;

            // Face travel direction.
            float dx = next.x - pos.x;
            if (Mathf.Abs(dx) > 0.0005f) body.flipX = dx < 0f;

            if ((next - target).sqrMagnitude < 0.003f)
                pathIndex++;
        }

        void UpdateFlash(float dt)
        {
            if (flashT <= 0f) return;
            flashT -= dt;
            var c = flash.color;
            c.a = Mathf.Max(0f, flashT / 0.14f) * 0.85f;
            flash.color = c;
        }

        protected virtual void Tick(float dt) { }

        // ---- Working / carrying visuals ---------------------------------------

        protected void SetWorking(bool on)
        {
            working = on;
            if (!on && icon != null) icon.localPosition = icon == body.transform ? bodyRestPos : Vector3.zero;
        }

        void UpdateWorkAnim(float dt)
        {
            if (!working || icon == null) return;
            workAnimT += dt * 6f;
            var rest = icon == body.transform ? bodyRestPos : Vector3.zero;
            icon.localPosition = rest + new Vector3(0f, Mathf.Abs(Mathf.Sin(workAnimT)) * 0.12f, 0f);
        }

        public void SetCarrying(ResourceType? res)
        {
            carryDot.gameObject.SetActive(res.HasValue);
            if (res.HasValue) carryDot.color = ResourceInfo.Color(res.Value);
        }

        // ---- Frame animation (KaM-style clips; only when animated art exists) --

        void UpdateAnimState(float dt)
        {
            if (animator == null) return;
            if (oneShotT > 0f) { oneShotT -= dt; return; } // let a swing finish first
            PlayLoop(IsMoving ? "walk" : (working ? "work" : "idle"));
        }

        void PlayLoop(string action)
        {
            if (animator == null || animAction == action) return;
            // Fall back through the always-present locomotion clips so a partial
            // clip set (e.g. walk only) still animates.
            var frames = SpriteBank.Clip(Def.Type, action)
                         ?? SpriteBank.Clip(Def.Type, "idle")
                         ?? SpriteBank.Clip(Def.Type, "walk");
            if (frames == null) return;
            animAction = action;
            animator.Play(action, frames, ClipFps(action), true);
        }

        /// <summary>Play a one-shot attack swing, then locomotion resumes automatically.</summary>
        public void PlayAttackAnim()
        {
            if (animator == null) return;
            var frames = SpriteBank.Clip(Def.Type, "attack");
            if (frames == null) return;
            float fps = ClipFps("attack");
            oneShotT = frames.Length / fps;
            animAction = ""; // force a fresh locomotion pick when the swing ends
            animator.Play("attack", frames, fps, false);
        }

        static float ClipFps(string action) => action switch
        {
            "idle" => 5f,
            "attack" => 12f,
            _ => 9f, // walk, work
        };

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

        public void TakeDamage(int amount) => ApplyDamage(amount, null, combat: true);

        public void TakeDamage(int amount, Unit attacker) => ApplyDamage(amount, attacker, combat: true);

        /// <summary>Starvation: quiet chip damage, no hit SFX or numbers.</summary>
        public void Starve(int amount) => ApplyDamage(amount, null, combat: false);

        void ApplyDamage(int amount, Unit attacker, bool combat)
        {
            if (IsDead) return;
            Health = Mathf.Max(0, Health - amount);
            healthBar.Show(Health < Def.MaxHealth);
            healthBar.Set((float)Health / Def.MaxHealth);

            if (combat)
            {
                Audio.AudioManager.Play(Audio.Sfx.Hit);
                flashT = 0.14f;
                Rendering.FloatingText.Spawn(
                    new Vector2(Pos.x, Pos.y + 0.4f), amount.ToString(),
                    Def.IsEnemy ? new Color(1f, 0.95f, 0.5f) : new Color(1f, 0.55f, 0.5f), 0.85f);
            }

            if (Health == 0) Die();
            else if (combat) OnDamaged(attacker);
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
            // Dust puff on death.
            for (int i = 0; i < 4; i++)
                Rendering.FadeOutSprite.Spawn(
                    Pos + Random.insideUnitCircle * 0.3f, Rendering.SpriteFactory.Circle,
                    new Color(0.8f, 0.75f, 0.65f, 0.9f), Random.Range(0.25f, 0.45f), 0.35f);
            OnDie();
            Died?.Invoke(this);
        }
    }
}
