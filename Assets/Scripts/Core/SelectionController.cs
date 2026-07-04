using SnoopyKnights.Buildings;
using SnoopyKnights.Grid;
using SnoopyKnights.Rendering;
using UnityEngine;

namespace SnoopyKnights.Core
{
    /// <summary>
    /// Handles taps in the default input mode: tap a building to select it,
    /// tap empty ground to deselect. (Unit selection arrives with units.)
    /// </summary>
    public sealed class SelectionController : MonoBehaviour
    {
        public event System.Action<Building> BuildingSelected;

        GridMap map;
        SpriteRenderer highlight;

        public Building SelectedBuilding { get; private set; }

        public void Init(GridMap gridMap, InputRouter input, BuildingManager buildings)
        {
            map = gridMap;
            input.TapWorld += OnTapWorld;
            buildings.Removed += b =>
            {
                if (b == SelectedBuilding) Deselect();
            };

            highlight = SpriteFactory.NewRenderer(
                transform, "SelectionHighlight", SpriteFactory.Square,
                new Color(1f, 1f, 1f, 0.3f), SortLayer.Highlight);
            highlight.gameObject.SetActive(false);
        }

        void OnTapWorld(Vector2 world)
        {
            var t = GridMap.WorldToTile(world);
            if (!map.InBounds(t))
            {
                Deselect();
                return;
            }

            var building = map.Get(t).Occupant as Building;
            if (building != null && building != SelectedBuilding)
                SelectBuilding(building);
            else
                Deselect();
        }

        void SelectBuilding(Building b)
        {
            SelectedBuilding = b;
            highlight.transform.position = b.CenterWorld;
            highlight.transform.localScale = new Vector3(b.Def.Width + 0.15f, b.Def.Height + 0.15f, 1f);
            highlight.gameObject.SetActive(true);
            BuildingSelected?.Invoke(b);
        }

        public void Deselect()
        {
            if (SelectedBuilding == null && !highlight.gameObject.activeSelf) return;
            SelectedBuilding = null;
            highlight.gameObject.SetActive(false);
            BuildingSelected?.Invoke(null);
        }
    }
}
