# Phase 02 Plan 01: Runtime Nav Areas Summary

**Combat-cell-driven nav area sync with deterministic radius/priority rules and a public director-side `CombatCell` contract**

## Accomplishments
- Added `CombatCellNavAreaSyncSystem` to create, update, and remove `CombatCellNavArea` state directly from director-owned `CombatCell` facts on the server.
- Centralized nav area parameter derivation in `CombatCellNavAreaRules` with deterministic build-radius, source-collect-radius, and priority rules.
- Unblocked the feature boundary by adding the missing public `CombatCell` contract to `Frontier.Contracts` and referencing it from `AiNavigation.Logic`.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Systems/Server/CombatCellNavAreaSyncSystem.cs` - Server sync system that owns `CombatCellNavArea`.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Domain/CombatCellNavAreaRules.cs` - Pure deterministic nav area derivation rules for combat cells.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/AiNavigationLogicFeature.cs` - Registered nav area sync at `GameplaySystemOrder.Gameplay - 97`.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/StaticMlp.Features.AiNavigation.Logic.asmdef` - Added dependency on `StaticMlp.Features.Frontier.Contracts`.
- `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Contracts/CombatCell.cs` - Public director-owned combat cell contract consumed by `AiNavigation`.
- `.planning/ROADMAP.md` - Marked Phase 02 in progress with 1 of 2 plans complete.

## Decisions Made
- Kept nav area ownership on the same entity as `CombatCell` because no separate director/nav-area entity topology exists yet in the current codebase.
- Put `CombatCell` in `Frontier.Contracts` rather than `AiNavigation` so the director-side fact remains owned by the director boundary and `AiNavigation` stays a reader.
- Registered the sync system at `GameplaySystemOrder.Gameplay - 97` so it runs before the current frontier director logic slot at `-96`, leaving the earlier navigation pipeline position open for upcoming queue/rebuild systems.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added the missing director-owned `CombatCell` contract**
- **Found during:** Task 1 (Add nav area sync system)
- **Issue:** The repo did not contain any `CombatCell` type under `Assets/Scripts`, so the planned sync system had no valid source contract to read.
- **Fix:** Added `CombatCell` to `StaticMlp.Features.Frontier.Contracts` and referenced that assembly from `AiNavigation.Logic`.
- **Files modified:** `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Contracts/CombatCell.cs`, `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/StaticMlp.Features.AiNavigation.Logic.asmdef`
- **Verification:** `CombatCellNavAreaSyncSystem` now reads `CombatCell` from the owning director-side contracts assembly without introducing a logic-to-logic dependency.
- **Commit:** Included in the plan completion commit for `feat(02-01)`.

---

**Total deviations:** 1 auto-fixed (1 blocking), 0 deferred
**Impact on plan:** The unblock was required for the requested architecture to exist at all. No scope creep beyond establishing the missing public contract boundary.

## Issues Encountered
None

## Next Phase Readiness
`AiNavigation` now owns `CombatCellNavArea` runtime state while reading director-owned `CombatCell` facts through a contracts-only dependency.
Runtime nav rebuild queue/build systems can be added next ahead of the existing frontier logic order slot.
Ready for 02-02-PLAN.md.

---
*Phase: 02-runtime-nav-areas*
*Completed: 2026-05-19*
