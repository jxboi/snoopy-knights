using UnityEngine;

namespace SnoopyKnights.UI
{
    /// <summary>Fits its RectTransform to Screen.safeArea (iPhone notch/home bar).</summary>
    public sealed class SafeArea : MonoBehaviour
    {
        Rect applied = Rect.zero;

        void Update()
        {
            if (Screen.safeArea != applied)
                Apply();
        }

        void Apply()
        {
            applied = Screen.safeArea;
            var rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(applied.xMin / Screen.width, applied.yMin / Screen.height);
            rt.anchorMax = new Vector2(applied.xMax / Screen.width, applied.yMax / Screen.height);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
