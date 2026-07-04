using SnoopyKnights.Rendering;
using SnoopyKnights.Res;
using UnityEngine;
using UnityEngine.UI;

namespace SnoopyKnights.UI
{
    /// <summary>Top bar showing the four resource stocks and the population.</summary>
    public sealed class ResourceBar : MonoBehaviour
    {
        Core.Game game;
        ResourceStock stock;
        readonly Text[] labels = new Text[ResourceInfo.Count];
        Text popLabel;

        public static ResourceBar Create(Transform parent, Core.Game game)
        {
            var stock = game.Stock;
            var rt = UiFactory.Panel(parent, "ResourceBar", new Color(0f, 0f, 0f, 0.5f));
            UiFactory.TopBar(rt, 76f);

            var bar = rt.gameObject.AddComponent<ResourceBar>();
            bar.game = game;
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

            var popIcon = UiFactory.Icon(rt, "IconPop", SpriteFactory.Circle,
                new Color(0.85f, 0.85f, 1f));
            UiFactory.Place((RectTransform)popIcon.transform, new Vector2(1f, 0.5f),
                new Vector2(-240f, 0f), new Vector2(46f, 46f));

            bar.popLabel = UiFactory.Label(rt, "Population", "0/0", 40, Color.white);
            UiFactory.Place((RectTransform)bar.popLabel.transform, new Vector2(1f, 0.5f),
                new Vector2(-180f, 0f), new Vector2(170f, 60f));

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

        void Update()
        {
            if (game.Economy == null) return;
            popLabel.text = $"{game.Economy.Population}/{game.Economy.PopulationCap}";

            // Flash the food count red while starving.
            var foodLabel = labels[(int)ResourceType.Food];
            foodLabel.color = game.Economy.Starving && Mathf.PingPong(Time.unscaledTime, 0.6f) > 0.3f
                ? new Color(1f, 0.3f, 0.25f)
                : Color.white;
        }

        void OnDestroy()
        {
            if (stock != null) stock.Changed -= Refresh;
        }
    }
}
