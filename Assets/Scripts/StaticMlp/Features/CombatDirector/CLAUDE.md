# Combat Director Agent Guide

Read this before changing `StaticMlp.Features.CombatDirector`.

Combat Director owns encounter pressure, combat cells, threat budget, director phase, spawn source selection, spawn request construction, and the Combat Director metadata attached to spawned enemies.

It does not own AI behavior, combat damage, health rules, navigation backend implementation, transport, prefab authoring, or UI composition.

## Read First

- `ai/combat_director.md`
- `ai/CombatDirector_AiNavigation_Contract.md`
- `Runtime/Logic/CombatDirectorGameplayFeature.cs`
- `Runtime/Contracts/Components/*`
- `Runtime/Logic/WorldResources/EncounterDirectorConfig.cs`
- `Runtime/Logic/Catalogs/EnemySpawnCatalog.cs`
- `Runtime/Logic/Systems/*`
- `Runtime/Presentation/CombatDirectorPresentationFeature.cs` when changing client visuals/audio

## Runtime Shape

- `Contracts` contains public components and enums that other features may read.
- `Logic` contains server-authoritative state transitions, validation, and enemy creation.
- `Presentation` contains client-only visual/audio/view state.
- `EncounterDirectorConfig` and `EnemySpawnCatalog` are server world resources.
- `SpawnSourcePlacementSeedSystem` creates `SpawnSource` entities from `OpenWorldChunkGenerationCompleted.SpawnPlacements`.
- Spawned enemies are created through `AiBotFactory`; do not create a second Combat Director network entity factory for AI bots.

## Current Server Flow

Preserve this ordering unless the phase architecture changes explicitly:

```text
SpawnSourcePlacementSeedSystem
CombatCellTrackingSystem
PlayerThreatInputSystem
CellAttentionInputSystem
CellAttentionDecaySystem
DirectorPhaseSystem
SpawnSourceSelectionSystem
SpawnRequestBuildSystem
SpawnRequestValidationSystem
EnemySpawnApplySystem
```

Key dependencies:

- `SpawnSourcePlacementSeedSystem` must run before source selection so generated open-world spawn placements are available as active `SpawnSource` entities.
- `CombatCellTrackingSystem` must run before budget, phase, and spawn systems because it owns the director cell entity.
- `CellAttentionInputSystem` and `CellAttentionDecaySystem` must run before phase selection. `ThreatBudget` is temporary compatibility state until phase/spawn logic migrates fully to `CellAttention`.
- `SpawnSourceSelectionSystem` must run before request building.
- `SpawnRequestValidationSystem` must run before apply.
- `EnemySpawnApplySystem` is the only system in this feature that creates enemy entities.
- `EnemySpawnApplySystem` must pass the current combat-cell player target through `AiBotSpawnSpec`; otherwise enemies spawned outside normal AI perception range can remain idle.

## Spawn Source Reality Check

The director seeds production `SpawnSource` entities from open-world `SpawnPlacement` facts.

If no active `SpawnSource` exists near the combat cell, enemy spawning is expected to produce no enemies. Do not hide that with fallback spawn positions. Check whether the server open-world generation pipeline emitted `OpenWorldChunkGenerationCompleted` with non-empty `SpawnPlacements`.

`SpawnSourcePlacementRules` maps open-world placement kinds to director source types. Unknown placement kinds are integration errors; do not silently remap them to a default source type.

Valid source selection currently requires:

- `SpawnSource.IsActive == true`;
- finite `SpawnSource.Position`;
- distance inside `EncounterDirectorConfig.MinSpawnSourceDistance..MaxSpawnSourceDistance`;
- directional scoring from the active player group.

`AiNavigation` owns reachability state such as `SpawnSourceNavState`. If selection starts requiring reachable sources, read that public contract; do not move navigation logic into Combat Director.

## Boundaries

- Other features may read Combat Director contracts, but must not mutate Combat Director-owned state directly.
- Combat Director may read player position, attack/noise inputs, carried resources, and alive enemy state.
- Combat Director must not mutate foreign feature state such as health, damage, inventory, AI decisions, or navigation backend state.
- Cross-feature writes must use typed events or an explicit factory boundary owned by the target feature.
- Keep `SpawnRequest` server-only. Do not replicate raw spawn requests to clients.
- Replicate only stable presentation metadata that clients need, such as `DirectorState`, `EnemyArchetype`, and `EnemySpawnSource`.
- Do not add view paths, prefab paths, audio clips, or Unity object references to `Contracts` or `Logic`.

## Fail Fast

- Missing director cell after initialization is an architecture bug.
- Multiple director cells are currently unsupported and should throw.
- Invalid config values should throw before simulation continues.
- Invalid spawn requests from normal gameplay conditions should be rejected by destroying the request before it is marked valid.
- Do not add fallback spawn locations, fallback roles, or fallback catalogs.

## Presentation Rules

- Client presentation reads replicated state and writes client-only view state.
- `Runtime/Presentation` must not call `SW`, mutate replicated gameplay components, or create server gameplay entities.
- `EnemyViewBindSystem` should fail fast when a replicated enemy lacks the client recipe state needed by the existing EcsViews pipeline.
- Prefabs and inspector wiring are human-authored in Unity. Do not create prefab assets or YAML.

## Common Debug Path

When enemies do not spawn, check in this order:

1. Did the server receive `OpenWorldChunkGenerationCompleted` with non-empty `SpawnPlacements`?
2. Does `SpawnSourcePlacementSeedSystem` create active `SpawnSource` entities?
3. Does the server have a player with `PlayerTag` and `CharacterNetState`?
4. Does `CombatCellTrackingSystem` create exactly one entity with `CombatCell`, `CellAttention`, temporary `ThreatBudget`, and `DirectorState`?
5. Does `CellAttention.Current` explain the temporary `ThreatBudget.Current` value?
6. Is there an active `SpawnSource` at a valid distance from the cell center?
7. Does the director entity get `SelectedSpawnSource`?
8. Does `SpawnRequestBuildSystem` create `SpawnRequest`?
9. Does validation add `ValidSpawnRequestTag` or destroy the request?
10. Does `EnemySpawnApplySystem` create entities through `AiBotFactory`?
11. Do spawned entities have `EnemyTag`, `EnemyArchetype`, `EnemySpawnSource`, `NetworkIdentity`, and `CharacterNetState`?
12. Do spawned enemies have `AiCoreVariableIds.Enemy` assigned to a player in the combat cell?

If step 6 fails, the current implementation is behaving as designed: no source means no spawn.
