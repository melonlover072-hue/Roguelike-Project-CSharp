using System.Drawing;
using RoguelikeSkeleton;
using RoguelikeSkeleton.Items;
using RoguelikeSkeleton.World;

namespace RoguelikeSkeleton.Entities;

public class Player : Entity
{
    /// <summary>Items picked up off the floor. Currently just a simple list;
    /// a "use item" command would be the natural next step.</summary>
    public List<Item> Inventory { get; } = new();

    public Player(int x, int y) : base(x, y)
    {
        Glyph = '@';
        Color = Color.Gold;
        Name = "Player";
        Health = MaxHealth = 20;
        AttackPower = 3;
        Speed = 100;
        Energy = 100; // start ready to act
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
