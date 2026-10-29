using System.Drawing;

namespace RoguelikeSkeleton.Items;

public enum EquipmentSlot
{
    None,   // consumables and plain junk
    Weapon,
    Armor
}

/// <summary>
/// An item that can lie on the dungeon floor and be picked up into the
/// player's inventory. One item covers several roles, distinguished by
/// Slot and whichever stat fields are non-zero:
/// - weapons deal DamageMin-DamageMax when equipped
/// - armor adds ArmorBonus to the wearer's AC when equipped
/// - consumables heal HealAmount and/or raise MaxHealthBonus when quaffed
/// </summary>
public class Item
{
    public string Name { get; }
    public char Glyph { get; }
    public Color Color { get; }
    public EquipmentSlot Slot { get; }

    public int DamageMin { get; }
    public int DamageMax { get; }
    public int ArmorBonus { get; }

    public int HealAmount { get; }
    public int MaxHealthBonus { get; }

    public int X { get; set; }
    public int Y { get; set; }

    public Item(string name, char glyph, Color color,
                EquipmentSlot slot = EquipmentSlot.None,
                int damageMin = 0, int damageMax = 0,
                int armorBonus = 0,
                int healAmount = 0, int maxHealthBonus = 0)
    {
        Name = name;
        Glyph = glyph;
        Color = color;
        Slot = slot;
        DamageMin = damageMin;
        DamageMax = damageMax;
        ArmorBonus = armorBonus;
        HealAmount = healAmount;
        MaxHealthBonus = maxHealthBonus;
    }

    /// <summary>Short summary for inventory listings.</summary>
    public string Describe() => Slot switch
    {
        EquipmentSlot.Weapon => $"{Name} (weapon, {DamageMin}-{DamageMax} dmg)",
        EquipmentSlot.Armor => $"{Name} (armor, +{ArmorBonus} AC)",
        _ when MaxHealthBonus > 0 => $"{Name} (+{MaxHealthBonus} max HP, heals {HealAmount})",
        _ when HealAmount > 0 => $"{Name} (heals {HealAmount})",
        _ => Name,
    };
}
