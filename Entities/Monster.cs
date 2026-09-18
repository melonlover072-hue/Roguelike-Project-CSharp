using System.Drawing;
using RoguelikeSkeleton;
using RoguelikeSkeleton.World;

namespace RoguelikeSkeleton.Entities;

public class Monster : Entity
{
    // How far the monster can spot the player. Seeing them inside this
    // range switches it from wandering to chasing.
    private const int PerceptionRange = 10;

    private readonly Random _rng = new();

    public Monster(int x, int y, string name = "Goblin", char glyph = 'g',
                   int health = 5, int attackPower = 2, int speed = 100, Color? color = null)
        : base(x, y)
    {
        Name = name;
        Glyph = glyph;
        Color = color ?? Color.IndianRed;
        Health = MaxHealth = health;
        AttackPower = attackPower;
        Speed = speed;
    }

    public override void TakeTurn(Game game)
    {
        int distX = game.Player.X - X;
        int distY = game.Player.Y - Y;

        // Adjacent? Attack.
        if (game.Player.IsAlive && Math.Abs(distX) <= 1 && Math.Abs(distY) <= 1)
        {
            game.ResolveMelee(this, game.Player);
            return;
        }

        // Can perceive the player? Chase them, one greedy step at a time.
        // No pathfinding yet - they get stuck on walls, which keeps doors
        // and corridors tactically interesting for now.
        if (game.Player.IsAlive && CanPerceive(game, distX, distY))
        {
            if (TryStep(game, Math.Sign(distX), Math.Sign(distY)))
                return;
            if (TryStep(game, Math.Sign(distX), 0))
                return;
            TryStep(game, 0, Math.Sign(distY));
            return;
        }

        // Otherwise: placeholder wander AI.
        int dx = _rng.Next(-1, 2);
        int dy = _rng.Next(-1, 2);
        TryStep(game, dx, dy);
    }

    private bool CanPerceive(Game game, int distX, int distY)
    {
        int range = Math.Max(Math.Abs(distX), Math.Abs(distY));
        return range <= PerceptionRange
            && FieldOfView.CanSee(game.Map, X, Y, game.Player.X, game.Player.Y);
    }

    private bool TryStep(Game game, int dx, int dy)
    {
        if (dx == 0 && dy == 0)
            return false;

        int newX = X + dx;
        int newY = Y + dy;

        if (game.Map.IsWalkable(newX, newY) && game.EntityAt(newX, newY) is null)
        {
            X = newX;
            Y = newY;
            return true;
        }
        return false;
    }
}
