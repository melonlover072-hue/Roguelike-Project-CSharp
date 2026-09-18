using System.Drawing;
using RoguelikeSkeleton.Entities;
using RoguelikeSkeleton.Input;
using RoguelikeSkeleton.Items;
using RoguelikeSkeleton.World;

namespace RoguelikeSkeleton;

public class Game
{
    private const int FovRadius = 8;
    private const int ActionCost = 100; // energy one action costs

    // Every dungeon floor the player has ever set foot on, kept alive for
    // the whole run. Returning to level 3 shows it exactly as it was left -
    // dropped items, dead monsters staying dead, explored map and all.
    private readonly Dictionary<int, Level> _levels = new();
    private readonly Random _rng = new();

    private Level CurrentLevel => _levels[Depth];

    public int Depth { get; private set; } = 1;

    public Map Map => CurrentLevel.Map;
    public List<Entity> Entities => CurrentLevel.Entities;
    public List<Item> Items => CurrentLevel.Items;
    public Player Player { get; private set; } = null!;
    public MessageLog Log { get; } = new();

    public IEnumerable<Entity> AllEntities => Entities.Append(Player);

    public Entity? EntityAt(int x, int y) =>
        AllEntities.FirstOrDefault(e => e.IsAlive && e.X == x && e.Y == y);

    public Item? ItemAt(int x, int y) =>
        Items.FirstOrDefault(i => i.X == x && i.Y == y);

    public void Initialize()
    {
        Player = new Player(0, 0);

        _levels[Depth] = GenerateLevel(Depth);
        Player.X = CurrentLevel.StairsUp.x;
        Player.Y = CurrentLevel.StairsUp.y;

        UpdateFov();

        Log.Add("Welcome to the dungeon of certain doom.");
    }

    /// <summary>
    /// Executes a single command and returns whether it consumed a turn.
    /// After the player acts, the world ticks forward (energy gain + monster
    /// actions) until the player has recovered enough energy to act again -
    /// that recovery loop is what makes speed differences actually matter.
    /// Game has no idea whether it's being driven by a console loop, a
    /// WinForms KeyDown event, or anything else - that's entirely the
    /// front-end's problem.
    /// </summary>
    public bool ExecuteTurn(ICommand command)
    {
        bool turnTaken = command.Execute(this);
        if (!turnTaken)
            return false;

        Player.Energy -= ActionCost;

        while (Player.Energy < ActionCost && Player.IsAlive)
            Tick();

        Entities.RemoveAll(e => !e.IsAlive); // clear the corpses
        UpdateFov();
        return true;
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

    /// <summary>Attempts to move the player up or down a dungeon level.</summary>
    public bool TryChangeLevel(int delta)
    {
        var standingOn = Map.GetTile(Player.X, Player.Y).Type;

        if (delta > 0 && standingOn != TileType.StairsDown)
        {
            Log.Add("There are no stairs leading down here.", Color.Gray);
            return false;
        }
        if (delta < 0 && standingOn != TileType.StairsUp)
        {
            Log.Add("There are no stairs leading up here.", Color.Gray);
            return false;
        }
        if (delta < 0 && Depth == 1)
        {
            Log.Add("You cannot escape that easily.", Color.Gold);
            return false;
        }

        ChangeLevel(delta);
        return true;
    }

    private void ChangeLevel(int delta)
    {
        Depth += delta;

        // Only generate a floor the first time it's entered. After that the
        // stored Level is reused, so floors can't be re-rolled for loot.
        if (!_levels.ContainsKey(Depth))
            _levels[Depth] = GenerateLevel(Depth);

        // Arrive standing on the corresponding stairs of the new floor.
        var stairs = delta > 0 ? CurrentLevel.StairsUp : CurrentLevel.StairsDown;
        Player.X = stairs.x;
        Player.Y = stairs.y;

        UpdateFov();

        string flavor = delta > 0
            ? $"You descend the stairs. Welcome to level {Depth}."
            : $"You climb the stairs. Welcome back to level {Depth}.";
        Log.Add(flavor, Color.Gold);
    }

    private Level GenerateLevel(int depth)
    {
        var generator = new MapGenerator(_rng);
        var (map, _, stairsUp, stairsDown) = generator.Generate(width: 60, height: 25);
        var level = new Level(map, stairsUp, stairsDown);

        // A small bestiary showing off the initiative system: normal goblins,
        // fast bats that act twice per round, and slow ogres that only act
        // every other round. Monster count grows a little with depth.
        int monsterCount = 3 + depth;
        for (int i = 0; i < monsterCount; i++)
        {
            if (!TryFindSpawn(map, level, stairsUp, out int x, out int y))
                break;

            level.Entities.Add(_rng.Next(10) switch
            {
                0 or 1 => new Monster(x, y, "Ogre", 'O', health: 10, attackPower: 4, speed: 50, color: Color.DarkOrange),
                <= 5 => new Monster(x, y, "Bat", 'b', health: 3, attackPower: 1, speed: 200, color: Color.LightSteelBlue),
                _ => new Monster(x, y), // Goblin
            });
        }

        // A few potions scattered on the floor, ready for PickUpCommand.
        for (int i = 0; i < 5; i++)
        {
            if (!TryFindSpawn(map, level, stairsUp, out int x, out int y))
                break;

            level.Items.Add(new Item("Health Potion", '!', Color.Magenta, healAmount: 6) { X = x, Y = y });
        }

        return level;
    }

    /// <summary>One tick of the initiative clock: everyone gains energy,
    /// then anyone who can afford an action takes it - fast monsters can
    /// act several times in a single tick, slow ones save up across ticks.
    /// The player just banks energy here; the loop in ExecuteTurn stops
    /// ticking once they're ready to act again.</summary>
    private void Tick()
    {
        foreach (var entity in Entities.Where(e => e.IsAlive))
            entity.Energy += entity.Speed;

        foreach (var entity in Entities.Where(e => e.IsAlive && e.Energy >= ActionCost))
        {
            while (entity.Energy >= ActionCost && entity.IsAlive)
            {
                entity.Energy -= ActionCost;
                entity.TakeTurn(this);
            }
        }

        Player.Energy += Player.Speed;
    }

    // Finds a walkable floor tile that isn't stairs and isn't occupied,
    // keeping a couple of tiles of breathing room around the entry stairs
    // so a fresh floor doesn't instantly ambush you.
    private bool TryFindSpawn(Map map, Level level, (int x, int y) stairsUp, out int x, out int y)
    {
        for (int attempt = 0; attempt < 200; attempt++)
        {
            int candidateX = _rng.Next(map.Width);
            int candidateY = _rng.Next(map.Height);

            if (!map.IsWalkable(candidateX, candidateY))
                continue;
            if (map.GetTile(candidateX, candidateY).Type != TileType.Floor)
                continue;
            if (Math.Abs(candidateX - stairsUp.x) <= 2 && Math.Abs(candidateY - stairsUp.y) <= 2)
                continue;
            if (level.Entities.Any(e => e.X == candidateX && e.Y == candidateY))
                continue;
            if (level.Items.Any(i => i.X == candidateX && i.Y == candidateY))
                continue;

            x = candidateX;
            y = candidateY;
            return true;
        }

        x = y = 0;
        return false;
    }

    private void UpdateFov() => FieldOfView.Compute(Map, Player.X, Player.Y, FovRadius);
}
