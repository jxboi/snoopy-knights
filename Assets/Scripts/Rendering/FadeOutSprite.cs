using UnityEngine;

namespace SnoopyKnights.Rendering
{
    /// <summary>
    /// A short-lived sprite that fades out and destroys itself (move markers,
    /// hit puffs, smoke). Optional drift velocity in world units/second.
    /// </summary>
    public sealed class FadeOutSprite : MonoBehaviour
    {
        SpriteRenderer sr;
        Vector2 velocity;
        float life, maxLife, startAlpha;

        public static void Spawn(Vector2 pos, Sprite sprite, Color color, float scale,
            float seconds, Vector2 velocity = default)
        {
            var go = new GameObject("Fx");
            go.transform.position = pos;
            var fx = go.AddComponent<FadeOutSprite>();
            fx.maxLife = fx.life = seconds;
            fx.velocity = velocity;
            fx.startAlpha = color.a;
            fx.sr = SpriteFactory.NewRenderer(go.transform, "Sprite", sprite, color,
                SortLayer.Highlight, Vector2.zero, scale);
        }

        void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0f) { Destroy(gameObject); return; }
            transform.position += (Vector3)(velocity * Time.deltaTime);
            var c = sr.color;
            c.a = startAlpha * (life / maxLife);
            sr.color = c;
        }
    }
}
