using SnoopyKnights.Core;
using SnoopyKnights.Units;
using UnityEngine;
using UnityEngine.UI;

namespace SnoopyKnights.UI
{
    /// <summary>Bottom-left info panel for the selected unit.</summary>
    public sealed class UnitPanel : MonoBehaviour
    {
        Game game;
        Unit current;
        RectTransform root;
        Text title, status;

        public static UnitPanel Create(Transform parent, Game game)
        {
            var root = UiFactory.Panel(parent, "UnitPanel", new Color(0f, 0f, 0f, 0.62f));
            UiFactory.Place(root, new Vector2(0f, 0f), new Vector2(24f, 24f), new Vector2(460f, 210f));

            var panel = root.gameObject.AddComponent<UnitPanel>();
            panel.game = game;
            panel.root = root;
            panel.BuildUi();
            game.Selection.UnitSelected += panel.Show;
            root.gameObject.SetActive(false);
            return panel;
        }

        void BuildUi()
        {
            title = UiFactory.Label(root, "Title", "", 40, Color.white);
            UiFactory.Place((RectTransform)title.transform, new Vector2(0f, 1f),
                new Vector2(24f, -14f), new Vector2(320f, 50f));
            title.alignment = TextAnchor.UpperLeft;

            var closeBtn = UiFactory.Button(root, "Close", "X", 36,
                new Color(0.35f, 0.3f, 0.25f, 0.9f), () => game.Selection.Deselect());
            UiFactory.Place((RectTransform)closeBtn.transform, new Vector2(1f, 1f),
                new Vector2(-10f, -10f), new Vector2(76f, 76f));

            status = UiFactory.Label(root, "Status", "", 28,
                new Color(0.9f, 0.9f, 0.85f), TextAnchor.UpperLeft, wrap: true);
            UiFactory.Place((RectTransform)status.transform, new Vector2(0f, 1f),
                new Vector2(24f, -72f), new Vector2(410f, 120f));
        }

        void Show(Unit u)
        {
            current = u;
            root.gameObject.SetActive(u != null);
            if (u == null) return;
            title.text = u.Def.Name;
            Refresh();
        }

        void Update()
        {
            if (current == null)
            {
                if (root.gameObject.activeSelf) root.gameObject.SetActive(false);
                return;
            }
            Refresh();
        }

        void Refresh()
        {
            string hint = current is SoldierUnit ? "\nTap the map to move this soldier." : "";
            status.text = $"{current.Status}\nHP {current.Health}/{current.Def.MaxHealth}{hint}";
        }
    }
}
