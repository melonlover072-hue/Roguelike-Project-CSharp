namespace RoguelikeSkeleton.World;

/// <summary>
/// A grid of tiles. Deliberately "dumb" - it knows nothing about how the
/// layout was generated, and nothing about entities standing on it.
/// Keeping it dumb means you can swap MapGenerator implementations freely.
/// </summary>
public class Map
{
    public int Width { get; }
    public int Height { get; }
    private readonly Tile[,] _tiles;

    public Map(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new Tile[width, height];

        // Default everything to wall; a generator carves out floors afterward.
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _tiles[x, y] = new Tile(TileType.Wall);
    }

    public Tile GetTile(int x, int y) => _tiles[x, y];

    public void SetTile(int x, int y, TileType type) => _tiles[x, y] = new Tile(type);

    // Tile is a struct, so flags are updated copy-in/copy-out - GetTile
    // would only hand back a read-only snapshot.
    public void SetVisible(int x, int y, bool visible)
    {
        if (!InBounds(x, y))
            return;
        var tile = _tiles[x, y];
        tile.Visible = visible;
        _tiles[x, y] = tile;
    }

    public void SetExplored(int x, int y)
    {
        if (!InBounds(x, y))
            return;
        var tile = _tiles[x, y];
        tile.Explored = true;
        _tiles[x, y] = tile;
    }

    public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public bool IsWalkable(int x, int y) => InBounds(x, y) && _tiles[x, y].IsWalkable;
}
