namespace RoguelikeSkeleton.World;

public enum TileType
{
    Wall,
    Floor,
    StairsUp,
    StairsDown
}

public struct Tile
{
    public TileType Type;
    public bool Explored; // the player has seen this tile at least once
    public bool Visible;  // the tile is in the player's current line of sight

    public Tile(TileType type)
    {
        Type = type;
        Explored = false;
        Visible = false;
    }

    public readonly bool IsWalkable =>
        Type is TileType.Floor or TileType.StairsUp or TileType.StairsDown;

    public readonly char Glyph => Type switch
    {
        TileType.Wall => '#',
        TileType.Floor => '.',
        TileType.StairsUp => '<',
        TileType.StairsDown => '>',
        _ => ' '
    };
}
