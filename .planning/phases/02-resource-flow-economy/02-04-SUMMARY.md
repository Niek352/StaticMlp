# Phase 2 Plan 4: Production Station Architecture Summary

**Settlement production now uses station-scoped recipe ids, replicated station operation state, and shared input/output rows with Workbench migrated onto the model**

## Accomplishments
- Added production station ids, recipe ids, recipe definitions, and a station-scoped recipe catalog for current Workbench recipes.
- Added replicated `ProductionStationOperationState` plus station input/output multi rows and projection registration.
- Migrated Workbench bootstrap, panel presentation, production summaries, and worker demand queries to the production station contracts.
- Updated editor tests to validate the production catalog, Workbench station bootstrap, row access, and existing worker demand behavior.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Ids/ProductionStationId.cs` - Typed production station id.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Ids/ProductionRecipeId.cs` - Typed production recipe id.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Ids/ProductionStationIds.cs` - Stable Workbench station id.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Definitions/ProductionRecipeDefinition.cs` - Station-scoped recipe definition with optional fuel requirement.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Catalogs/ProductionRecipeCatalog.cs` - Production recipe catalog seeded with existing Workbench recipes.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Components/ProductionStationOperationState.cs` - Server-authoritative replicated production station state.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Components/ProductionStationInputResource.cs` - Input row for station resource buffers.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Components/ProductionStationOutputResource.cs` - Output row for station resource buffers.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Domain/ProductionStationResourceAccess.cs` - Station row initialization and read access.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Systems/Server/ServerWorkbenchBootstrapSystem.cs` - Boots completed Workbenches as production stations.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/SettlementSharedResourcesGameplayFeature.cs` - Registers production station projections.
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Queries/SettlementWorkerDemandQuery.cs` - Reads gather/process/haul demand from production stations.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/BuildingPanelPresentation.cs` - Reads Workbench panel recipes from production station state.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Definitions/BuildingProductionSummaryProvider.cs` - Lists Workbench recipes through the production catalog.
- `Assets/Tests/Editor/Settlement/WorkbenchOperationTests.cs` - Updated catalog/bootstrap/resource-row tests.
- `Assets/Tests/Editor/Ai/SettlementWorkersSystemsTests.cs` - Updated worker demand setup to create production stations.
- `docs/Roadmap по GDD.md` - Updated Phase 2 status with the production station architecture.
- Removed old Workbench-only recipe id/catalog/definition and row access/source row types.

## Decisions Made
- Kept production owned by Settlement Logic because settlement buildings own station lifecycle, operation state, recipes, and storage integration.
- Used a station-scoped `ProductionRecipeCatalog.Get(stationId, recipeId)` path so active recipes cannot silently cross station boundaries.
- Kept `WorkbenchOperationState` only as an `[Obsolete("Temp")]` source shim because existing generated replication files still reference it until Unity codegen regenerates them.

## Issues Encountered
- Unity replication codegen and compile/editor tests were not run by agent because project rules prohibit launching Unity or running `dotnet build` independently. Run `StaticMlp/Replication/Generate` in Unity, then compile/editor tests.

## Next Step
Run Unity `StaticMlp/Replication/Generate` and Unity compile/editor tests. After that, ready for `02-05-PLAN.md`.
