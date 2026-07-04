using System.Collections.Generic;
using SnoopyKnights.Audio;
using SnoopyKnights.Mission;
using UnityEngine;
using UnityEngine.UI;

namespace SnoopyKnights.UI
{
    /// <summary>
    /// Small objective checklist under the resource bar. Completing an
    /// objective chimes and flashes its row.
    /// </summary>
    public sealed class ObjectivesPanel : MonoBehaviour
    {
        static readonly Color DoneColor = new Color(0.6f, 0.95f, 0.6f);
        static readonly Color FlashColor = new Color(1f, 1f, 0.55f);

        MissionController mission;
        readonly List<Text> rows = new List<Text>();
        bool[] wasDone;
        float[] flash;
        float refreshT;
        float chimeMutedUntil; // objectives already done on (re)load stay quiet

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
            panel.wasDone = new bool[n];
            panel.flash = new float[n];
            panel.chimeMutedUntil = Time.unscaledTime + 1.5f;
            panel.Refresh();
            return panel;
        }

        void Update()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (flash[i] <= 0f) continue;
                flash[i] -= Time.unscaledDeltaTime;
                rows[i].color = Color.Lerp(DoneColor, FlashColor, Mathf.Clamp01(flash[i]));
            }

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
                if (o.Done && !wasDone[i])
                {
                    wasDone[i] = true;
                    if (Time.unscaledTime >= chimeMutedUntil)
                    {
                        AudioManager.Play(Sfx.Objective);
                        flash[i] = 1f;
                    }
                }
                rows[i].text = (o.Done ? "[x] " : "[  ] ") + o.Text;
                if (flash[i] <= 0f)
                    rows[i].color = o.Done ? DoneColor : Color.white;
            }
        }
    }
}
