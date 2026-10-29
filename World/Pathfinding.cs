namespace RoguelikeSkeleton.World;

/// <summary>
/// A* pathfinding over the walkable tiles of a Map. 8-directional movement
/// with diagonal moves costing a bit more, and no corner-cutting through
/// walls. At 60x25 this is cheap enough to run for every monster that
/// chases the player on a given turn.
/// </summary>
public static class Pathfinding
{
    private static readonly (int dx, int dy)[] Directions =
    {
        (1, 0), (-1, 0), (0, 1), (0, -1),
        (1, 1), (1, -1), (-1, 1), (-1, -1),
    };

    /// <summary>
    /// Finds the shortest route from start to goal. Returns the list of
    /// steps to take (excluding the start tile, including the goal), or
    /// null when the goal is unreachable. Entity occupancy is ignored -
    /// callers decide what to do about things standing in the way.
    /// </summary>
    public static List<(int x, int y)>? FindPath(Map map, int startX, int startY, int goalX, int goalY)
    {
        var start = (startX, startY);
        var goal = (goalX, goalY);

        if (start == goal)
            return new List<(int x, int y)>();

        var open = new PriorityQueue<(int x, int y), int>();
        var cameFrom = new Dictionary<(int x, int y), (int x, int y)>();
        var costSoFar = new Dictionary<(int x, int y), int> { [start] = 0 };

        open.Enqueue(start, Heuristic(start, goal));

        while (open.Count > 0)
        {
            var current = open.Dequeue();
            if (current == goal)
                return Reconstruct(cameFrom, start, goal);

            foreach (var (dx, dy) in Directions)
            {
                int nextX = current.x + dx;
                int nextY = current.y + dy;

                if (!map.IsWalkable(nextX, nextY))
                    continue;

                // Diagonal moves aren't allowed to squeeze between walls.
                if (dx != 0 && dy != 0 &&
                    (!map.IsWalkable(current.x + dx, current.y) ||
                     !map.IsWalkable(current.x, current.y + dy)))
                    continue;

                var next = (nextX, nextY);
                int cost = costSoFar[current] + (dx != 0 && dy != 0 ? 14 : 10);

                if (cost < costSoFar.GetValueOrDefault(next, int.MaxValue))
                {
                    cameFrom[next] = current;
                    costSoFar[next] = cost;
                    open.Enqueue(next, cost + Heuristic(next, goal));
                }
            }
        }

        return null; // no route exists
    }

    // Octile distance (diagonal steps cost ~14, straight ones ~10).
    private static int Heuristic((int x, int y) a, (int x, int y) b)
    {
        int dx = Math.Abs(a.x - b.x);
        int dy = Math.Abs(a.y - b.y);
        return 14 * Math.Min(dx, dy) + 10 * Math.Abs(dx - dy);
    }

    private static List<(int x, int y)> Reconstruct(
        Dictionary<(int x, int y), (int x, int y)> cameFrom,
        (int x, int y) start,
        (int x, int y) goal)
    {
        var path = new List<(int x, int y)> { goal };

        var current = goal;
        while (current != start)
        {
            current = cameFrom[current];
            if (current != start)
                path.Add(current);
        }

        path.Reverse();
        return path;
    }
}
