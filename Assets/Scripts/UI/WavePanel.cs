using SnoopyKnights.Audio;
using SnoopyKnights.Waves;
using UnityEngine;
using UnityEngine.UI;

namespace SnoopyKnights.UI
{
    /// <summary>
    /// Top-center banner: next-wave countdown with the scouted attack direction,
    /// enemies remaining during an attack. Pulses and ticks as a wave closes in.
    /// </summary>
    public sealed class WavePanel : MonoBehaviour
    {
        const float AlarmSeconds = 10f; // pulse red this close to a wave
        const int TickSeconds = 5;      // audible countdown ticks start here

        WaveManager waves;
        Text label, subLabel;
        Image bg;
        int lastTickSecond = -1;

        public static WavePanel Create(Transform parent, WaveManager waves)
        {
            var rt = UiFactory.Panel(parent, "WavePanel", new Color(0f, 0f, 0f, 0.5f));
            UiFactory.Place(rt, new Vector2(0.5f, 1f), new Vector2(0f, -86f), new Vector2(560f, 88f));

            var panel = rt.gameObject.AddComponent<WavePanel>();
            panel.waves = waves;
            panel.bg = rt.GetComponent<Image>();
            panel.label = UiFactory.Label(rt, "Label", "", 34, Color.white, TextAnchor.MiddleCenter);
            UiFactory.Place((RectTransform)panel.label.transform, new Vector2(0.5f, 1f),
                new Vector2(0f, -6f), new Vector2(560f, 44f));
            panel.subLabel = UiFactory.Label(rt, "Sub", "", 24,
                new Color(0.85f, 0.8f, 0.7f), TextAnchor.MiddleCenter);
            UiFactory.Place((RectTransform)panel.subLabel.transform, new Vector2(0.5f, 0f),
                new Vector2(0f, 8f), new Vector2(560f, 30f));
            return panel;
        }

        void Update()
        {
            if (waves.AllWavesCleared)
            {
                label.text = "All waves survived!";
                subLabel.text = "";
                bg.color = new Color(0.1f, 0.35f, 0.12f, 0.6f);
            }
            else if (waves.WaveActive)
            {
                label.text = $"Wave {waves.WaveNumber} / {waves.TotalWaves} — defend!";
                int left = waves.EnemiesAlive;
                subLabel.text = left == 1 ? "1 enemy remains" : $"{left} enemies remain";
                bg.color = new Color(0.45f, 0.1f, 0.08f, 0.6f);
                lastTickSecond = -1;
            }
            else
            {
                float t = Mathf.Max(0f, waves.NextWaveIn);
                int s = Mathf.CeilToInt(t);
                label.text = $"Wave {waves.WaveNumber + 1} / {waves.TotalWaves} in {s / 60}:{s % 60:00}";
                subLabel.text = $"scouts report raiders {EdgeText(waves.NextWaveEdges)}";

                if (t <= AlarmSeconds)
                {
                    float pulse = Mathf.PingPong(Time.unscaledTime * 2.4f, 1f);
                    bg.color = Color.Lerp(new Color(0.25f, 0.05f, 0.04f, 0.6f),
                        new Color(0.5f, 0.1f, 0.08f, 0.75f), pulse);
                    if (s != lastTickSecond && s <= TickSeconds)
                        AudioManager.Play(Sfx.Tick);
                    lastTickSecond = s;
                }
                else
                {
                    bg.color = new Color(0f, 0f, 0f, 0.5f);
                    lastTickSecond = -1;
                }
            }
        }

        static string EdgeText(SpawnEdge[] edges)
        {
            if (edges.Length >= 3) return "on all sides";
            if (edges.Length == 2) return $"to the {Name(edges[0])} and {Name(edges[1])}";
            if (edges.Length == 1) return $"to the {Name(edges[0])}";
            return "nearby";
        }

        static string Name(SpawnEdge e) => e switch
        {
            SpawnEdge.North => "north",
            SpawnEdge.East => "east",
            _ => "west"
        };
    }
}
