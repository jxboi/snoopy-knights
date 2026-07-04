using System.Collections.Generic;
using SnoopyKnights.Grid;
using UnityEngine;

namespace SnoopyKnights.Pathfinding
{
    /// <summary>
    /// A* over the tile grid, 4-directional. Road tiles cost half, so paths
    /// naturally prefer roads. Small map, so simple arrays are plenty fast.
    /// </summary>
    public static class Pathfinder
    {
        /// <summary>Path to a walkable tile. Excludes start, includes goal. Null if unreachable.</summary>
        public static List<Vector2Int> ToTile(GridMap map, Vector2Int start, Vector2Int goal)
        {
            if (!map.InBounds(goal) || !map.IsWalkable(goal)) return null;
            return Search(map, start, t => t == goal, t => Manhattan(t, goal));
        }

        /// <summary>Path to any walkable tile adjacent to target (which may itself be blocked).</summary>
        public static List<Vector2Int> ToAdjacent(GridMap map, Vector2Int start, Vector2Int target)
        {
            var goals = new HashSet<Vector2Int>();
            foreach (var d in GridMap.CardinalDirs)
            {
                var n = target + d;
                if (map.IsWalkable(n)) goals.Add(n);
            }
            if (goals.Count == 0) return null;
            return Search(map, start, goals.Contains, t => Manhattan(t, target));
        }

        static int Manhattan(Vector2Int a, Vector2Int b) =>
            Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        static List<Vector2Int> Search(GridMap map, Vector2Int start,
            System.Func<Vector2Int, bool> isGoal, System.Func<Vector2Int, int> heuristic)
        {
            if (isGoal(start)) return new List<Vector2Int>();
            if (!map.InBounds(start)) return null;

            int w = map.Width, h = map.Height;
            var g = new float[w * h];
            var cameFrom = new int[w * h];
            var closed = new bool[w * h];
            for (int i = 0; i < g.Length; i++) { g[i] = float.MaxValue; cameFrom[i] = -1; }

            // Roads cost 0.5/step, so scale the heuristic to stay admissible.
            float H(int idx) => heuristic(new Vector2Int(idx % w, idx / w)) * 0.5f;

            var open = new List<int>();
            int startIdx = start.y * w + start.x;
            g[startIdx] = 0f;
            open.Add(startIdx);

            while (open.Count > 0)
            {
                int best = 0;
                float bestF = g[open[0]] + H(open[0]);
                for (int i = 1; i < open.Count; i++)
                {
                    float f = g[open[i]] + H(open[i]);
                    if (f < bestF) { bestF = f; best = i; }
                }
                int cur = open[best];
                open.RemoveAt(best);
                if (closed[cur]) continue;
                closed[cur] = true;

                var curTile = new Vector2Int(cur % w, cur / w);
                if (isGoal(curTile))
                    return Reconstruct(cameFrom, cur, startIdx, w);

                foreach (var d in GridMap.CardinalDirs)
                {
                    int nx = curTile.x + d.x, ny = curTile.y + d.y;
                    if (!map.IsWalkable(nx, ny)) continue;
                    int ni = ny * w + nx;
                    if (closed[ni]) continue;
                    float tentative = g[cur] + map.MoveCost(nx, ny);
                    if (tentative < g[ni])
                    {
                        g[ni] = tentative;
                        cameFrom[ni] = cur;
                        open.Add(ni);
                    }
                }
            }
            return null;
        }

        static List<Vector2Int> Reconstruct(int[] cameFrom, int goal, int start, int w)
        {
            var path = new List<Vector2Int>();
            int cur = goal;
            while (cur != start && cur != -1)
            {
                path.Add(new Vector2Int(cur % w, cur / w));
                cur = cameFrom[cur];
            }
            path.Reverse();
            return path;
        }
    }
}
