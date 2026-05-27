# Phase 2 Plan 7: Unlock Gates Summary

**Data-driven unlock requirements for buildings and recipes with catalog validation, read-only `ISettlementUnlockReadModel` filtering, and a clear Phase 3 NPC handoff path**

## Accomplishments
- Added `UnlockRequirementKind` enum and `UnlockRequirement` struct to BuildingCatalog, supporting `None`, `SettlementLevel`, and `BuildingConstructed` gates with an extension point for future `NpcRecruited` kind.
- Added `UnlockRequirement` fields to `BuildingDefinition` and `ProductionRecipeDefinition`; all current Phase 2 buildings and recipes default to `UnlockRequirement.None`.
- Extended `BuildingCatalogValidator` and `ProductionRecipeCatalog.Validate` to verify unlock requirements (positive level values, valid and non-self-referencing building IDs, no unknown kinds).
- Created `ISettlementUnlockReadModel` in `Settlement.Contracts` as the owner-provided read model for unlock evaluation.
- Created `SettlementProgressionState` and `SettlementUnlockState` resources in Settlement.Logic to track settlement level and query constructed buildings via ECS.
- Created `AvailableBuildingsQuery` and `AvailableRecipesQuery` in Settlement.Presentation for client-side filtering of locked entries, reading only owner-provided contracts and local ECS state.
- Documented Phase 3 handoff in `docs/Roadmap по GDD.md`: NPC-specific unlock facts (e.g., Carpenter recruited → Resin Forge) must be emitted by NPC/Settlement owner systems via typed events or read models.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Definitions/UnlockRequirementKind.cs` - Phase 2 unlock gate kinds with Phase 3 extension point.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Definitions/UnlockRequirement.cs` - Data contract for unlock requirements on catalog entries.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Domain/UnlockEvaluation.cs` - Static evaluator matching requirements against `ISettlementUnlockReadModel`.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Definitions/BuildingDefinition.cs` - Added `UnlockRequirement` field.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Validation/BuildingCatalogValidator.cs` - Added unlock requirement validation.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Definitions/ProductionRecipeDefinition.cs` - Added `UnlockRequirement` field.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Catalogs/ProductionRecipeCatalog.cs` - Added unlock requirement validation.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Contracts/ISettlementUnlockReadModel.cs` - Owner-provided read model contract.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/WorldResources/SettlementProgressionState.cs` - Tracks settlement level.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/WorldResources/SettlementUnlockState.cs` - Server-side `ISettlementUnlockReadModel` implementation.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Domain/AvailableBuildingsQuery.cs` - Client-side building availability filter.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Domain/AvailableRecipesQuery.cs` - Client-side recipe availability filter.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/SettlementSharedResourcesGameplayFeature.cs` - Registered new resources.
- `docs/Roadmap по GDD.md` - Added Phase 2 deliverable and Phase 3 handoff notes.

## Decisions Made
- `UnlockRequirement` lives in BuildingCatalog since it describes catalog data; both building and recipe definitions reuse the same struct shape because Settlement.Logic already references BuildingCatalog.
- `ISettlementUnlockReadModel` lives in `Settlement.Contracts` because Settlement owns progression facts; BuildingCatalog references `Settlement.Contracts` already, so `UnlockEvaluation` can consume the interface without circular dependencies.
- `SettlementUnlockState` queries `FinishedBuildingTag` + `BuildingNetworkDefinition` directly on the server instead of maintaining a separate list, keeping the state automatically synchronized with the ECS world.
- Presentation queries create lightweight local implementations of `ISettlementUnlockReadModel` using `CW` and `SettlementProgressionState`, avoiding a dependency on foreign Logic assemblies.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Unity code generation, compile and editor tests were not run because project rules forbid launching Unity batchmode or running `dotnet build`; manual Unity verification is still required.

## Next Phase Readiness
- Unlock gate infrastructure is data-driven, validated, and ready for UI integration.
- Phase 3 NPC systems can extend `UnlockRequirementKind` with `NpcRecruited` and extend `ISettlementUnlockReadModel` with `HasNpc` to drive building/recipe availability without changes to the core filtering shape.
- Ready for `02-08-RESEARCH.md` or Phase 3 planning.

---
*Phase: 02-resource-flow-economy*
*Completed: 2026-05-27*
