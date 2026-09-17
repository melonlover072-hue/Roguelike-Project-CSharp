using RoguelikeSkeleton;

namespace RoguelikeSkeleton.Input;

public interface ICommand
{
    /// <summary>
    /// Executes the command against the game state. Returns true if it
    /// consumed a game turn (so monsters should get to act too), or false
    /// if it didn't (e.g. walking into a wall, or an unrecognized key).
    /// </summary>
    bool Execute(Game game);
}
