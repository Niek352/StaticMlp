# Phase 01 Plan 01: AiNavigation Contract Surface Summary

**Standalone `AiNavigation` contracts assembly with public nav-area, spawn reachability, performance budget, resolved spawn point, and far AI movement data contracts**

## Accomplishments
- Created the `StaticMlp.Features.AiNavigation.Contracts` asmdef as a standalone public boundary.
- Added one-type-per-file ECS contract types for nav areas, spawn-source nav state, resolved spawn points, performance budgets, far AI state, and navigation modes.
- Kept the contract surface data-only and used `EntityGID` for resolved spawn source identity instead of raw entity ids.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/StaticMlp.Features.AiNavigation.Contracts.asmdef` - Contracts-only assembly definition for AiNavigation.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/CombatCellNavArea.cs` - Combat cell nav area contract.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/SpawnSourceNavStatus.cs` - Spawn source reachability status enum.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/SpawnSourceNavState.cs` - Spawn source navigation cache state contract.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/ResolvedSpawnPoint.cs` - Cached resolved spawn point contract with stable source identity.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/CombatCellPerformanceBudget.cs` - Technical nav/combat cell budget contract.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/AiFarSimulationState.cs` - Far AI logical movement state contract.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/AiNavigationMode.cs` - Navigation mode enum for far/local handoff.
- `.planning/ROADMAP.md` - Marked Phase 01 as in progress with 1 of 2 plans complete.

## Decisions Made
- Used a dedicated `StaticMlp.Features.AiNavigation.Contracts` assembly with references limited to `FFS.StaticEcs` and `Game.Core`, preserving dependency direction toward shared contracts.
- Kept position fields as `float3` to match the navigation contract documents and backend-oriented nav data shape.
- Adopted the fuller `AiNavigationMode` enum from `ai/AiNavigation.md` so later phases can add far/local handoff without changing the public enum contract.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None

## Next Phase Readiness

The standalone contracts boundary is in place and consumable by future logic assemblies.
Ready for 01-02-PLAN.md.

---
*Phase: 01-ainavigation-architecture*
*Completed: 2026-05-19*
