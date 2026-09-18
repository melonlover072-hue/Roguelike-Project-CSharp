using System.Drawing;
using RoguelikeSkeleton;

namespace RoguelikeSkeleton.Entities;

public class Monster : Entity
{
    private readonly Random _rng = new();

    public Monster(int x, int y, string name = "Goblin", char glyph = 'g') : base(x, y)
    {
        Name = name;
        Glyph = glyph;
        Color = Color.IndianRed;
        Health = MaxHealth = 5;
        AttackPower = 2;
    }

    public override void TakeTurn(Game game)
    {
        // If the player is adjacent, attack instead of wandering.
        int distX = game.Player.X - X;
        int distY = game.Player.Y - Y;

        if (game.Player.IsAlive && Math.Abs(distX) <= 1 && Math.Abs(distY) <= 1)
        {
            game.ResolveMelee(this, game.Player);
            return;
        }

        // Placeholder AI: wander randomly onto walkable, unoccupied tiles.
        // This is the spot to add pathfinding, aggro ranges, line-of-sight, etc.
        int dx = _rng.Next(-1, 2);
        int dy = _rng.Next(-1, 2);

        int newX = X + dx;
        int newY = Y + dy;

        if (game.Map.IsWalkable(newX, newY) && game.EntityAt(newX, newY) is null)
        {
            X = newX;
            Y = newY;
        }
    }
}
