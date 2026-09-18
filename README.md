# RoguelikeSkeleton

A small turn-based roguelike written in C# / .NET, rendered in a real WinForms window (Windows only).

The project started as a minimal skeleton for experimenting with roguelike architecture in C#, but has since grown into a functional early-stage roguelike with procedural dungeon generation, persistent dungeon levels, monsters with different speeds, field-of-view-based perception, combat, stairs, an inventory, items, consumables, and a scrolling message log.

## Running it

```text
dotnet run
```

A window opens with a procedurally generated dungeon.

Move with the arrow keys or WASD. `Space` waits a turn, `Q` quaffs a health potion when one is in your inventory, and `Esc` quits.

Requires the .NET SDK on Windows. No extra NuGet packages are required for the game itself, since WinForms ships with the .NET SDK.

## Project layout

```text
RoguelikeSkeleton/
├── Program.cs               WinForms entry point ([STAThread] Main)
├── Game.cs                  Owns game state and executes turns
├── World/
│   ├── Tile.cs              A single map cell (wall/floor/stairs/etc.)
│   ├── Map.cs               Grid of tiles representing a dungeon level
│   └── MapGenerator.cs      Room-and-corridor procedural generator
├── Entities/
│   ├── Entity.cs            Base class for things with position and stats
│   ├── Player.cs            Player-specific state and behaviour
│   └── Monster.cs           Monster state, perception, movement and AI
├── Items/
│   └── Item.cs              Items that can exist in the world or inventory
├── Input/
│   ├── ICommand.cs          Command interface
│   ├── Commands.cs          Player actions such as movement, waiting,
│   │                         picking up and quaffing
│   └── InputHandler.cs      WinForms Keys -> ICommand translation
├── Rendering/
│   └── GameForm.cs          The WinForms window and GDI+ renderer
└── MessageLog.cs             Stores recent in-game messages and their colors
```

## Why it's structured this way

* **Turn-based, event-driven.** There is no traditional constantly-running
  game loop. The WinForms window sits idle until a key is pressed.
  `GameForm` translates the key into an `ICommand`, `Game.ExecuteTurn`
  executes it, the world advances when appropriate, and the window
  repaints. This fits naturally with WinForms' event-driven architecture.

* **`Game` doesn't know a window exists.** The game logic is separated from
  the WinForms presentation layer. `Game`, `Map`, `Entity`, `Player`,
  `Monster`, items and commands operate on game state without needing to
  know how that state is displayed.

* **Command pattern.** Player actions are represented by `ICommand`
  implementations. Movement, waiting, picking up items and quaffing
  consumables all go through the same command/turn system. This keeps input
  handling separate from the actual game logic.

* **Persistent dungeon levels.** A dungeon level is more than just a map.
  Levels retain their state when the player leaves them, allowing monsters,
  items and exploration state to remain where they were. Stairs connect
  levels and allow the player to travel between them.

* **Energy-based turn system.** Entities have a speed and an energy value
  rather than simply receiving exactly one action per player turn. A normal
  creature can act once per normal turn, while faster or slower creatures
  can act more or less frequently. This is currently exercised by enemies
  with different speeds, including fast bats and slow ogres.

* **Monster perception and AI.** Monsters can detect the player through
  field of view and will chase them when they have line of sight. When the
  player is no longer visible, monsters return to wandering behaviour.
  Full pathfinding is not implemented yet, so the current chase behaviour
  is intentionally simple.

* **Shared combat logic.** Melee attacks use common combat resolution rather
  than having completely separate player and monster combat systems.
  Monsters can attack the player when they get close enough.

* **Items and inventory.** Items can exist on dungeon levels and can be
  picked up into the player's inventory. Consumable items can then be used
  through commands; health potions currently provide healing.

* **Message log.** Important events are recorded in a message log and
  displayed beside the dungeon. Messages are color-coded and wrapped to
  fit the available space instead of simply being cut off at the edge of
  the panel.

* **GDI+ text-grid rendering.** `GameForm` draws the dungeon as a fixed-size
  character grid using GDI+. The cell dimensions are measured from the
  selected font so the map remains aligned, and the game only repaints when
  the state changes.

## Current gameplay systems

The current implementation includes:

* Procedurally generated room-and-corridor dungeons
* Multiple persistent dungeon levels
* Up/down stairs
* Player movement and waiting
* Monsters and basic monster AI
* Field-of-view-based monster perception
* Melee combat
* Health and damage
* Energy/speed-based turn handling
* Multiple monster speeds
* Items in the dungeon
* Player inventory
* Item pickup
* Health potions and quaffing
* A color-coded, word-wrapped message log
* WinForms/GDI+ rendering
* Automated build and test workflow through GitHub Actions

## Natural next steps

The exact order is likely to change as development continues, but some obvious areas for expansion are:

1. **Improve monster AI** — the current chase behaviour works without
   proper pathfinding, but eventually monsters will need more sophisticated
   navigation around walls and obstacles.

2. **Expand the item system** — more consumables, equipment, weapons,
   armour and other item behaviours can build on the existing inventory and
   command structure.

3. **Expand combat** — more detailed combat rules, different attacks,
   armour, resistances and enemy abilities can be added once the basic
   system is established.

4. **Improve the dungeon** — additional terrain, doors, rooms, hazards,
   special locations and more interesting procedural generation.

5. **More creature behaviour** — different AI types, ranged enemies,
   creatures with special abilities and more varied speed/initiative
   behaviour.

6. **UI improvements** — the current WinForms renderer is deliberately
   simple. More information can eventually be exposed through the UI
   without putting presentation logic into the game systems.

The project is intentionally being developed incrementally. The goal is to
keep the underlying systems understandable while adding actual roguelike
mechanics rather than designing the entire game architecture in advance.

---

AI generated README, and code comments. Everything else is written and coded by hand. For full transparency.
