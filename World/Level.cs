using RoguelikeSkeleton.Entities;
using RoguelikeSkeleton.Items;

namespace RoguelikeSkeleton.World;

/// <summary>
/// Everything that makes up one dungeon floor: the map, its monsters, its
/// dropped items, and the stairs connecting it to neighbouring floors.
/// Game keeps every visited Level alive for the whole run, so a floor the
/// player leaves and later returns to is exactly as they left it - no
/// regeneration, no infinite loot from re-rolling the same stairs.
/// </summary>
public class Level
{
    public Map Map { get; }
    public List<Entity> Entities { get; } = new();
    public List<Item> Items { get; } = new();

    public (int x, int y) StairsUp { get; }
    public (int x, int y) StairsDown { get; }

    public Level(Map map, (int x, int y) stairsUp, (int x, int y) stairsDown)
    {
        Map = map;
        StairsUp = stairsUp;
        StairsDown = stairsDown;
    }
}
