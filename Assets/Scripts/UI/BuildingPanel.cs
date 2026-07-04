using SnoopyKnights.Buildings;
using SnoopyKnights.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SnoopyKnights.UI
{
    /// <summary>Bottom-left info panel for the selected building.</summary>
    public sealed class BuildingPanel : MonoBehaviour
    {
        Game game;
        Building current;
        RectTransform root;
        Text title, status;
        Button demolishBtn;

        public static BuildingPanel Create(Transform parent, Game game)
        {
            var root = UiFactory.Panel(parent, "BuildingPanel", new Color(0f, 0f, 0f, 0.62f));
            UiFactory.Place(root, new Vector2(0f, 0f), new Vector2(24f, 24f), new Vector2(520f, 290f));

            var panel = root.gameObject.AddComponent<BuildingPanel>();
            panel.game = game;
            panel.root = root;
            panel.BuildUi();
            game.Selection.BuildingSelected += panel.Show;
            root.gameObject.SetActive(false);
            return panel;
        }

        void BuildUi()
        {
            title = UiFactory.Label(root, "Title", "", 40, Color.white);
            UiFactory.Place((RectTransform)title.transform, new Vector2(0f, 1f),
                new Vector2(24f, -14f), new Vector2(380f, 50f));
            title.alignment = TextAnchor.UpperLeft;

            var closeBtn = UiFactory.Button(root, "Close", "X", 36,
                new Color(0.35f, 0.3f, 0.25f, 0.9f), () => game.Selection.Deselect());
            UiFactory.Place((RectTransform)closeBtn.transform, new Vector2(1f, 1f),
                new Vector2(-10f, -10f), new Vector2(76f, 76f));

            status = UiFactory.Label(root, "Status", "", 28,
                new Color(0.9f, 0.9f, 0.85f), TextAnchor.UpperLeft, wrap: true);
            UiFactory.Place((RectTransform)status.transform, new Vector2(0f, 1f),
                new Vector2(24f, -72f), new Vector2(470f, 120f));

            demolishBtn = UiFactory.Button(root, "Demolish", "Demolish", 32,
                new Color(0.55f, 0.18f, 0.15f, 0.95f), Demolish);
            UiFactory.Place((RectTransform)demolishBtn.transform, new Vector2(1f, 0f),
                new Vector2(-16f, 16f), new Vector2(220f, 90f));
        }

        void Show(Building b)
        {
            current = b;
            root.gameObject.SetActive(b != null);
            if (b == null) return;
            title.text = b.Def.Name;
            demolishBtn.gameObject.SetActive(b.Def.CanDemolish);
            Refresh();
        }

        void Update()
        {
            if (current != null) Refresh();
        }

        void Refresh()
        {
            string state = current.State == BuildingState.Construction
                ? $"Under construction  {(int)(current.ConstructionProgress * 100)}%"
                : "Operational";

            var sb = new System.Text.StringBuilder();
            sb.Append(state).Append("\nHP ").Append(current.Health).Append('/').Append(current.MaxHealth);
            if (current.Def.NeedsWorker && current.IsOperational)
                sb.Append(current.AssignedWorker != null ? "\nWorker: staffed" : "\nWorker: needed");
            if (current.Def.Produces.HasValue)
                sb.Append("\nAwaiting pickup: ").Append(current.OutputBuffer);
            sb.Append('\n').Append(current.Def.Description);
            status.text = sb.ToString();
        }

        void Demolish()
        {
            if (current == null) return;
            game.Buildings.Demolish(current);
            game.Selection.Deselect();
        }
    }
}
