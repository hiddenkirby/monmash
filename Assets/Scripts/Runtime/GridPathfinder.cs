using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tidepool.Runtime
{
    public static class GridPathfinder
    {
        private static readonly Vector2Int[] NeighborOffsets =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, Func<Vector2Int, bool> isWalkable)
        {
            Queue<Vector2Int> frontier = new Queue<Vector2Int>();
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

            frontier.Enqueue(start);
            cameFrom[start] = start;

            while (frontier.Count > 0)
            {
                Vector2Int current = frontier.Dequeue();
                if (current == goal)
                {
                    return ReconstructPath(start, goal, cameFrom);
                }

                for (int i = 0; i < NeighborOffsets.Length; i++)
                {
                    Vector2Int next = current + NeighborOffsets[i];
                    if (cameFrom.ContainsKey(next) || !isWalkable(next))
                    {
                        continue;
                    }

                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }

            return new List<Vector2Int>();
        }

        private static List<Vector2Int> ReconstructPath(
            Vector2Int start,
            Vector2Int goal,
            Dictionary<Vector2Int, Vector2Int> cameFrom)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int current = goal;

            while (current != start)
            {
                path.Add(current);
                current = cameFrom[current];
            }

            path.Add(start);
            path.Reverse();
            return path;
        }
    }
}

