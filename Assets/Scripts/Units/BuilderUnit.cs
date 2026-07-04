using SnoopyKnights.Buildings;
using UnityEngine;

namespace SnoopyKnights.Units
{
    /// <summary>
    /// Finds construction sites automatically and builds them. Spreads out
    /// across sites (fewest claimed builders first, then nearest).
    /// </summary>
    public sealed class BuilderUnit : Unit
    {
        enum State { Idle, Traveling, Building }

        State state = State.Idle;
        Building site;
        float scanT;

        protected override void Tick(float dt)
        {
            switch (state)
            {
                case State.Idle:
                    scanT -= dt;
                    if (scanT <= 0f)
                    {
                        scanT = 0.5f;
                        if (!FindSite())
                            IdleDrift();
                    }
                    break;

                case State.Traveling:
                    if (site == null || site.IsOperational) { DropJob(); break; }
                    if (IsMoving) break;
                    if (site.IsAdjacentToFootprint(CurrentTile))
                    {
                        state = State.Building;
                        Status = "Building";
                        SetWorking(true);
                    }
                    else if (!MoveTo(site.EntranceTile) && !MoveAdjacentTo(site.Origin))
                    {
                        DropJob(); // unreachable, try again later
                    }
                    break;

                case State.Building:
                    if (site == null) { DropJob(); break; }
                    site.AdvanceConstruction(dt);
                    if (site == null || site.IsOperational) DropJob();
                    break;
            }
        }

        bool FindSite()
        {
            Building best = null;
            float bestScore = float.MaxValue;
            foreach (var b in Ctx.Buildings.All)
            {
                if (b.IsOperational) continue;
                // Prefer unclaimed sites, then closer ones.
                float score = b.ClaimedBuilders * 1000f + (b.CenterWorld - Pos).sqrMagnitude;
                if (score < bestScore) { bestScore = score; best = b; }
            }
            if (best == null) return false;
            if (!MoveTo(best.EntranceTile) && !MoveAdjacentTo(best.Origin)) return false;

            site = best;
            site.ClaimedBuilders++;
            state = State.Traveling;
            Status = "Heading to site";
            return true;
        }

        void DropJob()
        {
            if (site != null) site.ClaimedBuilders--;
            site = null;
            SetWorking(false);
            state = State.Idle;
            Status = "Idle";
        }

        protected override void OnDie() => DropJob();
    }
}
