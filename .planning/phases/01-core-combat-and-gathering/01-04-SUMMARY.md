# Phase 1 Plan 4: Inventory and Pickup Summary

**Owner-only carried raw inventory now uses Multi rows, while harvested resources spawn shared server-owned pickups with client magnetic presentation.**

## Accomplishments
- Reworked `ResourcesInventoryMinimal` into logic/presentation folders while keeping the existing logic asmdef name for generated reference compatibility.
- Replaced the minimal wood/stone counters with a 20-unit carried inventory serialized from `Multi<CarriedResourceEntry>`.
- Added shared replicated resource pickup entities, server-authoritative collection, overflow preservation, and client-only magnetic pickup visuals.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/ResourcesInventoryMinimal/Runtime/Logic` - Inventory Multi rows, access rules, pickup state, factory, and server systems.
- `Assets/Scripts/StaticMlp/Features/ResourcesInventoryMinimal/Runtime/Presentation` - Pickup view state, presentation feature, and magnetic view system.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/PlayerThreatInputSystem.cs` - Reads carried loot through `ResourcesInventoryAccess`.
- `.planning/ROADMAP.md` - Marks Phase 1 progress as four of six plans complete.
- `.planning/phases/01-core-combat-and-gathering/01-PHASE-SUMMARY.md` - Updates Phase 1 progress.

## Decisions Made
- Used `Multi<CarriedResourceEntry>` for carried inventory rows, matching the Settlement replicated resource pattern.
- Kept one carried resource unit equal to one slot; row stacking is compact storage by resource id.
- Implemented pickups as shared server-owned replicated entities with archetype id `410`.
- Kept pickup magnet movement presentation-only; server collection remains authoritative.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 4 - Architectural] Changed inventory storage from explicit fixed fields to Multi rows**
- **Found during:** Task 1 (Inventory refactor)
- **Issue:** Fixed fields were the wrong local architecture for a resource row collection.
- **Fix:** Replaced fixed fields with `CarriedResourceEntry : IMultiComponent` and `ResourcesInventoryAccess`.
- **Files modified:** `ResourcesInventory.cs`, `CarriedResourceEntry.cs`, `ResourcesInventoryAccess.cs`, server systems, CombatDirector read path.
- **Verification:** Source checks confirm no remaining `Wood`/`Stone` inventory counters or `Settlement.Logic` dependency.

---

**Total deviations:** 1 architectural correction requested by the user.
**Impact on plan:** The behavior is unchanged, but the inventory shape now follows the project Multi pattern.

## Issues Encountered
None during source implementation.

## Next Step
Run Unity replication/codegen for the changed `ResourcesInventory` layout and new `ResourcePickup` replicated component, then run Unity compile/play checks. Ready for `01-05-PLAN.md`.

---
*Phase: 01-core-combat-and-gathering*
*Completed: 2026-05-26*
