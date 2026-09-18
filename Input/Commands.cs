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
public class QuaffCommand : ICommand
{
    public bool Execute(Game game)
    {
        if (game.Player.Health >= game.Player.MaxHealth)
        {
            game.Log.Add("You are already at full health.", Color.Gray);
            return false;
        }

        var potion = game.Player.Inventory
            .FirstOrDefault(item => item.HealAmount > 0);

        if (potion is null)
        {
            game.Log.Add("You have no potions to drink.", Color.Gray);
            return false;
        }

        game.Player.Health = Math.Min(
            game.Player.Health + potion.HealAmount,
            game.Player.MaxHealth
        );

        game.Player.Inventory.Remove(potion);

        game.Log.Add(
            $"You quaff the {potion.Name} and recover {potion.HealAmount} health.",
            Color.Magenta
        );

        return true;
    }
}