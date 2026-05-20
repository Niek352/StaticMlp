# Phase 1 Plan 3: Construction Ledger Summary

**Construction resource delivery now uses explicit Stage1 construction fields for Wood, Stone, Planks, and Simple Parts.**

## Accomplishments
- Expanded `ConstructionResources` with explicit required, delivered, remaining, and completion access for Planks and Simple Parts.
- Generalized construction deposit planning/application over all Stage1 construction resources while preserving Wood and Stone convenience behavior.
- Updated construction site and finished building factories to derive ledger requirements from catalog `ResourceAmount` costs.
- Extended deposit request/result contracts, server handling, client projection, client interaction, and worker delivery paths with explicit Planks and Simple Parts amounts.
- Updated Stage1 resource-readiness checks so future construction costs can gate on non-wood/stone resources.
- Added tests for Planks delivery and negative client deposit request rejection.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/ConstructionResources.cs` - Adds explicit Stage1 construction fields and typed accessors.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/SettlementConstructionRules.cs` - Generalizes construction ledger creation, planning, delivery, and completion.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Events/DepositConstructionResourcesEvent.cs` - Carries explicit Planks and Simple Parts deposits.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Systems/Server/ServerDepositConstructionResourcesSystem.cs` - Applies generalized server-side deposit events.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Contracts/DepositConstructionResourcesRequestEvent.cs` - Adds explicit Planks and Simple Parts request fields.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Contracts/DepositConstructionResourcesResultEvent.cs` - Reports accepted Planks and Simple Parts deposits.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Requests/DepositConstructionResourcesHandler.cs` - Validates client request amounts and applies generalized deposits.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Requests/DepositConstructionResourcesProjector.cs` - Projects generalized deposits client-side.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Factories/ConstructionSiteFactory.cs` - Creates construction ledgers from catalog costs.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Factories/FinishedBuildingFactory.cs` - Creates completed ledgers from catalog costs.
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Actions/DeliveryBuildResources/*` - Uses generalized deposit planning for worker delivery.
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Systems/Server/ServerSettlementWorkerCampBuilderJobSystem.cs` - Finds construction delivery demand using generalized resources.
- `Assets/Scripts/StaticMlp/Features/Stage1/Runtime/Logic/Systems/Server/ServerStage1FlowSystem.cs` - Checks all construction resource requirements for repair readiness.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Validation/BuildingCatalogValidator.cs` - Rejects non-construction resources in construction costs.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Domain/ConstructionRules.cs` - Removes the unused wood/stone-only deposit rule path.
- `Assets/Tests/Editor/Combat/Stage1RepairFlowTests.cs` - Adds generalized deposit and request-validation coverage.
- `Assets/Tests/Editor/Architecture/FeatureWriteBoundaryTests.cs` - Updates existing baseline line numbers after planned edits moved known debt.

## Decisions Made
- Limited the construction ledger expansion to resources currently flagged for construction: Wood, Stone, Planks, and Simple Parts.
- Used explicit request/result fields instead of arrays because the current replicated event serialization path is generated from fixed fields.
- Left generated serializers untouched; the source contracts now require the project replication/event codegen workflow.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Generalized worker delivery and Stage1 readiness callers**
- **Found during:** Task 2 (generalized deposit handling)
- **Issue:** Worker delivery demand and Stage1 resource readiness still depended on wood/stone-only checks.
- **Fix:** Routed worker delivery and Stage1 readiness through the generalized construction resource fields.
- **Files modified:** `DeliveryBuildResourcesExecutor.cs`, `DeliveryBuildResourcesCollectVariables.cs`, `ServerSettlementWorkerCampBuilderJobSystem.cs`, `ServerStage1FlowSystem.cs`
- **Verification:** Static review plus new generalized construction deposit test coverage.

**2. [Rule 3 - Blocking] Updated architecture guard line baseline after planned edits**
- **Found during:** Verification
- **Issue:** Existing known cross-feature write baseline entries moved line numbers after the deposit handler/projector and factory edits.
- **Fix:** Updated only the affected baseline line numbers; no new baseline entries were added.
- **Files modified:** `Assets/Tests/Editor/Architecture/FeatureWriteBoundaryTests.cs`
- **Verification:** Spot-checked the updated baseline lines against the current source files.

## Issues Encountered
Unity EditMode tests, project compilation, and replication/event codegen were not run because agents must not launch Unity, run project build checks, or edit generated files manually.

## Next Phase Readiness
Phase 1 complete, ready for `02-01-PLAN.md`.

---
*Phase: 01-settlement-foundation*
*Completed: 2026-05-20*
