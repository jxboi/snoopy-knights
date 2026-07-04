namespace SnoopyKnights.Res
{
    /// <summary>The player's global resource stock. UI observes via Changed.</summary>
    public sealed class ResourceStock
    {
        readonly int[] amounts = new int[ResourceInfo.Count];

        public event System.Action Changed;

        public int Get(ResourceType t) => amounts[(int)t];

        public void Add(ResourceType t, int amount)
        {
            amounts[(int)t] = System.Math.Max(0, amounts[(int)t] + amount);
            Changed?.Invoke();
        }

        public bool CanAfford(ResourceAmount[] cost)
        {
            if (cost == null) return true;
            foreach (var c in cost)
                if (Get(c.Type) < c.Amount)
                    return false;
            return true;
        }

        public bool TrySpend(ResourceAmount[] cost)
        {
            if (!CanAfford(cost)) return false;
            foreach (var c in cost)
                amounts[(int)c.Type] -= c.Amount;
            Changed?.Invoke();
            return true;
        }

        public bool TrySpend(ResourceType t, int amount)
        {
            if (Get(t) < amount) return false;
            amounts[(int)t] -= amount;
            Changed?.Invoke();
            return true;
        }

        /// <summary>Used by save/load.</summary>
        public void SetAll(int[] values)
        {
            for (int i = 0; i < amounts.Length && i < values.Length; i++)
                amounts[i] = values[i];
            Changed?.Invoke();
        }

        public int[] GetAll() => (int[])amounts.Clone();
    }
}
