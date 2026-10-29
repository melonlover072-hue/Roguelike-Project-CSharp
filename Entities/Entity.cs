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

    // Combat: AttackBonus is added to the attacker's 1d20 roll and compared
    // against the target's ArmorClass - equal or higher hits. Damage is a
    // plain roll between DamageMin and DamageMax on a hit. The defaults are
    // bare fists against unarmoured skin.
    public int AttackBonus { get; set; }
    public virtual int ArmorClass => 10;
    public virtual int DamageMin => 1;
    public virtual int DamageMax => 2;

    // Initiative: Speed is how much energy an entity regains per world tick
    // (100 = acts every tick; 200 = acts twice; 50 = every other tick).
    // Energy is the entity's current banked action points.
    public int Speed { get; set; } = 100;
    public int Energy { get; set; }

    public bool IsAlive => Health > 0;

    protected Entity(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Called when this entity has banked enough energy to act. Override
    /// this for monster AI. The player's own actions are driven directly by
    /// input commands instead, so Player doesn't need to override this.
    /// </summary>
    public virtual void TakeTurn(Game game) { }
}
