using System.Collections.Generic;
using SnoopyKnights.Mission;
using UnityEngine;
using UnityEngine.UI;

namespace SnoopyKnights.UI
{
    /// <summary>Small objective checklist under the resource bar.</summary>
    public sealed class ObjectivesPanel : MonoBehaviour
    {
        MissionController mission;
        readonly List<Text> rows = new List<Text>();
        float refreshT;

        public static ObjectivesPanel Create(Transform parent, MissionController mission)
        {
            var rt = UiFactory.Panel(parent, "Objectives", new Color(0f, 0f, 0f, 0.35f));

            var panel = rt.gameObject.AddComponent<ObjectivesPanel>();
            panel.mission = mission;

            int n = mission.Objectives.Count;
            UiFactory.Place(rt, new Vector2(0f, 1f), new Vector2(16f, -92f),
                new Vector2(520f, 26f + n * 38f));

            for (int i = 0; i < n; i++)
            {
                var row = UiFactory.Label(rt, $"Row{i}", "", 26, Color.white);
                UiFactory.Place((RectTransform)row.transform, new Vector2(0f, 1f),
                    new Vector2(18f, -12f - i * 38f), new Vector2(490f, 34f));
                row.alignment = TextAnchor.UpperLeft;
                panel.rows.Add(row);
            }
            panel.Refresh();
            return panel;
        }

        void Update()
        {
            refreshT -= Time.unscaledDeltaTime;
            if (refreshT > 0f) return;
            refreshT = 0.5f;
            Refresh();
        }

        void Refresh()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                var o = mission.Objectives[i];
                rows[i].text = (o.Done ? "[x] " : "[  ] ") + o.Text;
                rows[i].color = o.Done ? new Color(0.6f, 0.95f, 0.6f) : Color.white;
            }
        }
    }
}
