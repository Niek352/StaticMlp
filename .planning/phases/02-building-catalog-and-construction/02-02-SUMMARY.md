# Phase 2 Plan 2: Building Registration Summary

**Stage1 buildings are wired through network and presentation catalogs.**

## Accomplishments
- Added stable blueprint and finished network archetype ids for Camp Core, Stockpile, Bedroll Shelter, Lumber Camp, Stone Mine, and Workbench.
- Registered every Stage1 building in the network catalog so existing catalog-driven prefab registration and factory spawning can resolve blueprint and finished archetypes.
- Registered every Stage1 building in the presentation catalog with matching display names and temporary shared WoodenHut view paths.
- Added editor catalog tests that require every `BuildingCatalogData.All` entry to have matching network and presentation definitions.
- Replaced remaining default Camp Core registration/test references with explicit Camp Core constants instead of the WoodenHut alias.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/NetworkEntityTypes/BuildingNetworkArchetypeIds.cs` - Adds Stage1 blueprint/finished archetype ids and keeps WoodenHut aliases for compatibility.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Catalogs/BuildingNetworkCatalog.cs` - Registers network definitions for all Stage1 buildings.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Presentation/Catalogs/BuildingPresentationCatalog.cs` - Registers presentation definitions for all Stage1 buildings.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/NetworkEntityTypes/ConstructionSiteNetworkEntity.cs` - Uses the explicit Camp Core blueprint archetype as the default.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/NetworkEntityTypes/FinishedBuildingNetworkEntity.cs` - Uses the explicit Camp Core finished archetype as the default.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Presentation/BuildingMenuController.cs` - Uses Camp Core as the default selected building id.
- `Assets/Tests/Editor/Settlement/BuildingCatalogValidatorTests.cs` - Adds coverage for network and presentation catalog completeness.
- `Assets/Tests/Editor/Combat/BuildingEntitySpawnerTests.cs` - Updates build-spawn tests to reference Camp Core directly.
- `.planning/ROADMAP.md` - Marks Phase 2 Plan 2 complete.

## Decisions Made
- Reused the existing `Views/Buildings/WoodenHutGhostPreview`, `Views/Buildings/WoodenHutBlueprint`, and `Views/Buildings/WoodenHut` paths for every new Stage1 building as temporary placeholder visuals.
- Kept presentation paths client-side in `BuildingPresentationCatalog`; server archetype recipes still contain only gameplay/network state.
- Kept `WoodenHutBlueprint` and `WoodenHutFinished` as compatibility aliases of Camp Core archetype ids while moving new code and tests to explicit Camp Core names.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Unity EditMode tests were not run because agents must not launch Unity or run project build checks.
- Placeholder visual reuse means the new buildings will display with WoodenHut visuals until a human wires distinct Unity view assets/prefabxml entries.

## Next Step
Ready for `02-03-PLAN.md`.

---
*Phase: 02-building-catalog-and-construction*
*Completed: 2026-05-20*
