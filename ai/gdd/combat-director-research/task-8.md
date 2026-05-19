# Task 8 - Debug Overlay And Director Monitoring

## Goal

Add debug visualization for director state so designers and developers can observe encounter pressure in play mode.

## Dependencies

Requires:

- `task-3.md`
- `task-4.md`

## Architecture

Debug overlay is client-only or editor-only. It must not affect server gameplay.

If the project has an existing debug overlay system, hook into it. Otherwise, add a minimal `CombatDirectorDebugSystem` that draws gizmos or writes to a debug UI canvas.

## Code Scope

Add:

```text
Runtime/Presentation/Systems/CombatDirectorDebugSystem.cs
```

Visualize:

- combat cell radius (wire sphere or circle at cell center)
- current threat budget (numeric readout or bar)
- current director phase (label)
- alive enemy count in cell
- active spawn sources (markers)

If the project uses `UnityEngine.Gizmos` or `Debug.DrawLine`, keep the system behind compilation conditionals or runtime flags so it does not run in release builds.

If the project uses a UI Toolkit or IMGUI debug panel, add a passive read-only binding to `CombatCell`, `ThreatBudget`, and `DirectorState`.

## Out Of Scope

- Production UI or HUD.
- Persistent analytics or telemetry.
- Multiplayer-synced debug state.

## Tests

No automated tests required for debug visualization unless the project has screenshot or gizmo tests.

Verify manually:

- cell radius draws at the correct position
- budget value updates in real time
- phase label changes with state machine

## Acceptance Criteria

- Debug state is observable in play mode.
- Debug code does not run server gameplay systems.
- No performance impact when disabled.
