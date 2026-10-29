using System.Drawing;
using RoguelikeSkeleton;
using RoguelikeSkeleton.World;

namespace RoguelikeSkeleton.Entities;

public class Monster : Entity
{
    // How far the monster can spot the player. Seeing them inside this
    // range refreshes its memory of their position and triggers a chase.
    private const int PerceptionRange = 10;

    private readonly Random _rng = new();

    // Where the monster last saw the player. It keeps hunting that spot
    // even after losing line of sight, and gives up on arrival.
    private (int x, int y)? _lastSeenPlayer;

    public Monster(int x, int y, string name = "Goblin", char glyph = 'g',
                   int health = 5, int damageMin = 1, int damageMax = 3,
                   int attackBonus = 1, int armorClass = 11,
                   int speed = 100, Color? color = null)
        : base(x, y)
    {
        Name = name;
        Glyph = glyph;
        Color = color ?? Color.IndianRed;
        Health = MaxHealth = health;
        AttackBonus = attackBonus;
        Speed = speed;

        // ArmorClass/Damage are virtual on Entity with no settable backing,
        // so tuck the per-species values behind small overrides.
        _armorClass = armorClass;
        _damageMin = damageMin;
        _damageMax = damageMax;
    }

    private readonly int _armorClass;
    private readonly int _damageMin;
    private readonly int _damageMax;

    public override int ArmorClass => _armorClass;
    public override int DamageMin => _damageMin;
    public override int DamageMax => _damageMax;

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

        // Can see the player? Remember where they are and hunt them.
        if (game.Player.IsAlive && CanPerceive(game, distX, distY))
        {
            _lastSeenPlayer = (game.Player.X, game.Player.Y);
            Chase(game, _lastSeenPlayer.Value);
            return;
        }

        // Lost sight of them? Keep heading for the last known position,
        // then give up and go back to wandering.
        if (_lastSeenPlayer is { } memory)
        {
            if (X == memory.x && Y == memory.y)
            {
                _lastSeenPlayer = null;
            }
            else
            {
                Chase(game, memory);
                return;
            }
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

    // Follows an A* path toward the goal. If the next step is occupied by
    // another monster, it waits a moment instead of bumping into it.
    private void Chase(Game game, (int x, int y) goal)
    {
        var path = Pathfinding.FindPath(game.Map, X, Y, goal.x, goal.y);
        if (path is null || path.Count == 0)
            return;

        var step = path[0];
        if (game.EntityAt(step.x, step.y) is null)
        {
            X = step.x;
            Y = step.y;
        }
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
