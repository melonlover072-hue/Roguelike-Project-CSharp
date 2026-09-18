# RoguelikeSkeleton

A minimal turn-based roguelike skeleton in C# / .NET, rendered in a real
WinForms window (Windows only).

## Running it

```
dotnet run
```

A window opens with a procedurally generated dungeon. Move with arrow keys
or WASD, `Space` to wait a turn, `Esc` to quit.

Requires the .NET SDK on Windows - no extra NuGet packages needed, since
WinForms ships with the SDK.

## Project layout

```
RoguelikeSkeleton/
├── Program.cs               WinForms entry point ([STAThread] Main)
├── Game.cs                  Owns Map/Player/Entities and executes turns
├── World/
│   ├── Tile.cs               A single map cell (wall/floor)
│   ├── Map.cs                Dumb grid of tiles - no generation logic
│   └── MapGenerator.cs       Room-and-corridor procedural generator
├── Entities/
│   ├── Entity.cs             Base class: position, glyph, color, health
│   ├── Player.cs             Player-specific movement
│   └── Monster.cs            Placeholder wander AI
├── Input/
│   ├── ICommand.cs           Command interface
│   ├── Commands.cs           MoveCommand / WaitCommand
│   └── InputHandler.cs       WinForms Keys -> ICommand translation
└── Rendering/
    └── GameForm.cs           The window: GDI+ drawing + key handling
```

## Why it's structured this way

- **Turn-based, event-driven.** There's no game loop anymore - the window
  sits idle until a key is pressed, `GameForm.OnKeyDown` turns that into an
  `ICommand`, `Game.ExecuteTurn` runs it (and lets monsters act if a turn
  was spent), then the window repaints. This maps naturally onto how
  WinForms already works (it's an event-driven UI framework), and it's
  exactly the same turn structure the console version used.
- **`Game` doesn't know a window exists.** `Game`, `Map`, `Entity`, `Player`,
  and `Monster` are completely unchanged in spirit from the console
  version - `Game` just exposes `Initialize()` and `ExecuteTurn(ICommand)`
  instead of owning a `Run()` loop. This is why swapping the console
  `Renderer`/`InputHandler` for `GameForm` didn't require touching any
  game logic at all.
- **Command pattern, still.** `MoveCommand`/`WaitCommand` are unchanged;
  only `InputHandler` changed, because it's the only class that knows
  about a specific input source (console keys vs. WinForms `Keys`).
- **GDI+ text-grid rendering.** `GameForm` draws each tile/entity glyph as
  a character in a fixed-size cell, measured once from the font so the
  grid stays aligned. It's simple and plenty fast for a turn-based game
  that only repaints on keypress.

## Natural next steps, roughly in order of how much they unlock

1. **A message log** — a scrolling panel/list of recent events ("Goblin
   hits you for 2 damage") instead of just the HP in the status bar.
2. **Better monster AI** — swap the random wander in `Monster.TakeTurn` for
   simple chase-the-player-if-visible logic, then real pathfinding (A*)
   later.
3. **Turn order / initiative** — right now every monster acts once per
   player turn in list order. An energy/initiative systgiem lets you have
   fast and slow monsters.
4. **Multiple dungeon levels** — stairs down/up, and `Game` holding a stack
   or list of `Map`s instead of just one.


   AI generated README, and code comments. Everything else is written and coded by hand. For full transparency.