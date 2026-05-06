# Input Feature

How to use the ECS-friendly gameplay input layer for new client features.

## Goal

`StaticMlp.Features.Input` is the only place where gameplay talks to Unity Input System.

Use it when a feature needs:

- movement or look input;
- press/hold/release actions;
- pointer position;
- aim ray for world interactions.

Do not read `Keyboard.current`, `Mouse.current`, `InputAction`, or raw Unity input APIs from gameplay systems.

## Architecture

Flow:

```text
Unity Input System
-> InputResource
-> ClientInputState resource
-> InputActionPressedEvent / InputActionReleasedEvent
-> client gameplay systems
```

Split of responsibilities:

- `InputResource` owns generated `InputSystem_Actions`, enables only the `Player` map, caches action metadata, and captures raw values every frame.
- `ClientInputCaptureSystem` creates `InputResource` and `ClientInputState`, then copies captured input into ECS resources.
- `ClientInputEventPublishSystem` publishes per-frame button edge events.
- `StaticMlp.Features.Player.ClientCameraInputSystem` converts look input into `ClientCameraState`.
- Feature systems read `ClientInputState` or input events.

## Where contracts live

Shared input contracts live in `Game.Core`:

- `StaticMlp.Game.Input.ClientInputState`
- `StaticMlp.Game.Input.InputActionName`
- `StaticMlp.Game.Input.InputActionPressedEvent`
- `StaticMlp.Game.Input.InputActionReleasedEvent`
- `StaticMlp.Game.Input.CoreInputActions`

Feature-local action names live in the feature assembly, for example:

```csharp
using StaticMlp.Game.Input;

namespace StaticMlp.Features.Buildings
{
    public static class BuildingsInputActions
    {
        public static readonly InputActionName BuildMenuToggle = new("BuildMenuToggle");
        public static readonly InputActionName PlacementRotate = new("PlacementRotate");
        public static readonly InputActionName BuildConstruction = new("BuildConstruction");
    }
}
```

This keeps shared contracts in `Game.Core` and avoids asmdef dependency cycles.

## Runtime setup

No scene bridge is required.

`ClientInputCaptureSystem` creates and owns `InputResource` at runtime. `InputResource` internally constructs the generated `InputSystem_Actions` wrapper and enables only the `Player` action map.

Camera ownership is outside the input feature and belongs to `StaticMlp.Features.Player`.

Aim setup comes from player camera presentation:

- `GameplayCamera` writes `ClientCameraConfig.AimCamera`;
- `InputResource` uses that camera to build `TryGetAimRay`;
- if the camera is missing, that is a bug and should be fixed, not silently worked around.

## Reuse existing actions first

Before adding a new action, check whether one of the shared core actions already matches the intent:

- `Move`
- `Look`
- `Primary`
- `Secondary`
- `Interact`
- `Cancel`
- `Jump`
- `Sprint`
- `Previous`
- `Next`
- `PointerPosition`

Examples:

- use `Primary` for confirm/place/fire;
- use `Secondary` for right-click style alternate action;
- use `Cancel` for escape/cancel/close;
- use `Interact` for one-shot interaction;
- use `Move` and `Look` for continuous axes.

## Add input to a new feature

### 1. Add the action to `InputSystem_Actions.inputactions`

Add a new action to the `Player` map.

Rules:

- gameplay actions belong to the `Player` map, not `UI`;
- button actions should use `expectedControlType: Button`;
- stick/mouse-like 2D values should use `expectedControlType: Vector2`.

`InputResource` discovers actions from the `Player` map automatically. No bridge code changes are needed for new actions.

### 2. Add a typed action constant in the feature

Create a small feature-local constants file:

```csharp
using StaticMlp.Game.Input;

namespace StaticMlp.Features.FeatureA
{
    public static class FeatureAInputActions
    {
        public static readonly InputActionName ToggleMode = new("ToggleMode");
    }
}
```

Do not use magic strings directly in systems.

### 3. Read `ClientInputState` from a client system

For continuous state or per-frame polling:

```csharp
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Input;
using StaticMlp.Networking;

public sealed class FeatureAClientSystem : ISystem
{
    public void Update()
    {
        var inputState = CW.GetResource<ClientInputState>();

        if (inputState.WasPressed(FeatureAInputActions.ToggleMode))
        {
            // react once this frame
        }

        var move = inputState.ReadVector2(CoreInputActions.Move);
        var isBuilding = inputState.IsPressed(FeatureAInputActions.ToggleMode);
    }
}
```

Available accessors:

- `ReadVector2(action)` for analog or 2D input.
- `IsPressed(action)` for current hold state.
- `WasPressed(action)` for one-frame press edge.
- `WasReleased(action)` for one-frame release edge.
- `PointerPosition` for pointer screen position.
- `TryGetAimRay(out ray)` for world targeting.

### 4. Or consume input as ECS events

If the feature wants event-driven handling, use:

- `InputActionPressedEvent`
- `InputActionReleasedEvent`

Pattern:

```csharp
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Input;
using StaticMlp.Networking;

public sealed class FeatureAInputEventSystem : ISystem
{
    private EventReceiver<ClientCoreWT, InputActionPressedEvent> _pressed;

    public void Init()
    {
        _pressed = CW.RegisterEventReceiver<InputActionPressedEvent>();
    }

    public void Destroy()
    {
        CW.DeleteEventReceiver(ref _pressed);
    }

    public void Update()
    {
        foreach (var evt in _pressed)
        {
            if (evt.Value.Action != FeatureAInputActions.ToggleMode)
                continue;

            // react to press
        }
    }
}
```

Use events when the feature is naturally command-driven.
Use `ClientInputState` when the feature needs polling or analog values every frame.

## Aim and pointing

For world placement, targeting, or mouse-driven aiming:

```csharp
if (inputState.TryGetAimRay(out var ray))
{
    if (Physics.Raycast(ray, out var hit, 500f))
    {
        // use hit.point
    }
}
```

Behavior:

- if pointer is available, the ray uses pointer position;
- if cursor is locked or pointer action is unavailable, the ray uses screen center;
- if no aim camera is assigned, `TryGetAimRay` returns `false`.

## Camera

Do not read raw look input in feature code just to get yaw.

Camera control is owned by `StaticMlp.Features.Player`.

Shared camera state is still exposed through `ClientCameraState` so other features can read yaw without duplicating camera logic.

Use it when gameplay needs the current camera yaw:

```csharp
var cameraState = CW.GetResource<ClientCameraState>();
var yaw = cameraState.Yaw;
```

## Fail-fast rule

Missing required input resources are bugs.

Do this:

```csharp
var inputState = CW.GetResource<ClientInputState>();
```

Do not do this:

```csharp
if (!CW.HasResource<ClientInputState>())
    return;
```

If a system requires `ClientInputState`, `ClientCameraState`, or another input dependency, then that system must only be registered in contexts where those resources exist. Do not silently skip work and leave control broken without an obvious error.

## Registration

Normal gameplay features do not need to register the input feature manually.

`InputGameplayFeature` is discovered through `GameplayFeatureDiscovery` like other features.

Your feature still needs to register its own client systems in its own `GameplayFeature`.

## Recommended patterns

- Prefer `CoreInputActions` when semantics already fit.
- Add feature-local action constants only for genuinely feature-specific actions.
- Keep input translation in client systems, not in domain definitions.
- Send replicated gameplay commands after interpreting input, not from the input layer itself.
- Treat `ClientInputState` as client UX state, not replicated core state.

## Avoid

- Reading `Keyboard.current` or `Mouse.current` in gameplay systems.
- Creating feature-specific static input providers like old `BuildingPlacementInput`.
- Mixing UI button callbacks into the input feature. UI commands can stay in their own bridge layer.
- Putting feature-specific actions into `CoreInputActions`.
- Using raw strings in systems instead of `InputActionName` constants.
- Hiding broken configuration behind `HasResource` guards and silent early returns.

## Current examples

Reference implementations in the repo:

- player movement reads `CoreInputActions.Move`
- placement confirm reads `CoreInputActions.Primary`
- placement cancel reads `CoreInputActions.Cancel` and `CoreInputActions.Secondary`
- building menu toggle reads `BuildingsInputActions.BuildMenuToggle`
- build hold reads `BuildingsInputActions.BuildConstruction`
- debug cube spawn reads `BuiltinInputActions.DebugSpawnCube`
