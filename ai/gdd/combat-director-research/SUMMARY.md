# Task 1 Summary - Combat Director Contracts And Feature Boundary

## Completed

- Added `StaticMlp.Features.CombatDirector.Contracts` and `StaticMlp.Features.CombatDirector.Logic`.
- Added public Combat Director contract enums/components:
  - `DirectorPhase`
  - `EnemyRole`
  - `SpawnSourceType`
  - `EnemyTag`
  - `CombatCell`
  - `ThreatBudget`
  - `DirectorState`
  - `PlayerNoise`
  - `CarriedLootValue`
  - `SpawnSource`
  - `EnemyArchetype`
- Added server-only `SpawnRequest` in `Runtime/Logic/Events`.
- Added `CombatDirectorGameplayFeature` with projection registration for replicated director state.
- Added editor tests for enum ordering, replicated GUID stability, and compact contract footprint.

## Architecture Deviation

- Re-homed legacy `CombatCell`, `SpawnSource`, and `SpawnSourceType` ownership from `StaticMlp.Features.Frontier` into `StaticMlp.Features.CombatDirector`.
- Updated `AiNavigation` asmdefs and source references to read those contracts from `CombatDirector`.

Reason:

- The local design docs already assign `CombatDirector` ownership of combat cells and spawn sources.
- Leaving the same concepts in `Frontier` and adding duplicate `CombatDirector` contracts would split ownership of the same gameplay boundary and make future tasks unstable.

## Intentional Scope Limit

- `SpawnSource` remains a normal ECS component in this task.
- It was not made replicated yet because client telegraph/VFX is explicitly out of scope for task 1, and the current server-side `AiNavigation` usage already depends on the `float3` contract.
- `DirectorState` and `EnemyArchetype` were made replicated because their durable replicated shape is already well-defined and future tasks depend on it.

## Verification

- Added editor tests in `Assets/Tests/Editor/CombatDirector`.
- Ran targeted repository searches to confirm `AiNavigation` now resolves combat-cell and spawn-source contracts through `CombatDirector`.
- Ran `git diff --check` for the repo and found an unrelated pre-existing whitespace issue in `Assets/Editor/StaticEcsViewConfig.asset`.

## Manual Follow-Up Required

- Run Unity compile checks.
- Run `StaticMlp/Replication/Generate` so the new replicated components get generated registration/serializer output.
- Run the editor test suite after Unity recompiles.

# Task 2 Summary - Encounter Director Config And Enemy Spawn Catalog

## Completed

- Added `EncounterDirectorConfig` as a Combat Director world resource with authoritative encounter tuning values for cell size, threat rates, phase timings, and spawn caps.
- Added `EnemySpawnDefinition`, `EnemySpawnCatalog`, and `EnemySpawnCatalogValidator` under `CombatDirector.Runtime.Logic`.
- Registered default Combat Director config and spawn catalog in `CombatDirectorGameplayFeature.RegisterServerResources()`.
- Added editor tests covering the default catalog plus duplicate-role, zero-cost, and inverted min/max validation failures.

## Architecture Deviation

- Placed `EncounterDirectorConfig` in `Runtime/Logic/WorldResources` instead of the task's suggested `Runtime/Logic/Configs`.

Reason:

- Project feature layout requires `IResource` scripts to live in `WorldResources`.
- Introducing a new top-level `Configs` bucket for a single ECS resource would conflict with the repository's enforced layout rules.

## Intentional Scope Limit

- The config and catalog are server resources only in this task.
- They were not mirrored into client-core state because the task calls for authoritative runtime definitions, not client-side prediction or presentation access.
- Runtime spawn logic, prefab/view mapping, and AI behavior binding remain out of scope.

## Verification

- Added focused editor tests in `Assets/Tests/Editor/CombatDirector`.
- Verified the gameplay feature now seeds the Combat Director config and spawn catalog during server resource registration.
- Manual Unity compile and editor test execution are still required because repository rules prohibit local `dotnet build` verification here.

## Manual Follow-Up Required

- Run Unity compile checks.
- Run the Combat Director editor tests in Unity.

# Task 3 Summary - Combat Cell Tracking And Threat Input Systems

## Completed

- Added `CombatCellTrackingSystem` to own the vertical-slice single combat cell and keep its center/radius updated from current server player positions.
- Added `PlayerThreatInputSystem` to seed Combat Director player state, derive per-frame attack/harvest noise, map carried loot from `ResourcesInventory`, and add spawn-source proximity plus time-in-cell pressure.
- Registered both systems from `CombatDirectorGameplayFeature`.
- Added editor tests covering:
  - single-player cell center
  - multi-player averaged cell center
  - attack plus harvest noise accumulation
  - carried loot projection from inventory

## Architecture Deviation

- The single-cell active group selects the largest connected player cluster using `2 * CellRadius` adjacency before averaging positions.
- Proximity threat currently reads active `SpawnSource` entities only.

Reason:

- The task explicitly constrains the vertical slice to one combat cell, but a naive average of all players would produce unstable centers when groups split.
- The current feature boundary exposes spawn sources, while "enemy placements" do not yet exist as a stable public contract owned by Combat Director or another feature.

## Intentional Scope Limit

- Attack noise is derived from authoritative `ServerCombatAttackState.LastAcceptedShotSequence` rather than transient combat request entities.
- Harvest noise is derived from the typed `TryHarvestOpenWorldResourceCommand` event stream.
- `CarriedLootValue` currently maps raw `ResourcesInventory` totals because the project does not yet expose a richer loot valuation catalog.

## Verification

- Added focused editor runtime-system tests in `Assets/Tests/Editor/CombatDirector`.
- Verified the Combat Director gameplay feature now registers server systems for cell tracking and threat input accumulation.
- Manual Unity compile and editor test execution are still required because repository rules prohibit local build verification here.

## Manual Follow-Up Required

- Run Unity compile checks.
- Run the Combat Director editor tests in Unity.
