# ScreenFXBuddy Effects as GameDonkey Scriptable Actions

## Goal

Expose every screen effect in the `ScreenFxBuddy` NuGet package (already referenced in
`GameDonkey/GameDonkey.csproj`, pending) as a scriptable `EActionType` in the GameDonkey
state machine, so designers can trigger screen effects (screen shake, chromatic
aberration, freeze frame, etc.) from state action XML the same way they trigger
`CameraShake`, `PointLight`, `ParticleEffect`, and so on today.

## Source of truth

`ScreenFXBuddy.IScreenFXService` (in the `ScreenFxBuddy` package) exposes one
`Trigger*` method per effect. Each `Trigger*` method becomes exactly one new
GameDonkey action, with one exception noted below.

## The 18 actions

| EActionType | Trigger signature (defaults from `IScreenFXService`) |
|---|---|
| `ForceRipple` | `Vector2 position, float strength=0.05f, float speed=0.4f, float size=0.08f, float time=2f` |
| `GravityWave` | `Vector2 position, float strength=0.04f, float startHeight=0.05f, float endHeight=0.25f, float speed=0.5f, float duration=1.5f` |
| `ScreenShake` | `float length=1f, float delta=0.1f, float amount=0.1f` |
| `ChromaticAberration` | `Vector2 startPosition, float distance=0.1f, float time=2f, FadeCurve curve=Linear` |
| `ChromaticSplit` | `Vector2 position, float maxDistance=0.05f, float duration=0.3f` |
| `HeatHaze` | `Vector2 position, float strength=0.02f, float radius=0.15f, float height=0.40f, float duration=3.0f` |
| `HitFlash` | `Color blendColor, FadeMode mode=FadeOut, FadeCurve curve=Linear, EffectBlendMode blendMode=LinearDodge, float time=1f` |
| `AnimeSuper` | `Color color, float flashIn=0.05f, float hold=0.30f, float fadeOut=0.40f` |
| `Letterbox` | `float barHeight=0.10f, float slideIn=0.15f, float hold=1.00f, float slideOut=0.15f` |
| `FreezeFrame` | `Color tintColor, float flashIn=0.10f, float hold=0.40f, float fadeOut=0.30f` |
| `ZoomBlur` | `Vector2 position, float strength=0.05f, float radius=1.0f, float duration=0.4f` |
| `ScreenTilt` | `float angle=3.0f, float duration=0.4f, float delta=0.2f` |
| `Electric` | `Vector2 position, Color color, float radius=0.20f, float duration=0.60f` |
| `Frost` | `Vector2 position, Color tintColor, float radius=0.25f, float duration=1.50f` |
| `Vortex` | `Vector2 position, float radius=0.25f, float speed=1f, float spinInTime=0.3f, float spinOutTime=0.3f, FadeCurve fadeCurve=Linear, bool clockwise=true` |
| `Smoke` | `Vector2 position, Color color, float radius=0.15f, float duration=2.0f` |
| `GlassShatter` | `Vector2 position, float strength=0.04f, int numCells=20, float duration=0.8f` |
| `SpeedLines` | `Vector2 position, Color color, SpeedLinesMode linesMode=Expand, FadeMode fadeMode=FadeOut, FadeCurve fadeCurve=Logarithmic, int lineCount=24, float maxRadius=1.0f, float duration=1f` |

**ChromaticAberration vs ChromaticSplit:** both live on `ChromaticAberrationLayer` in
ScreenFXBuddy but are triggered via two distinct `IScreenFXService` methods with
different parameter sets. Modeled as two independent action types (not one action
with a mode flag), consistent with the 1-effect-1-action shape used everywhere else.

**Position handling:** every action with a `Vector2 position` parameter passes
`Owner.Position` directly. No offset property (unlike `PointLightAction.StartOffset`) —
YAGNI until a concrete need for per-instance nudging shows up.

## Per-effect deliverables (× 18)

1. **Enum** — `GameDonkey/StateMachine/ActionType.cs`: append new `EActionType` values
   after `TemplateAction`, in the table order above.
2. **Model** — `GameDonkey/Models/Actions/<Name>ActionModel.cs`: subclass
   `BaseActionModel`, one property per trigger parameter (defaults matching the table),
   constructors `()`, `(<Name>Action action)`, `(BaseAction action)`, plus
   `ParseXmlNode`/`WriteActionXml` overrides. Follow `CameraShakeActionModel.cs` /
   `PointLightActionModel.cs` structure exactly.
3. **Action** — `GameDonkey/StateMachine/Actions/<Name>Action.cs`: subclass
   `BaseAction`, constructors `(BaseObject owner)`, `(BaseObject owner, <Name>ActionModel actionModel)`,
   `(BaseObject owner, BaseActionModel actionModel)`. `LoadContent` grabs
   `engine.ScreenFX`. `Execute` calls the matching `Trigger*` method, then
   `return base.Execute(currentTime);`. Follow `CameraShakeAction.cs` /
   `PointLightAction.cs` structure exactly.
4. **Factory wiring** — `GameDonkey/StateMachine/StateActionFactory.cs`: add a case to
   all four switch statements (`CreateActionModel(string)`, `CreateActionModel(BaseAction)`,
   `CreateStateAction(BaseActionModel, BaseObject, IStateContainer)`,
   `CreateStateAction(EActionType, BaseObject)`).
5. **Tests** — `GameDonkey.Tests/Actions/<Name>Tests.cs`: `ModelToAction`,
   `ActionToModel`, `Persist` (XML round-trip via `StateContainerModel`), following
   `CameraShakeTests.cs`.

## Engine plumbing (one-time, not per-effect)

- **`GameDonkey/IGameDonkey.cs`**: add `IScreenFXService ScreenFX { get; }`.
- **`GameDonkey/GameDonkey.cs`**: instantiate `ScreenFX = new ScreenFXComponent(Game)`
  in the constructor (alongside `ParticleEngine = new ParticleEngine()`), call its
  `LoadContent` during `GameDonkey.LoadContent`, and call `ScreenFX.Update(MasterClock)`
  alongside the existing `ParticleEngine.Update(MasterClock)` call. This is enough for
  `Trigger*` calls to run without throwing and for effect lifetimes to tick down
  correctly.
- **Explicitly out of scope for this pass**: wiring `BeginCapture`/`EndCapture` into
  the render pass. No triggered effect will be visible on screen yet — that requires a
  separate decision about where in `GameDonkey.Render()` to capture the scene, what
  virtual resolution to use, and how to compose the transform matrix. This is a known,
  intentional gap, not an oversight.

## XML parsing conventions (no new helpers needed)

- Enums (`FadeCurve`, `FadeMode`, `EffectBlendMode`, `SpeedLinesMode`):
  `(T)Enum.Parse(typeof(T), value)`, matching `DirectionActionModel` /
  `PlayAnimationActionModel`.
- `Color` / `Vector2`: existing `Vector2Extensions.ToColor()` / `.ToVector2()`.
- `bool` / `int`: `Convert.ToBoolean(value)` / `Convert.ToInt32(value)`, matching
  `BoardModel` and others.

## Explicitly not doing

- No render-pipeline integration (see above).
- No offset/bone-attachment support for positioned effects.
- No changes to `ScreenFXBuddy`/`ScreenFxBuddy` itself — consumed as-is via NuGet.
