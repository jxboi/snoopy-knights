using SnoopyKnights.Buildings;
using SnoopyKnights.CameraControl;
using SnoopyKnights.Grid;
using SnoopyKnights.Res;
using SnoopyKnights.Units;
using UnityEngine;

namespace SnoopyKnights.Core
{
    /// <summary>
    /// Composition root. Creates and wires all subsystems; owns no game logic.
    /// </summary>
    public sealed class Game : MonoBehaviour
    {
        public static Game Instance { get; private set; }

        public GridMap Map { get; private set; }
        public GridRenderer MapRenderer { get; private set; }
        public CameraController Cam { get; private set; }
        public InputRouter InputRouter { get; private set; }
        public SelectionController Selection { get; private set; }
        public ResourceStock Stock { get; private set; }
        public BuildingManager Buildings { get; private set; }
        public Building TownCenter { get; private set; }
        public UnitManager Units { get; private set; }
        public UI.Hud Hud { get; private set; }

        void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;

            Map = MapGenerator.Generate(seed: 20260704);

            MapRenderer = CreateChild<GridRenderer>("Map");
            MapRenderer.Build(Map);

            Cam = CameraController.CreateMainCamera(Map);

            InputRouter = CreateChild<InputRouter>("Input");
            InputRouter.Init(Cam);

            Stock = new ResourceStock();
            Stock.Add(ResourceType.Wood, GameConfig.StartWood);
            Stock.Add(ResourceType.Stone, GameConfig.StartStone);
            Stock.Add(ResourceType.Food, GameConfig.StartFood);
            Stock.Add(ResourceType.Gold, GameConfig.StartGold);

            Buildings = CreateChild<BuildingManager>("Buildings");
            Buildings.Init(Map, Stock);
            Buildings.AutoConstruct = false; // builders do the work now

            Units = CreateChild<UnitManager>("Units");
            Units.Init(Map, Buildings, Stock);

            TownCenter = Buildings.Place(BuildingType.TownCenter,
                GameConfig.TownCenterOrigin, instant: true, free: true);

            SpawnStartingUnits();

            Selection = CreateChild<SelectionController>("Selection");
            Selection.Init(Map, InputRouter, Buildings, Units);

            Hud = UI.Hud.Create(this);

            Cam.CenterOn(TownCenter.CenterWorld);
        }

        void SpawnStartingUnits()
        {
            Vector2 door = GridMap.TileCenter(TownCenter.EntranceTile);
            UnitType[] starters =
            {
                UnitType.Builder, UnitType.Builder,
                UnitType.Carrier, UnitType.Carrier,
                UnitType.Farmer, UnitType.Farmer
            };
            for (int i = 0; i < starters.Length; i++)
            {
                Vector2 offset = new Vector2((i % 3 - 1) * 0.8f, -(i / 3) * 0.8f);
                Units.Spawn(starters[i], door + offset);
            }
        }

        T CreateChild<T>(string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go.AddComponent<T>();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
