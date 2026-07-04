using UnityEngine;

namespace SnoopyKnights.Res
{
    public enum ResourceType { Wood, Stone, Food, Gold }

    [System.Serializable]
    public struct ResourceAmount
    {
        public ResourceType Type;
        public int Amount;

        public ResourceAmount(ResourceType type, int amount)
        {
            Type = type;
            Amount = amount;
        }
    }

    public static class ResourceInfo
    {
        public const int Count = 4;

        public static string ShortName(ResourceType t) => t switch
        {
            ResourceType.Wood => "wood",
            ResourceType.Stone => "stone",
            ResourceType.Food => "food",
            _ => "gold"
        };

        public static Color Color(ResourceType t) => t switch
        {
            ResourceType.Wood => new Color(0.72f, 0.5f, 0.25f),
            ResourceType.Stone => new Color(0.75f, 0.75f, 0.78f),
            ResourceType.Food => new Color(0.95f, 0.83f, 0.35f),
            _ => new Color(1f, 0.78f, 0.1f)
        };

        /// <summary>Formats a cost like "15w 10s" for compact mobile UI.</summary>
        public static string CostString(ResourceAmount[] cost)
        {
            if (cost == null || cost.Length == 0) return "free";
            var sb = new System.Text.StringBuilder();
            foreach (var c in cost)
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(c.Amount).Append(ShortName(c.Type)[0]);
            }
            return sb.ToString();
        }
    }
}
