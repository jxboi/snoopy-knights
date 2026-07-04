using SnoopyKnights.Buildings;
using SnoopyKnights.Res;
using UnityEngine;

namespace SnoopyKnights.Units
{
    /// <summary>
    /// Hauls finished goods from production buildings to the nearest storage.
    /// The resource only counts in the player's stock once delivered.
    /// </summary>
    public sealed class CarrierUnit : Unit
    {
        enum State { Idle, ToPickup, PickingUp, ToStorage, Depositing }

        State state = State.Idle;
        Building source;
        Building storage;
        ResourceType carried;
        bool hasCargo;
        float timer;

        protected override void Tick(float dt)
        {
            switch (state)
            {
                case State.Idle:
                    timer -= dt;
                    if (timer <= 0f)
                    {
                        timer = 0.4f;
                        if (!FindPickup())
                            IdleDrift();
                    }
                    break;

                case State.ToPickup:
                    if (source == null) { AbortJob(); break; }
                    if (IsMoving) break;
                    if (source.IsAdjacentToFootprint(CurrentTile))
                    {
                        state = State.PickingUp;
                        timer = 0.4f;
                        SetWorking(true);
                        Status = "Picking up";
                    }
                    else if (!MoveTo(source.EntranceTile) && !MoveAdjacentTo(source.Origin))
                    {
                        AbortJob();
                    }
                    break;

                case State.PickingUp:
                    timer -= dt;
                    if (timer > 0f) break;
                    SetWorking(false);
                    if (source != null && source.Def.Produces.HasValue && source.TryTakeOutput())
                    {
                        source.ClaimedOutput--;
                        carried = source.Def.Produces.Value;
                        hasCargo = true;
                        SetCarrying(carried);
                        source = null;
                        GoToStorage();
                    }
                    else
                    {
                        AbortJob();
                    }
                    break;

                case State.ToStorage:
                    if (storage == null) // storage lost/unreachable; retry on a timer
                    {
                        timer -= dt;
                        if (timer <= 0f) GoToStorage();
                        break;
                    }
                    if (IsMoving) break;
                    if (storage.IsAdjacentToFootprint(CurrentTile))
                    {
                        state = State.Depositing;
                        timer = 0.3f;
                        SetWorking(true);
                        Status = "Delivering";
                    }
                    else
                    {
                        GoToStorage();
                    }
                    break;

                case State.Depositing:
                    timer -= dt;
                    if (timer > 0f) break;
                    SetWorking(false);
                    Ctx.Stock.Add(carried, 1);
                    Rendering.FloatingText.Spawn(new Vector2(Pos.x, Pos.y + 0.5f),
                        $"+1 {Res.ResourceInfo.ShortName(carried)}",
                        Res.ResourceInfo.Color(carried), 0.8f);
                    hasCargo = false;
                    SetCarrying(null);
                    state = State.Idle;
                    Status = "Idle";
                    break;
            }
        }

        bool FindPickup()
        {
            Building best = null;
            float bestDist = float.MaxValue;
            foreach (var b in Ctx.Buildings.All)
            {
                if (!b.HasOutputForPickup) continue;
                float d = (b.CenterWorld - Pos).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = b; }
            }
            if (best == null) return false;
            if (!MoveTo(best.EntranceTile) && !MoveAdjacentTo(best.Origin)) return false;

            source = best;
            source.ClaimedOutput++;
            state = State.ToPickup;
            Status = "Fetching goods";
            return true;
        }

        void GoToStorage()
        {
            state = State.ToStorage; // cargo stays on our back until delivered
            storage = Ctx.Buildings.FindNearestStorage(Pos);
            if (storage == null)
            {
                Status = "No storage!";
                timer = 1f;
                return;
            }
            if (!MoveTo(storage.EntranceTile) && !MoveAdjacentTo(storage.Origin))
            {
                storage = null;
                Status = "Storage unreachable";
                timer = 1f;
                return;
            }
            Status = "Delivering";
        }

        void AbortJob()
        {
            if (source != null) source.ClaimedOutput--;
            source = null;
            SetWorking(false);
            if (hasCargo)
            {
                GoToStorage(); // don't drop cargo on the floor
            }
            else
            {
                state = State.Idle;
                Status = "Idle";
            }
        }

        protected override void OnDie()
        {
            if (source != null) source.ClaimedOutput--;
        }
    }
}
