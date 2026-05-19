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

# Task 5 Summary - Spawn Source Selection And Spawn Request Build

## Completed

- Added server-side spawn-source selection using active-state, configured min/max distance, and a behind/side directional heuristic from current player forward vectors.
- Added selected-source ECS state so request building can consume the exact source identity through `EntityGID`.
- Extended `SpawnRequest` with source identity and type for later validation.
- Added spawn request composition for low, medium, and high budgets using `EnemySpawnCatalog` costs and `EncounterDirectorConfig.MaxAliveEnemiesPerCell`.
- Registered the new selection/build systems in `CombatDirectorGameplayFeature`.
- Added editor tests for inactive/distance rejection, budget-level composition, and cap-limited request generation.

## Architecture Deviation

- Added `MinSpawnSourceDistance` and `MaxSpawnSourceDistance` to `EncounterDirectorConfig`.

Reason:

- The task requires min/max source-distance validation, but the previous config did not expose those values.
- Keeping them in the existing authoritative director config avoids hardcoded system-local tuning.

## Intentional Scope Limit

- Safe-zone rejection is not implemented because no stable safe-zone contract is available to Combat Director yet.
- Visibility/occlusion remains out of scope as requested.

## Verification

- Ran `git diff --check` on the touched Combat Director runtime and test paths.
- Added focused editor coverage in `Assets/Tests/Editor/CombatDirector`.
- Unity compile and editor test execution still require manual validation in-editor.

## Manual Follow-Up Required

- Run Unity compile checks.
- Run the Combat Director editor tests in Unity.

# Task 6 Summary - Enemy Spawn Apply And Authoritative Entity Creation

## Completed

- Added spawn-request validation for active target phase, source liveness, source distance, catalog role existence, alive cap, and available threat budget.
- Added valid-request tagging so the apply system only consumes explicitly validated requests.
- Added enemy spawn application through the existing `AiBotFactory` networked entity factory instead of hand-writing `NetworkIdentity`.
- Added Combat Director-owned spawned enemy metadata:
  - `EnemyTag`
  - `EnemyArchetype`
  - `EnemySpawnSource`
- Added internal `EnemySpawnedEvent` with `EntityGID`, `EnemyRole`, and `SpawnSourceType`.
- Registered validation/apply systems in the server pipeline.
- Added editor tests for invalid-role rejection and valid spawn application with budget consumption and event emission.

## Architecture Deviation

- Used `AiBotFactory` as the authoritative networked entity creation boundary rather than introducing a second Combat Director `NetEntityFactory`.

Reason:

- The project already owns AI bot network entity creation and combat/AI initialization through `AiBots`.
- Combat Director should choose encounter composition, not duplicate AI behavior or combat component setup.

## Intentional Scope Limit

- Role-specific AI behavior, health scaling, and presentation/view binding remain out of scope.
- The spawned-event is an internal ECS event only; no raw transport packets or replicated events were added.

## Verification

- Ran `git diff --check` on the touched Combat Director runtime and test paths.
- Added focused editor coverage in `Assets/Tests/Editor/CombatDirector`.
- Unity compile and editor test execution still require manual validation in-editor.

## Manual Follow-Up Required

- Run Unity compile checks.
- Run the Combat Director editor tests in Unity.

# Task 4 Summary - Threat Budget Accumulation And Phase Machine

## Completed

- Added `ThreatBudgetAccumulationSystem` to sum in-cell `PlayerNoise` and `CarriedLootValue`, convert them through `EncounterDirectorConfig`, and accumulate clamped threat budget on the authoritative Combat Director cell entity.
- Added `DirectorPhaseSystem` to drive deterministic `Calm -> BuildUp -> Peak -> Relief -> Cooldown` transitions from budget, timers, active spawn-source availability, and alive-enemy counts.
- Extended `CombatCellTrackingSystem` so the single authoritative combat-cell entity also owns initialized `ThreatBudget` and `DirectorState`.
- Registered the new systems in `CombatDirectorGameplayFeature`.
- Added editor runtime-system tests for:
  - budget accumulation from noise and loot
  - budget clamping
  - threshold-based phase transitions
  - relief/cooldown timing enforcement
  - deterministic identical-input results

## Architecture Deviation

- `Peak -> Relief` currently uses the alive-enemy condition only; task 4 does not yet introduce a separate "wave spawned" signal.
- `BuildUp -> Peak` currently requires at least one active `SpawnSource`, not task-5 distance/directional scoring.

Reason:

- Adding a speculative pre-task-5 spawn marker or source-selection contract here would create unstable cross-task state ownership.
- The current implementation stays within the existing Combat Director contracts and leaves concrete spawn-source validation/build decisions to task 5.

## Intentional Scope Limit

- Threat budget max initializes from the configured phase thresholds because task 2 does not yet expose a separate budget-cap field.
- Relief currently preserves budget; it does not add design-specific decay until that behavior is explicitly defined.

## Verification

- Added focused editor runtime-system tests in `Assets/Tests/Editor/CombatDirector`.
- Kept verification to repository-safe static checks; Unity compile and test execution still require manual validation in-editor.

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
