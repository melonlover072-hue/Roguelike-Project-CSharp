using System.Drawing;
using RoguelikeSkeleton;

namespace RoguelikeSkeleton.Input;

public class MoveCommand : ICommand
{
    private readonly int _dx;
    private readonly int _dy;

    public MoveCommand(int dx, int dy)
    {
        _dx = dx;
        _dy = dy;
    }

    public bool Execute(Game game)
    {
        // Bump-to-attack: if something alive stands on the target tile,
        // moving into it becomes a melee attack instead.
        var occupant = game.EntityAt(game.Player.X + _dx, game.Player.Y + _dy);
        if (occupant is not null && occupant != game.Player)
            return new MeleeAttackCommand(_dx, _dy).Execute(game);

        return game.Player.TryMove(_dx, _dy, game.Map);
    }
}

public class MeleeAttackCommand : ICommand
{
    private readonly int _dx;
    private readonly int _dy;

    public MeleeAttackCommand(int dx, int dy)
    {
        _dx = dx;
        _dy = dy;
    }

    public bool Execute(Game game)
    {
        var target = game.EntityAt(game.Player.X + _dx, game.Player.Y + _dy);
        if (target is null || target == game.Player)
        {
            game.Log.Add("You swing at empty air.", Color.Gray);
            return false; // nothing there - the turn isn't spent
        }

        game.Player.Attack(target, game);
        return true;
    }
}

public class PickUpCommand : ICommand
{
    public bool Execute(Game game)
    {
        var item = game.ItemAt(game.Player.X, game.Player.Y);
        if (item is null)
        {
            game.Log.Add("There is nothing here to pick up.", Color.Gray);
            return false;
        }

        game.Items.Remove(item);
        game.Player.Inventory.Add(item);
        game.Log.Add($"You pick up the {item.Name}.", Color.Yellow);
        return true;
    }
}

public class DescendCommand : ICommand
{
    // Going deeper always costs a turn (if there are stairs to take).
    public bool Execute(Game game) => game.TryChangeLevel(+1);
}

public class AscendCommand : ICommand
{
    public bool Execute(Game game) => game.TryChangeLevel(-1);
}

public class WaitCommand : ICommand
{
    // Passing a turn still costs a turn - the world keeps ticking while you wait.
    public bool Execute(Game game) => true;
}
