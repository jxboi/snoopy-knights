using SnoopyKnights.Waves;
using UnityEngine;
using UnityEngine.UI;

namespace SnoopyKnights.UI
{
    /// <summary>Top-center banner: next-wave countdown or "defend!" status.</summary>
    public sealed class WavePanel : MonoBehaviour
    {
        WaveManager waves;
        Text label;
        Image bg;

        public static WavePanel Create(Transform parent, WaveManager waves)
        {
            var rt = UiFactory.Panel(parent, "WavePanel", new Color(0f, 0f, 0f, 0.5f));
            UiFactory.Place(rt, new Vector2(0.5f, 1f), new Vector2(0f, -86f), new Vector2(560f, 64f));

            var panel = rt.gameObject.AddComponent<WavePanel>();
            panel.waves = waves;
            panel.bg = rt.GetComponent<Image>();
            panel.label = UiFactory.Label(rt, "Label", "", 34, Color.white, TextAnchor.MiddleCenter);
            UiFactory.Stretch((RectTransform)panel.label.transform);
            return panel;
        }

        void Update()
        {
            if (waves.AllWavesCleared)
            {
                label.text = "All waves survived!";
                bg.color = new Color(0.1f, 0.35f, 0.12f, 0.6f);
            }
            else if (waves.WaveActive)
            {
                label.text = $"Wave {waves.WaveNumber} / {waves.TotalWaves} — defend!";
                bg.color = new Color(0.45f, 0.1f, 0.08f, 0.6f);
            }
            else
            {
                int s = Mathf.Max(0, Mathf.CeilToInt(waves.NextWaveIn));
                label.text = $"Wave {waves.WaveNumber + 1} / {waves.TotalWaves} in {s / 60}:{s % 60:00}";
                bg.color = new Color(0f, 0f, 0f, 0.5f);
            }
        }
    }
}
