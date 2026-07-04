using System.Collections.Generic;
using SnoopyKnights.Buildings;
using SnoopyKnights.Res;
using SnoopyKnights.Units;
using SnoopyKnights.Waves;
using UnityEngine;

namespace SnoopyKnights.Mission
{
    public enum MissionState { Playing, Victory, Defeat }

    /// <summary>A mission goal. Latches once completed.</summary>
    public sealed class Objective
    {
        readonly System.Func<string> text;
        readonly System.Func<bool> check;

        public bool Done { get; private set; }
        public string Text => text();

        public Objective(System.Func<string> text, System.Func<bool> check)
        {
            this.text = text;
            this.check = check;
        }

        public void Refresh()
        {
            if (!Done && check()) Done = true;
        }
    }

    /// <summary>
    /// Win/lose rules and the objective list for the single mission:
    /// survive all waves; lose the Town Center (or all storage) and it's over.
    /// </summary>
    public sealed class MissionController : MonoBehaviour
    {
        public MissionState State { get; private set; } = MissionState.Playing;
        public string EndReason { get; private set; } = "";
        public IReadOnlyList<Objective> Objectives => objectives;

        /// <summary>Argument is true on victory.</summary>
        public event System.Action<bool> GameEnded;

        readonly List<Objective> objectives = new List<Objective>();
        Building townCenter;
        BuildingManager buildings;
        float refreshT;

        public void Init(Building townCenter, BuildingManager buildings,
            UnitManager units, ResourceStock stock, WaveManager waves)
        {
            this.townCenter = townCenter;
            this.buildings = buildings;

            objectives.Add(new Objective(
                () => "Build a Woodcutter, a Quarry and a Farm",
                () => HasOperational(BuildingType.Woodcutter) &&
                      HasOperational(BuildingType.Quarry) &&
                      HasOperational(BuildingType.Farm)));
            objectives.Add(new Objective(
                () => "Stock 20 food",
                () => stock.Get(ResourceType.Food) >= 20));
            objectives.Add(new Objective(
                () => "Train a Guard or an Archer",
                () => AnySoldier(units)));
            objectives.Add(new Objective(
                () => $"Survive all {waves.TotalWaves} waves ({waves.WavesCleared}/{waves.TotalWaves})",
                () => waves.AllWavesCleared));

            buildings.BuildingDestroyed += OnBuildingDestroyed;
            waves.AllCleared += () => End(true, "All waves defeated. The settlement stands!");
        }

        bool HasOperational(BuildingType type)
        {
            foreach (var b in buildings.All)
                if (b.Def.Type == type && b.IsOperational)
                    return true;
            return false;
        }

        static bool AnySoldier(UnitManager units)
        {
            foreach (var u in units.All)
                if (u.Def.Type == UnitType.Guard || u.Def.Type == UnitType.Archer)
                    return true;
            return false;
        }

        void OnBuildingDestroyed(Building b)
        {
            if (State != MissionState.Playing) return;
            if (b == townCenter)
                End(false, "The Town Center was destroyed.");
            else if (b.Def.IsStorage && !buildings.AnyStorageAlive())
                End(false, "All your storage was destroyed.");
        }

        void End(bool won, string reason)
        {
            if (State != MissionState.Playing) return;
            State = won ? MissionState.Victory : MissionState.Defeat;
            EndReason = reason;
            Time.timeScale = 0f;
            GameEnded?.Invoke(won);
        }

        void Update()
        {
            if (State != MissionState.Playing) return;
            refreshT -= Time.deltaTime;
            if (refreshT > 0f) return;
            refreshT = 0.5f;
            foreach (var o in objectives)
                o.Refresh();
        }
    }
}
