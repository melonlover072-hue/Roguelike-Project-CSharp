using System.Drawing;
using RoguelikeSkeleton;

namespace RoguelikeSkeleton.Entities;

/// <summary>
/// Base class for anything that occupies a tile and can be drawn: the
/// player, monsters, and eventually items/chests/etc. if you give those
/// a position on the ground.
/// </summary>
public abstract class Entity
{
    public int X { get; set; }
    public int Y { get; set; }
    public char Glyph { get; protected set; } = '?';
    public Color Color { get; protected set; } = Color.White;
    public string Name { get; protected set; } = "Entity";

    public int Health { get; set; } = 1;
    public int MaxHealth { get; set; } = 1;
    public int AttackPower { get; set; } = 1;
    public bool IsAlive => Health > 0;

    protected Entity(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Called once per game turn, after the player has acted. Override this
    /// for monster AI. The player's own movement is driven directly by input
    /// commands instead, so Player doesn't need to override this.
    /// </summary>
    public virtual void TakeTurn(Game game) { }
}
