# Task 1 - Combat Director Contracts And Feature Boundary

## Goal

Create the general Combat Director feature boundary that future encounter pressure, spawn, and enemy lifecycle work can depend on.

This task is foundation-only. It must not implement cell tracking, budget accumulation, phase transitions, spawn logic, AI behavior changes, or presentation.

## Current Context

The project already has:

- `StaticMlp.Features.AiBots` for generic AI agents/tasks.
- Combat/Health components likely exist in another feature (for example `StaticMlp.Features.Combat`).

The Combat Director design requires:

- A runtime combat cell around player groups.
- Threat budget and phase state.
- Spawn sources (Burrow, Rift).
- Enemy roles (Swarmer, Marker, Anchor Elite).
- Spawn requests that later become dynamic ECS enemy entities.

## Architecture

Add a new feature:

```text
Assets/Scripts/StaticMlp/Features/CombatDirector/
  Runtime/
    Contracts/
    Logic/
```

Use split asmdefs:

- `StaticMlp.Features.CombatDirector.Contracts`
- `StaticMlp.Features.CombatDirector.Logic`

Keep namespace:

```csharp
namespace StaticMlp.Features.CombatDirector
```

`CombatDirector.Logic` may reference:

- `StaticMlp.Features.CombatDirector.Contracts`
- `StaticMlp.Features.AiBots`
- `StaticMlp.Features.Combat` or its `.Contracts` assembly for public health/combat state
- `Game.Core`
- `Ecs.Networking`
- `FFS.StaticEcs`
- `FFS.StaticPack`

Do not make `AiBots` depend on `CombatDirector`.

## Code Scope

Create contract/domain types:

- `DirectorPhase : byte`
- `EnemyRole : byte`
- `SpawnSourceType : byte`
- `EnemyTag : ITag`
- `CombatCell : IComponent`
- `ThreatBudget : IComponent`
- `DirectorState : IComponent`
- `PlayerNoise : IComponent`
- `CarriedLootValue : IComponent`
- `SpawnSource : IComponent`
- `EnemyArchetype : IComponent`
- `SpawnRequest : IComponent`

`DirectorState` should be server-authoritative replicated state if clients need to observe phase for UI or audio:

- `[ReplicatedComponent(authority: ReplicationAuthority.Server, delivery: NetDelivery.ReliableSequenced, ...)]`
- stable GUID through `IComponentConfig<DirectorState>`
- `ITrackableAdded`, `ITrackableChanged`, `ITrackableDeleted`
- replicated fields should stay primitive/enums supported by codegen

Suggested fields for `DirectorState`:

```text
DirectorPhase Phase
float PhaseTimer
float TimeSinceLastPeak
```

`SpawnSource` should be replicated if clients need to play telegraph VFX at the source position:

- stable GUID and trackable traits if replicated
- fields: `SpawnSourceType Type`, `float3 Position`, `float Radius`, `bool IsActive`

`EnemyArchetype` should be replicated because it travels with the enemy entity:

- fields: `EnemyRole Role`

`SpawnRequest` should remain a server-only event component or internal ECS event. Do not replicate spawn requests directly; replicate the resulting enemy entities.

Do not include view paths, prefab paths, network archetype ids, AI behavior ids, health values, or damage data in the core Combat Director contracts.

Create `CombatDirectorGameplayFeature : GameplayFeature` in `Runtime/Logic`.

For this task it should only register components/tags and replication/projection if the current project pattern requires it. No systems are required yet unless registration cannot happen otherwise.

## Out Of Scope

- Systems (cell tracking, budget, phases, spawn).
- Configs and catalogs.
- Client presentation/VFX/UI.
- AI behavior changes.
- Damage or health component mutation.

## Tests

Add editor tests covering pure contract behavior if useful:

- enum values match Design Lock numeric ordering
- component sizes are reasonable for replication

Do not manually edit generated replication files. The code-writing agent should ask for Unity replication codegen/compile verification after adding replicated components.

## Acceptance Criteria

- New Combat Director feature folders and asmdefs follow project layout.
- Contracts compile in isolation from AI behavior and combat damage logic.
- `DirectorState` is suitable for replicated durable director state.
- No existing AI behavior changes in this task.
- No MonoBehaviour, prefab, UI, or generated code changes.
