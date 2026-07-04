using System.Collections.Generic;
using System.IO;
using SnoopyKnights.Buildings;
using SnoopyKnights.Core;
using SnoopyKnights.Units;
using UnityEngine;

namespace SnoopyKnights.Save
{
    [System.Serializable]
    public sealed class SaveData
    {
        public int version = 1;
        public int[] resources;

        // Tile map (parallel arrays, row-major).
        public int[] tileTypes;
        public int[] tileRes;
        public int[] roads;

        public List<BuildingSave> buildings = new List<BuildingSave>();
        public List<UnitSave> units = new List<UnitSave>();
        public WaveSave wave = new WaveSave();
        public StatsSave stats = new StatsSave(); // zeroes when loading a pre-stats save

        public float camX, camY, camSize;
    }

    [System.Serializable]
    public sealed class BuildingSave
    {
        public int type;
        public int x, y;
        public bool operational;
        public float progress;
        public int health;
        public int output;
        public List<int> trainQueue = new List<int>();
        public float trainProgress;
    }

    [System.Serializable]
    public sealed class UnitSave
    {
        public int type;
        public float x, y;
        public int health;
    }

    [System.Serializable]
    public sealed class WaveSave
    {
        public int waveNumber;
        public int wavesCleared;
        public float nextIn;
    }

    [System.Serializable]
    public sealed class StatsSave
    {
        public int enemiesSlain, villagersLost, buildingsLost;
        public float playSeconds;
    }

    /// <summary>
    /// Basic JSON save/load. Loading stashes the data and reloads the scene;
    /// Game.Awake picks it up instead of the fresh-mission setup. Worker jobs
    /// aren't saved — workers simply re-acquire them after loading.
    /// </summary>
    public static class SaveSystem
    {
        static string FilePath => Path.Combine(Application.persistentDataPath, "snoopy_save.json");

        public static SaveData PendingLoad { get; private set; }

        public static bool HasSave => File.Exists(FilePath);

        public static SaveData ConsumePending()
        {
            var d = PendingLoad;
            PendingLoad = null;
            return d;
        }

        public static void Save(Game game)
        {
            var data = new SaveData { resources = game.Stock.GetAll() };

            var map = game.Map;
            int n = map.Width * map.Height;
            data.tileTypes = new int[n];
            data.tileRes = new int[n];
            data.roads = new int[n];
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    var tile = map.Get(x, y);
                    int i = y * map.Width + x;
                    data.tileTypes[i] = (int)tile.Type;
                    data.tileRes[i] = tile.ResourceLeft;
                    data.roads[i] = tile.HasRoad ? 1 : 0;
                }

            foreach (var b in game.Buildings.All)
            {
                var bs = new BuildingSave
                {
                    type = (int)b.Def.Type,
                    x = b.Origin.x,
                    y = b.Origin.y,
                    operational = b.IsOperational,
                    progress = b.ConstructionProgress,
                    health = b.Health,
                    output = b.OutputBuffer
                };
                var host = b.GetComponent<TrainingHost>();
                if (host != null)
                {
                    foreach (var t in host.Queue) bs.trainQueue.Add((int)t);
                    bs.trainProgress = host.Progress01;
                }
                data.buildings.Add(bs);
            }

            foreach (var u in game.Units.All)
                data.units.Add(new UnitSave
                {
                    type = (int)u.Def.Type,
                    x = u.Pos.x,
                    y = u.Pos.y,
                    health = u.Health
                });

            data.wave.waveNumber = game.Waves.WaveNumber;
            data.wave.wavesCleared = game.Waves.WavesCleared;
            data.wave.nextIn = game.Waves.NextWaveIn;

            data.stats.enemiesSlain = game.Stats.EnemiesSlain;
            data.stats.villagersLost = game.Stats.VillagersLost;
            data.stats.buildingsLost = game.Stats.BuildingsLost;
            data.stats.playSeconds = game.Stats.PlaySeconds;

            var camPos = game.Cam.transform.position;
            data.camX = camPos.x;
            data.camY = camPos.y;
            data.camSize = game.Cam.Camera.orthographicSize;

            File.WriteAllText(FilePath, JsonUtility.ToJson(data));
        }

        /// <summary>Reads the save and reloads the scene; returns false if none exists.</summary>
        public static bool LoadAndRestart()
        {
            if (!HasSave) return false;
            try
            {
                PendingLoad = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Save] Failed to read save: {e.Message}");
                PendingLoad = null;
                return false;
            }
            UI.GameOverScreen.Restart();
            return true;
        }
    }
}
