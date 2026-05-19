# Task 5 - Spawn Source Selection And Spawn Request Build

## Goal

Select active spawn sources and build concrete spawn requests from the current threat budget.

This task bridges the director phase machine to actual enemy composition.

## Dependencies

Requires:

- `task-1.md`
- `task-2.md`
- `task-4.md`

## Architecture

Server systems only. These systems run after the phase machine has decided that spawn pressure is appropriate.

`SpawnSourceSelectionSystem` picks a valid source.
`SpawnRequestBuildSystem` composes a wave definition into `SpawnRequest` components.

## Code Scope

Add systems:

```text
Runtime/Logic/Systems/SpawnSourceSelectionSystem.cs
Runtime/Logic/Systems/SpawnRequestBuildSystem.cs
```

### SpawnSourceSelectionSystem

Select a source that satisfies:

- `minDistance <= distance(cellCenter, source) <= maxDistance`
- source is active (`SpawnSource.IsActive == true`)
- source is not inside a base/safe zone (if safe-zone data is available)
- prefer behind or to the side relative to player forward (MVP heuristic)

MVP scope does not include an expensive visibility service. Use distance and directional heuristics only.

If no valid source exists, the system should not block; the phase machine may stay in BuildUp or fall back.

### SpawnRequestBuildSystem

MVP composition based on budget level:

- low budget: 6–10 swarmers
- medium budget: 8–14 swarmers + 1 marker
- high budget: 10–18 swarmers + 1 marker + 1 anchor

Use `EnemySpawnCatalog` to resolve role costs and validate against remaining budget.

Create `SpawnRequest` components containing:

```text
int CellId
EnemyRole Role
int Count
float3 SpawnPosition (derived from selected source)
```

Respect `EncounterDirectorConfig.MaxAliveEnemiesPerCell` as a cap.

## Validation Scope

- Selected source must be active and within valid distance.
- Spawn count must not exceed the alive enemy cap.
- Budget cost must not exceed `ThreatBudget.Current`.

## Out Of Scope

- Creating authoritative enemy entities (task-6).
- Client telegraph/VFX (task-7).
- Visibility raycasts or occlusion checks.
- Biome-specific source rules.

## Tests

Cover:

- low/medium/high budget creates expected role counts
- inactive source is not selected
- source too close or too far is rejected
- spawn cap prevents over-spawn
- invalid catalog id fails

## Acceptance Criteria

- Spawn source selection respects distance and activity rules.
- Wave composition matches budget level.
- Alive enemy cap is enforced before spawning.
- Spawn requests are pure ECS components, not raw network packets.
