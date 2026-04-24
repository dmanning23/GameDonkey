# GameDonkey

A MonoGame game engine for character-based action games. Most games have a *game engine* — mine are so shitty that the engine is more of a *game donkey*, barely pulling the thing along. Idunno, it was funny at the time.

[![NuGet](https://img.shields.io/nuget/v/GameDonkey.svg)](https://www.nuget.org/packages/GameDonkey/)

## What It Does

GameDonkey handles the core loop for 2D character-based games (fighters, brawlers, action platformers) built on MonoGame:

- **State machine + timed actions** — characters are driven by XML-defined state machines; each state fires actions at specific timestamps
- **Physics & collision detection** — player-vs-player, player-vs-level-object, and world boundary collisions
- **Board/level management** — parallax backgrounds, spawn points, foreground/background layers, level objects
- **Player management** — player queues with health, garments (costume layers), ragdoll, trails, shadows
- **Projectiles** — first-class projectile objects with their own physics containers
- **AI controllers** — drop-in AI to replace human input
- **Particle effects, sounds, camera shake** — all triggerable as timed state actions

## Installation

```
dotnet add package GameDonkey
```

## Architecture

```
IGameDonkey          — main engine (update loop, render, load content)
├── IBoard           — the game world (boundaries, level objects, spawn points)
├── IPlayerQueue[]   — one per player (human or AI, wraps the active character(s))
│   └── StateContainer — binds a StringStateMachine to a list of timed actions
│       └── BaseAction — executes at a specific time within a state
└── ParticleEngine   — managed by the engine, triggered via actions
```

### Object types

| Class | Purpose |
|---|---|
| `PlayerObject` | Human/AI controlled characters |
| `LevelObject` | Static and dynamic level geometry |
| `ProjectileObject` | Bullets, thrown objects, hit circles |

Each has its own `PhysicsContainer` subclass.

### State machine actions

Actions are attached to states and fire at a given time (in seconds) after entering the state. All 25 action types:

`AddGarment` `PlayAnimation` `AddVelocity` `SetVelocity` `ConstantAcceleration` `ConstantDecceleration` `Shield` `Evade` `Projectile` `PlaySound` `Trail` `CreateAttack` `Block` `CreateHitCircle` `CreateThrow` `ParticleEffect` `PointLight` `SendStateMessage` `Deactivate` `Rotate` `TargetRotation` `CameraShake` `KillPlayer` `Random` `SendToBack`

## Basic Usage

```csharp
// 1. Create the engine
var engine = new GameDonkey(renderer, this);
engine.LoadContent(GraphicsDevice, Content);

// 2. Load a board
engine.LoadBoard(new Filename("levels/stage1.xml"), Content);

// 3. Load players
engine.LoadPlayer(Color.Blue, new Filename("characters/fighter.xml"),
    playerIndex: 0, playerName: "P1");

// 4. Game loop
protected override void Update(GameTime gameTime)
{
    engine.UpdateInput(inputState);
    engine.Update(gameTime);
}

protected override void Draw(GameTime gameTime)
{
    engine.UpdateCameraMatrix();
    engine.Render(BlendState.AlphaBlend);
}
```

## Extending GameDonkey

Subclass `GameDonkey` to override:

- `CheckForWinner()` — your win condition
- `KillPlayer(IPlayerQueue)` — death handling and respawn logic
- `CollisionDetection()` — add custom collision response
- `UpdateStuff()` — anything else per-tick
- `CreatePlayerQueue(Color)` — factory for custom player queue types
- `CreateBoard()` — factory for custom board types

## Data Files (XML)

Characters and boards are defined in XML, loaded via MonoGame's `ContentManager`.

```
characters/fighter.xml       — BaseObjectModel (model, animations, garments, states)
states/fighter_states.xml    — state machine transitions
states/fighter_actions.xml   — StateContainerModel (timed actions per state)
levels/stage1.xml            — BoardModel
moves/MoveList.xml           — input move list (HadoukInput format)
```

## Dependencies

All available on NuGet:

`MonoGame.Framework.DesktopGL` `AnimationLib` `CameraBuddy` `CollisionBuddy` `DrawListBuddy` `FilenameBuddy` `FontBuddy` `GameTimer` `HadoukInput` `ParallaxBackgroundBuddy` `ParticleBuddy` `PrimitiveBuddy` `RenderBuddy` `ResolutionBuddy` `StateMachineBuddy` `UndoRedoBuddy` `XmlBuddy`

## License

MIT
