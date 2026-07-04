using SnoopyKnights.Grid;
using SnoopyKnights.Res;
using SnoopyKnights.Units;
using UnityEngine;

namespace SnoopyKnights.Waves
{
    public enum SpawnEdge { North, East, West }

    public sealed class WaveDef
    {
        public float PrepSeconds;                    // countdown before this wave
        public (UnitType type, int count)[] Groups;
        public SpawnEdge[] Edges;                    // groups split across these
        public int ClearGoldBonus;
    }

    /// <summary>
    /// Schedules the mission's 5 waves: countdown, spawn at map edges, track
    /// survivors, pay a gold bonus when cleared. Waves never overlap.
    /// </summary>
    public sealed class WaveManager : MonoBehaviour
    {
        GridMap map;
        UnitManager units;
        ResourceStock stock;

        WaveDef[] waves;
        int aliveEnemies;

        /// <summary>1-based; 0 while waiting for the first wave.</summary>
        public int WaveNumber { get; private set; }
        public int WavesCleared { get; private set; }
        public int TotalWaves => waves.Length;
        public float NextWaveIn { get; private set; }
        public bool WaveActive { get; private set; }
        public bool AllWavesCleared { get; private set; }
        public int EnemiesAlive => aliveEnemies;

        /// <summary>Edges the upcoming wave spawns from; empty once every wave has begun.</summary>
        public SpawnEdge[] NextWaveEdges =>
            WaveNumber < waves.Length ? waves[WaveNumber].Edges : System.Array.Empty<SpawnEdge>();

        public event System.Action<int> WaveStarted;
        public event System.Action<int> WaveCleared;
        public event System.Action AllCleared;

        public void Init(GridMap map, UnitManager units, ResourceStock stock)
        {
            this.map = map;
            this.units = units;
            this.stock = stock;
            waves = BuildWaves();
            NextWaveIn = waves[0].PrepSeconds;
            units.UnitDied += OnUnitDied;
        }

        static WaveDef[] BuildWaves() => new[]
        {
            new WaveDef
            {
                PrepSeconds = 150f, ClearGoldBonus = 15,
                Groups = new[] { (UnitType.Raider, 4) },
                Edges = new[] { SpawnEdge.North }
            },
            new WaveDef
            {
                PrepSeconds = 130f, ClearGoldBonus = 18,
                Groups = new[] { (UnitType.Raider, 7) },
                Edges = new[] { SpawnEdge.East }
            },
            new WaveDef
            {
                PrepSeconds = 130f, ClearGoldBonus = 22,
                Groups = new[] { (UnitType.Raider, 8), (UnitType.Brute, 1) },
                Edges = new[] { SpawnEdge.West }
            },
            new WaveDef
            {
                PrepSeconds = 140f, ClearGoldBonus = 26,
                Groups = new[] { (UnitType.Raider, 10), (UnitType.Brute, 2) },
                Edges = new[] { SpawnEdge.North, SpawnEdge.East }
            },
            new WaveDef
            {
                PrepSeconds = 150f, ClearGoldBonus = 40,
                Groups = new[] { (UnitType.Raider, 12), (UnitType.Brute, 4) },
                Edges = new[] { SpawnEdge.North, SpawnEdge.East, SpawnEdge.West }
            }
        };

        /// <summary>Used by save/load. Enemy count comes from the restored units.</summary>
        public void Restore(int waveNumber, int wavesCleared, float nextIn, int enemiesAlive)
        {
            WaveNumber = waveNumber;
            WavesCleared = wavesCleared;
            NextWaveIn = nextIn;
            aliveEnemies = enemiesAlive;
            WaveActive = enemiesAlive > 0;
            AllWavesCleared = wavesCleared >= waves.Length;
        }

        void Update()
        {
            if (WaveActive || AllWavesCleared || WaveNumber >= waves.Length)
                return;

            NextWaveIn -= Time.deltaTime;
            if (NextWaveIn <= 0f)
                StartWave();
        }

        void StartWave()
        {
            var def = waves[WaveNumber];
            WaveNumber++;
            WaveActive = true;
            aliveEnemies = 0;

            int edgeIndex = 0;
            foreach (var (type, count) in def.Groups)
            {
                for (int i = 0; i < count; i++)
                {
                    var edge = def.Edges[edgeIndex % def.Edges.Length];
                    edgeIndex++;
                    var pos = FindSpawnPos(edge);
                    units.Spawn(type, pos);
                    aliveEnemies++;
                }
            }
            WaveStarted?.Invoke(WaveNumber);
        }

        Vector2 FindSpawnPos(SpawnEdge edge)
        {
            for (int attempt = 0; attempt < 60; attempt++)
            {
                Vector2Int t = edge switch
                {
                    SpawnEdge.North => new Vector2Int(Random.Range(2, map.Width - 2), map.Height - 1),
                    SpawnEdge.East => new Vector2Int(map.Width - 1, Random.Range(2, map.Height - 2)),
                    _ => new Vector2Int(0, Random.Range(2, map.Height - 2))
                };
                if (map.IsWalkable(t))
                    return GridMap.TileCenter(t) + Random.insideUnitCircle * 0.3f;
            }
            return GridMap.TileCenter(new Vector2Int(map.Width / 2, map.Height - 1));
        }

        void OnUnitDied(Unit u)
        {
            if (!u.Def.IsEnemy || !WaveActive) return;
            aliveEnemies--;
            if (aliveEnemies > 0) return;

            WaveActive = false;
            WavesCleared = WaveNumber;
            stock.Add(ResourceType.Gold, waves[WaveNumber - 1].ClearGoldBonus);
            WaveCleared?.Invoke(WaveNumber);

            if (WaveNumber >= waves.Length)
            {
                AllWavesCleared = true;
                AllCleared?.Invoke();
            }
            else
            {
                NextWaveIn = waves[WaveNumber].PrepSeconds;
            }
        }
    }
}
