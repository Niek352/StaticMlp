# Phase 2 Plan 5: Recipe Processing Summary

**Server-authoritative production stations now reserve shared inputs, process recipes into output buffers, and claim finished goods into settlement storage**

## Accomplishments
- Added server production rules and processing for enabled production stations, including shared input reservation, worker-scaled work progress, output capacity checks and output commits.
- Added a typed claim request/result path that validates player range, target station state, resource, amount and shared storage capacity before transferring outputs.
- Surfaced production state in the building management panel with recipe progress, input/output buffers and a claim action that forwards intent only.
- Added editor test coverage for successful recipe processing, full output buffer blocking and capacity-limited output claiming.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Components/ProductionStationOperationState.cs` - Added custom replication serialization for station input/output multi rows.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Domain/ProductionStationResourceAccess.cs` - Added projected reads and server mutation helpers for station buffers.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Domain/ProductionStationRules.cs` - Added recipe processing, reservation, output capacity and station-output validation rules.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Systems/Server/ServerProductionStationProcessingSystem.cs` - Added server-authoritative recipe tick processing.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Contracts/Events/ClaimProductionOutputRequestEvent.cs` - Added output claim request contract.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Contracts/Events/ClaimProductionOutputResultEvent.cs` - Added output claim result contract.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Contracts/Events/ClaimProductionOutputEventCodec.cs` - Added manual network codecs for claim request/result events.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Events/ClaimProductionOutputToStorageEvent.cs` - Added internal transfer event for applying accepted claims.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Requests/ClaimProductionOutputHandler.cs` - Added server request validation and accepted transfer dispatch.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Systems/Server/ServerClaimProductionOutputToStorageSystem.cs` - Added settlement-owned output-to-storage mutation system.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/SettlementSharedResourcesGameplayFeature.cs` - Registered codecs, request handling and server systems.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Catalogs/ProductionRecipeCatalog.cs` - Added validation preventing the same resource from being both recipe input and fuel.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Definitions/ProductionResourceBufferEntry.cs` - Added panel read model for production buffer rows.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Definitions/WorkbenchPanelState.cs` - Expanded workbench panel state with enabled, output capacity and buffer row data.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Definitions/BuildingPanelActionKind.cs` - Added claim production output action kind.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/BuildingPanelPresentation.cs` - Added production projection reads and claim action creation.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/BuildingManagementPanelController.cs` - Added claim output request sending.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/BuildingManagementPanelView.cs` - Added production status and buffer rendering.
- `Assets/Tests/Editor/Settlement/SettlementOperationTestWorldScope.cs` - Added simulation time resource setup for server production tests.
- `Assets/Tests/Editor/Settlement/WorkbenchOperationTests.cs` - Added recipe processing and output claim tests.
- `docs/Roadmap по GDD.md` - Updated Phase 2 status to include source-complete recipe processing and output claiming.

## Decisions Made
- Claimed production output goes into settlement shared storage for Phase 2, matching the plan's preference and existing stockpile flow.
- Output claims use a typed request/result pair plus an internal settlement event so the request handler validates intent while the settlement feature owns the storage and station buffer mutation.
- Production station input/output rows are serialized with `ProductionStationOperationState` because the panel needs replicated buffer state and generated fields do not cover the station multi rows.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Replicated station rows were not serialized**
- **Found during:** Task 3 (Surface production state in the existing building panel)
- **Issue:** `ProductionStationOperationState` replicated scalar fields, but its input/output multi rows were not serialized with the state, so server buffer changes would not reliably reach client presentation.
- **Fix:** Added custom component config serialization/deserialization for input and output rows, following the existing `SettlementSharedResources` pattern.
- **Files modified:** `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Components/ProductionStationOperationState.cs`
- **Verification:** Static diff review plus `rg "IComponentConfig<ProductionStationOperationState>|ProductionStationInputResource|ProductionStationOutputResource" Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic -g "*.cs"`
- **Commit:** Included in the final plan completion commit

**2. [Rule 1 - Bug] Optional fuel could duplicate a normal input resource**
- **Found during:** Task 1 (Add production processing domain rules and server system)
- **Issue:** Future recipes using the same resource as both input and fuel could pass catalog validation but under-reserve before commit.
- **Fix:** Added catalog validation that rejects recipes using the same resource as both input and fuel.
- **Files modified:** `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Catalogs/ProductionRecipeCatalog.cs`
- **Verification:** Static review of `ProductionRecipeCatalog.ValidateFuel`
- **Commit:** Included in the final plan completion commit

### Deferred Enhancements

None.

---

**Total deviations:** 2 auto-fixed (2 bug fixes), 0 deferred
**Impact on plan:** Both fixes are required for correctness of the planned production loop and do not add unrelated scope.

## Issues Encountered
- Unity code generation, compile and editor tests were not run by the agent because project rules forbid launching Unity batchmode or running `dotnet build`; manual Unity verification is still required.
- The referenced `ai/mvc_package_review.md` document was not present in the workspace. The UI work followed `ai/mvc_usage_guidelines.md` and existing settlement MVC patterns instead.

## Next Phase Readiness
Production processing and output claiming are source-complete and ready for `02-06-PLAN.md` after Unity codegen/compile/editor test and host/client verification.

---
*Phase: 02-resource-flow-economy*
*Completed: 2026-05-27*
