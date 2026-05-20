# Phase 06 Plan 01: Combat Director OpenWorld Audit Summary

**CombatDirector wave-first runtime mapped to open-world attention migration targets**

## Accomplishments
- Created `ai/gdd/AiCombatDirector_OpenWorld_Audit.md` with a file-level map of current CombatDirector contracts, logic, presentation, events, resources, catalogs, and tests.
- Identified the current wave-first contracts and systems that must be replaced or refactored before open-world ambient/encounter work.
- Recorded architecture conflicts and a safe phase 07-10 migration order with `07-01-PLAN.md` as the next step.

## Files Created/Modified
- `ai/gdd/AiCombatDirector_OpenWorld_Audit.md` - Audit note mapping existing runtime files to keep/refactor/replace/defer decisions and documenting open-world conflicts.
- `.planning/phases/06-combat-director-openworld-audit/06-01-SUMMARY.md` - Phase execution summary.
- `.planning/ROADMAP.md` - Phase 06 status updated to complete.

## Decisions Made
- Keep the existing `StaticMlp.Features.CombatDirector` feature boundary and refactor it in place.
- Preserve the server-authoritative spawn/apply boundary around `SpawnRequestValidationSystem` and `EnemySpawnApplySystem`.
- Replace `ThreatBudget` first in phase 07 before changing phase semantics, source classification, or ambient spawning.
- Do not add fallback spawn positions, hidden compatibility wrappers, or temporary runtime code in this audit phase.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None.

## Next Phase Readiness
Ready for `07-01-PLAN.md`.

Phase 07 has a concrete starting point: replace `ThreatBudget` and `ThreatBudgetAccumulationSystem` with reason-based `CellAttention`, then update `PlayerThreatInputSystem`, `CombatCellTrackingSystem`, `EncounterDirectorConfig`, and CombatDirector tests around the invariant that passive exploration does not create dangerous pressure.

---
*Phase: 06-combat-director-openworld-audit*
*Completed: 2026-05-20*
