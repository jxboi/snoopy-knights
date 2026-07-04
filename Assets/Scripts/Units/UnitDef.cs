using System.Collections.Generic;
using SnoopyKnights.Buildings;
using SnoopyKnights.Res;
using UnityEngine;

namespace SnoopyKnights.Units
{
    public enum UnitType { Builder, Carrier, Farmer, Guard, Archer, Raider, Brute }

    /// <summary>Static data for one unit type, including combat stats (used from M5).</summary>
    public sealed class UnitDef
    {
        public UnitType Type;
        public string Name;
        public Color Color = Color.white;
        public IconShape Icon = IconShape.Circle;
        public float MoveSpeed = 2.2f;
        public int MaxHealth = 40;
        public bool IsEnemy;
        public bool IsWorker;

        // Combat (zero damage = non-combatant).
        public int Damage;
        public float AttackInterval = 1f;
        public float AttackRange = 0.9f;
        public float AggroRange = 4f;
        public bool Ranged;

        // Training (at Town Center for workers, Barracks for soldiers).
        public ResourceAmount[] TrainCost = System.Array.Empty<ResourceAmount>();
        public float TrainSeconds = 8f;

        /// <summary>Gold paid to the player when this (enemy) unit dies.</summary>
        public int GoldReward;
    }

    public static class UnitDefs
    {
        static readonly Dictionary<UnitType, UnitDef> defs = Build();

        public static UnitDef Get(UnitType t) => defs[t];

        static ResourceAmount[] Cost(int food = 0, int gold = 0)
        {
            var list = new List<ResourceAmount>();
            if (food > 0) list.Add(new ResourceAmount(ResourceType.Food, food));
            if (gold > 0) list.Add(new ResourceAmount(ResourceType.Gold, gold));
            return list.ToArray();
        }

        static Dictionary<UnitType, UnitDef> Build()
        {
            var list = new[]
            {
                new UnitDef
                {
                    Type = UnitType.Builder, Name = "Builder", IsWorker = true,
                    Color = new Color(0.9f, 0.62f, 0.25f), Icon = IconShape.Triangle,
                    MoveSpeed = 2.2f, MaxHealth = 40,
                    TrainCost = Cost(food: 5), TrainSeconds = 7f
                },
                new UnitDef
                {
                    Type = UnitType.Carrier, Name = "Carrier", IsWorker = true,
                    Color = new Color(0.92f, 0.88f, 0.7f), Icon = IconShape.Square,
                    MoveSpeed = 2.5f, MaxHealth = 40,
                    TrainCost = Cost(food: 5), TrainSeconds = 7f
                },
                new UnitDef
                {
                    Type = UnitType.Farmer, Name = "Worker", IsWorker = true,
                    Color = new Color(0.55f, 0.8f, 0.35f), Icon = IconShape.Circle,
                    MoveSpeed = 2.2f, MaxHealth = 40,
                    TrainCost = Cost(food: 5), TrainSeconds = 7f
                },
                new UnitDef
                {
                    Type = UnitType.Guard, Name = "Guard",
                    Color = new Color(0.35f, 0.5f, 0.85f), Icon = IconShape.Square,
                    MoveSpeed = 2.0f, MaxHealth = 130,
                    Damage = 11, AttackInterval = 0.9f, AttackRange = 0.95f, AggroRange = 3.5f,
                    TrainCost = Cost(food: 10, gold: 15), TrainSeconds = 10f
                },
                new UnitDef
                {
                    Type = UnitType.Archer, Name = "Archer",
                    Color = new Color(0.3f, 0.72f, 0.55f), Icon = IconShape.Triangle,
                    MoveSpeed = 2.3f, MaxHealth = 70,
                    Damage = 7, AttackInterval = 1.3f, AttackRange = 4.5f, AggroRange = 5.5f, Ranged = true,
                    TrainCost = Cost(food: 10, gold: 20), TrainSeconds = 10f
                },
                new UnitDef
                {
                    Type = UnitType.Raider, Name = "Raider", IsEnemy = true,
                    Color = new Color(0.25f, 0.22f, 0.28f), Icon = IconShape.Diamond,
                    MoveSpeed = 1.9f, MaxHealth = 70,
                    Damage = 8, AttackInterval = 1f, AttackRange = 0.9f, AggroRange = 5f,
                    GoldReward = 4
                },
                new UnitDef
                {
                    Type = UnitType.Brute, Name = "Brute", IsEnemy = true,
                    Color = new Color(0.45f, 0.15f, 0.15f), Icon = IconShape.Diamond,
                    MoveSpeed = 1.55f, MaxHealth = 230,
                    Damage = 20, AttackInterval = 1.4f, AttackRange = 1f, AggroRange = 6f,
                    GoldReward = 10
                }
            };

            var dict = new Dictionary<UnitType, UnitDef>();
            foreach (var d in list) dict[d.Type] = d;
            return dict;
        }
    }
}
