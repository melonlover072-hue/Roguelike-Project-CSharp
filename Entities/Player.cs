using System.Drawing;
using RoguelikeSkeleton;
using RoguelikeSkeleton.Items;
using RoguelikeSkeleton.World;

namespace RoguelikeSkeleton.Entities;

public class Player : Entity
{
    /// <summary>Items picked up off the floor. Equipment in here isn't
    /// doing anything yet - it has to be equipped first.</summary>
    public List<Item> Inventory { get; } = new();

    /// <summary>What the player is currently holding and wearing; at most
    /// one of each. Swapped via EquipCommand.</summary>
    public Item? EquippedWeapon { get; set; }
    public Item? EquippedArmor { get; set; }

    public override int ArmorClass => 10 + (EquippedArmor?.ArmorBonus ?? 0);
    public override int DamageMin => EquippedWeapon?.DamageMin ?? 1;
    public override int DamageMax => EquippedWeapon?.DamageMax ?? 2;

    public Player(int x, int y) : base(x, y)
    {
        Glyph = '@';
        Color = Color.Gold;
        Name = "Player";
        Health = MaxHealth = 20;
        AttackBonus = 3;
        Speed = 100;
        Energy = 100; // start ready to act

        // Everyone starts with something pointy.
        EquippedWeapon = new Item("Rusty Dagger", '(', Color.LightGray,
            slot: EquipmentSlot.Weapon, damageMin: 2, damageMax: 4);
    }

    /// <summary>Attempts to move by (dx, dy). Returns false if blocked, so the
    /// caller (MoveCommand) knows whether a turn was actually spent.</summary>
    public bool TryMove(int dx, int dy, Map map)
    {
        int newX = X + dx;
        int newY = Y + dy;

        if (!map.IsWalkable(newX, newY))
            return false;

        X = newX;
        Y = newY;
        return true;
    }

    /// <summary>The player's melee attack action. Damage resolution and
    /// logging live in Game so monsters can share them.</summary>
    public void Attack(Entity target, Game game) => game.ResolveMelee(this, target);
}
