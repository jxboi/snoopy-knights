using SnoopyKnights.Grid;
using SnoopyKnights.Rendering;
using UnityEngine;

namespace SnoopyKnights.Buildings
{
    public enum BuildingState { Construction, Operational }

    /// <summary>
    /// A placed building: construction progress, health, and procedural visuals.
    /// Production logic lives in the economy layer, not here.
    /// </summary>
    public sealed class Building : MonoBehaviour, ITileOccupant, Combat.IDamageable
    {
        public BuildingDef Def { get; private set; }
        public Vector2Int Origin { get; private set; }
        public BuildingState State { get; private set; }
        public float ConstructionProgress { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth => Def.MaxHealth;
        public bool IsOperational => State == BuildingState.Operational;

        public bool BlocksMovement => true;

        /// <summary>Raised when construction completes.</summary>
        public event System.Action<Building> Completed;
        /// <summary>Raised when the building's health reaches zero.</summary>
        public event System.Action<Building> Destroyed;

        SpriteRenderer body;
        WorldBar buildBar;
        WorldBar healthBar;
        SpriteRenderer[] outputDots;

        // ---- Economy bookkeeping (used by worker/carrier AI) -----------------

        /// <summary>Finished goods waiting for a carrier.</summary>
        public int OutputBuffer { get; private set; }
        /// <summary>Carriers already on their way to pick up.</summary>
        public int ClaimedOutput;
        /// <summary>Builders working on / walking to this site.</summary>
        public int ClaimedBuilders;
        /// <summary>The production worker staffing this building.</summary>
        public Units.Unit AssignedWorker;

        public bool WantsWorker => Def.NeedsWorker && IsOperational && AssignedWorker == null;
        public bool HasOutputForPickup => OutputBuffer - ClaimedOutput > 0;

        public Vector2 CenterWorld => new Vector2(Origin.x + Def.Width * 0.5f, Origin.y + Def.Height * 0.5f);

        /// <summary>The tile in front of the building where units interact with it.</summary>
        public Vector2Int EntranceTile => new Vector2Int(Origin.x + Def.Width / 2, Origin.y - 1);

        /// <summary>True if the tile touches the footprint (units can work from there).</summary>
        public bool IsAdjacentToFootprint(Vector2Int t) =>
            t.x >= Origin.x - 1 && t.x <= Origin.x + Def.Width &&
            t.y >= Origin.y - 1 && t.y <= Origin.y + Def.Height;

        public Vector2 ClosestPoint(Vector2 from) => new Vector2(
            Mathf.Clamp(from.x, Origin.x, Origin.x + Def.Width),
            Mathf.Clamp(from.y, Origin.y, Origin.y + Def.Height));

        /// <summary>Distance to the footprint edge (0 when touching), for attack range checks.</summary>
        public float DistanceTo(Vector2 from) => Vector2.Distance(from, ClosestPoint(from));

        public Vector2Int ClosestFootprintTile(Vector2 from)
        {
            var p = ClosestPoint(from);
            return new Vector2Int(
                Mathf.Clamp(Mathf.FloorToInt(p.x), Origin.x, Origin.x + Def.Width - 1),
                Mathf.Clamp(Mathf.FloorToInt(p.y), Origin.y, Origin.y + Def.Height - 1));
        }

        public void Init(BuildingDef def, Vector2Int origin, bool instant)
        {
            Def = def;
            Origin = origin;
            Health = def.MaxHealth;
            State = BuildingState.Construction;
            transform.position = CenterWorld;
            BuildVisuals();

            if (instant) FinishConstruction();
            else SetProgress(0f);
        }

        void BuildVisuals()
        {
            float w = Def.Width, h = Def.Height;

            SpriteFactory.NewRenderer(transform, "Border", SpriteFactory.Square,
                new Color(0.16f, 0.12f, 0.08f), SortLayer.Building)
                .transform.localScale = new Vector3(w - 0.06f, h - 0.06f, 1f);

            body = SpriteFactory.NewRenderer(transform, "Body", SpriteFactory.Square,
                Def.BodyColor, SortLayer.Building + 1);
            body.transform.localScale = new Vector3(w - 0.22f, h - 0.22f, 1f);

            var iconSprite = Def.Icon switch
            {
                IconShape.Circle => SpriteFactory.Circle,
                IconShape.Diamond => SpriteFactory.Diamond,
                IconShape.Triangle => SpriteFactory.Triangle,
                _ => SpriteFactory.Square
            };
            SpriteFactory.NewRenderer(transform, "Icon", iconSprite, Def.IconColor,
                SortLayer.Building + 2, Vector2.zero, Mathf.Min(w, h) * 0.5f);

            buildBar = WorldBar.Create(transform, new Vector2(0f, -h * 0.5f + 0.18f),
                w * 0.8f, new Color(0.35f, 0.7f, 1f));
            healthBar = WorldBar.Create(transform, new Vector2(0f, h * 0.5f - 0.18f),
                w * 0.8f, new Color(0.3f, 0.85f, 0.3f));
            healthBar.Show(false);

            if (Def.Produces.HasValue)
            {
                outputDots = new SpriteRenderer[Def.OutputCap];
                var dotColor = Res.ResourceInfo.Color(Def.Produces.Value);
                for (int i = 0; i < outputDots.Length; i++)
                {
                    outputDots[i] = SpriteFactory.NewRenderer(transform, "OutputDot",
                        SpriteFactory.Square, dotColor, SortLayer.Building + 3,
                        new Vector2((i - (outputDots.Length - 1) * 0.5f) * 0.3f, -h * 0.5f + 0.42f), 0.2f);
                    outputDots[i].gameObject.SetActive(false);
                }
            }
        }

        // ---- Output buffer ---------------------------------------------------

        public bool AddOutput(int amount)
        {
            if (!Def.Produces.HasValue || OutputBuffer >= Def.OutputCap) return false;
            OutputBuffer = Mathf.Min(Def.OutputCap, OutputBuffer + amount);
            RefreshOutputDots();
            return true;
        }

        public bool TryTakeOutput()
        {
            if (OutputBuffer <= 0) return false;
            OutputBuffer--;
            RefreshOutputDots();
            return true;
        }

        /// <summary>Used by save/load.</summary>
        public void SetOutputBuffer(int value)
        {
            OutputBuffer = Mathf.Clamp(value, 0, Def.OutputCap);
            RefreshOutputDots();
        }

        /// <summary>Used by save/load: restore a partially built site.</summary>
        public void RestoreConstruction(float progress)
        {
            if (State == BuildingState.Construction)
                SetProgress(Mathf.Clamp(progress, 0f, 0.999f));
        }

        /// <summary>Used by save/load: set health without triggering damage events.</summary>
        public void RestoreHealth(int value)
        {
            Health = Mathf.Clamp(value, 1, MaxHealth);
            healthBar.Show(Health < MaxHealth);
            healthBar.Set((float)Health / MaxHealth);
        }

        void RefreshOutputDots()
        {
            if (outputDots == null) return;
            for (int i = 0; i < outputDots.Length; i++)
                outputDots[i].gameObject.SetActive(i < OutputBuffer);
        }

        // ---- Construction --------------------------------------------------

        public void AdvanceConstruction(float seconds)
        {
            if (State != BuildingState.Construction) return;
            SetProgress(ConstructionProgress + seconds / Mathf.Max(0.1f, Def.BuildSeconds));
        }

        void SetProgress(float t)
        {
            ConstructionProgress = Mathf.Clamp01(t);
            var c = body.color;
            c.a = 0.35f + 0.65f * ConstructionProgress;
            body.color = c;
            buildBar.Set(ConstructionProgress);
            if (ConstructionProgress >= 1f)
                FinishConstruction();
        }

        void FinishConstruction()
        {
            ConstructionProgress = 1f;
            State = BuildingState.Operational;
            var c = body.color; c.a = 1f; body.color = c;
            buildBar.Show(false);
            Completed?.Invoke(this);
        }

        // ---- Damage ----------------------------------------------------------

        public bool IsAlive => Health > 0;
        public Vector2 AimPoint => CenterWorld;

        public void TakeDamage(int amount)
        {
            if (Health <= 0) return;
            Audio.AudioManager.Play(Audio.Sfx.Hit);
            Health = Mathf.Max(0, Health - amount);
            healthBar.Show(Health < MaxHealth);
            healthBar.Set((float)Health / MaxHealth);
            if (Health == 0)
                Destroyed?.Invoke(this);
        }
    }
}
