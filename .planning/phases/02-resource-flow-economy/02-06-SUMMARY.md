# Phase 2 Plan 6: Economy Sinks Summary

**Fuel requirements added to all workbench recipes with explicit blocked-state reporting; construction and recipe input/output validation tightened to enforce resource usage flags**

## Accomplishments
- Added `FuelId` requirement (1 unit per recipe cycle) to all three workbench recipes (Planks, Simple Parts, Repair Kits).
- Introduced `ProductionStationBlockedReason` enum and replicated `BlockedReasonValue` field in `ProductionStationOperationState` so missing fuel, full output buffer, no workers, or missing inputs are reported explicitly instead of silently skipped.
- Added `IsBlockedByFuel` domain rule that checks station buffer and shared storage before work advances, preventing fuel from being silently ignored.
- Tightened `ProductionRecipeCatalog` validation to require `ProductionInput` flag on recipe inputs, `ProductionOutput` flag on recipe outputs, and `Fuel` flag on fuel resources, catching invalid catalog entries at load time.
- Updated `ServerProductionStationProcessingSystem` to set the blocked reason before every `continue`, preserving explicit state for UI or debugging.
- Added editor test coverage for fuel-missing blocked state and updated existing tests for the new fuel row and shared-storage fuel parameter.
- Documented deferred sink boundaries in the roadmap: NPC upkeep deferred to Phase 3, raid repairs deferred to Phase 8.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Definitions/ProductionStationBlockedReason.cs` - Added blocked-reason enum for production stations.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Components/ProductionStationOperationState.cs` - Added `BlockedReasonValue` with custom serialization.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Catalogs/ProductionRecipeCatalog.cs` - Added fuel requirements to all workbench recipes; added usage-flag validation for inputs, outputs, and fuel.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Domain/ProductionStationRules.cs` - Added `IsBlockedByFuel` rule.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Systems/Server/ServerProductionStationProcessingSystem.cs` - Writes explicit `BlockedReasonValue` instead of silently continuing.
- `Assets/Tests/Editor/Settlement/WorkbenchOperationTests.cs` - Updated for fuel rows and added blocked-by-fuel test.
- `Assets/Tests/Editor/Settlement/SettlementOperationTestWorldScope.cs` - Added `fuel` parameter to `CreateSharedResources`.
- `docs/Roadmap по GDD.md` - Added deferred sink boundary notes.

## Decisions Made
- Fuel is consumed once per recipe completion (at commit time) after being reserved from shared storage into the station input buffer, matching the existing reservation pattern. This keeps fuel deterministic and server-authoritative.
- Blocked state is replicated so client presentation can surface why a station is idle without querying shared storage directly.
- All three Phase 2 workbench recipes require fuel to create immediate demand for the `Fuel` resource; future stations (smelter, cooking station, etc.) can opt into fuel independently via their recipe definitions.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Unity code generation, compile and editor tests were not run by the agent because project rules forbid launching Unity batchmode or running `dotnet build`; manual Unity verification is still required.

## Next Phase Readiness
- Fuel and recipe sinks are owned by Settlement logic and ready for UI surfacing.
- No temporary NPC upkeep or raid repair systems were introduced.
- Resource usage validation catches invalid sink resources at catalog load time.
- Ready for `02-07-PLAN.md`.

---
*Phase: 02-resource-flow-economy*
*Completed: 2026-05-27*
