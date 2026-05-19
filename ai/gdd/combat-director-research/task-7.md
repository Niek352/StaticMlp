# Task 7 - Client Presentation And View Systems

## Goal

Bind replicated enemy state to client views and handle spawn telegraphs.

This task is client-only. It must not mutate gameplay state.

## Dependencies

Requires:

- `task-1.md`
- `task-6.md`

## Architecture

Client systems consume replicated state and create/update/destroy visual representations.

`DirectorTelegraphReceiveSystem` handles incoming spawn warnings.
`SpawnSourceVfxSystem` plays source effects.
`EnemyViewBindSystem` binds replicated enemy entities to views.
`EnemySpawnAudioSystem` plays spawn stingers.

Do not put gameplay rules, damage calculation, or server authority logic into these systems.

## Code Scope

Add client systems:

```text
Runtime/Presentation/Systems/DirectorTelegraphReceiveSystem.cs
Runtime/Presentation/Systems/SpawnSourceVfxSystem.cs
Runtime/Presentation/Systems/EnemyViewBindSystem.cs
Runtime/Presentation/Systems/EnemySpawnAudioSystem.cs
```

### DirectorTelegraphReceiveSystem

If the server replicates a telegraph event before spawn (for example a `SpawnTelegraphEvent`), this system receives it and prepares client-side warning state.

If the MVP does not include a replicated telegraph event, this system can read `SpawnSource` state changes and infer a warning window.

### SpawnSourceVfxSystem

On detected spawn source activation or telegraph:

- trigger dust/portal/burrow animation at `SpawnSource.Position`
- respect source type (Burrow vs Rift) for visual variation

### EnemyViewBindSystem

When a replicated enemy entity appears on the client:

- create or bind a View through the existing project view-sync pattern (for example `EntityView` or `ClientProjection`).
- use `EnemyArchetype.Role` to select visual variation if the project supports it.
- clean up view on entity deletion.

### EnemySpawnAudioSystem

Play sound stingers for:

- source activation
- enemy entity appearance
- phase transitions (Peak entry)

Keep audio state client-only.

## Out Of Scope

- Server spawn logic.
- Damage or combat state mutation.
- UI panels or HUD widgets (unless the project already has a debug/danger meter system).
- AI behavior changes.

## Tests

Cover if the project has client-side test infrastructure:

- client creates a view when a replicated enemy entity appears
- view cleanup happens on entity deletion
- telegraph triggers VFX state change

If no client test harness exists, mark tests as manual/visual verification.

## Acceptance Criteria

- Client visuals spawn for enemies and sources.
- Views are created only on the client.
- No gameplay state is stored in MonoBehaviours.
- Presentation systems do not write replicated gameplay components.
