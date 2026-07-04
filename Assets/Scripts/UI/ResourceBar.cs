using SnoopyKnights.Rendering;
using SnoopyKnights.Res;
using UnityEngine;
using UnityEngine.UI;

namespace SnoopyKnights.UI
{
    /// <summary>Top bar showing the four resource stocks (population added in M4).</summary>
    public sealed class ResourceBar : MonoBehaviour
    {
        ResourceStock stock;
        readonly Text[] labels = new Text[ResourceInfo.Count];

        public static ResourceBar Create(Transform parent, ResourceStock stock)
        {
            var rt = UiFactory.Panel(parent, "ResourceBar", new Color(0f, 0f, 0f, 0.5f));
            UiFactory.TopBar(rt, 76f);

            var bar = rt.gameObject.AddComponent<ResourceBar>();
            bar.stock = stock;

            for (int i = 0; i < ResourceInfo.Count; i++)
            {
                var type = (ResourceType)i;
                float x = 46f + i * 250f;

                var icon = UiFactory.Icon(rt, $"Icon{type}", IconFor(type), ResourceInfo.Color(type));
                UiFactory.Place((RectTransform)icon.transform, new Vector2(0f, 0.5f),
                    new Vector2(x, 0f), new Vector2(46f, 46f));

                bar.labels[i] = UiFactory.Label(rt, $"Amount{type}", "0", 40, Color.white);
                UiFactory.Place((RectTransform)bar.labels[i].transform, new Vector2(0f, 0.5f),
                    new Vector2(x + 60f, 0f), new Vector2(160f, 60f));
            }

            stock.Changed += bar.Refresh;
            bar.Refresh();
            return bar;
        }

        static Sprite IconFor(ResourceType t) => t switch
        {
            ResourceType.Wood => SpriteFactory.Triangle,
            ResourceType.Stone => SpriteFactory.Circle,
            ResourceType.Food => SpriteFactory.Circle,
            _ => SpriteFactory.Diamond
        };

        void Refresh()
        {
            for (int i = 0; i < labels.Length; i++)
                labels[i].text = stock.Get((ResourceType)i).ToString();
        }

        void OnDestroy()
        {
            if (stock != null) stock.Changed -= Refresh;
        }
    }
}
