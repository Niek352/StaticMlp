# Phase 5 Plan 1: Stage1 Economy Gates Summary

**Stage1 now has explicit settlement economy gates before loadout preparation.**

## Accomplishments
- Extended `Stage1SettlementProgressStage` with four new economy gate stages inserted between `WorkerAssigned` and `LoadoutPrepared`: `StockpilePlaced`, `ShelterPlaced`, `ExtractionOnline`, `WorkbenchOnline`. `LoadoutPrepared` shifted from ordinal 5 to 9; all adjacent-byte monotonicity for `CanAdvanceTo` is preserved.
- Added four typed Stage1 fact events: `Stage1StockpilePlacedEvent`, `Stage1ShelterPlacedEvent`, `Stage1ExtractionOnlineEvent`, `Stage1WorkbenchOnlineEvent` — all following the same `IEvent` / `SettlementAnchorId` pattern as existing Stage1 events.
- Updated `ServerStage1FlowSystem` to register and consume all four new event receivers, chaining `WorkerAssigned → StockpilePlaced → ShelterPlaced → ExtractionOnline → WorkbenchOnline → LoadoutPrepared` via the existing `AdvanceFromFact` pattern.
- Fixed the existing `ServerStage1FlowSystem_WhenLoadoutPreparedEventReceived_AdvancesToLoadoutPrepared` test to start from `WorkbenchOnline` (the correct new prior stage instead of `WorkerAssigned`).
- Added 8 new tests: 4 advancement tests (one per economy gate) and 4 out-of-order guard tests verifying that out-of-order facts are silently dropped.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Stage1/Runtime/Contracts/Stage1SettlementProgressStage.cs` — Added four new stages; `LoadoutPrepared` is now ordinal 9.
- `Assets/Scripts/StaticMlp/Features/Stage1/Runtime/Contracts/Stage1StockpilePlacedEvent.cs` — **NEW** typed fact event.
- `Assets/Scripts/StaticMlp/Features/Stage1/Runtime/Contracts/Stage1ShelterPlacedEvent.cs` — **NEW** typed fact event.
- `Assets/Scripts/StaticMlp/Features/Stage1/Runtime/Contracts/Stage1ExtractionOnlineEvent.cs` — **NEW** typed fact event.
- `Assets/Scripts/StaticMlp/Features/Stage1/Runtime/Contracts/Stage1WorkbenchOnlineEvent.cs` — **NEW** typed fact event.
- `Assets/Scripts/StaticMlp/Features/Stage1/Runtime/Logic/Systems/Server/ServerStage1FlowSystem.cs` — Added four new event receivers, Init/Destroy registration, and `AdvanceFromFact` calls. Updated `LoadoutPreparedEvent` predecessor from `WorkerAssigned` to `WorkbenchOnline`.
- `Assets/Tests/Editor/Combat/Stage1RepairFlowTests.cs` — Fixed broken `LoadoutPrepared` test; added 8 new economy gate tests.

## Decisions Made
- `WorkerAssigned` stays immediately after `CampRepaired` (ordinal 4) — the worker assignment remains the first post-repair action.
- `LoadoutPrepared` is now the final economy gate (ordinal 9). Flow is: `WorkerAssigned → StockpilePlaced → ShelterPlaced → ExtractionOnline → WorkbenchOnline → LoadoutPrepared`.
- Existing `>= WorkerAssigned` and `< LoadoutPrepared` byte threshold comparisons in `ServerStage1FlowViewStateSystem`, `CombatTestServerWorldScope`, and `Stage1PresentationClientWorldScope` remain valid without change — the new stages fall naturally between the existing ordinals and preserve the semantic intent of each comparison.
- `CanOpenLoadoutPreparation` in the view state keeps its `>= WorkerAssigned` threshold (UX gate stays open during economy phases); the actual flow requires `WorkbenchOnline` before `LoadoutPrepared` is reachable.

## Issues Encountered
- Unity EditMode tests were not run because agents must not launch Unity or run project build checks. Please run targeted Unity EditMode tests for `StaticMlp.Tests.Combat` (particularly `Stage1RepairFlowTests`).
- `.meta` files for the four new event contracts are not included — Unity Editor will auto-generate them on next import.

## Deviations from Plan
None — plan executed exactly as written.

## Next Step
Ready for `05-02-PLAN.md`.

---
*Phase: 05-stage1-progression-and-loadout*
*Completed: 2026-05-21*
