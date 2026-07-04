using SnoopyKnights.Audio;
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
    /// Starts either a fresh mission or restores a pending save.
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
        public Economy.EconomySystem Economy { get; private set; }
        public Waves.WaveManager Waves { get; private set; }
        public Mission.MissionController Mission { get; private set; }
        public Mission.MissionStats Stats { get; private set; }
        public UI.Hud Hud { get; private set; }

        void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;
            GameSpeed.Reset(); // back to 1x and unfrozen after a restart from the end screen
            AudioManager.Ensure();

            Map = MapGenerator.Generate(seed: 20260704);

            MapRenderer = CreateChild<GridRenderer>("Map");
            MapRenderer.Build(Map);

            Cam = CameraController.CreateMainCamera(Map);

            InputRouter = CreateChild<InputRouter>("Input");
            InputRouter.Init(Cam);

            Stock = new ResourceStock();

            Buildings = CreateChild<BuildingManager>("Buildings");
            Buildings.Init(Map, Stock);
            Buildings.AutoConstruct = false; // builders do the work

            Units = CreateChild<UnitManager>("Units");
            Units.Init(Map, Buildings, Stock);

            Economy = CreateChild<Economy.EconomySystem>("Economy");
            Economy.Init(Stock, Buildings, Units);

            // Watchtowers get their combat behaviour on completion.
            Buildings.BuildingCompleted += b =>
            {
                if (b.Def.Type == BuildingType.Watchtower)
                    b.gameObject.AddComponent<Combat.TowerCombat>().Init(b, Units);
            };

            Waves = CreateChild<Waves.WaveManager>("Waves");
            Waves.Init(Map, Units, Stock);

            Stats = CreateChild<Mission.MissionStats>("Stats");
            Stats.Init(Units, Buildings);

            var pending = Save.SaveSystem.ConsumePending();
            if (pending != null)
                RestoreFromSave(pending);
            else
                StartFreshMission();

            Mission = CreateChild<Mission.MissionController>("Mission");
            Mission.Init(TownCenter, Buildings, Units, Stock, Waves);

            Selection = CreateChild<SelectionController>("Selection");
            Selection.Init(Map, InputRouter, Buildings, Units);

            Hud = UI.Hud.Create(this);

            WireSounds();
        }

        void StartFreshMission()
        {
            Stock.Add(ResourceType.Wood, GameConfig.StartWood);
            Stock.Add(ResourceType.Stone, GameConfig.StartStone);
            Stock.Add(ResourceType.Food, GameConfig.StartFood);
            Stock.Add(ResourceType.Gold, GameConfig.StartGold);

            TownCenter = Buildings.Place(BuildingType.TownCenter,
                GameConfig.TownCenterOrigin, instant: true, free: true);

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

            Cam.CenterOn(TownCenter.CenterWorld);
        }

        void RestoreFromSave(Save.SaveData data)
        {
            Stock.SetAll(data.resources);

            for (int y = 0; y < Map.Height; y++)
                for (int x = 0; x < Map.Width; x++)
                {
                    int i = y * Map.Width + x;
                    if (i >= data.tileTypes.Length) break;
                    var tile = Map.Get(x, y);
                    tile.Type = (TileType)data.tileTypes[i];
                    tile.ResourceLeft = data.tileRes[i];
                    tile.HasRoad = data.roads[i] == 1;
                    Map.NotifyChanged(x, y);
                }

            foreach (var bs in data.buildings)
            {
                var b = Buildings.Place((BuildingType)bs.type,
                    new Vector2Int(bs.x, bs.y), instant: bs.operational, free: true);
                if (b == null) continue;
                if (!bs.operational) b.RestoreConstruction(bs.progress);
                b.RestoreHealth(bs.health);
                b.SetOutputBuffer(bs.output);
                if (bs.trainQueue.Count > 0)
                {
                    var host = b.GetComponent<TrainingHost>();
                    if (host != null)
                    {
                        var types = new System.Collections.Generic.List<UnitType>();
                        foreach (var t in bs.trainQueue) types.Add((UnitType)t);
                        host.RestoreQueue(types, bs.trainProgress);
                    }
                }
                if (b.Def.Type == BuildingType.TownCenter)
                    TownCenter = b;
            }

            int enemies = 0;
            foreach (var us in data.units)
            {
                var u = Units.Spawn((UnitType)us.type, new Vector2(us.x, us.y));
                u.RestoreHealth(us.health);
                if (u.Def.IsEnemy) enemies++;
            }

            Waves.Restore(data.wave.waveNumber, data.wave.wavesCleared, data.wave.nextIn, enemies);
            Stats.Restore(data.stats.enemiesSlain, data.stats.villagersLost,
                data.stats.buildingsLost, data.stats.playSeconds);
            Cam.SetView(new Vector2(data.camX, data.camY), data.camSize);

            // A corrupt save without a Town Center would be unplayable; recover.
            if (TownCenter == null)
                TownCenter = Buildings.Place(BuildingType.TownCenter,
                    GameConfig.TownCenterOrigin, instant: true, free: true);
        }

        void WireSounds()
        {
            Buildings.Added += _ => AudioManager.Play(Sfx.Place);
            Buildings.BuildingCompleted += _ => AudioManager.Play(Sfx.Complete);
            Waves.WaveStarted += _ => AudioManager.Play(Sfx.Horn);
            Mission.GameEnded += won => AudioManager.Play(won ? Sfx.Victory : Sfx.Defeat);
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
