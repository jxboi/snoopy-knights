using UnityEngine;

namespace SnoopyKnights.Rendering
{
    /// <summary>A short-lived sprite that fades out and destroys itself (move markers, hit flashes).</summary>
    public sealed class FadeOutSprite : MonoBehaviour
    {
        SpriteRenderer sr;
        float life, maxLife;

        public static void Spawn(Vector2 pos, Sprite sprite, Color color, float scale, float seconds)
        {
            var go = new GameObject("Fx");
            go.transform.position = pos;
            var fx = go.AddComponent<FadeOutSprite>();
            fx.maxLife = fx.life = seconds;
            fx.sr = SpriteFactory.NewRenderer(go.transform, "Sprite", sprite, color,
                SortLayer.Highlight, Vector2.zero, scale);
        }

        void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0f) { Destroy(gameObject); return; }
            var c = sr.color;
            c.a = life / maxLife;
            sr.color = c;
        }
    }
}
