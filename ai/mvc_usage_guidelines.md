# MVC Usage Guidelines

Target package:

```text
Assets/Scripts/StaticMlp/Features/Mvc
```

## Package Role

Treat this MVC package as a client UX adapter over StaticEcs.

Use it for:

- opening and closing windows;
- focus and stack rules;
- translating Unity UI callbacks into ECS intent;
- applying prepared ECS presentation state to views.

Do not use it for:

- gameplay rules;
- domain validation;
- authoritative state transitions;
- hidden feature orchestration outside ECS.

## Recommended Data Flow

```text
View callback
    -> Controller
    -> CW.SendEvent(...) or client UX resource update
    -> ordinary ECS systems
    -> presentation/read-model state
    -> MVC presentation sync
    -> View
```

## Controller Rules

Controllers should:

- own window lifecycle;
- translate UI actions into ECS intent;
- keep references to view-only concerns;
- avoid gameplay and domain decisions.

If a controller uses `ViewLayer.Persistent`, it must declare a unique `PersistentSortOrder`.

The package now fails fast when:

- a persistent controller does not declare `PersistentSortOrder`;
- two persistent controllers reuse the same order.

## ECS Bridge Rules

`ControllerEcsBridgeSystem<TController>` is presentation-only.

Use it to:

- read prepared ECS presentation state;
- apply that state to the bound controller or view;
- perform sync only while the screen is active.

Do not use it to:

- decide gameplay outcomes;
- validate gameplay commands;
- replace normal ECS systems that should react to `IEvent` or component/resource state.

## Good Example

```text
Build button clicked
    -> BuildingMenuController sends BuildRequested event
    -> ECS systems validate and update build presentation state
    -> active presentation sync reads the state
    -> view refreshes button state, labels, and progress
```

## Persistent Layer Guidance

Persistent views are not stack-driven like popups.

They should have explicit, stable responsibilities and explicit sorting orders, for example:

```text
HUD               0
Context panels   100
Notifications    200
Debug UI         900
```

Keep those values near the feature that owns the screen so the ordering stays readable.
