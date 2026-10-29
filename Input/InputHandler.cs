using System.Drawing;
using System.Windows.Forms;

namespace RoguelikeSkeleton.Input;

/// <summary>
/// Translates raw key presses into ICommand objects. A small state machine:
/// most keys map straight to commands, but E/P first switch into item
/// selection mode, where the next letter picks an inventory slot and Esc
    /// backs out. This is the file to touch for rebinding keys.
/// </summary>
public class InputHandler
{
    private string? _pendingAction; // null = normal mode, otherwise the verb for item selection

    public bool IsSelecting => _pendingAction is not null;

    public string? Prompt =>
        IsSelecting ? $"{_pendingAction} which item? (letter, Esc to cancel)" : null;

    public void CancelSelection() => _pendingAction = null;

    public ICommand? Translate(Keys key, Game game)
    {
        if (IsSelecting)
            return TranslateSelection(key, game);

        ICommand? command = key switch
        {
            Keys.Up or Keys.W => new MoveCommand(0, -1),
            Keys.Down or Keys.S => new MoveCommand(0, 1),
            Keys.Left or Keys.A => new MoveCommand(-1, 0),
            Keys.Right or Keys.D => new MoveCommand(1, 0),
            Keys.G => new PickUpCommand(),
            Keys.Q => new QuaffCommand(),
            Keys.OemPeriod => new DescendCommand(), // '.' or '>'
            Keys.Oemcomma => new AscendCommand(),   // ',' or '<'
            Keys.Space => new WaitCommand(),
            _ => null,
        };
        if (command is not null)
            return command;

        if (key == Keys.E)
        {
            BeginSelection(game, "Equip");
            return null;
        }
        if (key == Keys.P)
        {
            BeginSelection(game, "Put down");
            return null;
        }

        return null; // Unrecognized key: ignore it, don't spend a turn.
    }

    // Lists the pack in the log with letters, then waits for a letter key.
    private void BeginSelection(Game game, string verb)
    {
        if (game.Player.Inventory.Count == 0)
        {
            game.Log.Add("Your pack is empty.", Color.Gray);
            return;
        }

        var listing = string.Join("   ",
            game.Player.Inventory.Select((item, i) => $"{(char)('a' + i)}) {item.Describe()}"));
        game.Log.Add($"{verb}: {listing}", Color.White);

        _pendingAction = verb;
    }

    private ICommand? TranslateSelection(Keys key, Game game)
    {
        string verb = _pendingAction!;
        _pendingAction = null; // whatever happens, selection ends here

        if (key == Keys.Escape)
        {
            game.Log.Add("Never mind.", Color.Gray);
            return null;
        }

        int index = (int)key - (int)Keys.A;
        if (index < 0 || index >= 26)
        {
            game.Log.Add("Please choose a letter.", Color.Gray);
            return null;
        }

        return verb switch
        {
            "Equip" => new EquipCommand(index),
            _ => new DropCommand(index),
        };
    }
}
