# StaticMlp Navigation And OpenWorld Combat Director Roadmap

## Milestone: AiNavigation v1

Goal: implement a clean, cache-driven contract between `CombatDirector`, shared navigation infrastructure, existing `AiBots` movement, and future base NPC navigation.

## Phase 01: Architecture And Contracts

Status: Complete (2/2 plans complete, 2026-05-19)

Outcome:

- `StaticMlp.Features.AiNavigation` exists as a separate feature.
- Public contracts are readable by `CombatDirector`, `AiBots`, and base NPC logic without pulling in ProjectDawn/backend implementation.
- Existing local bot movement remains in `AiBots`.

## Phase 02: Runtime Nav Areas

Status: Complete (2/2 plans complete, 2026-05-19)

Outcome:

- Combat cells can request/prewarm nav areas without `CombatDirector` knowing NavMesh implementation.
- Expensive nav work is queued and budgeted outside peak combat.

## Phase 03: Spawn Source Reachability

Status: Complete (2/2 plans complete, 2026-05-19)

Outcome:

- `SpawnSourceSelection` can use cached reachable state only.
- `SpawnRequestBuildSystem` does not search NavMesh directly.
- Reachable candidates and resolved spawn points are explicit navigation-owned cached outputs.

## Phase 04: AI Navigation Modes

Status: Complete (2/2 plans complete, 2026-05-19)

Outcome:

- Far AI can move logically without a loaded chunk.
- When local nav is available, AI resumes local movement through an explicit handoff into the existing bot navigation path.

## Phase 05: Debug And Verification

Status: Complete (1/1 plans complete, 2026-05-19)

Outcome:

- Debug tooling can inspect nav areas, nav versions, reachability, best reachable/resolved source facts, and per-tick budget counters without mutating gameplay state.

## Milestone: AiCombatDirector OpenWorld v1

Goal: refactor the existing `CombatDirector` from wave-first pressure pacing into a conservative open-world attention and encounter director.

Scope limit for this planning pass: phases 06-10 only.

## Phase 06: Combat Director OpenWorld Audit

Status: Complete (1/1 plans complete, 2026-05-20)

Plans:

- `06-01-PLAN.md`: audit current code, docs, tests, and architecture conflicts before implementation.

Outcome:

- Existing runtime files and tests are mapped to the open-world target.
- Architectural conflicts are recorded before code changes.
- Phase 07 can replace threat budget from a known baseline.

## Phase 07: Cell Attention

Status: Complete (1/1 plans complete, 2026-05-20)

Plans:

- `07-01-PLAN.md`: introduce reason-based `CellAttention` and replace passive threat accumulation.

Outcome:

- Passive exploration does not accumulate dangerous pressure.
- Noise, combat, loot, trespass, and alarms become explicit attention inputs.
- Attention decays back toward calm after causes stop.

## Phase 08: OpenWorld Phase And Encounters

Status: Planned

Plans:

- `08-01-PLAN.md`: add open-world director phases and encounter state contracts.

Outcome:

- `Ambient`, `Contact`, `Suspicion`, `Escalation`, `PressureEvent`, `Recovery`, and `Cooldown` replace the old wave-first phase semantics.
- Solo/small encounters can exist and resolve without becoming waves.

## Phase 09: Spawn Source Classification

Status: Planned

Plans:

- `09-01-PLAN.md`: classify spawn sources by world role and allowed usage.

Outcome:

- Ambient, escalation, and pressure-event sources are explicit.
- Spawn request construction can reason about source kind without hardcoding every placement as a wave source.

## Phase 10: Ambient Layer

Status: Planned

Plans:

- `10-01-PLAN.md`: add ambient solo/small-group encounter spawning with cooldowns and caps.

Outcome:

- Calm exploration can produce believable solo/small-group encounters.
- Killing an ambient AI does not automatically start a wave.
- Ambient respawn is capped and cooldown-driven.
