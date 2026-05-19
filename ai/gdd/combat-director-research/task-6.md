# Task 6 - Enemy Spawn Apply And Authoritative Entity Creation

## Goal

Convert validated spawn requests into authoritative server ECS enemy entities.

This is the server-only spawn execution step.

## Dependencies

Requires:

- `task-1.md`
- `task-2.md`
- `task-5.md`

## Architecture

Server-authoritative entity creation. Use `NetEntityFactory` if the project requires networked enemy entities.

Do not create Unity Views on the server. Do not use `GameObject` as gameplay state.

If enemy health/combat state components belong to another feature (for example `StaticMlp.Features.Combat`), the Combat Director should not mutate foreign state directly. Either:

- add minimal NPC-owned state here and let the owning feature apply health/combat components through events, or
- confirm that the owning feature exposes a factory/spawner contract that Combat Director may call.

For the MVP, the minimal acceptable path is to create the entity and attach `EnemyTag`, `EnemyArchetype`, and `NetworkIdentity` inside Combat Director, then emit an event so the combat feature can attach its owned components.

## Code Scope

Add systems:

```text
Runtime/Logic/Systems/SpawnRequestValidationSystem.cs
Runtime/Logic/Systems/EnemySpawnApplySystem.cs
```

### SpawnRequestValidationSystem

Validate each `SpawnRequest` before spawn:

- target cell still exists and is active
- selected source is still valid
- role exists in `EnemySpawnCatalog`
- spawn would not exceed `MaxAliveEnemiesPerCell`
- budget is sufficient

Delete or mark invalid `SpawnRequest` components. Do not silently ignore them.

### EnemySpawnApplySystem

On valid requests:

- create ECS entities through `NetEntityFactory` (or the project spawn pattern).
- assign `NetworkIdentity` / GID if the project uses cross-client ids.
- add `EnemyTag`.
- add `EnemyArchetype` with the requested `EnemyRole`.
- add `SpawnSourceType` reference if the source type matters for AI behavior.
- subtract consumed budget from `ThreatBudget.Current`.
- emit an internal ECS event such as `EnemySpawnedEvent` containing the new `EntityGID` and `EnemyRole` so other features can react.

Do not attach ViewPath or MonoBehaviour references here.

If the project has an `AiBotFactory` or similar, Combat Director may call it as a factory dependency, but must not inline AI logic.

## Events

Add internal ECS event:

```text
EnemySpawnedEvent
```

Suggested fields:

```text
EntityGID SpawnedEntity
EnemyRole Role
SpawnSourceType SourceType
```

This is a normal ECS event inside the feature, not a raw transport packet.

## Out Of Scope

- Client view binding (task-7).
- AI behavior initialization (owned by AiBots).
- Health/damage component attachment unless explicitly owned by Combat Director.
- Presentation/VFX.

## Tests

Cover:

- valid spawn request creates gameplay entity
- created entity has `EnemyTag` and `EnemyArchetype`
- invalid request is rejected and does not create entity
- spawn cap prevents entity creation
- budget is consumed after spawn
- `EntityGID` is used, not raw `ulong`

## Acceptance Criteria

- Server creates authoritative ECS enemy entities.
- No View or GameObject state is created on the server.
- Spawn respects the alive enemy cap.
- Spawn consumes threat budget.
- Other features can observe spawned entities through the internal event.
