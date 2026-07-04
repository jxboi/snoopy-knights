using SnoopyKnights.Grid;
using SnoopyKnights.Rendering;
using UnityEngine;

namespace SnoopyKnights.Core
{
    /// <summary>
    /// Handles taps in the default input mode: selecting tiles (and later,
    /// buildings and units). Shows a highlight over the selection.
    /// </summary>
    public sealed class SelectionController : MonoBehaviour
    {
        public event System.Action<Vector2Int?> TileSelected;

        GridMap map;
        SpriteRenderer highlight;
        Vector2Int? selectedTile;

        public Vector2Int? SelectedTile => selectedTile;

        public void Init(GridMap gridMap, InputRouter input)
        {
            map = gridMap;
            input.TapWorld += OnTapWorld;

            highlight = SpriteFactory.NewRenderer(
                transform, "TileHighlight", SpriteFactory.Square,
                new Color(1f, 1f, 1f, 0.35f), SortLayer.Highlight);
            highlight.gameObject.SetActive(false);
        }

        void OnTapWorld(Vector2 world)
        {
            var t = GridMap.WorldToTile(world);
            if (!map.InBounds(t) || (selectedTile.HasValue && selectedTile.Value == t))
                Deselect();
            else
                SelectTile(t);
        }

        void SelectTile(Vector2Int t)
        {
            selectedTile = t;
            highlight.transform.position = GridMap.TileCenter(t);
            highlight.gameObject.SetActive(true);
            TileSelected?.Invoke(t);
        }

        public void Deselect()
        {
            selectedTile = null;
            highlight.gameObject.SetActive(false);
            TileSelected?.Invoke(null);
        }
    }
}
