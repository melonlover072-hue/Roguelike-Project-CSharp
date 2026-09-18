namespace RoguelikeSkeleton.World;

public enum TileType
{
    Wall,
    Floor
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

    public readonly bool IsWalkable => Type == TileType.Floor;

    public readonly char Glyph => Type switch
    {
        TileType.Wall => '#',
        TileType.Floor => '.',
        _ => ' '
    };
}
