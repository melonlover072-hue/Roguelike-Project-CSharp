namespace RoguelikeSkeleton.World;

/// <summary>
/// A minimal room-and-corridor generator: scatter non-overlapping rectangular
/// rooms, then connect each new room to the previous one with an L-shaped
/// corridor. It's not fancy, but it's easy to read and easy to replace later
/// with BSP trees, cellular automata, or whatever you want to graduate to.
/// </summary>
public class MapGenerator
{
    private readonly Random _rng;

    public MapGenerator(Random? rng = null)
    {
        _rng = rng ?? new Random();
    }

    private readonly struct Room
    {
        public readonly int X, Y, Width, Height;

        public Room(int x, int y, int w, int h)
        {
            X = x; Y = y; Width = w; Height = h;
        }

        public (int x, int y) Center => (X + Width / 2, Y + Height / 2);

        public bool Overlaps(Room other, int padding = 1) =>
            X - padding < other.X + other.Width &&
            X + Width + padding > other.X &&
            Y - padding < other.Y + other.Height &&
            Y + Height + padding > other.Y;
    }

    public (Map map, (int x, int y) playerStart, (int x, int y) stairsUp, (int x, int y) stairsDown)
        Generate(int width, int height, int roomAttempts = 15)
    {
        var map = new Map(width, height);
        var rooms = new List<Room>();

        for (int i = 0; i < roomAttempts; i++)
        {
            int w = _rng.Next(4, 9);
            int h = _rng.Next(3, 7);
            int x = _rng.Next(1, Math.Max(2, width - w - 1));
            int y = _rng.Next(1, Math.Max(2, height - h - 1));
            var newRoom = new Room(x, y, w, h);

            if (rooms.Any(r => r.Overlaps(newRoom)))
                continue;

            CarveRoom(map, newRoom);

            if (rooms.Count > 0)
                CarveCorridor(map, rooms[^1].Center, newRoom.Center);

            rooms.Add(newRoom);
        }

(int x, int y) start = rooms.Count > 0 ? rooms[0].Center : (width / 2, height / 2);

// You arrive standing on the up-stairs; the way down goes in the
// last (and usually farthest) room.
(int x, int y) stairsUp = start;
(int x, int y) stairsDown = start;

if (rooms.Count > 1)
{
    stairsDown = rooms[^1].Center;
}
else
{
    // Degenerate single-room map: any floor tile that isn't the start.
    for (int y = 1; y < height - 1 && stairsDown == start; y++)
        for (int x = 1; x < width - 1 && stairsDown == start; x++)
            if (map.IsWalkable(x, y) && (x, y) != start)
                stairsDown = (x, y);
}

map.SetTile(stairsUp.x, stairsUp.y, TileType.StairsUp);
if (stairsDown != start)
    map.SetTile(stairsDown.x, stairsDown.y, TileType.StairsDown);

return (map, start, stairsUp, stairsDown);
    }

    private static void CarveRoom(Map map, Room room)
    {
        for (int x = room.X; x < room.X + room.Width; x++)
            for (int y = room.Y; y < room.Y + room.Height; y++)
                map.SetTile(x, y, TileType.Floor);
    }

    private void CarveCorridor(Map map, (int x, int y) from, (int x, int y) to)
    {
        // Randomize whether we go horizontal-then-vertical or vice versa,
        // just so corridors don't all look the same shape.
        if (_rng.Next(2) == 0)
        {
            CarveHorizontal(map, from.x, to.x, from.y);
            CarveVertical(map, from.y, to.y, to.x);
        }
        else
        {
            CarveVertical(map, from.y, to.y, from.x);
            CarveHorizontal(map, from.x, to.x, to.y);
        }
    }

    private static void CarveHorizontal(Map map, int x1, int x2, int y)
    {
        for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
            map.SetTile(x, y, TileType.Floor);
    }

    private static void CarveVertical(Map map, int y1, int y2, int x)
    {
        for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
            map.SetTile(x, y, TileType.Floor);
    }
}
