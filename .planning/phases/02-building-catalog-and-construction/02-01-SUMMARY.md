# Phase 2 Plan 1: Stage1 Building Catalog Summary

**The Stage1 building set is represented as typed catalog data.**

## Accomplishments
- Added stable Stage1 building ids and catalog definitions for Camp Core, Stockpile, Bedroll Shelter, Lumber Camp, Stone Mine, and Workbench.
- Filled each definition with category, capability flags, construction cost, footprint, build work, interactions, NPC profile, and operation profile data.
- Tied the initial Stage1 repair construction site to `BuildingCatalogData.CampCoreId`.
- Extended catalog and repair-flow tests to assert the Stage1 building set and the initial Camp Core construction site.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Catalogs/BuildingCatalogData.cs` - Adds Stage1 building ids and definitions.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Stage1SettlementSeedManifest.cs` - Uses `BuildingCatalogData.CampCoreId` for the initial repair site.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/StaticMlp.Features.BuildingCatalog.asmdef` - Removes an unnecessary dependency on `Settlement.Logic`.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/StaticMlp.Features.Settlement.Logic.asmdef` - Adds the directed dependency needed for seed data to reference catalog ids.
- `Assets/Tests/Editor/Settlement/BuildingCatalogValidatorTests.cs` - Validates the full Stage1 catalog set appears exactly once.
- `Assets/Tests/Editor/Combat/Stage1RepairFlowTests.cs` - Verifies the initial spawned construction site is Camp Core.
- `.planning/ROADMAP.md` - Marks Phase 2 Plan 1 complete.

## Decisions Made
- Kept `WoodenHutId` as a compatibility alias for `CampCoreId` so current presentation/network catalog entries can keep working until `02-02-PLAN.md` registers explicit entries for the new buildings.
- Kept first-wave construction costs on Wood and Stone only, so Planks and Simple Parts do not block the initial path before Workbench production exists.
- Kept definitions as runtime catalog data only; no prefab, view path, or presentation asset data was added.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Rebalanced asmdef dependencies for the Camp Core catalog reference**
- **Found during:** Task 2 (aligning Stage1 seed with Camp Core)
- **Issue:** `Settlement.Logic` needed to reference `BuildingCatalogData.CampCoreId`, while `BuildingCatalog` had an unnecessary `Settlement.Logic` reference that would create a circular dependency.
- **Fix:** Removed the unused `BuildingCatalog -> Settlement.Logic` asmdef reference and added the required `Settlement.Logic -> BuildingCatalog` reference.
- **Files modified:** `StaticMlp.Features.BuildingCatalog.asmdef`, `StaticMlp.Features.Settlement.Logic.asmdef`
- **Verification:** Static dependency review confirmed `BuildingCatalog` only needs `Settlement.Contracts`.

---

**Total deviations:** 1 auto-fixed blocking issue, 0 deferred.
**Impact on plan:** The dependency adjustment was required to implement the planned typed Camp Core id reference without introducing an assembly cycle.

## Issues Encountered
Unity EditMode tests were not run because agents must not launch Unity or run project build checks.

## Next Phase Readiness
Ready for `02-02-PLAN.md`.

---
*Phase: 02-building-catalog-and-construction*
*Completed: 2026-05-20*
