# Task 3 - Combat Cell Tracking And Threat Input Systems

## Goal

Implement server-side combat cell tracking and threat input accumulation.

These systems feed the director with player-derived threat data.

## Dependencies

Requires:

- `task-1.md`

## Architecture

These are server-authoritative gameplay systems. They must not spawn enemies, mutate client-only state, or read raw network inboxes.

`CombatCellTrackingSystem` computes the cell center and radius.
`PlayerThreatInputSystem` computes per-player noise and loot contributions.

## Code Scope

Add systems:

```text
Runtime/Logic/Systems/CombatCellTrackingSystem.cs
Runtime/Logic/Systems/PlayerThreatInputSystem.cs
```

### CombatCellTrackingSystem

- If one player is active, cell center = player position.
- If multiple players are nearby, cell center = average position of the active group.
- Cell radius is taken from `EncounterDirectorConfig.CellRadius`.
- For the vertical slice, begin with a single combat cell.

Write the computed center/radius into `CombatCell` on a dedicated director entity or on the cell entity itself.

### PlayerThreatInputSystem

Compute per-player threat inputs:

- noise from attacks (read from combat state or animation events)
- mining/harvesting actions
- carried loot value (read `CarriedLootValue`)
- proximity to enemy placements or spawn sources
- time spent inside the cell

Write computed inputs into `PlayerNoise` and update `CarriedLootValue` if needed. Do not spawn enemies here.

If the project already has a `PlayerTag` or player identity component, query players through the existing project pattern.

## Validation Scope

- Cell center must be finite (no NaN/Infinity).
- Player noise must be non-negative.

## Out Of Scope

- Threat budget accumulation (task-4).
- Phase transitions (task-4).
- Spawn logic.
- Client presentation.
- Multi-cell support.

## Tests

Cover:

- single player cell center equals player position
- multiple player cell center equals average position
- player noise accumulates from mocked attack/harvest inputs
- carried loot value is read correctly

## Acceptance Criteria

- Cell tracks the active player group.
- Threat inputs are calculated and stored in ECS components.
- No gameplay state is stored in MonoBehaviours.
