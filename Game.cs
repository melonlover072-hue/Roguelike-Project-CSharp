using RoguelikeSkeleton.Entities;
using RoguelikeSkeleton.Input;
using RoguelikeSkeleton.World;

namespace RoguelikeSkeleton;

public class Game
{
    public Map Map { get; private set; } = null!;
    public Player Player { get; private set; } = null!;
    public List<Entity> Entities { get; } = new();

    public IEnumerable<Entity> AllEntities => Entities.Append(Player);

    public void Initialize()
    {
        var generator = new MapGenerator();
        var (map, start) = generator.Generate(width: 60, height: 25);
        Map = map;

        Player = new Player(start.x, start.y);

        // A couple of placeholder monsters just to prove entities work end-to-end.
        var rng = new Random();
        for (int i = 0; i < 3; i++)
        {
            int x, y;
            do
            {
                x = rng.Next(Map.Width);
                y = rng.Next(Map.Height);
            } while (!Map.IsWalkable(x, y) || (x == Player.X && y == Player.Y));

            Entities.Add(new Monster(x, y));
        }
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
            RunMonsterTurns();
        return turnTaken;
    }

    private void RunMonsterTurns()
    {
        foreach (var entity in Entities.Where(e => e.IsAlive))
            entity.TakeTurn(this);
    }
}
