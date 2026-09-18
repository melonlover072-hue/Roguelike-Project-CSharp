namespace RoguelikeSkeleton.World;

/// <summary>
/// Line-of-sight field of view. For every tile within a radius of the
/// viewer, a Bresenham line is walked from the viewer to that tile; the
/// tile is visible unless a wall stands somewhere along the way. Visible
/// tiles also get their Explored flag set so the renderer can keep drawing
/// them, dimmed, after they leave sight.
///
/// This is the simple "cast a line to every tile" version - totally fine at
/// 60x25. The usual upgrade is recursive shadowcasting for smoother edges.
/// </summary>
public static class FieldOfView
{
    public static void Compute(Map map, int originX, int originY, int radius)
    {
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                map.SetVisible(x, y, visible: false);

        int radiusSquared = radius * radius;

        int minX = Math.Max(0, originX - radius);
        int maxX = Math.Min(map.Width - 1, originX + radius);
        int minY = Math.Max(0, originY - radius);
        int maxY = Math.Min(map.Height - 1, originY + radius);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                int dx = x - originX;
                int dy = y - originY;
                if (dx * dx + dy * dy > radiusSquared)
                    continue;
                if (!HasLineOfSight(map, originX, originY, x, y))
                    continue;

                map.SetVisible(x, y, visible: true);
                map.SetExplored(x, y);
            }
        }
    }

    /// <summary>
    /// True if nothing blocks the straight line between the two points.
    /// The target tile itself never blocks, so wall faces the viewer stands
    /// next to are still drawn.
    /// </summary>
    private static bool HasLineOfSight(Map map, int x0, int y0, int x1, int y1)
    {
        int dx = Math.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = Math.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        int x = x0;
        int y = y0;

        while (x != x1 || y != y1)
        {
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 < dx) { err += dx; y += sy; }

            if (x == x1 && y == y1)
                break; // Reached the target; only walls in between block.

            if (!map.GetTile(x, y).IsWalkable)
                return false;
        }

        return true;
    }
}
