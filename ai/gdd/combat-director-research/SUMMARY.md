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
