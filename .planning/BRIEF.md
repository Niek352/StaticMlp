# AiNavigation And OpenWorld Combat Director Brief

## Objective

Build `AiNavigation` as the shared server-authoritative navigation infrastructure, then refactor `CombatDirector` from a wave-first encounter pressure feature into an open-world attention and encounter director.

The resulting director must support Bellwright/Palworld-like exploration:

- calm travel is the default state;
- ordinary world encounters are solo AI or small groups from world placements;
- large pressure events require explicit world reasons;
- spawn decisions stay server-authoritative and ECS-first;
- client-only code remains limited to View/VFX/UI/debug presentation;
- `AiNavigation` answers reachability and movement questions without choosing combat drama.

## Source Context

- `ai/gdd/AiCombatDirector_OpenWorld_Plan.md`
- `ai/combat_director.md`
- `ai/CombatDirector_AiNavigation_Contract.md`
- `Assets/Scripts/StaticMlp/Features/CombatDirector/AGENTS.md`
- `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/Navigation/AiNavigation.md`
- `Assets/Scripts/StaticMlp/Features/AiBots/AGENTS.md`
- `ai/ECS_Feature_Architecture_Layout_StaticEcs.md`

## Architecture Decision

Keep the existing `StaticMlp.Features.CombatDirector` feature and refactor it in place. Do not create a parallel `AiCombatDirector` feature unless a later audit proves the existing feature boundary is unsalvageable.

The current implementation owns `CombatCell`, `ThreatBudget`, `DirectorState`, `SpawnSource`, spawn request construction, enemy creation, presentation state, and tests. The open-world work should replace the always-growing threat model incrementally while preserving the server-authoritative spawn/apply boundary and the `Runtime/Logic` versus `Runtime/Presentation` split.

## Non-Goals

- Do not create endless random waves during exploration.
- Do not keep a global threat value that grows only because a player exists.
- Do not spawn enemies directly near the player without a source and reason.
- Do not move NavMesh, ProjectDawn, or `UnityEngine.AI` details into `CombatDirector`.
- Do not let `AiNavigation` mutate director phase, attention, encounter state, enemy composition, loot, or progression.
- Do not use GameObject/MonoBehaviour state for gameplay truth.
- Do not create prefab assets or scene wiring from planning or code agents.
- Do not run `dotnet build`; ask the user to run Unity compile/play checks when needed.

## Current State

AiNavigation v1 planning is complete through phases 01-05.

`CombatDirector` exists and currently follows a wave-first model:

```text
SpawnSourcePlacementSeedSystem
CombatCellTrackingSystem
PlayerThreatInputSystem
ThreatBudgetAccumulationSystem
DirectorPhaseSystem
SpawnSourceSelectionSystem
SpawnRequestBuildSystem
SpawnRequestValidationSystem
EnemySpawnApplySystem
```

The next milestone is limited to phases 06-10:

```text
Audit current CombatDirector
Replace ThreatBudget with CellAttention
Add open-world phase and encounter contracts
Classify spawn sources for ambient/escalation/pressure use
Add the ambient solo/small-group layer
```
