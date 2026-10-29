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

    /// <summary>Shared melee resolution for the player and monsters:
    /// 1d20 + AttackBonus against ArmorClass to hit, then a damage roll.</summary>
    public void ResolveMelee(Entity attacker, Entity target)
    {
        string attackerName = attacker is Player ? "You" : attacker.Name;
        string targetName = target is Player ? "you" : target.Name;

        int roll = _rng.Next(1, 21); // 1d20
        if (roll + attacker.AttackBonus < target.ArmorClass)
        {
            string missVerb = attacker is Player ? "miss" : "misses";
            Log.Add($"{attackerName} {missVerb} {targetName}.", Color.Gray);
            return;
        }

        int damage = _rng.Next(attacker.DamageMin, attacker.DamageMax + 1);
        target.Health -= damage;

        if (!target.IsAlive)
        {
            string killVerb = attacker is Player ? "kill" : "kills";
            Log.Add($"{attackerName} {killVerb} {targetName}!", Color.OrangeRed);
            return;
        }

        string verb = attacker is Player ? "hit" : "hits";
        var color = target is Player ? Color.OrangeRed : Color.White;
        Log.Add($"{attackerName} {verb} {targetName} for {damage} damage.", color);
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

        // A small bestiary exercising the initiative and combat systems:
        // goblins are average, bats are fast but flimsy, ogres are slow
        // bruisers that are easy to hit but hit back hard.
        int monsterCount = 3 + depth;
        for (int i = 0; i < monsterCount; i++)
        {
            if (!TryFindSpawn(map, level, stairsUp, out int x, out int y))
                break;

            level.Entities.Add(_rng.Next(10) switch
            {
                0 or 1 => new Monster(x, y, "Ogre", 'O',
                    health: 12, damageMin: 3, damageMax: 6,
                    attackBonus: 2, armorClass: 9,
                    speed: 50, color: Color.DarkOrange),
                <= 5 => new Monster(x, y, "Bat", 'b',
                    health: 3, damageMin: 1, damageMax: 2,
                    attackBonus: 2, armorClass: 13,
                    speed: 200, color: Color.LightSteelBlue),
                _ => new Monster(x, y), // Goblin
            });
        }

        // Scattered loot: mostly consumables, with weapons and armor mixed
        // in. Deeper floors see better gear.
        for (int i = 0; i < 6; i++)
        {
            if (!TryFindSpawn(map, level, stairsUp, out int x, out int y))
                break;

            Item item = CreateLoot(depth);
            item.X = x;
            item.Y = y;
            level.Items.Add(item);
        }

        return level;
    }

    private Item CreateLoot(int depth) => _rng.Next(10) switch
    {
        5 or 6 => CreateWeapon(depth),
        7 or 8 => CreateArmor(depth),
        _ => CreatePotion(), // everything else
    };

    private Item CreatePotion() => _rng.Next(4) switch
    {
        0 => new Item("Greater Health Potion", '!', Color.Fuchsia, healAmount: 15),
        1 => new Item("Potion of Vitality", '!', Color.DeepPink, healAmount: 3, maxHealthBonus: 3),
        _ => new Item("Health Potion", '!', Color.Magenta, healAmount: 6),
    };

    private Item CreateWeapon(int depth)
    {
        if (depth >= 3 && _rng.Next(2) == 0)
            return new Item("Battle Axe", '(', Color.Silver,
                slot: EquipmentSlot.Weapon, damageMin: 4, damageMax: 9);

        return _rng.Next(2) == 0
            ? new Item("Rusty Dagger", '(', Color.LightGray,
                slot: EquipmentSlot.Weapon, damageMin: 2, damageMax: 4)
            : new Item("Short Sword", '(', Color.White,
                slot: EquipmentSlot.Weapon, damageMin: 3, damageMax: 6);
    }

    private Item CreateArmor(int depth)
    {
        if (depth >= 3 && _rng.Next(2) == 0)
            return new Item("Chain Mail", '[', Color.Silver,
                slot: EquipmentSlot.Armor, armorBonus: 4);

        return _rng.Next(2) == 0
            ? new Item("Cloth Armor", '[', Color.Tan,
                slot: EquipmentSlot.Armor, armorBonus: 1)
            : new Item("Leather Armor", '[', Color.SaddleBrown,
                slot: EquipmentSlot.Armor, armorBonus: 2);
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
