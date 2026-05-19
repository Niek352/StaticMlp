# Task 4 - Threat Budget Accumulation And Phase Machine

## Goal

Implement threat budget accumulation and deterministic director phase transitions.

This is the core decision engine of the Combat Director.

## Dependencies

Requires:

- `task-1.md`
- `task-2.md`
- `task-3.md`

## Architecture

Server systems only. The phase machine is authoritative.

`ThreatBudgetAccumulationSystem` reads threat inputs and config, then updates `ThreatBudget.Current`.
`DirectorPhaseSystem` evaluates thresholds and transitions `DirectorState.Phase`.

## Code Scope

Add systems:

```text
Runtime/Logic/Systems/ThreatBudgetAccumulationSystem.cs
Runtime/Logic/Systems/DirectorPhaseSystem.cs
```

### ThreatBudgetAccumulationSystem

```text
threatDelta =
    BaseThreatPerSecond
  + noise * NoiseThreatMultiplier
  + carriedLootValue * LootThreatMultiplier
  + activeObjectiveBonus (if applicable)
```

- Read `PlayerNoise` and `CarriedLootValue` from players in the cell.
- Accumulate into `ThreatBudget.Current`.
- Clamp `Current` to `ThreatBudget.Max`.
- Do not reset budget on phase transition here; let the phase system decide.

### DirectorPhaseSystem

Transition rules:

```text
Calm -> BuildUp, if Current > BuildUpThreshold
BuildUp -> Peak, if Current > PeakThreshold and valid spawn source exists
Peak -> Relief, if wave spawned or alive enemies below threshold after peak
Relief -> Cooldown, if relief timer >= MinReliefSeconds
Cooldown -> Calm, if cooldown timer >= MinCooldownSeconds
```

- Update `DirectorState.PhaseTimer` and `DirectorState.TimeSinceLastPeak`.
- On entering Peak, the phase system should not itself spawn; it sets the phase so that spawn systems (task-5) can react.
- On entering Relief, optionally decay or freeze budget depending on design intent.

## Validation Scope

- Budget must never exceed `Max`.
- Phase transitions must be deterministic given the same inputs.
- `MinReliefSeconds` and `MinCooldownSeconds` must be positive.

## Out Of Scope

- Spawn source selection (task-5).
- Spawn request creation (task-5).
- Entity spawning (task-6).
- Client phase VFX/UI.

## Tests

Cover:

- threat budget accumulates correctly with noise and loot multipliers
- phase transitions happen at configured thresholds
- Relief prevents immediate second peak (cooldown enforced)
- budget clamp prevents overflow
- deterministic transition given identical input state

## Acceptance Criteria

- Threat budget grows with player inputs.
- Director phases transition deterministically.
- Cooldown and relief timers are enforced.
- All state is server-authoritative ECS data.
