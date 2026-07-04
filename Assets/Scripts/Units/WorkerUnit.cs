using SnoopyKnights.Buildings;
using SnoopyKnights.Grid;
using UnityEngine;

namespace SnoopyKnights.Units
{
    /// <summary>
    /// Staffs a production building. Woodcutters/quarry workers walk out to
    /// harvest nearby tiles; farm/kitchen workers work on site. Output goes to
    /// the building's buffer for carriers to haul away.
    /// </summary>
    public sealed class WorkerUnit : Unit
    {
        enum State { Idle, ToBuilding, Waiting, WorkingOnSite, ToResource, Harvesting, Returning }

        State state = State.Idle;
        Building work;
        Vector2Int gatherTile;
        float timer;

        protected override void Tick(float dt)
        {
            if (work == null && state != State.Idle)
            {
                Unassign();
                return;
            }

            switch (state)
            {
                case State.Idle:
                    timer -= dt;
                    if (timer <= 0f)
                    {
                        timer = 0.6f;
                        if (!FindWorkplace())
                            IdleDrift();
                    }
                    break;

                case State.ToBuilding:
                    if (IsMoving) break;
                    if (work.IsAdjacentToFootprint(CurrentTile))
                        StartCycle();
                    else if (!MoveTo(work.EntranceTile) && !MoveAdjacentTo(work.Origin))
                        Unassign();
                    break;

                case State.Waiting: // output full or no resources; re-check periodically
                    timer -= dt;
                    if (timer <= 0f) StartCycle();
                    break;

                case State.WorkingOnSite:
                    timer -= dt;
                    if (timer <= 0f)
                    {
                        SetWorking(false);
                        if (work.Def.Produces.HasValue)
                            work.AddOutput(ProduceAmount());
                        StartCycle();
                    }
                    break;

                case State.ToResource:
                    if (!GatherTileValid()) { StartCycle(); break; }
                    if (IsMoving) break;
                    if (AdjacentToTile(gatherTile))
                    {
                        state = State.Harvesting;
                        timer = work.Def.CycleSeconds;
                        SetWorking(true);
                        Status = work.Def.GatherTile == TileType.Forest ? "Chopping" : "Mining";
                    }
                    else
                    {
                        StartCycle(); // path blocked; find another tile
                    }
                    break;

                case State.Harvesting:
                    timer -= dt;
                    if (timer > 0f) break;
                    SetWorking(false);
                    if (GatherTileValid())
                    {
                        var tile = Map.Get(gatherTile);
                        tile.ResourceLeft--;
                        if (tile.ResourceLeft <= 0)
                        {
                            tile.Type = TileType.Grass;
                            Map.NotifyChanged(gatherTile.x, gatherTile.y);
                        }
                        SetCarrying(work.Def.Produces);
                        state = State.Returning;
                        Status = "Returning";
                        if (!MoveTo(work.EntranceTile) && !MoveAdjacentTo(work.Origin))
                            timer = 1f; // stuck; retry via Returning
                    }
                    else
                    {
                        StartCycle();
                    }
                    break;

                case State.Returning:
                    if (IsMoving) break;
                    if (work.IsAdjacentToFootprint(CurrentTile))
                    {
                        if (work.AddOutput(1))
                        {
                            SetCarrying(null);
                            StartCycle();
                        }
                        else
                        {
                            Status = "Output full";
                            timer = 1.2f; // wait for a carrier, keep holding the goods
                        }
                    }
                    else
                    {
                        timer -= dt;
                        if (timer <= 0f)
                        {
                            timer = 1f;
                            if (!MoveTo(work.EntranceTile)) MoveAdjacentTo(work.Origin);
                        }
                    }
                    break;
            }
        }

        bool FindWorkplace()
        {
            Building best = null;
            float bestDist = float.MaxValue;
            foreach (var b in Ctx.Buildings.All)
            {
                if (!b.WantsWorker) continue;
                float d = (b.CenterWorld - Pos).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = b; }
            }
            if (best == null) return false;
            if (!MoveTo(best.EntranceTile) && !MoveAdjacentTo(best.Origin)) return false;

            work = best;
            work.AssignedWorker = this;
            state = State.ToBuilding;
            Status = $"Working at {work.Def.Name}";
            return true;
        }

        void StartCycle()
        {
            if (work == null) { Unassign(); return; }

            if (work.Def.Produces.HasValue && work.OutputBuffer >= work.Def.OutputCap)
            {
                state = State.Waiting;
                timer = 1.5f;
                Status = "Output full";
                return;
            }

            if (work.Def.GatherTile.HasValue)
            {
                if (FindGatherTile())
                {
                    if (MoveAdjacentTo(gatherTile))
                    {
                        state = State.ToResource;
                        Status = "Heading out";
                        return;
                    }
                }
                state = State.Waiting;
                timer = 3f;
                Status = work.Def.GatherTile == TileType.Forest ? "No trees in range" : "No rocks in range";
            }
            else
            {
                state = State.WorkingOnSite;
                timer = work.Def.CycleSeconds;
                SetWorking(true);
                Status = work.Def.Produces.HasValue ? "Working" : "Cooking";
            }
        }

        bool GatherTileValid() =>
            Map.InBounds(gatherTile) &&
            Map.Get(gatherTile).Type == work.Def.GatherTile &&
            Map.Get(gatherTile).ResourceLeft > 0;

        bool FindGatherTile()
        {
            var center = GridMap.WorldToTile(work.CenterWorld);
            int r = work.Def.GatherRadius;
            float bestDist = float.MaxValue;
            bool found = false;

            for (int y = center.y - r; y <= center.y + r; y++)
                for (int x = center.x - r; x <= center.x + r; x++)
                {
                    if (!Map.InBounds(x, y)) continue;
                    var tile = Map.Get(x, y);
                    if (tile.Type != work.Def.GatherTile || tile.ResourceLeft <= 0) continue;

                    bool reachable = false;
                    foreach (var d in GridMap.CardinalDirs)
                        if (Map.IsWalkable(x + d.x, y + d.y)) { reachable = true; break; }
                    if (!reachable) continue;

                    float dist = (new Vector2(x, y) - (Vector2)center).sqrMagnitude;
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        gatherTile = new Vector2Int(x, y);
                        found = true;
                    }
                }
            return found;
        }

        /// <summary>Hook for the kitchen bonus (wired up in the economy milestone).</summary>
        int ProduceAmount() => 1;

        void Unassign()
        {
            if (work != null && work.AssignedWorker == this)
                work.AssignedWorker = null;
            work = null;
            SetWorking(false);
            SetCarrying(null);
            state = State.Idle;
            Status = "Idle";
        }

        protected override void OnDie() => Unassign();
    }
}
