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
    }

    public override void TakeTurn(Game game)
    {
        // Placeholder AI: wander randomly onto walkable tiles.
        // This is the spot to add pathfinding, aggro ranges, line-of-sight, etc.
        int dx = _rng.Next(-1, 2);
        int dy = _rng.Next(-1, 2);

        int newX = X + dx;
        int newY = Y + dy;

        if (game.Map.IsWalkable(newX, newY))
        {
            X = newX;
            Y = newY;
        }
    }
}
