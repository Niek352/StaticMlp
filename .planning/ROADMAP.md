# AiNavigation Roadmap

## Milestone: AiNavigation v1

Goal: implement a clean, cache-driven contract between `CombatDirector`, shared navigation infrastructure, existing `AiBots` movement, and future base NPC navigation.

## Phase 01: Architecture And Contracts

Status: Complete (2/2 plans complete, 2026-05-19)

Plans:

- `01-01-PLAN.md`: create the standalone feature shell and public contracts.
- `01-02-PLAN.md`: add the minimal server registration path and validate dependency direction.

Outcome:

- `StaticMlp.Features.AiNavigation` exists as a separate feature.
- Public contracts are readable by `CombatDirector`, `AiBots`, and base NPC logic without pulling in ProjectDawn/backend implementation.
- Existing local bot movement remains in `AiBots`.

## Phase 02: Runtime Nav Areas

Status: Complete (2/2 plans complete, 2026-05-19)

Plans:

- `02-01-PLAN.md`: sync `CombatCell` into `CombatCellNavArea`.
- `02-02-PLAN.md`: add nav rebuild queue, versioning, and technical performance budget counters.

Outcome:

- Combat cells can request/prewarm nav areas without `CombatDirector` knowing NavMesh implementation.
- Expensive nav work is queued and budgeted outside `Peak`.

## Phase 03: Spawn Source Reachability

Status: Complete (2/2 plans complete, 2026-05-19)

Plans:

- `03-01-PLAN.md`: add spawn-source nav state and bounded reachability checks.
- `03-02-PLAN.md`: provide cached reachable candidates and resolved spawn points for director scoring/building.

Outcome:

- `SpawnSourceSelection` uses cached reachable state only.
- `SpawnRequestBuildSystem` does not search NavMesh directly.
- Reachable candidates and resolved spawn points are explicit navigation-owned cached outputs.

## Phase 04: AI Navigation Modes

Status: Complete (2/2 plans complete, 2026-05-19)

Plans:

- `04-01-PLAN.md`: introduce `AiNavigationMode` and far simulation contracts.
- `04-02-PLAN.md`: connect far approximate movement to local movement handoff through `AiMoveRequest`.

Outcome:

- Far AI can move logically without a loaded chunk.
- When local nav is available, AI resumes local movement through an explicit handoff into the existing bot navigation path.

## Phase 05: Debug And Verification

Status: planned

Plans:

- `05-01-PLAN.md`: add server debug snapshot contracts and presentation-only debug view data.

Outcome:

- Debug tooling can inspect nav areas, nav versions, reachability, selected sources, and per-tick budget counters without mutating gameplay state.
