# Phase 02 Plan 02: Runtime Nav Areas Summary

**Navigation-owned rebuild queue, versioned runtime nav zone state, and peak-throttled backend execution gate**

## Accomplishments
- Added navigation-owned runtime zone state, rebuild request state, and per-tick technical work counters on combat-cell nav entities.
- Added a bounded rebuild queue system that versions requests from `CombatCellNavArea` changes without depending on `CombatDirector` internals.
- Added a peak-aware build system that reads a public frontier phase contract and throws explicitly when queued runtime NavMesh backend execution is invoked before implementation exists.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/RuntimeNavMeshBuildState.cs` - Runtime nav build lifecycle enum.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/RuntimeNavMeshZoneState.cs` - Versioned runtime zone state and queued area snapshot.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/NavRebuildReason.cs` - Explicit rebuild reasons for queued work.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/NavRebuildRequest.cs` - Queued rebuild request state with peak allowance flag.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/NavWorkBudgetCounter.cs` - Per-tick navigation technical counters, separate from threat/dramaturgy.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/CombatCellPerformanceBudget.cs` - Added explicit rebuild-start budget field.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Domain/CombatCellPerformanceBudgetDefaults.cs` - Conservative default technical budget seeding for combat-cell nav areas.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Systems/Server/CombatCellNavAreaSyncSystem.cs` - Initializes and cleans up navigation-owned queue/budget state on combat-cell nav entities.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Systems/Server/RuntimeNavMeshRebuildQueueSystem.cs` - Enqueues bounded versioned rebuild work from nav-area changes.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Systems/Server/RuntimeNavMeshBuildSystem.cs` - Selects one eligible rebuild request, blocks non-prewarmed peak work, and fail-fast throws for missing backend execution.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/AiNavigationLogicFeature.cs` - Registered queue/build systems in the server pipeline.
- `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Contracts/FrontierNavWorkPhase.cs` - Public phase enum for navigation throttling.
- `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Contracts/FrontierNavWorkPhaseState.cs` - Public read-only phase component consumed by navigation.
- `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Logic/Systems/Server/ServerFrontierNavWorkPhaseSyncSystem.cs` - Maps current frontier threat phases into the public navigation throttling contract.
- `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Logic/FrontierLogicFeature.cs` - Registered the public phase sync system.
- `.planning/ROADMAP.md` - Marked Phase 02 complete.

## Decisions Made
- Kept rebuild queue, runtime nav versioning, and technical counters on the same entity as `CombatCellNavArea` so navigation-owned state stays co-located with the nav interest area it governs.
- Added a dedicated public `FrontierNavWorkPhaseState` contract instead of referencing `Frontier.Logic` from `AiNavigation.Logic`; this preserves the contracts-only dependency boundary while still allowing peak-sensitive throttling.
- Mapped current `ThreatPhase.RaidPending` and `ThreatPhase.RaidActive` to navigation `Peak` until a fuller combat-director phase contract exists in public contracts.
- Added `MaxNavRebuildStartsPerTick` to `CombatCellPerformanceBudget` because bounded rebuild execution needs an explicit technical budget knob rather than hidden hardcoded behavior.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added a public frontier nav-work phase contract**
- **Found during:** Task 2 (Implement bounded rebuild scheduling)
- **Issue:** The plan required `Peak` throttling, but `AiNavigation.Logic` only depended on `Frontier.Contracts` while the live phase state existed only in `Frontier.Logic` as `ThreatState`.
- **Fix:** Added `FrontierNavWorkPhase` and `FrontierNavWorkPhaseState` to `Frontier.Contracts`, then synced them from `ThreatState` in `Frontier.Logic`.
- **Files modified:** `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Contracts/FrontierNavWorkPhase.cs`, `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Contracts/FrontierNavWorkPhaseState.cs`, `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Logic/Systems/Server/ServerFrontierNavWorkPhaseSyncSystem.cs`, `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Logic/FrontierLogicFeature.cs`
- **Verification:** `AiNavigation` peak throttling now reads only `FrontierNavWorkPhaseState` from the contracts assembly and introduces no logic-to-logic dependency.
- **Commit:** Included in the plan completion commit for `feat(02-02)`.

**2. [Rule 2 - Missing Critical] Added an explicit rebuild-start budget field**
- **Found during:** Task 2 (Implement bounded rebuild scheduling)
- **Issue:** `CombatCellPerformanceBudget` had path/reachability limits but no explicit rebuild-start budget, which left runtime nav rebuild throttling under-specified.
- **Fix:** Added `MaxNavRebuildStartsPerTick` and wired the build system to honor it.
- **Files modified:** `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/CombatCellPerformanceBudget.cs`, `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Systems/Server/RuntimeNavMeshBuildSystem.cs`
- **Verification:** The build system checks `CombatCellPerformanceBudget.MaxNavRebuildStartsPerTick` before starting queued work.
- **Commit:** Included in the plan completion commit for `feat(02-02)`.

**3. [Rule 3 - Blocking] Seeded default technical budgets for nav-area entities**
- **Found during:** Task 2 (Implement bounded rebuild scheduling)
- **Issue:** No system in the current repo populated `CombatCellPerformanceBudget`, so the new queue/build pipeline had no valid technical budget source on combat-cell nav entities.
- **Fix:** Added `CombatCellPerformanceBudgetDefaults.Create()` and initialized missing budgets in `CombatCellNavAreaSyncSystem`.
- **Files modified:** `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Domain/CombatCellPerformanceBudgetDefaults.cs`, `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Systems/Server/CombatCellNavAreaSyncSystem.cs`
- **Verification:** Combat-cell nav entities now receive a concrete `CombatCellPerformanceBudget`, including rebuild-start limits, when navigation-owned state is created.
- **Commit:** Included in the plan completion commit for `feat(02-02)`.

---

**Total deviations:** 3 auto-fixed (1 missing critical, 2 blocking), 0 deferred
**Impact on plan:** All deviations were required to keep phase throttling on a public contract boundary and to make rebuild budgeting explicit instead of implicit.

## Issues Encountered
None

## Next Phase Readiness
Runtime nav work now has explicit queue state, version state, per-tick technical counters, and a fail-fast backend execution gate.
Peak-sensitive throttling is available through a public frontier contract without exposing `AiNavigation` to frontier logic internals.
Ready for 03-01-PLAN.md.

---
*Phase: 02-runtime-nav-areas*
*Completed: 2026-05-19*
