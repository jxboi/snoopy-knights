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
    public sealed class Building : MonoBehaviour, ITileOccupant
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

        public Vector2 CenterWorld => new Vector2(Origin.x + Def.Width * 0.5f, Origin.y + Def.Height * 0.5f);

        /// <summary>The tile in front of the building where units interact with it.</summary>
        public Vector2Int EntranceTile => new Vector2Int(Origin.x + Def.Width / 2, Origin.y - 1);

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

        public void TakeDamage(int amount)
        {
            if (Health <= 0) return;
            Health = Mathf.Max(0, Health - amount);
            healthBar.Show(Health < MaxHealth);
            healthBar.Set((float)Health / MaxHealth);
            if (Health == 0)
                Destroyed?.Invoke(this);
        }
    }
}
