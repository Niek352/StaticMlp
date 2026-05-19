# Task 9 - Integration Tests And Acceptance Validation

## Goal

Validate the full Combat Director pipeline end-to-end and confirm acceptance criteria.

## Dependencies

Requires:

- `task-3.md` through `task-8.md`

## Architecture

Integration tests should exercise server systems together. If the project has a client/server test harness, include client view binding validation as well.

## Code Scope

Add or extend test coverage:

```text
Assets/Tests/Editor/CombatDirector/
```

### Full Pipeline Tests

- Player noise triggers BuildUp.
- Peak creates enemies through the full chain (source selection -> request -> spawn apply).
- Relief prevents immediate second wave (cooldown enforced).
- Spawn cap prevents over-spawn when budget is high.
- Server creates gameplay entity; client creates View only.
- Invalid catalog id fails during validation.

### Determinism Tests

- Phase transitions are deterministic given identical initial state and inputs.
- Budget composition creates expected role counts for low/medium/high budget.

### Replication Boundary Tests

- `DirectorState` replicates reliably to observing clients.
- `EnemyArchetype` replicates with the enemy entity.
- Spawn requests are not replicated directly (only resulting entities are).

## Out Of Scope

- Performance or load tests.
- Multi-cell scaling tests.
- AI behavior correctness tests (owned by AiBots).

## Tests

Cover all original acceptance criteria from `02-combat-director.md`:

- threat budget accumulates correctly
- phase transitions are deterministic
- budget composition creates expected role counts
- spawn cap prevents over-spawn
- invalid catalog id fails
- player noise triggers BuildUp
- Peak creates enemies
- Relief prevents immediate second wave
- server creates gameplay entity, client creates View only

## Acceptance Criteria

- All tests pass (or are verified) in the Unity test workflow.
- Server-authoritative spawn is proven by tests.
- Client View creation is proven by tests or manual verification.
- The director can run a full Calm -> BuildUp -> Peak -> Relief -> Cooldown cycle.
