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

    public bool Execute(Game game) => game.Player.TryMove(_dx, _dy, game.Map);
}

public class WaitCommand : ICommand
{
    // Passing a turn still costs a turn - monsters keep moving while you wait.
    public bool Execute(Game game) => true;
}
