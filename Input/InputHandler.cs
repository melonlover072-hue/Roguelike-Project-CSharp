using System.Windows.Forms;

namespace RoguelikeSkeleton.Input;

/// <summary>
/// Translates raw key presses into ICommand objects. This is the only file
/// that needs to change to rebind keys or add new player actions.
/// </summary>
public static class InputHandler
{
    public static ICommand? Translate(Keys key) => key switch
    {
        Keys.Up or Keys.W => new MoveCommand(0, -1),
        Keys.Down or Keys.S => new MoveCommand(0, 1),
        Keys.Left or Keys.A => new MoveCommand(-1, 0),
        Keys.Right or Keys.D => new MoveCommand(1, 0),
        Keys.G => new PickUpCommand(),
        Keys.OemPeriod => new DescendCommand(), // '.' or '>'
        Keys.Oemcomma => new AscendCommand(),   // ',' or '<'
        Keys.Space => new WaitCommand(),
        _ => null // Unrecognized key: ignore it, don't spend a turn.
    };
}
