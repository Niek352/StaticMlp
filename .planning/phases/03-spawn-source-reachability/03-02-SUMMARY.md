# Phase 03 Plan 02: Cached Spawn Candidates And Resolved Points Summary

**Navigation-owned reachable source candidates and cached resolved spawn-point outputs with an explicit fail-fast projection boundary**

## Accomplishments
- Added `ReachableSpawnSourceCandidate` so director-side selection can query active reachable sources from cached navigation state instead of triggering pathfinding work.
- Added `ResolvedSpawnPointSystem` so navigation owns cached resolved spawn-point output keyed by source, zone, and nav version.
- Kept the backend boundary explicit with `ISpawnSourcePointResolver`, avoiding any raw-source-position fallback in director or navigation logic.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/ReachableSpawnSourceCandidate.cs` - Navigation-owned cached candidate fact for reachable spawn sources.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/ISpawnSourcePointResolver.cs` - Fail-fast resource contract for nav-backed spawn-point projection.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Systems/Server/ReachableSpawnSourceCacheSystem.cs` - Builds and clears reachable source candidate cache from existing navigation state.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Systems/Server/ResolvedSpawnPointSystem.cs` - Writes cached resolved spawn-point data and removes stale entries when nav state changes.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/AiNavigationLogicFeature.cs` - Registers candidate and resolved-point cache systems in the server gameplay pipeline.
- `.planning/ROADMAP.md` - Marked Phase 03 complete with 2 of 2 plans finished.

## Decisions Made
- Kept candidate and resolved-point outputs on the source entity as navigation-owned components so `CombatDirector` can consume cached facts without introducing a navigation queue or direct backend dependency.
- Derived candidate validity only from source activity, cached reachability status, active nav-area membership, and nav-version agreement because no separate safe-distance or penalty facts exist in the repo yet.
- Required resolved-point projection through an explicit resource contract so missing backend support fails fast at the navigation boundary rather than silently using `SpawnSource.Position`.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added an explicit resolved spawn-point projection seam**
- **Found during:** Task 2 (Prepare resolved spawn points from cached nav data)
- **Issue:** The repo had no existing runtime backend contract for projecting a final spawn point from cached navigation state, and the plan explicitly forbade silently falling back to the raw source position.
- **Fix:** Added `ISpawnSourcePointResolver` as a navigation-owned resource contract and routed `ResolvedSpawnPointSystem` through it.
- **Files modified:** `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/ISpawnSourcePointResolver.cs`, `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Systems/Server/ResolvedSpawnPointSystem.cs`
- **Verification:** `ResolvedSpawnPointSystem` reads the resolver resource directly and contains no raw-position fallback path.
- **Commit:** Included in the plan completion commit for `feat(03-02)`.

---

**Total deviations:** 1 auto-fixed (1 blocking), 0 deferred
**Impact on plan:** The added seam was required to satisfy the plan's fail-fast boundary for spawn-point projection without leaking backend logic into director systems.

## Issues Encountered
None

## Next Phase Readiness
Reachable spawn candidates and resolved spawn-point outputs are now explicit `AiNavigation` caches owned by navigation state and nav-version validity.
Ready for `04-01-PLAN.md`.

---
*Phase: 03-spawn-source-reachability*
*Completed: 2026-05-19*
