# Phase 3 Plan 1: Stockpile Operation Summary

**Stockpile buildings now provide typed settlement storage capacity.**

## Accomplishments
- Added `StockpileOperationState` as server-authoritative operation state for finished Stockpile buildings.
- Routed Stockpile construction completion through a Settlement-owned server bootstrap system that increases shared storage capacity.
- Enforced explicit capacity clamping in `SettlementSharedResourcesAccess` and covered over-capacity behavior in tests.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Components/StockpileOperationState.cs` - Tracks each Stockpile building's enabled state and capacity contribution.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Domain/StockpileRules.cs` - Provides pure capacity contribution and acceptance rules.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Systems/Server/ServerStockpileOperationBootstrapSystem.cs` - Consumes building completion facts and applies Stockpile operation state.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/StaticMlp.Features.Settlement.Logic.asmdef` - References Buildings contracts so Settlement can consume completion facts directly.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/BuildingsGameplayFeature.cs` - Removes the pre-existing Stockpile forwarding system registration.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Server/ServerForwardStockpileContributionSystem.cs` - Removed the pre-existing Buildings-side Stockpile operation forwarder.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Events/StockpileCapacityContributionEvent.cs` - Removed the extra forwarding event.
- `Assets/Tests/Editor/Settlement/SettlementOperationTestWorldScope.cs` - Adds a focused server-world test scope for Settlement operation bootstrap tests.
- `Assets/Tests/Editor/Settlement/StockpileOperationTests.cs` - Covers capacity clamping, over-capacity rejection, and Stockpile bootstrap state.
- `Assets/Tests/Editor/Settlement/StaticMlp.Tests.Settlement.asmdef` - Adds references needed by operation bootstrap tests.
- `.planning/ROADMAP.md` - Marks Phase 3 as in progress with 1/3 plans complete.

## Decisions Made
- Kept Stockpile operation state server-authoritative instead of adding a replicated component because no client presentation or UI consumption exists yet.
- Settlement consumes `BuildingConstructionCompletedEvent` directly, so Buildings does not know Stockpile internals or mutate Settlement operation state.
- Shared storage keeps the settlement-owned aggregate, while finished Stockpiles contribute finite capacity.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Replaced pre-existing Buildings-side Stockpile forwarding**
- **Found during:** Task 2 (Bootstrap stockpile state from building completion events)
- **Issue:** Existing partial code routed Stockpile capacity through a Buildings feature system and a Settlement-specific forwarding event, which made Buildings aware of Settlement operation semantics.
- **Fix:** Removed the forwarding system/event and made `ServerStockpileOperationBootstrapSystem` consume `BuildingConstructionCompletedEvent` directly.
- **Files modified:** `BuildingsGameplayFeature.cs`, `ServerForwardStockpileContributionSystem.cs`, `StockpileCapacityContributionEvent.cs`, `ServerStockpileOperationBootstrapSystem.cs`
- **Verification:** Static reference search confirms no stale forwarding references remain.

---

**Total deviations:** 1 auto-fixed (1 bug), 0 deferred
**Impact on plan:** The fix aligns the implementation with the requested Settlement-owned operation boundary.

## Issues Encountered
- Unity EditMode tests were not run because agents must not launch Unity or run project build checks.

## Next Step
Ready for `03-02-PLAN.md`.
