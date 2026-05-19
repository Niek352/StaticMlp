# Phase 03 Plan 01: Bounded Spawn-Source Reachability Summary

**Navigation-owned spawn-source caching with explicit recheck cadence, per-cell reachability budgeting, and director-facing `SpawnSource` contracts**

## Accomplishments
- Added the missing director-owned `SpawnSource` and `SpawnSourceType` public contracts in `Frontier.Contracts` so `AiNavigation` can consume spawn-source state without a logic-to-logic dependency.
- Added deterministic reachability rules that choose an active nav area, decide when a source must be rechecked, and assign explicit `WaitingForNavMesh`, `Reachable`, `Unreachable`, and `TemporarilyBlocked` cache states.
- Added `SpawnSourceReachabilitySystem` to schedule and consume bounded reachability work per combat-cell nav area, updating only navigation-owned state and counters.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Contracts/SpawnSourceType.cs` - Director-owned spawn-source type contract.
- `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Contracts/SpawnSource.cs` - Director-owned spawn-source ECS component contract.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/SpawnSourceReachabilityRequest.cs` - Navigation-owned pending reachability work item.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Domain/SpawnSourceReachabilityRules.cs` - Deterministic scheduling, area selection, and state-transition rules.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Systems/Server/SpawnSourceReachabilitySystem.cs` - Bounded server-side reachability scheduling and cache updates.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/AiNavigationLogicFeature.cs` - Registered the reachability system in the server gameplay pipeline.
- `.planning/ROADMAP.md` - Marked Phase 03 as in progress with 1 of 2 plans complete.

## Decisions Made
- Kept `SpawnSource` in `Frontier.Contracts` because the combat director owns spawn-source gameplay state and `AiNavigation.Logic` already depends on the contracts assembly, not frontier logic.
- Used a navigation-owned request component on the source entity instead of a separate queue resource so the schedule remains explicit in ECS data and stays local to the feature.
- Scoped current reachability classification to navigation facts already available in this phase: sources outside active nav areas become `Unreachable`, sources in nav areas with non-ready runtime nav stay `WaitingForNavMesh`, due sources pending a ready-area budget slot become `TemporarilyBlocked`, and processed ready-area sources cache `Reachable`.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added the missing public spawn-source contract**
- **Found during:** Task 1 (Add reachability scheduling rules)
- **Issue:** The plan required `AiNavigation` to read director-owned `SpawnSource`, but the repo did not yet expose that contract anywhere in `Frontier.Contracts`.
- **Fix:** Added `SpawnSourceType` and `SpawnSource` to `Frontier.Contracts` so navigation can read source state through the documented public boundary.
- **Files modified:** `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Contracts/SpawnSourceType.cs`, `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Contracts/SpawnSource.cs`
- **Verification:** `SpawnSourceReachabilitySystem` now reads `SpawnSource` through `StaticMlp.Features.Frontier.Contracts` only; no frontier logic dependency was introduced.
- **Commit:** Included in the plan completion commit for `feat(03-01)`.

---

**Total deviations:** 1 auto-fixed (1 blocking), 0 deferred
**Impact on plan:** The added contract was required to execute the plan on the documented feature boundary. No scope creep beyond restoring the missing dependency.

## Issues Encountered
None

## Next Phase Readiness
Spawn sources now have navigation-owned cached state with explicit recheck cadence, per-cell work limits, and nav-version invalidation.
Ready for `03-02-PLAN.md`.

---
*Phase: 03-spawn-source-reachability*
*Completed: 2026-05-19*
