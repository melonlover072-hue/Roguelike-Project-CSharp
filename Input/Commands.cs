using System.Drawing;
using RoguelikeSkeleton;
using RoguelikeSkeleton.Items;

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

public class EquipCommand : ICommand
{
    private readonly int _inventoryIndex;

    public EquipCommand(int inventoryIndex)
    {
        _inventoryIndex = inventoryIndex;
    }

    public bool Execute(Game game)
    {
        var player = game.Player;
        if (_inventoryIndex < 0 || _inventoryIndex >= player.Inventory.Count)
        {
            game.Log.Add("No such item.", Color.Gray);
            return false;
        }

        var item = player.Inventory[_inventoryIndex];
        if (item.Slot == EquipmentSlot.None)
        {
            game.Log.Add($"You can't equip the {item.Name}.", Color.Gray);
            return false;
        }

        player.Inventory.RemoveAt(_inventoryIndex);

        if (item.Slot == EquipmentSlot.Weapon)
        {
            if (player.EquippedWeapon is { } oldWeapon)
                player.Inventory.Add(oldWeapon); // swap back into the pack
            player.EquippedWeapon = item;
            game.Log.Add($"You wield the {item.Name} ({item.DamageMin}-{item.DamageMax} damage).", Color.Yellow);
        }
        else
        {
            if (player.EquippedArmor is { } oldArmor)
                player.Inventory.Add(oldArmor);
            player.EquippedArmor = item;
            game.Log.Add($"You wear the {item.Name} (+{item.ArmorBonus} AC).", Color.Yellow);
        }

        return true;
    }
}

public class DropCommand : ICommand
{
    private readonly int _inventoryIndex;

    public DropCommand(int inventoryIndex)
    {
        _inventoryIndex = inventoryIndex;
    }

    public bool Execute(Game game)
    {
        var player = game.Player;
        if (_inventoryIndex < 0 || _inventoryIndex >= player.Inventory.Count)
        {
            game.Log.Add("No such item.", Color.Gray);
            return false;
        }

        var item = player.Inventory[_inventoryIndex];
        player.Inventory.RemoveAt(_inventoryIndex);
        item.X = player.X;
        item.Y = player.Y;
        game.Items.Add(item); // stays on this floor even if you leave

        game.Log.Add($"You drop the {item.Name}.", Color.Yellow);
        return true;
    }
}

public class QuaffCommand : ICommand
{
    public bool Execute(Game game)
    {
        var potion = game.Player.Inventory
            .FirstOrDefault(item => item.HealAmount > 0 || item.MaxHealthBonus > 0);

        if (potion is null)
        {
            game.Log.Add("You have nothing to drink.", Color.Gray);
            return false;
        }

        // Vitality is still worth drinking at full health; healing isn't.
        if (game.Player.Health >= game.Player.MaxHealth && potion.MaxHealthBonus <= 0)
        {
            game.Log.Add("You are already at full health.", Color.Gray);
            return false;
        }

        game.Player.MaxHealth += potion.MaxHealthBonus;
        game.Player.Health = Math.Min(
            game.Player.Health + potion.HealAmount,
            game.Player.MaxHealth
        );

        game.Player.Inventory.Remove(potion);

        string effect = potion.MaxHealthBonus > 0
            ? $"You feel hardier (+{potion.MaxHealthBonus} max HP)"
            : $"You recover {potion.HealAmount} health";
        game.Log.Add($"You quaff the {potion.Name}. {effect}.", Color.Magenta);

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
