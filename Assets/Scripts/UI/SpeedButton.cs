using SnoopyKnights.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SnoopyKnights.UI
{
    /// <summary>1x / 2x fast-forward toggle, top-right next to the pause button.</summary>
    public sealed class SpeedButton : MonoBehaviour
    {
        public static SpeedButton Create(Transform parent)
        {
            Text label = null;
            var btn = UiFactory.Button(parent, "SpeedBtn", "1x", 34,
                new Color(0.25f, 0.2f, 0.12f, 0.9f), () =>
                {
                    GameSpeed.Toggle();
                    label.text = GameSpeed.Multiplier + "x";
                    label.color = GameSpeed.Multiplier > 1
                        ? new Color(1f, 0.85f, 0.4f) : Color.white;
                });
            UiFactory.Place((RectTransform)btn.transform, new Vector2(1f, 1f),
                new Vector2(-128f, -6f), new Vector2(104f, 64f));
            label = btn.GetComponentInChildren<Text>();
            return btn.gameObject.AddComponent<SpeedButton>();
        }
    }
}
