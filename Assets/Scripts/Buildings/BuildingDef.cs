using System.Collections.Generic;
using SnoopyKnights.Res;
using UnityEngine;

namespace SnoopyKnights.Buildings
{
    public enum BuildingType
    {
        TownCenter, Storehouse, Woodcutter, Quarry, Farm, Kitchen, House, Barracks, Watchtower
    }

    public enum IconShape { Square, Circle, Diamond, Triangle }

    /// <summary>Static data for one building type.</summary>
    public sealed class BuildingDef
    {
        public BuildingType Type;
        public string Name;
        public string Description;
        public int Width = 2, Height = 2;
        public int MaxHealth = 250;
        public float BuildSeconds = 15f;
        public ResourceAmount[] Cost = System.Array.Empty<ResourceAmount>();
        public Color BodyColor = Color.gray;
        public IconShape Icon = IconShape.Square;
        public Color IconColor = Color.white;
        public bool CanDemolish = true;
        public bool IsStorage;          // carriers deliver here; losing all storage loses the game
        public int PopulationBonus;     // added to the population cap when operational
    }

    public static class BuildingDefs
    {
        static readonly Dictionary<BuildingType, BuildingDef> defs = Build();

        public static BuildingDef Get(BuildingType t) => defs[t];
        public static IEnumerable<BuildingDef> All => defs.Values;

        static ResourceAmount[] Cost(int wood = 0, int stone = 0, int gold = 0)
        {
            var list = new List<ResourceAmount>();
            if (wood > 0) list.Add(new ResourceAmount(ResourceType.Wood, wood));
            if (stone > 0) list.Add(new ResourceAmount(ResourceType.Stone, stone));
            if (gold > 0) list.Add(new ResourceAmount(ResourceType.Gold, gold));
            return list.ToArray();
        }

        static Dictionary<BuildingType, BuildingDef> Build()
        {
            var list = new[]
            {
                new BuildingDef
                {
                    Type = BuildingType.TownCenter, Name = "Town Center",
                    Description = "Stores resources and trains workers. Protect it!",
                    Width = 3, Height = 3, MaxHealth = 600, IsStorage = true,
                    CanDemolish = false, PopulationBonus = 6,
                    BodyColor = new Color(0.78f, 0.68f, 0.47f),
                    Icon = IconShape.Diamond, IconColor = new Color(0.9f, 0.2f, 0.2f)
                },
                new BuildingDef
                {
                    Type = BuildingType.Storehouse, Name = "Storehouse",
                    Description = "Extra storage point. Carriers deliver goods here.",
                    Width = 2, Height = 2, MaxHealth = 400, BuildSeconds = 20f,
                    Cost = Cost(wood: 20, stone: 10), IsStorage = true,
                    BodyColor = new Color(0.72f, 0.6f, 0.4f),
                    Icon = IconShape.Diamond, IconColor = new Color(1f, 0.8f, 0.15f)
                },
                new BuildingDef
                {
                    Type = BuildingType.Woodcutter, Name = "Woodcutter",
                    Description = "A worker chops nearby trees for wood.",
                    Width = 2, Height = 2, MaxHealth = 250, BuildSeconds = 12f,
                    Cost = Cost(wood: 15),
                    BodyColor = new Color(0.55f, 0.42f, 0.28f),
                    Icon = IconShape.Triangle, IconColor = new Color(0.2f, 0.5f, 0.2f)
                },
                new BuildingDef
                {
                    Type = BuildingType.Quarry, Name = "Quarry",
                    Description = "A worker mines nearby rocks for stone.",
                    Width = 2, Height = 2, MaxHealth = 250, BuildSeconds = 12f,
                    Cost = Cost(wood: 15),
                    BodyColor = new Color(0.6f, 0.6f, 0.62f),
                    Icon = IconShape.Circle, IconColor = new Color(0.45f, 0.45f, 0.48f)
                },
                new BuildingDef
                {
                    Type = BuildingType.Farm, Name = "Farm",
                    Description = "A farmer grows food in the surrounding fields.",
                    Width = 3, Height = 3, MaxHealth = 250, BuildSeconds = 18f,
                    Cost = Cost(wood: 20, stone: 5),
                    BodyColor = new Color(0.8f, 0.72f, 0.4f),
                    Icon = IconShape.Square, IconColor = new Color(0.95f, 0.85f, 0.3f)
                },
                new BuildingDef
                {
                    Type = BuildingType.Kitchen, Name = "Kitchen",
                    Description = "Turns harvests into hearty meals: farms yield extra food.",
                    Width = 2, Height = 2, MaxHealth = 250, BuildSeconds = 15f,
                    Cost = Cost(wood: 15, stone: 10),
                    BodyColor = new Color(0.75f, 0.5f, 0.35f),
                    Icon = IconShape.Circle, IconColor = new Color(0.95f, 0.55f, 0.2f)
                },
                new BuildingDef
                {
                    Type = BuildingType.House, Name = "House",
                    Description = "Room for more villagers: +3 population cap.",
                    Width = 2, Height = 2, MaxHealth = 200, BuildSeconds = 10f,
                    Cost = Cost(wood: 10, stone: 5), PopulationBonus = 3,
                    BodyColor = new Color(0.7f, 0.55f, 0.45f),
                    Icon = IconShape.Triangle, IconColor = new Color(0.75f, 0.25f, 0.2f)
                },
                new BuildingDef
                {
                    Type = BuildingType.Barracks, Name = "Barracks",
                    Description = "Trains guards and archers to defend the settlement.",
                    Width = 2, Height = 2, MaxHealth = 350, BuildSeconds = 25f,
                    Cost = Cost(wood: 25, stone: 20),
                    BodyColor = new Color(0.58f, 0.47f, 0.47f),
                    Icon = IconShape.Square, IconColor = new Color(0.6f, 0.15f, 0.15f)
                },
                new BuildingDef
                {
                    Type = BuildingType.Watchtower, Name = "Watchtower",
                    Description = "Shoots arrows at enemies that come close.",
                    Width = 1, Height = 1, MaxHealth = 350, BuildSeconds = 20f,
                    Cost = Cost(wood: 10, stone: 25),
                    BodyColor = new Color(0.66f, 0.62f, 0.56f),
                    Icon = IconShape.Triangle, IconColor = new Color(0.85f, 0.85f, 0.9f)
                }
            };

            var dict = new Dictionary<BuildingType, BuildingDef>();
            foreach (var d in list) dict[d.Type] = d;
            return dict;
        }
    }
}
