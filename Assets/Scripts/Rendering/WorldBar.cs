using UnityEngine;

namespace SnoopyKnights.Rendering
{
    /// <summary>Tiny world-space progress/health bar made of two sprites.</summary>
    public sealed class WorldBar : MonoBehaviour
    {
        SpriteRenderer fill;
        float width;

        public static WorldBar Create(Transform parent, Vector2 localPos, float width, Color fillColor)
        {
            var go = new GameObject("Bar");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);
            go.transform.localRotation = ViewTilt.Upright; // face the tilted camera

            var bar = go.AddComponent<WorldBar>();
            bar.width = width;

            var bg = SpriteFactory.NewRenderer(go.transform, "Bg", SpriteFactory.Square,
                new Color(0.08f, 0.08f, 0.08f, 0.85f), SortLayer.Bar);
            bg.transform.localScale = new Vector3(width, 0.14f, 1f);

            bar.fill = SpriteFactory.NewRenderer(go.transform, "Fill", SpriteFactory.Square,
                fillColor, SortLayer.Bar + 1);
            bar.Set(1f);
            return bar;
        }

        public void Set(float t)
        {
            t = Mathf.Clamp01(t);
            float w = (width - 0.03f) * t;
            fill.transform.localScale = new Vector3(w, 0.09f, 1f);
            fill.transform.localPosition = new Vector3(-(width - 0.03f) * 0.5f + w * 0.5f, 0f, 0f);
        }

        public void SetColor(Color c) => fill.color = c;

        public void Show(bool visible) => gameObject.SetActive(visible);
    }
}
