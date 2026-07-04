using SnoopyKnights.Buildings;
using SnoopyKnights.Grid;
using SnoopyKnights.Rendering;
using SnoopyKnights.Units;
using UnityEngine;

namespace SnoopyKnights.Core
{
    /// <summary>
    /// Handles taps in the default input mode: tap a unit or building to
    /// select it, tap empty ground to deselect.
    /// </summary>
    public sealed class SelectionController : MonoBehaviour
    {
        const float UnitTapRadius = 0.7f;

        public event System.Action<Building> BuildingSelected;
        public event System.Action<Unit> UnitSelected;

        GridMap map;
        UnitManager units;
        SpriteRenderer highlight;

        public Building SelectedBuilding { get; private set; }
        public Unit SelectedUnit { get; private set; }

        public void Init(GridMap gridMap, InputRouter input, BuildingManager buildings, UnitManager unitManager)
        {
            map = gridMap;
            units = unitManager;
            input.TapWorld += OnTapWorld;
            buildings.Removed += b =>
            {
                if (b == SelectedBuilding) Deselect();
            };
            unitManager.UnitDied += u =>
            {
                if (u == SelectedUnit) Deselect();
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

            // Units first (they're small and walk over building entrances).
            var unit = units.FindNearest(world, UnitTapRadius, u => u != SelectedUnit);
            if (unit != null)
            {
                SelectUnit(unit);
                return;
            }

            var building = map.Get(t).Occupant as Building;
            if (building != null && building != SelectedBuilding)
                SelectBuilding(building);
            else
                Deselect();
        }

        void SelectUnit(Unit u)
        {
            Clear();
            SelectedUnit = u;
            highlight.transform.SetParent(u.transform, false);
            highlight.transform.localPosition = Vector3.zero;
            highlight.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            highlight.gameObject.SetActive(true);
            UnitSelected?.Invoke(u);
        }

        void SelectBuilding(Building b)
        {
            Clear();
            SelectedBuilding = b;
            highlight.transform.position = b.CenterWorld;
            highlight.transform.localScale = new Vector3(b.Def.Width + 0.15f, b.Def.Height + 0.15f, 1f);
            highlight.gameObject.SetActive(true);
            BuildingSelected?.Invoke(b);
        }

        void Clear()
        {
            highlight.transform.SetParent(transform, false);
            if (SelectedBuilding != null)
            {
                SelectedBuilding = null;
                BuildingSelected?.Invoke(null);
            }
            if (SelectedUnit != null)
            {
                SelectedUnit = null;
                UnitSelected?.Invoke(null);
            }
        }

        public void Deselect()
        {
            Clear();
            highlight.gameObject.SetActive(false);
        }
    }
}
