using UnityEngine;

namespace SnoopyKnights.Rendering
{
    /// <summary>A short world-space label that rises and fades (damage, +resources).</summary>
    public sealed class FloatingText : MonoBehaviour
    {
        const float Lifetime = 0.8f;
        const float RiseSpeed = 1.1f;

        TextMesh mesh;
        float life;

        public static void Spawn(Vector2 pos, string text, Color color, float size = 1f)
        {
            var go = new GameObject("FloatingText");
            go.transform.position = new Vector3(pos.x, pos.y + 0.3f, 0f);
            go.transform.rotation = ViewTilt.Upright; // face the tilted camera

            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.font = UI.UiFactory.Font;
            mesh.GetComponent<MeshRenderer>().sharedMaterial = mesh.font.material;
            mesh.fontSize = 48;
            mesh.characterSize = 0.06f * size;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;
            mesh.GetComponent<MeshRenderer>().sortingOrder = SortLayer.Bar + 20;

            var ft = go.AddComponent<FloatingText>();
            ft.mesh = mesh;
        }

        void Update()
        {
            life += Time.deltaTime;
            transform.position += transform.up * (RiseSpeed * Time.deltaTime);
            float t = life / Lifetime;
            var c = mesh.color;
            c.a = 1f - t * t;
            mesh.color = c;
            // gentle pop-in scale
            float s = Mathf.Min(1f, life / 0.12f);
            transform.localScale = new Vector3(s, s, 1f);
            if (life >= Lifetime) Destroy(gameObject);
        }
    }
}
