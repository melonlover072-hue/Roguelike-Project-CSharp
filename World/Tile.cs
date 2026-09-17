namespace RoguelikeSkeleton.World;

public enum TileType
{
    Wall,
    Floor
}

public struct Tile
{
    public TileType Type;
    public bool Explored; // handy later for fog-of-war / memory of visited tiles

    public Tile(TileType type)
    {
        Type = type;
        Explored = false;
    }

    public readonly bool IsWalkable => Type == TileType.Floor;

    public readonly char Glyph => Type switch
    {
        TileType.Wall => '#',
        TileType.Floor => '.',
        _ => ' '
    };
}
