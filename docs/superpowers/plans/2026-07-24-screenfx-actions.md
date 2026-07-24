# ScreenFXBuddy Actions Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Expose all 18 `ScreenFXBuddy.IScreenFXService.Trigger*` methods as scriptable `EActionType` values in GameDonkey (enum, model, action, factory wiring, tests), plus the minimal engine plumbing so triggers execute without throwing.

**Architecture:** One `EActionType` + `BaseActionModel` subclass + `BaseAction` subclass per effect, following the existing `CameraShakeAction`/`CameraShakeActionModel` and `PointLightAction`/`PointLightActionModel` pattern exactly. All 18 actions read `engine.ScreenFX` (a new `IScreenFXService` property on `IGameDonkey`) in `LoadContent` and call the matching `Trigger*` method in `Execute`. No render-pipeline (`BeginCapture`/`EndCapture`) integration — that's explicitly out of scope per the design spec.

**Tech Stack:** .NET (GameDonkeyLib class library), MonoGame (`Microsoft.Xna.Framework`), NUnit + Shouldly for tests, `ScreenFxBuddy` NuGet package (namespace `ScreenFXBuddy`).

**Spec:** `docs/superpowers/specs/2026-07-24-screenfx-actions-design.md`

## Global Constraints

- Namespace for all production code: `GameDonkeyLib`. Namespace for all tests: `GameDonkey.Tests`.
- Indentation: 4 spaces (match `CameraShakeAction.cs` / `PointLightAction.cs`, not the older tab-indented files).
- Model classes derive from `BaseActionModel`; action classes derive from `BaseAction`. Each gets three constructors: `()` / `(BaseObject owner)`, `(<Name>Action action)` / `(BaseObject owner, <Name>ActionModel actionModel)`, and `(BaseAction action)` / `(BaseObject owner, BaseActionModel actionModel)` (the last delegates via `as` cast — see `CameraShakeAction.cs:31-34`).
- XML enum parsing: `(T)Enum.Parse(typeof(T), value)`. `Color`/`Vector2` parsing: `value.ToColor()` / `value.ToVector2()` from `Vector2Extensions` (`using Vector2Extensions;`). `bool`/`int`: `Convert.ToBoolean(value)` / `Convert.ToInt32(value)`. All XML node names compared via `name.ToLower()` in a `switch`.
- **Naming rule:** every ScreenFXBuddy trigger parameter literally called `time` (an effect's own duration) is renamed to `Duration` on the model/action to avoid confusion with `BaseActionModel.Time` / `BaseAction.Time`, which is the state-machine's own scheduling delay. All other trigger parameter names are kept as-is (`PascalCase`d).
- Effects with a `Vector2 position` trigger parameter do **not** store position on the model or action — `Execute()` passes `Owner.Position` directly. No offset property.
- Every model/action pair gets a test file `GameDonkey.Tests/Actions/<Name>Tests.cs` with exactly three tests: `ModelToAction`, `ActionToModel`, `Persist` (XML round-trip via `StateContainerModel`/`SingleStateActionsModel`), following `CameraShakeTests.cs`. Tests always construct actions with a `null` owner (`Execute()` is never called in these tests — matches existing convention).
- Test command: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj`. Build command: `dotnet build GameDonkey.sln`.
- Every task ends with a commit.

---

## Task 1: Enum values + engine plumbing

**Files:**
- Modify: `GameDonkey/StateMachine/ActionType.cs`
- Modify: `GameDonkey/IGameDonkey.cs`
- Modify: `GameDonkey/GameDonkey.cs`

**Interfaces:**
- Consumes: existing `EActionType` enum, existing `IGameDonkey`/`GameDonkey` structure, `ScreenFXBuddy.IScreenFXService`, `ScreenFXBuddy.ScreenFXComponent(Game game)` (constructor), `.LoadContent()`, `.Update(GameClock)`, and `IDisposable.Dispose()`.
- Produces: `EActionType.ForceRipple`, `.GravityWave`, `.ScreenShake`, `.ChromaticAberration`, `.ChromaticSplit`, `.HeatHaze`, `.HitFlash`, `.AnimeSuper`, `.Letterbox`, `.FreezeFrame`, `.ZoomBlur`, `.ScreenTilt`, `.Electric`, `.Frost`, `.Vortex`, `.Smoke`, `.GlassShatter`, `.SpeedLines`. `IGameDonkey.ScreenFX` (type `IScreenFXService`), readable by every later task's `LoadContent(IGameDonkey engine, ContentManager content)`.

- [ ] **Step 1: Add the 18 enum values**

In `GameDonkey/StateMachine/ActionType.cs`, replace:

```csharp
        TemplateAction
    }
```

with:

```csharp
        TemplateAction,
        ForceRipple,
        GravityWave,
        ScreenShake,
        ChromaticAberration,
        ChromaticSplit,
        HeatHaze,
        HitFlash,
        AnimeSuper,
        Letterbox,
        FreezeFrame,
        ZoomBlur,
        ScreenTilt,
        Electric,
        Frost,
        Vortex,
        Smoke,
        GlassShatter,
        SpeedLines
    }
```

- [ ] **Step 2: Add `ScreenFX` to `IGameDonkey`**

In `GameDonkey/IGameDonkey.cs`, add the using and the property. Replace:

```csharp
using FilenameBuddy;
using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ParticleBuddy;
using RenderBuddy;
using System;
using System.Collections.Generic;
```

with:

```csharp
using FilenameBuddy;
using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ParticleBuddy;
using RenderBuddy;
using ScreenFXBuddy;
using System;
using System.Collections.Generic;
```

Then replace:

```csharp
        ParticleEngine ParticleEngine { get; }
```

with:

```csharp
        ParticleEngine ParticleEngine { get; }

        IScreenFXService ScreenFX { get; }
```

- [ ] **Step 3: Wire `ScreenFX` into `GameDonkey`**

In `GameDonkey/GameDonkey.cs`, add the using. Replace:

```csharp
using ParticleBuddy;
using RenderBuddy;
using ResolutionBuddy;
using System;
```

with:

```csharp
using ParticleBuddy;
using RenderBuddy;
using ResolutionBuddy;
using ScreenFXBuddy;
using System;
```

Add the property. Replace:

```csharp
        public ParticleEngine ParticleEngine { get; protected set; }
```

with:

```csharp
        public ParticleEngine ParticleEngine { get; protected set; }

        public IScreenFXService ScreenFX { get; protected set; }
```

Instantiate it in the constructor, right after `Game` is assigned. Replace:

```csharp
            Game = game;
            SoundContent = new ContentManager(Game.Services, "Content");
```

with:

```csharp
            Game = game;
            ScreenFX = new ScreenFXComponent(Game);
            SoundContent = new ContentManager(Game.Services, "Content");
```

Load its content. Replace:

```csharp
        public virtual void LoadContent(GraphicsDevice graphics, ContentManager xmlContent)
        {
            Renderer.LoadContent(graphics);
        }
```

with:

```csharp
        public virtual void LoadContent(GraphicsDevice graphics, ContentManager xmlContent)
        {
            Renderer.LoadContent(graphics);
            ScreenFX.LoadContent();
        }
```

Dispose it. Replace:

```csharp
        public virtual void UnloadContent()
        {
            SoundContent?.Dispose();
            SoundContent = null;

            Renderer?.UnloadContent();
            Renderer = null;

            Players = null;
        }
```

with:

```csharp
        public virtual void UnloadContent()
        {
            SoundContent?.Dispose();
            SoundContent = null;

            Renderer?.UnloadContent();
            Renderer = null;

            (ScreenFX as IDisposable)?.Dispose();
            ScreenFX = null;

            Players = null;
        }
```

Tick it every frame, right after `ParticleEngine.Update`. Replace:

```csharp
            ParticleEngine.Update(MasterClock);

            UpdateStuff();
```

with:

```csharp
            ParticleEngine.Update(MasterClock);
            ScreenFX.Update(MasterClock);

            UpdateStuff();
```

- [ ] **Step 4: Build and verify**

Run: `dotnet build GameDonkey.sln`
Expected: build succeeds with no errors (there are no consumers of the new enum values or `ScreenFX` property yet, so this only validates the plumbing itself).

- [ ] **Step 5: Commit**

```bash
git add GameDonkey/StateMachine/ActionType.cs GameDonkey/IGameDonkey.cs GameDonkey/GameDonkey.cs
git commit -m "Add ScreenFX action types and wire IScreenFXService into the engine"
```

---

## Task 2: ForceRipple action

**Files:**
- Create: `GameDonkey/Models/Actions/ForceRippleActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/ForceRippleAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/ForceRippleTests.cs`

**Interfaces:**
- Consumes: `EActionType.ForceRipple` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerForceRipple(Vector2 position, float strength = 0.05f, float speed = 0.4f, float size = 0.08f, float time = 2f)`, `BaseActionModel`, `BaseAction`, `TestHelpers.InitFilePaths()`, `StateContainerModel`, `SingleStateActionsModel`.
- Produces: `ForceRippleActionModel` (properties `Strength`, `Speed`, `Size`, `Duration`), `ForceRippleAction` (same properties + `Execute`).

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/ForceRippleActionModel.cs`:

```csharp
using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class ForceRippleActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.ForceRipple;
            }
        }

        public float Strength { get; set; }
        public float Speed { get; set; }
        public float Size { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public ForceRippleActionModel()
        {
            Strength = 0.05f;
            Speed = 0.4f;
            Size = 0.08f;
            Duration = 2f;
        }

        public ForceRippleActionModel(ForceRippleAction action) : base(action)
        {
            Strength = action.Strength;
            Speed = action.Speed;
            Size = action.Size;
            Duration = action.Duration;
        }

        public ForceRippleActionModel(BaseAction action) : this(action as ForceRippleAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "strength":
                    {
                        Strength = Convert.ToSingle(value);
                    }
                    break;
                case "speed":
                    {
                        Speed = Convert.ToSingle(value);
                    }
                    break;
                case "size":
                    {
                        Size = Convert.ToSingle(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("Strength", Strength.ToString());
            xmlWriter.WriteAttributeString("Speed", Speed.ToString());
            xmlWriter.WriteAttributeString("Size", Size.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/ForceRippleAction.cs`:

```csharp
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class ForceRippleAction : BaseAction
    {
        #region Properties

        public float Strength { get; set; }
        public float Speed { get; set; }
        public float Size { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public ForceRippleAction(BaseObject owner) :
            base(owner, EActionType.ForceRipple)
        {
            Strength = 0.05f;
            Speed = 0.4f;
            Size = 0.08f;
            Duration = 2f;
        }

        public ForceRippleAction(BaseObject owner, ForceRippleActionModel actionModel) :
            base(owner, actionModel)
        {
            Strength = actionModel.Strength;
            Speed = actionModel.Speed;
            Size = actionModel.Size;
            Duration = actionModel.Duration;
        }

        public ForceRippleAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as ForceRippleActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerForceRipple(Owner.Position, Strength, Speed, Size, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, add a case immediately above the `default:` line in each of the 4 switch statements.

In `CreateActionModel(string actionType)`, replace:

```csharp
                case EActionType.Trail: { return new TrailActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.Trail: { return new TrailActionModel(); }
                case EActionType.ForceRipple: { return new ForceRippleActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

In `CreateActionModel(BaseAction actionModel)`, replace:

```csharp
                case EActionType.Trail: { return new TrailActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.Trail: { return new TrailActionModel(actionModel); }
                case EActionType.ForceRipple: { return new ForceRippleActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

In `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`, replace:

```csharp
                case EActionType.Trail: { return new TrailAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.Trail: { return new TrailAction(owner, actionModel); }
                case EActionType.ForceRipple: { return new ForceRippleAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

In `CreateStateAction(EActionType actionType, BaseObject owner)`, replace:

```csharp
                case EActionType.Trail: { return new TrailAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.Trail: { return new TrailAction(owner); }
                case EActionType.ForceRipple: { return new ForceRippleAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/ForceRippleTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ForceRippleTests
    {
        const float time = 1f;
        const float strength = 0.2f;
        const float speed = 0.6f;
        const float size = 0.15f;
        const float duration = 3.5f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new ForceRippleActionModel()
            {
                Time = time,
                Strength = strength,
                Speed = speed,
                Size = size,
                Duration = duration
            };

            var action = new ForceRippleAction(null, model);

            action.Time.ShouldBe(time);
            action.Strength.ShouldBe(strength);
            action.Speed.ShouldBe(speed);
            action.Size.ShouldBe(size);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new ForceRippleAction(null)
            {
                Time = time,
                Strength = strength,
                Speed = speed,
                Size = size,
                Duration = duration
            };

            var model = new ForceRippleActionModel(action);

            model.ActionType.ShouldBe(EActionType.ForceRipple);
            model.Time.ShouldBe(time);
            model.Strength.ShouldBe(strength);
            model.Speed.ShouldBe(speed);
            model.Size.ShouldBe(size);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new ForceRippleActionModel()
            {
                Time = time,
                Strength = strength,
                Speed = speed,
                Size = size,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("ForceRippleTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("ForceRippleTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as ForceRippleActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.ForceRipple);
            model2.Time.ShouldBe(time);
            model2.Strength.ShouldBe(strength);
            model2.Speed.ShouldBe(speed);
            model2.Size.ShouldBe(size);
            model2.Duration.ShouldBe(duration);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter ForceRippleTests`
Expected: 3 tests pass (`ModelToAction`, `ActionToModel`, `Persist`).

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/ForceRippleActionModel.cs GameDonkey/StateMachine/Actions/ForceRippleAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/ForceRippleTests.cs
git commit -m "Add ForceRipple screen effect action"
```

---

## Remaining effects (Tasks 3-19)

**Execution order matters.** Tasks 2 through 19 all edit the same four switch statements in `GameDonkey/StateMachine/StateActionFactory.cs`, each anchored on the case added by the immediately preceding task. They **must** be executed in numeric order (2, 3, 4, ... 19) — never in parallel, never out of order — or the factory edits in this plan will not find their anchor text.

### Task 3: GravityWave

**Files:**
- Create: `GameDonkey/Models/Actions/GravityWaveActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/GravityWaveAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/GravityWaveTests.cs`

**Interfaces:**
- Consumes: `EActionType.GravityWave` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerGravityWave(Vector2 position, float strength = 0.04f, float startHeight = 0.05f, float endHeight = 0.25f, float speed = 0.5f, float duration = 1.5f)`.
- Produces: `GravityWaveActionModel` (`Strength`, `StartHeight`, `EndHeight`, `Speed`, `Duration`), `GravityWaveAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/GravityWaveActionModel.cs`:

```csharp
using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class GravityWaveActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.GravityWave;
            }
        }

        public float Strength { get; set; }
        public float StartHeight { get; set; }
        public float EndHeight { get; set; }
        public float Speed { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public GravityWaveActionModel()
        {
            Strength = 0.04f;
            StartHeight = 0.05f;
            EndHeight = 0.25f;
            Speed = 0.5f;
            Duration = 1.5f;
        }

        public GravityWaveActionModel(GravityWaveAction action) : base(action)
        {
            Strength = action.Strength;
            StartHeight = action.StartHeight;
            EndHeight = action.EndHeight;
            Speed = action.Speed;
            Duration = action.Duration;
        }

        public GravityWaveActionModel(BaseAction action) : this(action as GravityWaveAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "strength":
                    {
                        Strength = Convert.ToSingle(value);
                    }
                    break;
                case "startheight":
                    {
                        StartHeight = Convert.ToSingle(value);
                    }
                    break;
                case "endheight":
                    {
                        EndHeight = Convert.ToSingle(value);
                    }
                    break;
                case "speed":
                    {
                        Speed = Convert.ToSingle(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("Strength", Strength.ToString());
            xmlWriter.WriteAttributeString("StartHeight", StartHeight.ToString());
            xmlWriter.WriteAttributeString("EndHeight", EndHeight.ToString());
            xmlWriter.WriteAttributeString("Speed", Speed.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/GravityWaveAction.cs`:

```csharp
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class GravityWaveAction : BaseAction
    {
        #region Properties

        public float Strength { get; set; }
        public float StartHeight { get; set; }
        public float EndHeight { get; set; }
        public float Speed { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public GravityWaveAction(BaseObject owner) :
            base(owner, EActionType.GravityWave)
        {
            Strength = 0.04f;
            StartHeight = 0.05f;
            EndHeight = 0.25f;
            Speed = 0.5f;
            Duration = 1.5f;
        }

        public GravityWaveAction(BaseObject owner, GravityWaveActionModel actionModel) :
            base(owner, actionModel)
        {
            Strength = actionModel.Strength;
            StartHeight = actionModel.StartHeight;
            EndHeight = actionModel.EndHeight;
            Speed = actionModel.Speed;
            Duration = actionModel.Duration;
        }

        public GravityWaveAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as GravityWaveActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerGravityWave(Owner.Position, Strength, StartHeight, EndHeight, Speed, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.ForceRipple: { return new ForceRippleActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.ForceRipple: { return new ForceRippleActionModel(); }
                case EActionType.GravityWave: { return new GravityWaveActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.ForceRipple: { return new ForceRippleActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.ForceRipple: { return new ForceRippleActionModel(actionModel); }
                case EActionType.GravityWave: { return new GravityWaveActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.ForceRipple: { return new ForceRippleAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.ForceRipple: { return new ForceRippleAction(owner, actionModel); }
                case EActionType.GravityWave: { return new GravityWaveAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.ForceRipple: { return new ForceRippleAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.ForceRipple: { return new ForceRippleAction(owner); }
                case EActionType.GravityWave: { return new GravityWaveAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/GravityWaveTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class GravityWaveTests
    {
        const float time = 1f;
        const float strength = 0.1f;
        const float startHeight = 0.15f;
        const float endHeight = 0.5f;
        const float speed = 0.8f;
        const float duration = 2.2f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new GravityWaveActionModel()
            {
                Time = time,
                Strength = strength,
                StartHeight = startHeight,
                EndHeight = endHeight,
                Speed = speed,
                Duration = duration
            };

            var action = new GravityWaveAction(null, model);

            action.Time.ShouldBe(time);
            action.Strength.ShouldBe(strength);
            action.StartHeight.ShouldBe(startHeight);
            action.EndHeight.ShouldBe(endHeight);
            action.Speed.ShouldBe(speed);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new GravityWaveAction(null)
            {
                Time = time,
                Strength = strength,
                StartHeight = startHeight,
                EndHeight = endHeight,
                Speed = speed,
                Duration = duration
            };

            var model = new GravityWaveActionModel(action);

            model.ActionType.ShouldBe(EActionType.GravityWave);
            model.Time.ShouldBe(time);
            model.Strength.ShouldBe(strength);
            model.StartHeight.ShouldBe(startHeight);
            model.EndHeight.ShouldBe(endHeight);
            model.Speed.ShouldBe(speed);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new GravityWaveActionModel()
            {
                Time = time,
                Strength = strength,
                StartHeight = startHeight,
                EndHeight = endHeight,
                Speed = speed,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("GravityWaveTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("GravityWaveTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as GravityWaveActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.GravityWave);
            model2.Time.ShouldBe(time);
            model2.Strength.ShouldBe(strength);
            model2.StartHeight.ShouldBe(startHeight);
            model2.EndHeight.ShouldBe(endHeight);
            model2.Speed.ShouldBe(speed);
            model2.Duration.ShouldBe(duration);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter GravityWaveTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/GravityWaveActionModel.cs GameDonkey/StateMachine/Actions/GravityWaveAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/GravityWaveTests.cs
git commit -m "Add GravityWave screen effect action"
```

---

### Task 4: ScreenShake

**Files:**
- Create: `GameDonkey/Models/Actions/ScreenShakeActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/ScreenShakeAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/ScreenShakeTests.cs`

**Interfaces:**
- Consumes: `EActionType.ScreenShake` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerScreenShake(float length = 1f, float delta = 0.1f, float amount = 0.1f)`. No `Vector2 position` parameter — this effect is screen-space only.
- Produces: `ScreenShakeActionModel` (`Length`, `Delta`, `Amount`), `ScreenShakeAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/ScreenShakeActionModel.cs`:

```csharp
using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class ScreenShakeActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.ScreenShake;
            }
        }

        public float Length { get; set; }
        public float Delta { get; set; }
        public float Amount { get; set; }

        #endregion //Properties

        #region Initialization

        public ScreenShakeActionModel()
        {
            Length = 1f;
            Delta = 0.1f;
            Amount = 0.1f;
        }

        public ScreenShakeActionModel(ScreenShakeAction action) : base(action)
        {
            Length = action.Length;
            Delta = action.Delta;
            Amount = action.Amount;
        }

        public ScreenShakeActionModel(BaseAction action) : this(action as ScreenShakeAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "length":
                    {
                        Length = Convert.ToSingle(value);
                    }
                    break;
                case "delta":
                    {
                        Delta = Convert.ToSingle(value);
                    }
                    break;
                case "amount":
                    {
                        Amount = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("Length", Length.ToString());
            xmlWriter.WriteAttributeString("Delta", Delta.ToString());
            xmlWriter.WriteAttributeString("Amount", Amount.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/ScreenShakeAction.cs`:

```csharp
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class ScreenShakeAction : BaseAction
    {
        #region Properties

        public float Length { get; set; }
        public float Delta { get; set; }
        public float Amount { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public ScreenShakeAction(BaseObject owner) :
            base(owner, EActionType.ScreenShake)
        {
            Length = 1f;
            Delta = 0.1f;
            Amount = 0.1f;
        }

        public ScreenShakeAction(BaseObject owner, ScreenShakeActionModel actionModel) :
            base(owner, actionModel)
        {
            Length = actionModel.Length;
            Delta = actionModel.Delta;
            Amount = actionModel.Amount;
        }

        public ScreenShakeAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as ScreenShakeActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerScreenShake(Length, Delta, Amount);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.GravityWave: { return new GravityWaveActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.GravityWave: { return new GravityWaveActionModel(); }
                case EActionType.ScreenShake: { return new ScreenShakeActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.GravityWave: { return new GravityWaveActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.GravityWave: { return new GravityWaveActionModel(actionModel); }
                case EActionType.ScreenShake: { return new ScreenShakeActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.GravityWave: { return new GravityWaveAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.GravityWave: { return new GravityWaveAction(owner, actionModel); }
                case EActionType.ScreenShake: { return new ScreenShakeAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.GravityWave: { return new GravityWaveAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.GravityWave: { return new GravityWaveAction(owner); }
                case EActionType.ScreenShake: { return new ScreenShakeAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/ScreenShakeTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ScreenShakeTests
    {
        const float time = 1f;
        const float length = 1.5f;
        const float delta = 0.25f;
        const float amount = 0.4f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new ScreenShakeActionModel()
            {
                Time = time,
                Length = length,
                Delta = delta,
                Amount = amount
            };

            var action = new ScreenShakeAction(null, model);

            action.Time.ShouldBe(time);
            action.Length.ShouldBe(length);
            action.Delta.ShouldBe(delta);
            action.Amount.ShouldBe(amount);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new ScreenShakeAction(null)
            {
                Time = time,
                Length = length,
                Delta = delta,
                Amount = amount
            };

            var model = new ScreenShakeActionModel(action);

            model.ActionType.ShouldBe(EActionType.ScreenShake);
            model.Time.ShouldBe(time);
            model.Length.ShouldBe(length);
            model.Delta.ShouldBe(delta);
            model.Amount.ShouldBe(amount);
        }

        [Test]
        public void Persist()
        {
            var model = new ScreenShakeActionModel()
            {
                Time = time,
                Length = length,
                Delta = delta,
                Amount = amount
            };

            var container = new StateContainerModel(new Filename("ScreenShakeTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("ScreenShakeTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as ScreenShakeActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.ScreenShake);
            model2.Time.ShouldBe(time);
            model2.Length.ShouldBe(length);
            model2.Delta.ShouldBe(delta);
            model2.Amount.ShouldBe(amount);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter ScreenShakeTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/ScreenShakeActionModel.cs GameDonkey/StateMachine/Actions/ScreenShakeAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/ScreenShakeTests.cs
git commit -m "Add ScreenShake screen effect action"
```

---

### Task 5: ChromaticAberration

**Files:**
- Create: `GameDonkey/Models/Actions/ChromaticAberrationActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/ChromaticAberrationAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/ChromaticAberrationTests.cs`

**Interfaces:**
- Consumes: `EActionType.ChromaticAberration` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerChromaticAberration(Vector2 startPosition, float distance = 0.1f, float time = 2f, FadeCurve curve = FadeCurve.Linear)`, `ScreenFXBuddy.FadeCurve` enum (`Linear`, `Logarithmic`, `Exponential`).
- Produces: `ChromaticAberrationActionModel` (`Distance`, `Duration`, `Curve`), `ChromaticAberrationAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/ChromaticAberrationActionModel.cs`:

```csharp
using ScreenFXBuddy;
using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class ChromaticAberrationActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.ChromaticAberration;
            }
        }

        public float Distance { get; set; }
        public float Duration { get; set; }
        public FadeCurve Curve { get; set; }

        #endregion //Properties

        #region Initialization

        public ChromaticAberrationActionModel()
        {
            Distance = 0.1f;
            Duration = 2f;
            Curve = FadeCurve.Linear;
        }

        public ChromaticAberrationActionModel(ChromaticAberrationAction action) : base(action)
        {
            Distance = action.Distance;
            Duration = action.Duration;
            Curve = action.Curve;
        }

        public ChromaticAberrationActionModel(BaseAction action) : this(action as ChromaticAberrationAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "distance":
                    {
                        Distance = Convert.ToSingle(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                case "curve":
                    {
                        Curve = (FadeCurve)Enum.Parse(typeof(FadeCurve), value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("Distance", Distance.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
            xmlWriter.WriteAttributeString("Curve", Curve.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/ChromaticAberrationAction.cs`:

```csharp
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class ChromaticAberrationAction : BaseAction
    {
        #region Properties

        public float Distance { get; set; }
        public float Duration { get; set; }
        public FadeCurve Curve { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public ChromaticAberrationAction(BaseObject owner) :
            base(owner, EActionType.ChromaticAberration)
        {
            Distance = 0.1f;
            Duration = 2f;
            Curve = FadeCurve.Linear;
        }

        public ChromaticAberrationAction(BaseObject owner, ChromaticAberrationActionModel actionModel) :
            base(owner, actionModel)
        {
            Distance = actionModel.Distance;
            Duration = actionModel.Duration;
            Curve = actionModel.Curve;
        }

        public ChromaticAberrationAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as ChromaticAberrationActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerChromaticAberration(Owner.Position, Distance, Duration, Curve);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.ScreenShake: { return new ScreenShakeActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.ScreenShake: { return new ScreenShakeActionModel(); }
                case EActionType.ChromaticAberration: { return new ChromaticAberrationActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.ScreenShake: { return new ScreenShakeActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.ScreenShake: { return new ScreenShakeActionModel(actionModel); }
                case EActionType.ChromaticAberration: { return new ChromaticAberrationActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.ScreenShake: { return new ScreenShakeAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.ScreenShake: { return new ScreenShakeAction(owner, actionModel); }
                case EActionType.ChromaticAberration: { return new ChromaticAberrationAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.ScreenShake: { return new ScreenShakeAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.ScreenShake: { return new ScreenShakeAction(owner); }
                case EActionType.ChromaticAberration: { return new ChromaticAberrationAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/ChromaticAberrationTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using ScreenFXBuddy;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ChromaticAberrationTests
    {
        const float time = 1f;
        const float distance = 0.3f;
        const float duration = 1.2f;
        const FadeCurve curve = FadeCurve.Exponential;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new ChromaticAberrationActionModel()
            {
                Time = time,
                Distance = distance,
                Duration = duration,
                Curve = curve
            };

            var action = new ChromaticAberrationAction(null, model);

            action.Time.ShouldBe(time);
            action.Distance.ShouldBe(distance);
            action.Duration.ShouldBe(duration);
            action.Curve.ShouldBe(curve);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new ChromaticAberrationAction(null)
            {
                Time = time,
                Distance = distance,
                Duration = duration,
                Curve = curve
            };

            var model = new ChromaticAberrationActionModel(action);

            model.ActionType.ShouldBe(EActionType.ChromaticAberration);
            model.Time.ShouldBe(time);
            model.Distance.ShouldBe(distance);
            model.Duration.ShouldBe(duration);
            model.Curve.ShouldBe(curve);
        }

        [Test]
        public void Persist()
        {
            var model = new ChromaticAberrationActionModel()
            {
                Time = time,
                Distance = distance,
                Duration = duration,
                Curve = curve
            };

            var container = new StateContainerModel(new Filename("ChromaticAberrationTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("ChromaticAberrationTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as ChromaticAberrationActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.ChromaticAberration);
            model2.Time.ShouldBe(time);
            model2.Distance.ShouldBe(distance);
            model2.Duration.ShouldBe(duration);
            model2.Curve.ShouldBe(curve);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter ChromaticAberrationTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/ChromaticAberrationActionModel.cs GameDonkey/StateMachine/Actions/ChromaticAberrationAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/ChromaticAberrationTests.cs
git commit -m "Add ChromaticAberration screen effect action"
```

---

### Task 6: ChromaticSplit

**Files:**
- Create: `GameDonkey/Models/Actions/ChromaticSplitActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/ChromaticSplitAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/ChromaticSplitTests.cs`

**Interfaces:**
- Consumes: `EActionType.ChromaticSplit` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerChromaticSplit(Vector2 position, float maxDistance = 0.05f, float duration = 0.3f)`.
- Produces: `ChromaticSplitActionModel` (`MaxDistance`, `Duration`), `ChromaticSplitAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/ChromaticSplitActionModel.cs`:

```csharp
using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class ChromaticSplitActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.ChromaticSplit;
            }
        }

        public float MaxDistance { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public ChromaticSplitActionModel()
        {
            MaxDistance = 0.05f;
            Duration = 0.3f;
        }

        public ChromaticSplitActionModel(ChromaticSplitAction action) : base(action)
        {
            MaxDistance = action.MaxDistance;
            Duration = action.Duration;
        }

        public ChromaticSplitActionModel(BaseAction action) : this(action as ChromaticSplitAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "maxdistance":
                    {
                        MaxDistance = Convert.ToSingle(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("MaxDistance", MaxDistance.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/ChromaticSplitAction.cs`:

```csharp
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class ChromaticSplitAction : BaseAction
    {
        #region Properties

        public float MaxDistance { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public ChromaticSplitAction(BaseObject owner) :
            base(owner, EActionType.ChromaticSplit)
        {
            MaxDistance = 0.05f;
            Duration = 0.3f;
        }

        public ChromaticSplitAction(BaseObject owner, ChromaticSplitActionModel actionModel) :
            base(owner, actionModel)
        {
            MaxDistance = actionModel.MaxDistance;
            Duration = actionModel.Duration;
        }

        public ChromaticSplitAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as ChromaticSplitActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerChromaticSplit(Owner.Position, MaxDistance, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.ChromaticAberration: { return new ChromaticAberrationActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.ChromaticAberration: { return new ChromaticAberrationActionModel(); }
                case EActionType.ChromaticSplit: { return new ChromaticSplitActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.ChromaticAberration: { return new ChromaticAberrationActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.ChromaticAberration: { return new ChromaticAberrationActionModel(actionModel); }
                case EActionType.ChromaticSplit: { return new ChromaticSplitActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.ChromaticAberration: { return new ChromaticAberrationAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.ChromaticAberration: { return new ChromaticAberrationAction(owner, actionModel); }
                case EActionType.ChromaticSplit: { return new ChromaticSplitAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.ChromaticAberration: { return new ChromaticAberrationAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.ChromaticAberration: { return new ChromaticAberrationAction(owner); }
                case EActionType.ChromaticSplit: { return new ChromaticSplitAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/ChromaticSplitTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ChromaticSplitTests
    {
        const float time = 1f;
        const float maxDistance = 0.12f;
        const float duration = 0.6f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new ChromaticSplitActionModel()
            {
                Time = time,
                MaxDistance = maxDistance,
                Duration = duration
            };

            var action = new ChromaticSplitAction(null, model);

            action.Time.ShouldBe(time);
            action.MaxDistance.ShouldBe(maxDistance);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new ChromaticSplitAction(null)
            {
                Time = time,
                MaxDistance = maxDistance,
                Duration = duration
            };

            var model = new ChromaticSplitActionModel(action);

            model.ActionType.ShouldBe(EActionType.ChromaticSplit);
            model.Time.ShouldBe(time);
            model.MaxDistance.ShouldBe(maxDistance);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new ChromaticSplitActionModel()
            {
                Time = time,
                MaxDistance = maxDistance,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("ChromaticSplitTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("ChromaticSplitTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as ChromaticSplitActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.ChromaticSplit);
            model2.Time.ShouldBe(time);
            model2.MaxDistance.ShouldBe(maxDistance);
            model2.Duration.ShouldBe(duration);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter ChromaticSplitTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/ChromaticSplitActionModel.cs GameDonkey/StateMachine/Actions/ChromaticSplitAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/ChromaticSplitTests.cs
git commit -m "Add ChromaticSplit screen effect action"
```

---

### Task 7: HeatHaze

**Files:**
- Create: `GameDonkey/Models/Actions/HeatHazeActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/HeatHazeAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/HeatHazeTests.cs`

**Interfaces:**
- Consumes: `EActionType.HeatHaze` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerHeatHaze(Vector2 position, float strength = 0.02f, float radius = 0.15f, float height = 0.40f, float duration = 3.0f)`.
- Produces: `HeatHazeActionModel` (`Strength`, `Radius`, `Height`, `Duration`), `HeatHazeAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/HeatHazeActionModel.cs`:

```csharp
using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class HeatHazeActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.HeatHaze;
            }
        }

        public float Strength { get; set; }
        public float Radius { get; set; }
        public float Height { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public HeatHazeActionModel()
        {
            Strength = 0.02f;
            Radius = 0.15f;
            Height = 0.40f;
            Duration = 3.0f;
        }

        public HeatHazeActionModel(HeatHazeAction action) : base(action)
        {
            Strength = action.Strength;
            Radius = action.Radius;
            Height = action.Height;
            Duration = action.Duration;
        }

        public HeatHazeActionModel(BaseAction action) : this(action as HeatHazeAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "strength":
                    {
                        Strength = Convert.ToSingle(value);
                    }
                    break;
                case "radius":
                    {
                        Radius = Convert.ToSingle(value);
                    }
                    break;
                case "height":
                    {
                        Height = Convert.ToSingle(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("Strength", Strength.ToString());
            xmlWriter.WriteAttributeString("Radius", Radius.ToString());
            xmlWriter.WriteAttributeString("Height", Height.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/HeatHazeAction.cs`:

```csharp
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class HeatHazeAction : BaseAction
    {
        #region Properties

        public float Strength { get; set; }
        public float Radius { get; set; }
        public float Height { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public HeatHazeAction(BaseObject owner) :
            base(owner, EActionType.HeatHaze)
        {
            Strength = 0.02f;
            Radius = 0.15f;
            Height = 0.40f;
            Duration = 3.0f;
        }

        public HeatHazeAction(BaseObject owner, HeatHazeActionModel actionModel) :
            base(owner, actionModel)
        {
            Strength = actionModel.Strength;
            Radius = actionModel.Radius;
            Height = actionModel.Height;
            Duration = actionModel.Duration;
        }

        public HeatHazeAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as HeatHazeActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerHeatHaze(Owner.Position, Strength, Radius, Height, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.ChromaticSplit: { return new ChromaticSplitActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.ChromaticSplit: { return new ChromaticSplitActionModel(); }
                case EActionType.HeatHaze: { return new HeatHazeActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.ChromaticSplit: { return new ChromaticSplitActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.ChromaticSplit: { return new ChromaticSplitActionModel(actionModel); }
                case EActionType.HeatHaze: { return new HeatHazeActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.ChromaticSplit: { return new ChromaticSplitAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.ChromaticSplit: { return new ChromaticSplitAction(owner, actionModel); }
                case EActionType.HeatHaze: { return new HeatHazeAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.ChromaticSplit: { return new ChromaticSplitAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.ChromaticSplit: { return new ChromaticSplitAction(owner); }
                case EActionType.HeatHaze: { return new HeatHazeAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/HeatHazeTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class HeatHazeTests
    {
        const float time = 1f;
        const float strength = 0.05f;
        const float radius = 0.3f;
        const float height = 0.6f;
        const float duration = 4.5f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new HeatHazeActionModel()
            {
                Time = time,
                Strength = strength,
                Radius = radius,
                Height = height,
                Duration = duration
            };

            var action = new HeatHazeAction(null, model);

            action.Time.ShouldBe(time);
            action.Strength.ShouldBe(strength);
            action.Radius.ShouldBe(radius);
            action.Height.ShouldBe(height);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new HeatHazeAction(null)
            {
                Time = time,
                Strength = strength,
                Radius = radius,
                Height = height,
                Duration = duration
            };

            var model = new HeatHazeActionModel(action);

            model.ActionType.ShouldBe(EActionType.HeatHaze);
            model.Time.ShouldBe(time);
            model.Strength.ShouldBe(strength);
            model.Radius.ShouldBe(radius);
            model.Height.ShouldBe(height);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new HeatHazeActionModel()
            {
                Time = time,
                Strength = strength,
                Radius = radius,
                Height = height,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("HeatHazeTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("HeatHazeTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as HeatHazeActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.HeatHaze);
            model2.Time.ShouldBe(time);
            model2.Strength.ShouldBe(strength);
            model2.Radius.ShouldBe(radius);
            model2.Height.ShouldBe(height);
            model2.Duration.ShouldBe(duration);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter HeatHazeTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/HeatHazeActionModel.cs GameDonkey/StateMachine/Actions/HeatHazeAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/HeatHazeTests.cs
git commit -m "Add HeatHaze screen effect action"
```

---

### Task 8: HitFlash

**Files:**
- Create: `GameDonkey/Models/Actions/HitFlashActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/HitFlashAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/HitFlashTests.cs`

**Interfaces:**
- Consumes: `EActionType.HitFlash` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerHitFlash(Color blendColor, FadeMode mode = FadeMode.FadeOut, FadeCurve curve = FadeCurve.Linear, EffectBlendMode blendMode = EffectBlendMode.LinearDodge, float time = 1f)`, `Vector2Extensions.ToColor()`/`.StringFromColor()`. No `Vector2 position` parameter.
- Produces: `HitFlashActionModel` (`BlendColor`, `Mode`, `Curve`, `BlendMode`, `Duration`), `HitFlashAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/HitFlashActionModel.cs`:

```csharp
using Microsoft.Xna.Framework;
using ScreenFXBuddy;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
    public class HitFlashActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.HitFlash;
            }
        }

        public Color BlendColor { get; set; }
        public FadeMode Mode { get; set; }
        public FadeCurve Curve { get; set; }
        public EffectBlendMode BlendMode { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public HitFlashActionModel()
        {
            BlendColor = Color.White;
            Mode = FadeMode.FadeOut;
            Curve = FadeCurve.Linear;
            BlendMode = EffectBlendMode.LinearDodge;
            Duration = 1f;
        }

        public HitFlashActionModel(HitFlashAction action) : base(action)
        {
            BlendColor = action.BlendColor;
            Mode = action.Mode;
            Curve = action.Curve;
            BlendMode = action.BlendMode;
            Duration = action.Duration;
        }

        public HitFlashActionModel(BaseAction action) : this(action as HitFlashAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "blendcolor":
                    {
                        BlendColor = value.ToColor();
                    }
                    break;
                case "mode":
                    {
                        Mode = (FadeMode)Enum.Parse(typeof(FadeMode), value);
                    }
                    break;
                case "curve":
                    {
                        Curve = (FadeCurve)Enum.Parse(typeof(FadeCurve), value);
                    }
                    break;
                case "blendmode":
                    {
                        BlendMode = (EffectBlendMode)Enum.Parse(typeof(EffectBlendMode), value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("BlendColor", BlendColor.StringFromColor());
            xmlWriter.WriteAttributeString("Mode", Mode.ToString());
            xmlWriter.WriteAttributeString("Curve", Curve.ToString());
            xmlWriter.WriteAttributeString("BlendMode", BlendMode.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/HitFlashAction.cs`:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class HitFlashAction : BaseAction
    {
        #region Properties

        public Color BlendColor { get; set; }
        public FadeMode Mode { get; set; }
        public FadeCurve Curve { get; set; }
        public EffectBlendMode BlendMode { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public HitFlashAction(BaseObject owner) :
            base(owner, EActionType.HitFlash)
        {
            BlendColor = Color.White;
            Mode = FadeMode.FadeOut;
            Curve = FadeCurve.Linear;
            BlendMode = EffectBlendMode.LinearDodge;
            Duration = 1f;
        }

        public HitFlashAction(BaseObject owner, HitFlashActionModel actionModel) :
            base(owner, actionModel)
        {
            BlendColor = actionModel.BlendColor;
            Mode = actionModel.Mode;
            Curve = actionModel.Curve;
            BlendMode = actionModel.BlendMode;
            Duration = actionModel.Duration;
        }

        public HitFlashAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as HitFlashActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerHitFlash(BlendColor, Mode, Curve, BlendMode, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.HeatHaze: { return new HeatHazeActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.HeatHaze: { return new HeatHazeActionModel(); }
                case EActionType.HitFlash: { return new HitFlashActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.HeatHaze: { return new HeatHazeActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.HeatHaze: { return new HeatHazeActionModel(actionModel); }
                case EActionType.HitFlash: { return new HitFlashActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.HeatHaze: { return new HeatHazeAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.HeatHaze: { return new HeatHazeAction(owner, actionModel); }
                case EActionType.HitFlash: { return new HitFlashAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.HeatHaze: { return new HeatHazeAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.HeatHaze: { return new HeatHazeAction(owner); }
                case EActionType.HitFlash: { return new HitFlashAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/HitFlashTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using ScreenFXBuddy;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class HitFlashTests
    {
        const float time = 1f;
        static readonly Color blendColor = Color.Red;
        const FadeMode mode = FadeMode.FadeIn;
        const FadeCurve curve = FadeCurve.Logarithmic;
        const EffectBlendMode blendMode = EffectBlendMode.Screen;
        const float duration = 2f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new HitFlashActionModel()
            {
                Time = time,
                BlendColor = blendColor,
                Mode = mode,
                Curve = curve,
                BlendMode = blendMode,
                Duration = duration
            };

            var action = new HitFlashAction(null, model);

            action.Time.ShouldBe(time);
            action.BlendColor.ShouldBe(blendColor);
            action.Mode.ShouldBe(mode);
            action.Curve.ShouldBe(curve);
            action.BlendMode.ShouldBe(blendMode);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new HitFlashAction(null)
            {
                Time = time,
                BlendColor = blendColor,
                Mode = mode,
                Curve = curve,
                BlendMode = blendMode,
                Duration = duration
            };

            var model = new HitFlashActionModel(action);

            model.ActionType.ShouldBe(EActionType.HitFlash);
            model.Time.ShouldBe(time);
            model.BlendColor.ShouldBe(blendColor);
            model.Mode.ShouldBe(mode);
            model.Curve.ShouldBe(curve);
            model.BlendMode.ShouldBe(blendMode);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new HitFlashActionModel()
            {
                Time = time,
                BlendColor = blendColor,
                Mode = mode,
                Curve = curve,
                BlendMode = blendMode,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("HitFlashTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("HitFlashTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as HitFlashActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.HitFlash);
            model2.Time.ShouldBe(time);
            model2.BlendColor.ShouldBe(blendColor);
            model2.Mode.ShouldBe(mode);
            model2.Curve.ShouldBe(curve);
            model2.BlendMode.ShouldBe(blendMode);
            model2.Duration.ShouldBe(duration);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter HitFlashTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/HitFlashActionModel.cs GameDonkey/StateMachine/Actions/HitFlashAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/HitFlashTests.cs
git commit -m "Add HitFlash screen effect action"
```

---

### Task 9: AnimeSuper

**Files:**
- Create: `GameDonkey/Models/Actions/AnimeSuperActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/AnimeSuperAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/AnimeSuperTests.cs`

**Interfaces:**
- Consumes: `EActionType.AnimeSuper` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerAnimeSuper(Color color, float flashIn = 0.05f, float hold = 0.30f, float fadeOut = 0.40f)`. No `Vector2 position` parameter. Property is named `SuperColor` (not `Color`) because `Color` is also the MonoGame type name used in the same file.
- Produces: `AnimeSuperActionModel` (`SuperColor`, `FlashIn`, `Hold`, `FadeOut`), `AnimeSuperAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/AnimeSuperActionModel.cs`:

```csharp
using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
    public class AnimeSuperActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.AnimeSuper;
            }
        }

        public Color SuperColor { get; set; }
        public float FlashIn { get; set; }
        public float Hold { get; set; }
        public float FadeOut { get; set; }

        #endregion //Properties

        #region Initialization

        public AnimeSuperActionModel()
        {
            SuperColor = Color.White;
            FlashIn = 0.05f;
            Hold = 0.30f;
            FadeOut = 0.40f;
        }

        public AnimeSuperActionModel(AnimeSuperAction action) : base(action)
        {
            SuperColor = action.SuperColor;
            FlashIn = action.FlashIn;
            Hold = action.Hold;
            FadeOut = action.FadeOut;
        }

        public AnimeSuperActionModel(BaseAction action) : this(action as AnimeSuperAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "supercolor":
                    {
                        SuperColor = value.ToColor();
                    }
                    break;
                case "flashin":
                    {
                        FlashIn = Convert.ToSingle(value);
                    }
                    break;
                case "hold":
                    {
                        Hold = Convert.ToSingle(value);
                    }
                    break;
                case "fadeout":
                    {
                        FadeOut = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("SuperColor", SuperColor.StringFromColor());
            xmlWriter.WriteAttributeString("FlashIn", FlashIn.ToString());
            xmlWriter.WriteAttributeString("Hold", Hold.ToString());
            xmlWriter.WriteAttributeString("FadeOut", FadeOut.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/AnimeSuperAction.cs`:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class AnimeSuperAction : BaseAction
    {
        #region Properties

        public Color SuperColor { get; set; }
        public float FlashIn { get; set; }
        public float Hold { get; set; }
        public float FadeOut { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public AnimeSuperAction(BaseObject owner) :
            base(owner, EActionType.AnimeSuper)
        {
            SuperColor = Color.White;
            FlashIn = 0.05f;
            Hold = 0.30f;
            FadeOut = 0.40f;
        }

        public AnimeSuperAction(BaseObject owner, AnimeSuperActionModel actionModel) :
            base(owner, actionModel)
        {
            SuperColor = actionModel.SuperColor;
            FlashIn = actionModel.FlashIn;
            Hold = actionModel.Hold;
            FadeOut = actionModel.FadeOut;
        }

        public AnimeSuperAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as AnimeSuperActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerAnimeSuper(SuperColor, FlashIn, Hold, FadeOut);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.HitFlash: { return new HitFlashActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.HitFlash: { return new HitFlashActionModel(); }
                case EActionType.AnimeSuper: { return new AnimeSuperActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.HitFlash: { return new HitFlashActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.HitFlash: { return new HitFlashActionModel(actionModel); }
                case EActionType.AnimeSuper: { return new AnimeSuperActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.HitFlash: { return new HitFlashAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.HitFlash: { return new HitFlashAction(owner, actionModel); }
                case EActionType.AnimeSuper: { return new AnimeSuperAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.HitFlash: { return new HitFlashAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.HitFlash: { return new HitFlashAction(owner); }
                case EActionType.AnimeSuper: { return new AnimeSuperAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/AnimeSuperTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class AnimeSuperTests
    {
        const float time = 1f;
        static readonly Color superColor = Color.Blue;
        const float flashIn = 0.1f;
        const float hold = 0.5f;
        const float fadeOut = 0.6f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new AnimeSuperActionModel()
            {
                Time = time,
                SuperColor = superColor,
                FlashIn = flashIn,
                Hold = hold,
                FadeOut = fadeOut
            };

            var action = new AnimeSuperAction(null, model);

            action.Time.ShouldBe(time);
            action.SuperColor.ShouldBe(superColor);
            action.FlashIn.ShouldBe(flashIn);
            action.Hold.ShouldBe(hold);
            action.FadeOut.ShouldBe(fadeOut);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new AnimeSuperAction(null)
            {
                Time = time,
                SuperColor = superColor,
                FlashIn = flashIn,
                Hold = hold,
                FadeOut = fadeOut
            };

            var model = new AnimeSuperActionModel(action);

            model.ActionType.ShouldBe(EActionType.AnimeSuper);
            model.Time.ShouldBe(time);
            model.SuperColor.ShouldBe(superColor);
            model.FlashIn.ShouldBe(flashIn);
            model.Hold.ShouldBe(hold);
            model.FadeOut.ShouldBe(fadeOut);
        }

        [Test]
        public void Persist()
        {
            var model = new AnimeSuperActionModel()
            {
                Time = time,
                SuperColor = superColor,
                FlashIn = flashIn,
                Hold = hold,
                FadeOut = fadeOut
            };

            var container = new StateContainerModel(new Filename("AnimeSuperTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("AnimeSuperTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as AnimeSuperActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.AnimeSuper);
            model2.Time.ShouldBe(time);
            model2.SuperColor.ShouldBe(superColor);
            model2.FlashIn.ShouldBe(flashIn);
            model2.Hold.ShouldBe(hold);
            model2.FadeOut.ShouldBe(fadeOut);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter AnimeSuperTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/AnimeSuperActionModel.cs GameDonkey/StateMachine/Actions/AnimeSuperAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/AnimeSuperTests.cs
git commit -m "Add AnimeSuper screen effect action"
```

---

### Task 10: Letterbox

**Files:**
- Create: `GameDonkey/Models/Actions/LetterboxActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/LetterboxAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/LetterboxTests.cs`

**Interfaces:**
- Consumes: `EActionType.Letterbox` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerLetterbox(float barHeight = 0.10f, float slideIn = 0.15f, float hold = 1.00f, float slideOut = 0.15f)`. No `Vector2 position` parameter.
- Produces: `LetterboxActionModel` (`BarHeight`, `SlideIn`, `Hold`, `SlideOut`), `LetterboxAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/LetterboxActionModel.cs`:

```csharp
using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class LetterboxActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.Letterbox;
            }
        }

        public float BarHeight { get; set; }
        public float SlideIn { get; set; }
        public float Hold { get; set; }
        public float SlideOut { get; set; }

        #endregion //Properties

        #region Initialization

        public LetterboxActionModel()
        {
            BarHeight = 0.10f;
            SlideIn = 0.15f;
            Hold = 1.00f;
            SlideOut = 0.15f;
        }

        public LetterboxActionModel(LetterboxAction action) : base(action)
        {
            BarHeight = action.BarHeight;
            SlideIn = action.SlideIn;
            Hold = action.Hold;
            SlideOut = action.SlideOut;
        }

        public LetterboxActionModel(BaseAction action) : this(action as LetterboxAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "barheight":
                    {
                        BarHeight = Convert.ToSingle(value);
                    }
                    break;
                case "slidein":
                    {
                        SlideIn = Convert.ToSingle(value);
                    }
                    break;
                case "hold":
                    {
                        Hold = Convert.ToSingle(value);
                    }
                    break;
                case "slideout":
                    {
                        SlideOut = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("BarHeight", BarHeight.ToString());
            xmlWriter.WriteAttributeString("SlideIn", SlideIn.ToString());
            xmlWriter.WriteAttributeString("Hold", Hold.ToString());
            xmlWriter.WriteAttributeString("SlideOut", SlideOut.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/LetterboxAction.cs`:

```csharp
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class LetterboxAction : BaseAction
    {
        #region Properties

        public float BarHeight { get; set; }
        public float SlideIn { get; set; }
        public float Hold { get; set; }
        public float SlideOut { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public LetterboxAction(BaseObject owner) :
            base(owner, EActionType.Letterbox)
        {
            BarHeight = 0.10f;
            SlideIn = 0.15f;
            Hold = 1.00f;
            SlideOut = 0.15f;
        }

        public LetterboxAction(BaseObject owner, LetterboxActionModel actionModel) :
            base(owner, actionModel)
        {
            BarHeight = actionModel.BarHeight;
            SlideIn = actionModel.SlideIn;
            Hold = actionModel.Hold;
            SlideOut = actionModel.SlideOut;
        }

        public LetterboxAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as LetterboxActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerLetterbox(BarHeight, SlideIn, Hold, SlideOut);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.AnimeSuper: { return new AnimeSuperActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.AnimeSuper: { return new AnimeSuperActionModel(); }
                case EActionType.Letterbox: { return new LetterboxActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.AnimeSuper: { return new AnimeSuperActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.AnimeSuper: { return new AnimeSuperActionModel(actionModel); }
                case EActionType.Letterbox: { return new LetterboxActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.AnimeSuper: { return new AnimeSuperAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.AnimeSuper: { return new AnimeSuperAction(owner, actionModel); }
                case EActionType.Letterbox: { return new LetterboxAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.AnimeSuper: { return new AnimeSuperAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.AnimeSuper: { return new AnimeSuperAction(owner); }
                case EActionType.Letterbox: { return new LetterboxAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/LetterboxTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class LetterboxTests
    {
        const float time = 1f;
        const float barHeight = 0.2f;
        const float slideIn = 0.3f;
        const float hold = 1.5f;
        const float slideOut = 0.25f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new LetterboxActionModel()
            {
                Time = time,
                BarHeight = barHeight,
                SlideIn = slideIn,
                Hold = hold,
                SlideOut = slideOut
            };

            var action = new LetterboxAction(null, model);

            action.Time.ShouldBe(time);
            action.BarHeight.ShouldBe(barHeight);
            action.SlideIn.ShouldBe(slideIn);
            action.Hold.ShouldBe(hold);
            action.SlideOut.ShouldBe(slideOut);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new LetterboxAction(null)
            {
                Time = time,
                BarHeight = barHeight,
                SlideIn = slideIn,
                Hold = hold,
                SlideOut = slideOut
            };

            var model = new LetterboxActionModel(action);

            model.ActionType.ShouldBe(EActionType.Letterbox);
            model.Time.ShouldBe(time);
            model.BarHeight.ShouldBe(barHeight);
            model.SlideIn.ShouldBe(slideIn);
            model.Hold.ShouldBe(hold);
            model.SlideOut.ShouldBe(slideOut);
        }

        [Test]
        public void Persist()
        {
            var model = new LetterboxActionModel()
            {
                Time = time,
                BarHeight = barHeight,
                SlideIn = slideIn,
                Hold = hold,
                SlideOut = slideOut
            };

            var container = new StateContainerModel(new Filename("LetterboxTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("LetterboxTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as LetterboxActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.Letterbox);
            model2.Time.ShouldBe(time);
            model2.BarHeight.ShouldBe(barHeight);
            model2.SlideIn.ShouldBe(slideIn);
            model2.Hold.ShouldBe(hold);
            model2.SlideOut.ShouldBe(slideOut);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter LetterboxTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/LetterboxActionModel.cs GameDonkey/StateMachine/Actions/LetterboxAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/LetterboxTests.cs
git commit -m "Add Letterbox screen effect action"
```

---

### Task 11: FreezeFrame

**Files:**
- Create: `GameDonkey/Models/Actions/FreezeFrameActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/FreezeFrameAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/FreezeFrameTests.cs`

**Interfaces:**
- Consumes: `EActionType.FreezeFrame` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerFreezeFrame(Color tintColor, float flashIn = 0.10f, float hold = 0.40f, float fadeOut = 0.30f)`, `Vector2Extensions.ToColor()`/`.StringFromColor()`. No `Vector2 position` parameter.
- Produces: `FreezeFrameActionModel` (`TintColor`, `FlashIn`, `Hold`, `FadeOut`), `FreezeFrameAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/FreezeFrameActionModel.cs`:

```csharp
using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
    public class FreezeFrameActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.FreezeFrame;
            }
        }

        public Color TintColor { get; set; }
        public float FlashIn { get; set; }
        public float Hold { get; set; }
        public float FadeOut { get; set; }

        #endregion //Properties

        #region Initialization

        public FreezeFrameActionModel()
        {
            TintColor = Color.White;
            FlashIn = 0.10f;
            Hold = 0.40f;
            FadeOut = 0.30f;
        }

        public FreezeFrameActionModel(FreezeFrameAction action) : base(action)
        {
            TintColor = action.TintColor;
            FlashIn = action.FlashIn;
            Hold = action.Hold;
            FadeOut = action.FadeOut;
        }

        public FreezeFrameActionModel(BaseAction action) : this(action as FreezeFrameAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "tintcolor":
                    {
                        TintColor = value.ToColor();
                    }
                    break;
                case "flashin":
                    {
                        FlashIn = Convert.ToSingle(value);
                    }
                    break;
                case "hold":
                    {
                        Hold = Convert.ToSingle(value);
                    }
                    break;
                case "fadeout":
                    {
                        FadeOut = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("TintColor", TintColor.StringFromColor());
            xmlWriter.WriteAttributeString("FlashIn", FlashIn.ToString());
            xmlWriter.WriteAttributeString("Hold", Hold.ToString());
            xmlWriter.WriteAttributeString("FadeOut", FadeOut.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/FreezeFrameAction.cs`:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class FreezeFrameAction : BaseAction
    {
        #region Properties

        public Color TintColor { get; set; }
        public float FlashIn { get; set; }
        public float Hold { get; set; }
        public float FadeOut { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public FreezeFrameAction(BaseObject owner) :
            base(owner, EActionType.FreezeFrame)
        {
            TintColor = Color.White;
            FlashIn = 0.10f;
            Hold = 0.40f;
            FadeOut = 0.30f;
        }

        public FreezeFrameAction(BaseObject owner, FreezeFrameActionModel actionModel) :
            base(owner, actionModel)
        {
            TintColor = actionModel.TintColor;
            FlashIn = actionModel.FlashIn;
            Hold = actionModel.Hold;
            FadeOut = actionModel.FadeOut;
        }

        public FreezeFrameAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as FreezeFrameActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerFreezeFrame(TintColor, FlashIn, Hold, FadeOut);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.Letterbox: { return new LetterboxActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.Letterbox: { return new LetterboxActionModel(); }
                case EActionType.FreezeFrame: { return new FreezeFrameActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.Letterbox: { return new LetterboxActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.Letterbox: { return new LetterboxActionModel(actionModel); }
                case EActionType.FreezeFrame: { return new FreezeFrameActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.Letterbox: { return new LetterboxAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.Letterbox: { return new LetterboxAction(owner, actionModel); }
                case EActionType.FreezeFrame: { return new FreezeFrameAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.Letterbox: { return new LetterboxAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.Letterbox: { return new LetterboxAction(owner); }
                case EActionType.FreezeFrame: { return new FreezeFrameAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/FreezeFrameTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class FreezeFrameTests
    {
        const float time = 1f;
        static readonly Color tintColor = Color.Gray;
        const float flashIn = 0.2f;
        const float hold = 0.6f;
        const float fadeOut = 0.5f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new FreezeFrameActionModel()
            {
                Time = time,
                TintColor = tintColor,
                FlashIn = flashIn,
                Hold = hold,
                FadeOut = fadeOut
            };

            var action = new FreezeFrameAction(null, model);

            action.Time.ShouldBe(time);
            action.TintColor.ShouldBe(tintColor);
            action.FlashIn.ShouldBe(flashIn);
            action.Hold.ShouldBe(hold);
            action.FadeOut.ShouldBe(fadeOut);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new FreezeFrameAction(null)
            {
                Time = time,
                TintColor = tintColor,
                FlashIn = flashIn,
                Hold = hold,
                FadeOut = fadeOut
            };

            var model = new FreezeFrameActionModel(action);

            model.ActionType.ShouldBe(EActionType.FreezeFrame);
            model.Time.ShouldBe(time);
            model.TintColor.ShouldBe(tintColor);
            model.FlashIn.ShouldBe(flashIn);
            model.Hold.ShouldBe(hold);
            model.FadeOut.ShouldBe(fadeOut);
        }

        [Test]
        public void Persist()
        {
            var model = new FreezeFrameActionModel()
            {
                Time = time,
                TintColor = tintColor,
                FlashIn = flashIn,
                Hold = hold,
                FadeOut = fadeOut
            };

            var container = new StateContainerModel(new Filename("FreezeFrameTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("FreezeFrameTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as FreezeFrameActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.FreezeFrame);
            model2.Time.ShouldBe(time);
            model2.TintColor.ShouldBe(tintColor);
            model2.FlashIn.ShouldBe(flashIn);
            model2.Hold.ShouldBe(hold);
            model2.FadeOut.ShouldBe(fadeOut);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter FreezeFrameTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/FreezeFrameActionModel.cs GameDonkey/StateMachine/Actions/FreezeFrameAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/FreezeFrameTests.cs
git commit -m "Add FreezeFrame screen effect action"
```

---

### Task 12: ZoomBlur

**Files:**
- Create: `GameDonkey/Models/Actions/ZoomBlurActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/ZoomBlurAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/ZoomBlurTests.cs`

**Interfaces:**
- Consumes: `EActionType.ZoomBlur` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerZoomBlur(Vector2 position, float strength = 0.05f, float radius = 1.0f, float duration = 0.4f)`.
- Produces: `ZoomBlurActionModel` (`Strength`, `Radius`, `Duration`), `ZoomBlurAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/ZoomBlurActionModel.cs`:

```csharp
using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class ZoomBlurActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.ZoomBlur;
            }
        }

        public float Strength { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public ZoomBlurActionModel()
        {
            Strength = 0.05f;
            Radius = 1.0f;
            Duration = 0.4f;
        }

        public ZoomBlurActionModel(ZoomBlurAction action) : base(action)
        {
            Strength = action.Strength;
            Radius = action.Radius;
            Duration = action.Duration;
        }

        public ZoomBlurActionModel(BaseAction action) : this(action as ZoomBlurAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "strength":
                    {
                        Strength = Convert.ToSingle(value);
                    }
                    break;
                case "radius":
                    {
                        Radius = Convert.ToSingle(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("Strength", Strength.ToString());
            xmlWriter.WriteAttributeString("Radius", Radius.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/ZoomBlurAction.cs`:

```csharp
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class ZoomBlurAction : BaseAction
    {
        #region Properties

        public float Strength { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public ZoomBlurAction(BaseObject owner) :
            base(owner, EActionType.ZoomBlur)
        {
            Strength = 0.05f;
            Radius = 1.0f;
            Duration = 0.4f;
        }

        public ZoomBlurAction(BaseObject owner, ZoomBlurActionModel actionModel) :
            base(owner, actionModel)
        {
            Strength = actionModel.Strength;
            Radius = actionModel.Radius;
            Duration = actionModel.Duration;
        }

        public ZoomBlurAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as ZoomBlurActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerZoomBlur(Owner.Position, Strength, Radius, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.FreezeFrame: { return new FreezeFrameActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.FreezeFrame: { return new FreezeFrameActionModel(); }
                case EActionType.ZoomBlur: { return new ZoomBlurActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.FreezeFrame: { return new FreezeFrameActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.FreezeFrame: { return new FreezeFrameActionModel(actionModel); }
                case EActionType.ZoomBlur: { return new ZoomBlurActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.FreezeFrame: { return new FreezeFrameAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.FreezeFrame: { return new FreezeFrameAction(owner, actionModel); }
                case EActionType.ZoomBlur: { return new ZoomBlurAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.FreezeFrame: { return new FreezeFrameAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.FreezeFrame: { return new FreezeFrameAction(owner); }
                case EActionType.ZoomBlur: { return new ZoomBlurAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/ZoomBlurTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ZoomBlurTests
    {
        const float time = 1f;
        const float strength = 0.15f;
        const float radius = 1.5f;
        const float duration = 0.8f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new ZoomBlurActionModel()
            {
                Time = time,
                Strength = strength,
                Radius = radius,
                Duration = duration
            };

            var action = new ZoomBlurAction(null, model);

            action.Time.ShouldBe(time);
            action.Strength.ShouldBe(strength);
            action.Radius.ShouldBe(radius);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new ZoomBlurAction(null)
            {
                Time = time,
                Strength = strength,
                Radius = radius,
                Duration = duration
            };

            var model = new ZoomBlurActionModel(action);

            model.ActionType.ShouldBe(EActionType.ZoomBlur);
            model.Time.ShouldBe(time);
            model.Strength.ShouldBe(strength);
            model.Radius.ShouldBe(radius);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new ZoomBlurActionModel()
            {
                Time = time,
                Strength = strength,
                Radius = radius,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("ZoomBlurTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("ZoomBlurTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as ZoomBlurActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.ZoomBlur);
            model2.Time.ShouldBe(time);
            model2.Strength.ShouldBe(strength);
            model2.Radius.ShouldBe(radius);
            model2.Duration.ShouldBe(duration);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter ZoomBlurTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/ZoomBlurActionModel.cs GameDonkey/StateMachine/Actions/ZoomBlurAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/ZoomBlurTests.cs
git commit -m "Add ZoomBlur screen effect action"
```

---

### Task 13: ScreenTilt

**Files:**
- Create: `GameDonkey/Models/Actions/ScreenTiltActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/ScreenTiltAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/ScreenTiltTests.cs`

**Interfaces:**
- Consumes: `EActionType.ScreenTilt` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerScreenTilt(float angle = 3.0f, float duration = 0.4f, float delta = 0.2f)`. No `Vector2 position` parameter.
- Produces: `ScreenTiltActionModel` (`Angle`, `Duration`, `Delta`), `ScreenTiltAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/ScreenTiltActionModel.cs`:

```csharp
using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class ScreenTiltActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.ScreenTilt;
            }
        }

        public float Angle { get; set; }
        public float Duration { get; set; }
        public float Delta { get; set; }

        #endregion //Properties

        #region Initialization

        public ScreenTiltActionModel()
        {
            Angle = 3.0f;
            Duration = 0.4f;
            Delta = 0.2f;
        }

        public ScreenTiltActionModel(ScreenTiltAction action) : base(action)
        {
            Angle = action.Angle;
            Duration = action.Duration;
            Delta = action.Delta;
        }

        public ScreenTiltActionModel(BaseAction action) : this(action as ScreenTiltAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "angle":
                    {
                        Angle = Convert.ToSingle(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                case "delta":
                    {
                        Delta = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("Angle", Angle.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
            xmlWriter.WriteAttributeString("Delta", Delta.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/ScreenTiltAction.cs`:

```csharp
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class ScreenTiltAction : BaseAction
    {
        #region Properties

        public float Angle { get; set; }
        public float Duration { get; set; }
        public float Delta { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public ScreenTiltAction(BaseObject owner) :
            base(owner, EActionType.ScreenTilt)
        {
            Angle = 3.0f;
            Duration = 0.4f;
            Delta = 0.2f;
        }

        public ScreenTiltAction(BaseObject owner, ScreenTiltActionModel actionModel) :
            base(owner, actionModel)
        {
            Angle = actionModel.Angle;
            Duration = actionModel.Duration;
            Delta = actionModel.Delta;
        }

        public ScreenTiltAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as ScreenTiltActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerScreenTilt(Angle, Duration, Delta);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.ZoomBlur: { return new ZoomBlurActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.ZoomBlur: { return new ZoomBlurActionModel(); }
                case EActionType.ScreenTilt: { return new ScreenTiltActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.ZoomBlur: { return new ZoomBlurActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.ZoomBlur: { return new ZoomBlurActionModel(actionModel); }
                case EActionType.ScreenTilt: { return new ScreenTiltActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.ZoomBlur: { return new ZoomBlurAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.ZoomBlur: { return new ZoomBlurAction(owner, actionModel); }
                case EActionType.ScreenTilt: { return new ScreenTiltAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.ZoomBlur: { return new ZoomBlurAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.ZoomBlur: { return new ZoomBlurAction(owner); }
                case EActionType.ScreenTilt: { return new ScreenTiltAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/ScreenTiltTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ScreenTiltTests
    {
        const float time = 1f;
        const float angle = 6f;
        const float duration = 0.7f;
        const float delta = 0.35f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new ScreenTiltActionModel()
            {
                Time = time,
                Angle = angle,
                Duration = duration,
                Delta = delta
            };

            var action = new ScreenTiltAction(null, model);

            action.Time.ShouldBe(time);
            action.Angle.ShouldBe(angle);
            action.Duration.ShouldBe(duration);
            action.Delta.ShouldBe(delta);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new ScreenTiltAction(null)
            {
                Time = time,
                Angle = angle,
                Duration = duration,
                Delta = delta
            };

            var model = new ScreenTiltActionModel(action);

            model.ActionType.ShouldBe(EActionType.ScreenTilt);
            model.Time.ShouldBe(time);
            model.Angle.ShouldBe(angle);
            model.Duration.ShouldBe(duration);
            model.Delta.ShouldBe(delta);
        }

        [Test]
        public void Persist()
        {
            var model = new ScreenTiltActionModel()
            {
                Time = time,
                Angle = angle,
                Duration = duration,
                Delta = delta
            };

            var container = new StateContainerModel(new Filename("ScreenTiltTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("ScreenTiltTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as ScreenTiltActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.ScreenTilt);
            model2.Time.ShouldBe(time);
            model2.Angle.ShouldBe(angle);
            model2.Duration.ShouldBe(duration);
            model2.Delta.ShouldBe(delta);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter ScreenTiltTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/ScreenTiltActionModel.cs GameDonkey/StateMachine/Actions/ScreenTiltAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/ScreenTiltTests.cs
git commit -m "Add ScreenTilt screen effect action"
```

---

### Task 14: Electric

**Files:**
- Create: `GameDonkey/Models/Actions/ElectricActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/ElectricAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/ElectricTests.cs`

**Interfaces:**
- Consumes: `EActionType.Electric` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerElectric(Vector2 position, Color color, float radius = 0.20f, float duration = 0.60f)`, `Vector2Extensions.ToColor()`/`.StringFromColor()`. Property named `ElectricColor` (not `Color`) to avoid colliding with the MonoGame `Color` type name in the same file.
- Produces: `ElectricActionModel` (`ElectricColor`, `Radius`, `Duration`), `ElectricAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/ElectricActionModel.cs`:

```csharp
using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
    public class ElectricActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.Electric;
            }
        }

        public Color ElectricColor { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public ElectricActionModel()
        {
            ElectricColor = Color.White;
            Radius = 0.20f;
            Duration = 0.60f;
        }

        public ElectricActionModel(ElectricAction action) : base(action)
        {
            ElectricColor = action.ElectricColor;
            Radius = action.Radius;
            Duration = action.Duration;
        }

        public ElectricActionModel(BaseAction action) : this(action as ElectricAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "electriccolor":
                    {
                        ElectricColor = value.ToColor();
                    }
                    break;
                case "radius":
                    {
                        Radius = Convert.ToSingle(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("ElectricColor", ElectricColor.StringFromColor());
            xmlWriter.WriteAttributeString("Radius", Radius.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/ElectricAction.cs`:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class ElectricAction : BaseAction
    {
        #region Properties

        public Color ElectricColor { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public ElectricAction(BaseObject owner) :
            base(owner, EActionType.Electric)
        {
            ElectricColor = Color.White;
            Radius = 0.20f;
            Duration = 0.60f;
        }

        public ElectricAction(BaseObject owner, ElectricActionModel actionModel) :
            base(owner, actionModel)
        {
            ElectricColor = actionModel.ElectricColor;
            Radius = actionModel.Radius;
            Duration = actionModel.Duration;
        }

        public ElectricAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as ElectricActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerElectric(Owner.Position, ElectricColor, Radius, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.ScreenTilt: { return new ScreenTiltActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.ScreenTilt: { return new ScreenTiltActionModel(); }
                case EActionType.Electric: { return new ElectricActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.ScreenTilt: { return new ScreenTiltActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.ScreenTilt: { return new ScreenTiltActionModel(actionModel); }
                case EActionType.Electric: { return new ElectricActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.ScreenTilt: { return new ScreenTiltAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.ScreenTilt: { return new ScreenTiltAction(owner, actionModel); }
                case EActionType.Electric: { return new ElectricAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.ScreenTilt: { return new ScreenTiltAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.ScreenTilt: { return new ScreenTiltAction(owner); }
                case EActionType.Electric: { return new ElectricAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/ElectricTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class ElectricTests
    {
        const float time = 1f;
        static readonly Color electricColor = Color.Cyan;
        const float radius = 0.35f;
        const float duration = 1f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new ElectricActionModel()
            {
                Time = time,
                ElectricColor = electricColor,
                Radius = radius,
                Duration = duration
            };

            var action = new ElectricAction(null, model);

            action.Time.ShouldBe(time);
            action.ElectricColor.ShouldBe(electricColor);
            action.Radius.ShouldBe(radius);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new ElectricAction(null)
            {
                Time = time,
                ElectricColor = electricColor,
                Radius = radius,
                Duration = duration
            };

            var model = new ElectricActionModel(action);

            model.ActionType.ShouldBe(EActionType.Electric);
            model.Time.ShouldBe(time);
            model.ElectricColor.ShouldBe(electricColor);
            model.Radius.ShouldBe(radius);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new ElectricActionModel()
            {
                Time = time,
                ElectricColor = electricColor,
                Radius = radius,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("ElectricTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("ElectricTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as ElectricActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.Electric);
            model2.Time.ShouldBe(time);
            model2.ElectricColor.ShouldBe(electricColor);
            model2.Radius.ShouldBe(radius);
            model2.Duration.ShouldBe(duration);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter ElectricTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/ElectricActionModel.cs GameDonkey/StateMachine/Actions/ElectricAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/ElectricTests.cs
git commit -m "Add Electric screen effect action"
```

---

### Task 15: Frost

**Files:**
- Create: `GameDonkey/Models/Actions/FrostActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/FrostAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/FrostTests.cs`

**Interfaces:**
- Consumes: `EActionType.Frost` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerFrost(Vector2 position, Color tintColor, float radius = 0.25f, float duration = 1.50f)`, `Vector2Extensions.ToColor()`/`.StringFromColor()`.
- Produces: `FrostActionModel` (`TintColor`, `Radius`, `Duration`), `FrostAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/FrostActionModel.cs`:

```csharp
using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
    public class FrostActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.Frost;
            }
        }

        public Color TintColor { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public FrostActionModel()
        {
            TintColor = Color.White;
            Radius = 0.25f;
            Duration = 1.50f;
        }

        public FrostActionModel(FrostAction action) : base(action)
        {
            TintColor = action.TintColor;
            Radius = action.Radius;
            Duration = action.Duration;
        }

        public FrostActionModel(BaseAction action) : this(action as FrostAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "tintcolor":
                    {
                        TintColor = value.ToColor();
                    }
                    break;
                case "radius":
                    {
                        Radius = Convert.ToSingle(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("TintColor", TintColor.StringFromColor());
            xmlWriter.WriteAttributeString("Radius", Radius.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/FrostAction.cs`:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class FrostAction : BaseAction
    {
        #region Properties

        public Color TintColor { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public FrostAction(BaseObject owner) :
            base(owner, EActionType.Frost)
        {
            TintColor = Color.White;
            Radius = 0.25f;
            Duration = 1.50f;
        }

        public FrostAction(BaseObject owner, FrostActionModel actionModel) :
            base(owner, actionModel)
        {
            TintColor = actionModel.TintColor;
            Radius = actionModel.Radius;
            Duration = actionModel.Duration;
        }

        public FrostAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as FrostActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerFrost(Owner.Position, TintColor, Radius, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.Electric: { return new ElectricActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.Electric: { return new ElectricActionModel(); }
                case EActionType.Frost: { return new FrostActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.Electric: { return new ElectricActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.Electric: { return new ElectricActionModel(actionModel); }
                case EActionType.Frost: { return new FrostActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.Electric: { return new ElectricAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.Electric: { return new ElectricAction(owner, actionModel); }
                case EActionType.Frost: { return new FrostAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.Electric: { return new ElectricAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.Electric: { return new ElectricAction(owner); }
                case EActionType.Frost: { return new FrostAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/FrostTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class FrostTests
    {
        const float time = 1f;
        static readonly Color tintColor = Color.LightBlue;
        const float radius = 0.4f;
        const float duration = 2.5f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new FrostActionModel()
            {
                Time = time,
                TintColor = tintColor,
                Radius = radius,
                Duration = duration
            };

            var action = new FrostAction(null, model);

            action.Time.ShouldBe(time);
            action.TintColor.ShouldBe(tintColor);
            action.Radius.ShouldBe(radius);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new FrostAction(null)
            {
                Time = time,
                TintColor = tintColor,
                Radius = radius,
                Duration = duration
            };

            var model = new FrostActionModel(action);

            model.ActionType.ShouldBe(EActionType.Frost);
            model.Time.ShouldBe(time);
            model.TintColor.ShouldBe(tintColor);
            model.Radius.ShouldBe(radius);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new FrostActionModel()
            {
                Time = time,
                TintColor = tintColor,
                Radius = radius,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("FrostTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("FrostTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as FrostActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.Frost);
            model2.Time.ShouldBe(time);
            model2.TintColor.ShouldBe(tintColor);
            model2.Radius.ShouldBe(radius);
            model2.Duration.ShouldBe(duration);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter FrostTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/FrostActionModel.cs GameDonkey/StateMachine/Actions/FrostAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/FrostTests.cs
git commit -m "Add Frost screen effect action"
```

---

### Task 16: Vortex

**Files:**
- Create: `GameDonkey/Models/Actions/VortexActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/VortexAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/VortexTests.cs`

**Interfaces:**
- Consumes: `EActionType.Vortex` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerVortex(Vector2 position, float radius = 0.25f, float speed = 1f, float spinInTime = 0.3f, float spinOutTime = 0.3f, FadeCurve fadeCurve = FadeCurve.Linear, bool clockwise = true)`. The `FadeCurve` property is named `VortexFadeCurve` (not `FadeCurve`) to avoid colliding with the `ScreenFXBuddy.FadeCurve` enum type name used in the same file.
- Produces: `VortexActionModel` (`Radius`, `Speed`, `SpinInTime`, `SpinOutTime`, `VortexFadeCurve`, `Clockwise`), `VortexAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/VortexActionModel.cs`:

```csharp
using ScreenFXBuddy;
using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class VortexActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.Vortex;
            }
        }

        public float Radius { get; set; }
        public float Speed { get; set; }
        public float SpinInTime { get; set; }
        public float SpinOutTime { get; set; }
        public FadeCurve VortexFadeCurve { get; set; }
        public bool Clockwise { get; set; }

        #endregion //Properties

        #region Initialization

        public VortexActionModel()
        {
            Radius = 0.25f;
            Speed = 1f;
            SpinInTime = 0.3f;
            SpinOutTime = 0.3f;
            VortexFadeCurve = FadeCurve.Linear;
            Clockwise = true;
        }

        public VortexActionModel(VortexAction action) : base(action)
        {
            Radius = action.Radius;
            Speed = action.Speed;
            SpinInTime = action.SpinInTime;
            SpinOutTime = action.SpinOutTime;
            VortexFadeCurve = action.VortexFadeCurve;
            Clockwise = action.Clockwise;
        }

        public VortexActionModel(BaseAction action) : this(action as VortexAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "radius":
                    {
                        Radius = Convert.ToSingle(value);
                    }
                    break;
                case "speed":
                    {
                        Speed = Convert.ToSingle(value);
                    }
                    break;
                case "spinintime":
                    {
                        SpinInTime = Convert.ToSingle(value);
                    }
                    break;
                case "spinouttime":
                    {
                        SpinOutTime = Convert.ToSingle(value);
                    }
                    break;
                case "vortexfadecurve":
                    {
                        VortexFadeCurve = (FadeCurve)Enum.Parse(typeof(FadeCurve), value);
                    }
                    break;
                case "clockwise":
                    {
                        Clockwise = Convert.ToBoolean(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("Radius", Radius.ToString());
            xmlWriter.WriteAttributeString("Speed", Speed.ToString());
            xmlWriter.WriteAttributeString("SpinInTime", SpinInTime.ToString());
            xmlWriter.WriteAttributeString("SpinOutTime", SpinOutTime.ToString());
            xmlWriter.WriteAttributeString("VortexFadeCurve", VortexFadeCurve.ToString());
            xmlWriter.WriteAttributeString("Clockwise", Clockwise.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/VortexAction.cs`:

```csharp
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class VortexAction : BaseAction
    {
        #region Properties

        public float Radius { get; set; }
        public float Speed { get; set; }
        public float SpinInTime { get; set; }
        public float SpinOutTime { get; set; }
        public FadeCurve VortexFadeCurve { get; set; }
        public bool Clockwise { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public VortexAction(BaseObject owner) :
            base(owner, EActionType.Vortex)
        {
            Radius = 0.25f;
            Speed = 1f;
            SpinInTime = 0.3f;
            SpinOutTime = 0.3f;
            VortexFadeCurve = FadeCurve.Linear;
            Clockwise = true;
        }

        public VortexAction(BaseObject owner, VortexActionModel actionModel) :
            base(owner, actionModel)
        {
            Radius = actionModel.Radius;
            Speed = actionModel.Speed;
            SpinInTime = actionModel.SpinInTime;
            SpinOutTime = actionModel.SpinOutTime;
            VortexFadeCurve = actionModel.VortexFadeCurve;
            Clockwise = actionModel.Clockwise;
        }

        public VortexAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as VortexActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerVortex(Owner.Position, Radius, Speed, SpinInTime, SpinOutTime, VortexFadeCurve, Clockwise);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.Frost: { return new FrostActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.Frost: { return new FrostActionModel(); }
                case EActionType.Vortex: { return new VortexActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.Frost: { return new FrostActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.Frost: { return new FrostActionModel(actionModel); }
                case EActionType.Vortex: { return new VortexActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.Frost: { return new FrostAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.Frost: { return new FrostAction(owner, actionModel); }
                case EActionType.Vortex: { return new VortexAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.Frost: { return new FrostAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.Frost: { return new FrostAction(owner); }
                case EActionType.Vortex: { return new VortexAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/VortexTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using ScreenFXBuddy;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class VortexTests
    {
        const float time = 1f;
        const float radius = 0.4f;
        const float speed = 1.5f;
        const float spinInTime = 0.5f;
        const float spinOutTime = 0.6f;
        const FadeCurve vortexFadeCurve = FadeCurve.Exponential;
        const bool clockwise = false;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new VortexActionModel()
            {
                Time = time,
                Radius = radius,
                Speed = speed,
                SpinInTime = spinInTime,
                SpinOutTime = spinOutTime,
                VortexFadeCurve = vortexFadeCurve,
                Clockwise = clockwise
            };

            var action = new VortexAction(null, model);

            action.Time.ShouldBe(time);
            action.Radius.ShouldBe(radius);
            action.Speed.ShouldBe(speed);
            action.SpinInTime.ShouldBe(spinInTime);
            action.SpinOutTime.ShouldBe(spinOutTime);
            action.VortexFadeCurve.ShouldBe(vortexFadeCurve);
            action.Clockwise.ShouldBe(clockwise);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new VortexAction(null)
            {
                Time = time,
                Radius = radius,
                Speed = speed,
                SpinInTime = spinInTime,
                SpinOutTime = spinOutTime,
                VortexFadeCurve = vortexFadeCurve,
                Clockwise = clockwise
            };

            var model = new VortexActionModel(action);

            model.ActionType.ShouldBe(EActionType.Vortex);
            model.Time.ShouldBe(time);
            model.Radius.ShouldBe(radius);
            model.Speed.ShouldBe(speed);
            model.SpinInTime.ShouldBe(spinInTime);
            model.SpinOutTime.ShouldBe(spinOutTime);
            model.VortexFadeCurve.ShouldBe(vortexFadeCurve);
            model.Clockwise.ShouldBe(clockwise);
        }

        [Test]
        public void Persist()
        {
            var model = new VortexActionModel()
            {
                Time = time,
                Radius = radius,
                Speed = speed,
                SpinInTime = spinInTime,
                SpinOutTime = spinOutTime,
                VortexFadeCurve = vortexFadeCurve,
                Clockwise = clockwise
            };

            var container = new StateContainerModel(new Filename("VortexTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("VortexTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as VortexActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.Vortex);
            model2.Time.ShouldBe(time);
            model2.Radius.ShouldBe(radius);
            model2.Speed.ShouldBe(speed);
            model2.SpinInTime.ShouldBe(spinInTime);
            model2.SpinOutTime.ShouldBe(spinOutTime);
            model2.VortexFadeCurve.ShouldBe(vortexFadeCurve);
            model2.Clockwise.ShouldBe(clockwise);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter VortexTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/VortexActionModel.cs GameDonkey/StateMachine/Actions/VortexAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/VortexTests.cs
git commit -m "Add Vortex screen effect action"
```

---

### Task 17: Smoke

**Files:**
- Create: `GameDonkey/Models/Actions/SmokeActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/SmokeAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/SmokeTests.cs`

**Interfaces:**
- Consumes: `EActionType.Smoke` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerSmoke(Vector2 position, Color color, float radius = 0.15f, float duration = 2.0f)`, `Vector2Extensions.ToColor()`/`.StringFromColor()`. Property named `SmokeColor` (not `Color`) to avoid colliding with the MonoGame `Color` type name in the same file.
- Produces: `SmokeActionModel` (`SmokeColor`, `Radius`, `Duration`), `SmokeAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/SmokeActionModel.cs`:

```csharp
using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
    public class SmokeActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.Smoke;
            }
        }

        public Color SmokeColor { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public SmokeActionModel()
        {
            SmokeColor = Color.White;
            Radius = 0.15f;
            Duration = 2.0f;
        }

        public SmokeActionModel(SmokeAction action) : base(action)
        {
            SmokeColor = action.SmokeColor;
            Radius = action.Radius;
            Duration = action.Duration;
        }

        public SmokeActionModel(BaseAction action) : this(action as SmokeAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "smokecolor":
                    {
                        SmokeColor = value.ToColor();
                    }
                    break;
                case "radius":
                    {
                        Radius = Convert.ToSingle(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("SmokeColor", SmokeColor.StringFromColor());
            xmlWriter.WriteAttributeString("Radius", Radius.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/SmokeAction.cs`:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class SmokeAction : BaseAction
    {
        #region Properties

        public Color SmokeColor { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public SmokeAction(BaseObject owner) :
            base(owner, EActionType.Smoke)
        {
            SmokeColor = Color.White;
            Radius = 0.15f;
            Duration = 2.0f;
        }

        public SmokeAction(BaseObject owner, SmokeActionModel actionModel) :
            base(owner, actionModel)
        {
            SmokeColor = actionModel.SmokeColor;
            Radius = actionModel.Radius;
            Duration = actionModel.Duration;
        }

        public SmokeAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as SmokeActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerSmoke(Owner.Position, SmokeColor, Radius, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.Vortex: { return new VortexActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.Vortex: { return new VortexActionModel(); }
                case EActionType.Smoke: { return new SmokeActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.Vortex: { return new VortexActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.Vortex: { return new VortexActionModel(actionModel); }
                case EActionType.Smoke: { return new SmokeActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.Vortex: { return new VortexAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.Vortex: { return new VortexAction(owner, actionModel); }
                case EActionType.Smoke: { return new SmokeAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.Vortex: { return new VortexAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.Vortex: { return new VortexAction(owner); }
                case EActionType.Smoke: { return new SmokeAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/SmokeTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class SmokeTests
    {
        const float time = 1f;
        static readonly Color smokeColor = Color.DarkGray;
        const float radius = 0.25f;
        const float duration = 3f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new SmokeActionModel()
            {
                Time = time,
                SmokeColor = smokeColor,
                Radius = radius,
                Duration = duration
            };

            var action = new SmokeAction(null, model);

            action.Time.ShouldBe(time);
            action.SmokeColor.ShouldBe(smokeColor);
            action.Radius.ShouldBe(radius);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new SmokeAction(null)
            {
                Time = time,
                SmokeColor = smokeColor,
                Radius = radius,
                Duration = duration
            };

            var model = new SmokeActionModel(action);

            model.ActionType.ShouldBe(EActionType.Smoke);
            model.Time.ShouldBe(time);
            model.SmokeColor.ShouldBe(smokeColor);
            model.Radius.ShouldBe(radius);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new SmokeActionModel()
            {
                Time = time,
                SmokeColor = smokeColor,
                Radius = radius,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("SmokeTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("SmokeTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as SmokeActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.Smoke);
            model2.Time.ShouldBe(time);
            model2.SmokeColor.ShouldBe(smokeColor);
            model2.Radius.ShouldBe(radius);
            model2.Duration.ShouldBe(duration);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter SmokeTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/SmokeActionModel.cs GameDonkey/StateMachine/Actions/SmokeAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/SmokeTests.cs
git commit -m "Add Smoke screen effect action"
```

---

### Task 18: GlassShatter

**Files:**
- Create: `GameDonkey/Models/Actions/GlassShatterActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/GlassShatterAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/GlassShatterTests.cs`

**Interfaces:**
- Consumes: `EActionType.GlassShatter` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerGlassShatter(Vector2 position, float strength = 0.04f, int numCells = 20, float duration = 0.8f)`.
- Produces: `GlassShatterActionModel` (`Strength`, `NumCells` (`int`), `Duration`), `GlassShatterAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/GlassShatterActionModel.cs`:

```csharp
using System;
using System.Xml;

namespace GameDonkeyLib
{
    public class GlassShatterActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.GlassShatter;
            }
        }

        public float Strength { get; set; }
        public int NumCells { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public GlassShatterActionModel()
        {
            Strength = 0.04f;
            NumCells = 20;
            Duration = 0.8f;
        }

        public GlassShatterActionModel(GlassShatterAction action) : base(action)
        {
            Strength = action.Strength;
            NumCells = action.NumCells;
            Duration = action.Duration;
        }

        public GlassShatterActionModel(BaseAction action) : this(action as GlassShatterAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "strength":
                    {
                        Strength = Convert.ToSingle(value);
                    }
                    break;
                case "numcells":
                    {
                        NumCells = Convert.ToInt32(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("Strength", Strength.ToString());
            xmlWriter.WriteAttributeString("NumCells", NumCells.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/GlassShatterAction.cs`:

```csharp
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class GlassShatterAction : BaseAction
    {
        #region Properties

        public float Strength { get; set; }
        public int NumCells { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public GlassShatterAction(BaseObject owner) :
            base(owner, EActionType.GlassShatter)
        {
            Strength = 0.04f;
            NumCells = 20;
            Duration = 0.8f;
        }

        public GlassShatterAction(BaseObject owner, GlassShatterActionModel actionModel) :
            base(owner, actionModel)
        {
            Strength = actionModel.Strength;
            NumCells = actionModel.NumCells;
            Duration = actionModel.Duration;
        }

        public GlassShatterAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as GlassShatterActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerGlassShatter(Owner.Position, Strength, NumCells, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.Smoke: { return new SmokeActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.Smoke: { return new SmokeActionModel(); }
                case EActionType.GlassShatter: { return new GlassShatterActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.Smoke: { return new SmokeActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.Smoke: { return new SmokeActionModel(actionModel); }
                case EActionType.GlassShatter: { return new GlassShatterActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.Smoke: { return new SmokeAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.Smoke: { return new SmokeAction(owner, actionModel); }
                case EActionType.GlassShatter: { return new GlassShatterAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.Smoke: { return new SmokeAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.Smoke: { return new SmokeAction(owner); }
                case EActionType.GlassShatter: { return new GlassShatterAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/GlassShatterTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class GlassShatterTests
    {
        const float time = 1f;
        const float strength = 0.08f;
        const int numCells = 35;
        const float duration = 1.2f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new GlassShatterActionModel()
            {
                Time = time,
                Strength = strength,
                NumCells = numCells,
                Duration = duration
            };

            var action = new GlassShatterAction(null, model);

            action.Time.ShouldBe(time);
            action.Strength.ShouldBe(strength);
            action.NumCells.ShouldBe(numCells);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new GlassShatterAction(null)
            {
                Time = time,
                Strength = strength,
                NumCells = numCells,
                Duration = duration
            };

            var model = new GlassShatterActionModel(action);

            model.ActionType.ShouldBe(EActionType.GlassShatter);
            model.Time.ShouldBe(time);
            model.Strength.ShouldBe(strength);
            model.NumCells.ShouldBe(numCells);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new GlassShatterActionModel()
            {
                Time = time,
                Strength = strength,
                NumCells = numCells,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("GlassShatterTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("GlassShatterTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as GlassShatterActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.GlassShatter);
            model2.Time.ShouldBe(time);
            model2.Strength.ShouldBe(strength);
            model2.NumCells.ShouldBe(numCells);
            model2.Duration.ShouldBe(duration);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter GlassShatterTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/GlassShatterActionModel.cs GameDonkey/StateMachine/Actions/GlassShatterAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/GlassShatterTests.cs
git commit -m "Add GlassShatter screen effect action"
```

---

### Task 19: SpeedLines

**Files:**
- Create: `GameDonkey/Models/Actions/SpeedLinesActionModel.cs`
- Create: `GameDonkey/StateMachine/Actions/SpeedLinesAction.cs`
- Modify: `GameDonkey/StateMachine/StateActionFactory.cs`
- Test: `GameDonkey.Tests/Actions/SpeedLinesTests.cs`

**Interfaces:**
- Consumes: `EActionType.SpeedLines` (Task 1), `IGameDonkey.ScreenFX` (Task 1), `IScreenFXService.TriggerSpeedLines(Vector2 position, Color color, SpeedLinesMode linesMode = SpeedLinesMode.Expand, FadeMode fadeMode = FadeMode.FadeOut, FadeCurve fadeCurve = FadeCurve.Logarithmic, int lineCount = 24, float maxRadius = 1.0f, float duration = 1f)`, `ScreenFXBuddy.Effects.SpeedLinesMode` enum (`Static`, `Expand`), `Vector2Extensions.ToColor()`/`.StringFromColor()`. `FadeMode`/`FadeCurve` properties are named `SpeedLinesFadeMode`/`SpeedLinesFadeCurve` (not `FadeMode`/`FadeCurve`) to avoid colliding with those enum type names in the same file; `Color` property is named `SpeedLinesColor` to avoid colliding with the MonoGame `Color` type name.
- Produces: `SpeedLinesActionModel` (`SpeedLinesColor`, `LinesMode`, `SpeedLinesFadeMode`, `SpeedLinesFadeCurve`, `LineCount` (`int`), `MaxRadius`, `Duration`), `SpeedLinesAction`.

- [ ] **Step 1: Write the model**

Create `GameDonkey/Models/Actions/SpeedLinesActionModel.cs`:

```csharp
using Microsoft.Xna.Framework;
using ScreenFXBuddy;
using ScreenFXBuddy.Effects;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
    public class SpeedLinesActionModel : BaseActionModel
    {
        #region Properties

        public override EActionType ActionType
        {
            get
            {
                return EActionType.SpeedLines;
            }
        }

        public Color SpeedLinesColor { get; set; }
        public SpeedLinesMode LinesMode { get; set; }
        public FadeMode SpeedLinesFadeMode { get; set; }
        public FadeCurve SpeedLinesFadeCurve { get; set; }
        public int LineCount { get; set; }
        public float MaxRadius { get; set; }
        public float Duration { get; set; }

        #endregion //Properties

        #region Initialization

        public SpeedLinesActionModel()
        {
            SpeedLinesColor = Color.White;
            LinesMode = SpeedLinesMode.Expand;
            SpeedLinesFadeMode = FadeMode.FadeOut;
            SpeedLinesFadeCurve = FadeCurve.Logarithmic;
            LineCount = 24;
            MaxRadius = 1.0f;
            Duration = 1f;
        }

        public SpeedLinesActionModel(SpeedLinesAction action) : base(action)
        {
            SpeedLinesColor = action.SpeedLinesColor;
            LinesMode = action.LinesMode;
            SpeedLinesFadeMode = action.SpeedLinesFadeMode;
            SpeedLinesFadeCurve = action.SpeedLinesFadeCurve;
            LineCount = action.LineCount;
            MaxRadius = action.MaxRadius;
            Duration = action.Duration;
        }

        public SpeedLinesActionModel(BaseAction action) : this(action as SpeedLinesAction)
        {
        }

        #endregion //Initialization

        #region Methods

        public override void ParseXmlNode(XmlNode node)
        {
            var name = node.Name;
            var value = node.InnerText;

            switch (name.ToLower())
            {
                case "speedlinescolor":
                    {
                        SpeedLinesColor = value.ToColor();
                    }
                    break;
                case "linesmode":
                    {
                        LinesMode = (SpeedLinesMode)Enum.Parse(typeof(SpeedLinesMode), value);
                    }
                    break;
                case "speedlinesfademode":
                    {
                        SpeedLinesFadeMode = (FadeMode)Enum.Parse(typeof(FadeMode), value);
                    }
                    break;
                case "speedlinesfadecurve":
                    {
                        SpeedLinesFadeCurve = (FadeCurve)Enum.Parse(typeof(FadeCurve), value);
                    }
                    break;
                case "linecount":
                    {
                        LineCount = Convert.ToInt32(value);
                    }
                    break;
                case "maxradius":
                    {
                        MaxRadius = Convert.ToSingle(value);
                    }
                    break;
                case "duration":
                    {
                        Duration = Convert.ToSingle(value);
                    }
                    break;
                default:
                    {
                        base.ParseXmlNode(node);
                    }
                    break;
            }
        }

        protected override void WriteActionXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("SpeedLinesColor", SpeedLinesColor.StringFromColor());
            xmlWriter.WriteAttributeString("LinesMode", LinesMode.ToString());
            xmlWriter.WriteAttributeString("SpeedLinesFadeMode", SpeedLinesFadeMode.ToString());
            xmlWriter.WriteAttributeString("SpeedLinesFadeCurve", SpeedLinesFadeCurve.ToString());
            xmlWriter.WriteAttributeString("LineCount", LineCount.ToString());
            xmlWriter.WriteAttributeString("MaxRadius", MaxRadius.ToString());
            xmlWriter.WriteAttributeString("Duration", Duration.ToString());
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 2: Write the action**

Create `GameDonkey/StateMachine/Actions/SpeedLinesAction.cs`:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;
using ScreenFXBuddy.Effects;

namespace GameDonkeyLib
{
    public class SpeedLinesAction : BaseAction
    {
        #region Properties

        public Color SpeedLinesColor { get; set; }
        public SpeedLinesMode LinesMode { get; set; }
        public FadeMode SpeedLinesFadeMode { get; set; }
        public FadeCurve SpeedLinesFadeCurve { get; set; }
        public int LineCount { get; set; }
        public float MaxRadius { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public SpeedLinesAction(BaseObject owner) :
            base(owner, EActionType.SpeedLines)
        {
            SpeedLinesColor = Color.White;
            LinesMode = SpeedLinesMode.Expand;
            SpeedLinesFadeMode = FadeMode.FadeOut;
            SpeedLinesFadeCurve = FadeCurve.Logarithmic;
            LineCount = 24;
            MaxRadius = 1.0f;
            Duration = 1f;
        }

        public SpeedLinesAction(BaseObject owner, SpeedLinesActionModel actionModel) :
            base(owner, actionModel)
        {
            SpeedLinesColor = actionModel.SpeedLinesColor;
            LinesMode = actionModel.LinesMode;
            SpeedLinesFadeMode = actionModel.SpeedLinesFadeMode;
            SpeedLinesFadeCurve = actionModel.SpeedLinesFadeCurve;
            LineCount = actionModel.LineCount;
            MaxRadius = actionModel.MaxRadius;
            Duration = actionModel.Duration;
        }

        public SpeedLinesAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as SpeedLinesActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerSpeedLines(Owner.Position, SpeedLinesColor, LinesMode, SpeedLinesFadeMode, SpeedLinesFadeCurve, LineCount, MaxRadius, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
```

- [ ] **Step 3: Wire into the factory**

In `GameDonkey/StateMachine/StateActionFactory.cs`, replace in `CreateActionModel(string actionType)`:

```csharp
                case EActionType.GlassShatter: { return new GlassShatterActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.GlassShatter: { return new GlassShatterActionModel(); }
                case EActionType.SpeedLines: { return new SpeedLinesActionModel(); }
                default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
```

Replace in `CreateActionModel(BaseAction actionModel)`:

```csharp
                case EActionType.GlassShatter: { return new GlassShatterActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

with:

```csharp
                case EActionType.GlassShatter: { return new GlassShatterActionModel(actionModel); }
                case EActionType.SpeedLines: { return new SpeedLinesActionModel(actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
```

Replace in `CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)`:

```csharp
                case EActionType.GlassShatter: { return new GlassShatterAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

with:

```csharp
                case EActionType.GlassShatter: { return new GlassShatterAction(owner, actionModel); }
                case EActionType.SpeedLines: { return new SpeedLinesAction(owner, actionModel); }
                default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
```

Replace in `CreateStateAction(EActionType actionType, BaseObject owner)`:

```csharp
                case EActionType.GlassShatter: { return new GlassShatterAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

with:

```csharp
                case EActionType.GlassShatter: { return new GlassShatterAction(owner); }
                case EActionType.SpeedLines: { return new SpeedLinesAction(owner); }
                default: { throw new Exception($"unknown actionType: {actionType}"); }
```

- [ ] **Step 4: Write the tests**

Create `GameDonkey.Tests/Actions/SpeedLinesTests.cs`:

```csharp
using FilenameBuddy;
using GameDonkeyLib;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using ScreenFXBuddy;
using ScreenFXBuddy.Effects;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class SpeedLinesTests
    {
        const float time = 1f;
        static readonly Color speedLinesColor = Color.Yellow;
        const SpeedLinesMode linesMode = SpeedLinesMode.Static;
        const FadeMode speedLinesFadeMode = FadeMode.Constant;
        const FadeCurve speedLinesFadeCurve = FadeCurve.Linear;
        const int lineCount = 40;
        const float maxRadius = 0.8f;
        const float duration = 1.6f;

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitFilePaths();
        }

        [Test]
        public void ModelToAction()
        {
            var model = new SpeedLinesActionModel()
            {
                Time = time,
                SpeedLinesColor = speedLinesColor,
                LinesMode = linesMode,
                SpeedLinesFadeMode = speedLinesFadeMode,
                SpeedLinesFadeCurve = speedLinesFadeCurve,
                LineCount = lineCount,
                MaxRadius = maxRadius,
                Duration = duration
            };

            var action = new SpeedLinesAction(null, model);

            action.Time.ShouldBe(time);
            action.SpeedLinesColor.ShouldBe(speedLinesColor);
            action.LinesMode.ShouldBe(linesMode);
            action.SpeedLinesFadeMode.ShouldBe(speedLinesFadeMode);
            action.SpeedLinesFadeCurve.ShouldBe(speedLinesFadeCurve);
            action.LineCount.ShouldBe(lineCount);
            action.MaxRadius.ShouldBe(maxRadius);
            action.Duration.ShouldBe(duration);
        }

        [Test]
        public void ActionToModel()
        {
            var action = new SpeedLinesAction(null)
            {
                Time = time,
                SpeedLinesColor = speedLinesColor,
                LinesMode = linesMode,
                SpeedLinesFadeMode = speedLinesFadeMode,
                SpeedLinesFadeCurve = speedLinesFadeCurve,
                LineCount = lineCount,
                MaxRadius = maxRadius,
                Duration = duration
            };

            var model = new SpeedLinesActionModel(action);

            model.ActionType.ShouldBe(EActionType.SpeedLines);
            model.Time.ShouldBe(time);
            model.SpeedLinesColor.ShouldBe(speedLinesColor);
            model.LinesMode.ShouldBe(linesMode);
            model.SpeedLinesFadeMode.ShouldBe(speedLinesFadeMode);
            model.SpeedLinesFadeCurve.ShouldBe(speedLinesFadeCurve);
            model.LineCount.ShouldBe(lineCount);
            model.MaxRadius.ShouldBe(maxRadius);
            model.Duration.ShouldBe(duration);
        }

        [Test]
        public void Persist()
        {
            var model = new SpeedLinesActionModel()
            {
                Time = time,
                SpeedLinesColor = speedLinesColor,
                LinesMode = linesMode,
                SpeedLinesFadeMode = speedLinesFadeMode,
                SpeedLinesFadeCurve = speedLinesFadeCurve,
                LineCount = lineCount,
                MaxRadius = maxRadius,
                Duration = duration
            };

            var container = new StateContainerModel(new Filename("SpeedLinesTests.xml"));
            var actions = new SingleStateActionsModel();
            container.StatesActions.Add(actions);
            actions.ActionModels.Add(model);
            container.WriteXml();

            var container2 = new StateContainerModel(new Filename("SpeedLinesTests.xml"));
            container2.ReadXmlFile();

            container2.StatesActions.Count.ShouldBe(1);
            container2.StatesActions[0].ActionModels.Count.ShouldBe(1);
            var model2 = container2.StatesActions[0].ActionModels[0] as SpeedLinesActionModel;
            model2.ShouldNotBeNull();
            model2.ActionType.ShouldBe(EActionType.SpeedLines);
            model2.Time.ShouldBe(time);
            model2.SpeedLinesColor.ShouldBe(speedLinesColor);
            model2.LinesMode.ShouldBe(linesMode);
            model2.SpeedLinesFadeMode.ShouldBe(speedLinesFadeMode);
            model2.SpeedLinesFadeCurve.ShouldBe(speedLinesFadeCurve);
            model2.LineCount.ShouldBe(lineCount);
            model2.MaxRadius.ShouldBe(maxRadius);
            model2.Duration.ShouldBe(duration);
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj --filter SpeedLinesTests`
Expected: 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add GameDonkey/Models/Actions/SpeedLinesActionModel.cs GameDonkey/StateMachine/Actions/SpeedLinesAction.cs GameDonkey/StateMachine/StateActionFactory.cs GameDonkey.Tests/Actions/SpeedLinesTests.cs
git commit -m "Add SpeedLines screen effect action"
```

---

## Task 20: Full solution verification

**Files:** none (verification only)

- [ ] **Step 1: Full build**

Run: `dotnet build GameDonkey.sln`
Expected: build succeeds, 0 errors.

- [ ] **Step 2: Full test run**

Run: `dotnet test GameDonkey.Tests/GameDonkey.Tests.csproj`
Expected: all tests pass, including the 18 new `*Tests` fixtures (54 new tests total) and all pre-existing tests (no regressions).

- [ ] **Step 3: Confirm factory completeness**

Each of the 18 new effects must appear in all 4 switch statements (`CreateActionModel(string)`, `CreateActionModel(BaseAction)`, `CreateStateAction(BaseActionModel, BaseObject, IStateContainer)`, `CreateStateAction(EActionType, BaseObject)`) in `GameDonkey/StateMachine/StateActionFactory.cs` — 4 occurrences of `EActionType.<Name>` per effect.

Run:

```bash
for name in ForceRipple GravityWave ScreenShake ChromaticAberration ChromaticSplit HeatHaze HitFlash AnimeSuper Letterbox FreezeFrame ZoomBlur ScreenTilt Electric Frost Vortex Smoke GlassShatter SpeedLines; do
  count=$(grep -c "EActionType\.$name:" GameDonkey/StateMachine/StateActionFactory.cs)
  echo "$name: $count"
done
```

Expected: every line reads `<Name>: 4`. Any line reading a different number means that effect is missing a case in one of the 4 switches.

- [ ] **Step 4: Commit (only if step 3 required a fix)**

If any switch was missing a case, add it, rerun steps 1-3, then:

```bash
git add GameDonkey/StateMachine/StateActionFactory.cs
git commit -m "Fix missing StateActionFactory case for ScreenFX action"
```

If nothing was missing, no commit is needed for this task.
