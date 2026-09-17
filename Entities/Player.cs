using System.Drawing;
using RoguelikeSkeleton.World;

namespace RoguelikeSkeleton.Entities;

public class Player : Entity
{
    public Player(int x, int y) : base(x, y)
    {
        Glyph = '@';
        Color = Color.Gold;
        Name = "Player";
        Health = MaxHealth = 20;
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
}
