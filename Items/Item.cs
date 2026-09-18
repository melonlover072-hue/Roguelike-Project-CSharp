using System.Drawing;

namespace RoguelikeSkeleton.Items;

/// <summary>
/// An item that can lie on the dungeon floor and be picked up into the
/// player's inventory. HealAmount is the hook for a future "use/quaff"
/// command - 0 means the item has no use effect yet.
/// </summary>
public class Item
{
    public string Name { get; }
    public char Glyph { get; }
    public Color Color { get; }
    public int HealAmount { get; }

    public int X { get; set; }
    public int Y { get; set; }

    public Item(string name, char glyph, Color color, int healAmount = 0)
    {
        Name = name;
        Glyph = glyph;
        Color = color;
        HealAmount = healAmount;
    }
}
