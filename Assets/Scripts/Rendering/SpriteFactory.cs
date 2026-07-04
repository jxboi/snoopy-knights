using UnityEngine;

namespace SnoopyKnights.Rendering
{
    /// <summary>
    /// Procedural placeholder sprites so the project ships zero art assets.
    /// All sprites are white; tint via SpriteRenderer.color / Image.color.
    /// Every sprite is 1x1 world units at scale 1.
    /// </summary>
    public static class SpriteFactory
    {
        static Sprite square, circle, diamond, triangle;

        public static Sprite Square => square != null ? square : square = MakeSquare();
        public static Sprite Circle => circle != null ? circle : circle = MakeShape(64, (x, y) =>
        {
            float dx = x - 31.5f, dy = y - 31.5f;
            return Mathf.Sqrt(dx * dx + dy * dy) <= 30f;
        });
        public static Sprite Diamond => diamond != null ? diamond : diamond = MakeShape(64, (x, y) =>
            Mathf.Abs(x - 31.5f) + Mathf.Abs(y - 31.5f) <= 30f);
        public static Sprite Triangle => triangle != null ? triangle : triangle = MakeShape(64, (x, y) =>
        {
            float t = y / 63f; // wide at bottom, point at top
            return Mathf.Abs(x - 31.5f) <= 30f * (1f - t) && y >= 2 && y <= 61;
        });

        static Sprite MakeSquare()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color32[16];
            for (int i = 0; i < 16; i++) pixels[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }

        static Sprite MakeShape(int size, System.Func<int, int, bool> inside)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    pixels[y * size + x] = inside(x, y)
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(255, 255, 255, 0);
            tex.SetPixels32(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        /// <summary>Creates a child GameObject with a tinted SpriteRenderer.</summary>
        public static SpriteRenderer NewRenderer(
            Transform parent, string name, Sprite sprite, Color color,
            int sortingOrder, Vector2 localPos = default, float scale = 1f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;
            return sr;
        }
    }

    /// <summary>Sprite sorting bands, back to front.</summary>
    public static class SortLayer
    {
        public const int Ground = 0;
        public const int Road = 10;
        public const int NodeOverlay = 20;   // trees, rocks
        public const int Building = 100;
        public const int Unit = 200;
        public const int Projectile = 300;
        public const int Bar = 400;          // health/progress bars
        public const int Highlight = 500;
    }
}
