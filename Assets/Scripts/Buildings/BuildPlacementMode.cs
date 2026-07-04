using SnoopyKnights.Core;
using SnoopyKnights.Grid;
using SnoopyKnights.Rendering;
using UnityEngine;

namespace SnoopyKnights.Buildings
{
    /// <summary>
    /// Input mode while placing a building: tap or drag moves the ghost so you
    /// can slide it into position. Confirm/cancel are driven by UI buttons.
    /// </summary>
    public sealed class BuildPlacementMode : IInputMode
    {
        public bool UsesDrag => true;

        public event System.Action Changed;
        /// <summary>Fired when the player taps the map to drop the building at its current spot.</summary>
        public event System.Action PlaceRequested;

        readonly BuildingManager manager;
        readonly GridMap map;
        readonly BuildingDef def;
        GameObject ghost;
        SpriteRenderer[] tileMarks;

        public BuildingDef Def => def;
        public Vector2Int Origin { get; private set; }
        public bool IsValid { get; private set; }

        public BuildPlacementMode(BuildingManager manager, GridMap map, BuildingDef def, Vector2 startWorld)
        {
            this.manager = manager;
            this.map = map;
            this.def = def;
            BuildGhost();
            MoveTo(WorldToOrigin(startWorld));
        }

        public void OnTap(Vector2 world)
        {
            MoveTo(WorldToOrigin(world));
            PlaceRequested?.Invoke();
        }
        public void OnHover(Vector2 world) => MoveTo(WorldToOrigin(world));
        public void OnDragStart(Vector2 world) => MoveTo(WorldToOrigin(world));
        public void OnDrag(Vector2 world) => MoveTo(WorldToOrigin(world));
        public void OnDragEnd(Vector2 world) { }

        Vector2Int WorldToOrigin(Vector2 world)
        {
            var t = GridMap.WorldToTile(world);
            var o = new Vector2Int(t.x - (def.Width - 1) / 2, t.y - (def.Height - 1) / 2);
            o.x = Mathf.Clamp(o.x, 1, map.Width - 1 - def.Width);
            o.y = Mathf.Clamp(o.y, 2, map.Height - 1 - def.Height);
            return o;
        }

        void MoveTo(Vector2Int origin)
        {
            Origin = origin;
            IsValid = manager.CanPlace(def, origin);
            ghost.transform.position = new Vector3(
                origin.x + def.Width * 0.5f, origin.y + def.Height * 0.5f, 0f);

            var markColor = IsValid
                ? new Color(0.3f, 0.9f, 0.3f, 0.4f)
                : new Color(0.9f, 0.25f, 0.2f, 0.5f);
            foreach (var m in tileMarks)
                m.color = markColor;

            Changed?.Invoke();
        }

        void BuildGhost()
        {
            ghost = new GameObject($"Ghost {def.Name}");

            // Show the real building art as a translucent preview (matching the
            // placed building's bottom-center pivot + width scaling). Fall back to
            // a tinted square when no art is available.
            var artSprite = SpriteBank.Building(def.Type);
            if (artSprite != null)
            {
                var body = SpriteFactory.NewRenderer(ghost.transform, "Body", artSprite,
                    new Color(1f, 1f, 1f, 0.6f), SortLayer.Highlight + 1,
                    new Vector2(0f, -def.Height * 0.5f));
                float unitWidth = artSprite.bounds.size.x;
                float scale = unitWidth > 0.01f ? def.Width / unitWidth : 1f;
                body.transform.localScale = new Vector3(scale, scale, 1f);
            }
            else
            {
                var body = SpriteFactory.NewRenderer(ghost.transform, "Body", SpriteFactory.Square,
                    new Color(def.BodyColor.r, def.BodyColor.g, def.BodyColor.b, 0.55f),
                    SortLayer.Highlight + 1);
                body.transform.localScale = new Vector3(def.Width - 0.2f, def.Height - 0.2f, 1f);
            }

            tileMarks = new SpriteRenderer[def.Width * def.Height + 1];
            int i = 0;
            for (int y = 0; y < def.Height; y++)
                for (int x = 0; x < def.Width; x++)
                    tileMarks[i++] = SpriteFactory.NewRenderer(ghost.transform, "Mark",
                        SpriteFactory.Square, Color.white, SortLayer.Highlight,
                        new Vector2(x - (def.Width - 1) * 0.5f, y - (def.Height - 1) * 0.5f), 0.96f);

            // Entrance marker below the footprint so players learn the access side.
            tileMarks[i] = SpriteFactory.NewRenderer(ghost.transform, "Entrance",
                SpriteFactory.Triangle, Color.white, SortLayer.Highlight,
                new Vector2(def.Width / 2 - (def.Width - 1) * 0.5f, -(def.Height - 1) * 0.5f - 1f), 0.5f);
        }

        /// <summary>Places the building. Returns it, or null if invalid/unaffordable.</summary>
        public Building Confirm()
        {
            var placed = manager.Place(def.Type, Origin);
            if (placed != null) Exit();
            return placed;
        }

        public void Exit()
        {
            if (ghost != null) Object.Destroy(ghost);
            ghost = null;
        }
    }
}
