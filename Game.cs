using System.Drawing;
using RoguelikeSkeleton.Entities;
using RoguelikeSkeleton.Input;
using RoguelikeSkeleton.Items;
using RoguelikeSkeleton.World;

namespace RoguelikeSkeleton;

public class Game
{
    private const int FovRadius = 8;

    public Map Map { get; private set; } = null!;
    public Player Player { get; private set; } = null!;
    public List<Entity> Entities { get; } = new();
    public List<Item> Items { get; } = new();
    public MessageLog Log { get; } = new();

    public IEnumerable<Entity> AllEntities => Entities.Append(Player);

    public Entity? EntityAt(int x, int y) =>
        AllEntities.FirstOrDefault(e => e.IsAlive && e.X == x && e.Y == y);

    public Item? ItemAt(int x, int y) =>
        Items.FirstOrDefault(i => i.X == x && i.Y == y);

    public void Initialize()
    {
        var generator = new MapGenerator();
        var (map, start) = generator.Generate(width: 60, height: 25);
        Map = map;

        Player = new Player(start.x, start.y);

        var rng = new Random();
        for (int i = 0; i < 5; i++)
        {
            int x, y;
            do
            {
                x = rng.Next(Map.Width);
                y = rng.Next(Map.Height);
            } while (!Map.IsWalkable(x, y) || (x == Player.X && y == Player.Y));

            Entities.Add(new Monster(x, y));
        }

        // A few potions scattered on the floor, ready for PickUpCommand.
        for (int i = 0; i < 5; i++)
        {
            int x, y;
            do
            {
                x = rng.Next(Map.Width);
                y = rng.Next(Map.Height);
            } while (!Map.IsWalkable(x, y)
                     || (x == Player.X && y == Player.Y)
                     || EntityAt(x, y) is not null
                     || ItemAt(x, y) is not null);

            Items.Add(new Item("Health Potion", '!', Color.Magenta, healAmount: 6) { X = x, Y = y });
        }

        UpdateFov();

        Log.Add("Welcome to the dungeon of certain doom.");
    }

    /// <summary>
    /// Executes a single command and returns whether it consumed a turn
    /// (in which case monsters also get to act). Game has no idea whether
    /// it's being driven by a console loop, a WinForms KeyDown event, or
    /// anything else - that's entirely the front-end's problem.
    /// </summary>
    public bool ExecuteTurn(ICommand command)
    {
        bool turnTaken = command.Execute(this);
        if (turnTaken)
        {
            RunMonsterTurns();
            Entities.RemoveAll(e => !e.IsAlive); // clear the corpses
            UpdateFov();
        }
        return turnTaken;
    }

    /// <summary>Shared melee resolution for the player and monsters.</summary>
    public void ResolveMelee(Entity attacker, Entity target)
    {
        string attackerName = attacker is Player ? "You" : attacker.Name;
        string targetName = target is Player ? "you" : target.Name;

        target.Health -= attacker.AttackPower;

        if (!target.IsAlive)
        {
            string killVerb = attacker is Player ? "kill" : "kills";
            Log.Add($"{attackerName} {killVerb} {targetName}!", Color.OrangeRed);
            return;
        }

        string verb = attacker is Player ? "hit" : "hits";
        var color = target is Player ? Color.OrangeRed : Color.White;
        Log.Add($"{attackerName} {verb} {targetName} for {attacker.AttackPower} damage.", color);
    }

    private void RunMonsterTurns()
    {
        foreach (var entity in Entities.Where(e => e.IsAlive))
            entity.TakeTurn(this);
    }

    private void UpdateFov() => FieldOfView.Compute(Map, Player.X, Player.Y, FovRadius);
}
