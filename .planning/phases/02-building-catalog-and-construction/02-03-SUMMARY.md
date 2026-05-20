# Phase 2 Plan 3: Construction Completion Facts Summary

**Construction completion now exposes a generic event for operation bootstrap.**

## Accomplishments
- Added `BuildingConstructionCompletedEvent` as a server-side ECS fact carrying `EntityGID` for the finished building, `BuildingId`, `SettlementAnchorId`, and construction transform data.
- Emitted the generic completion fact after `ServerCompleteConstructionSystem` successfully spawns the finished building.
- Preserved Stage1 repair compatibility for Camp Core while preventing non-Camp Core buildings from emitting `Stage1RepairCompletedEvent`.
- Updated server test setup so the new Buildings contracts event assembly is registered for EditMode tests.
- Extended repair-flow tests to verify generic completion emission and to prove Stockpile completion does not advance the Camp Core repair flow.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Contracts/BuildingConstructionCompletedEvent.cs` - Adds the generic construction completion fact.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Contracts/BuildingConstructionCompletedEvent.cs.meta` - Unity script metadata for the new contract file.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Server/ServerCompleteConstructionSystem.cs` - Emits the generic fact and keeps Stage1 repair facts scoped to Camp Core.
- `Assets/Tests/Editor/Combat/CombatTestServerWorldScope.cs` - Registers Buildings contracts event types in the test server world and lets tests spawn non-Camp Core construction sites.
- `Assets/Tests/Editor/Combat/Stage1RepairFlowTests.cs` - Verifies generic completion facts and existing Stage1 repair semantics.
- `.planning/ROADMAP.md` - Marks Phase 2 complete.

## Decisions Made
- Kept `BuildingConstructionCompletedEvent` local to server ECS gameplay instead of making it a replicated network event; operation bootstrap systems can consume it in the same server world.
- Included `ConstructionTransform` in the fact so operation owners can seed location-aware state without reaching back into despawned construction sites.
- Scoped `Stage1RepairCompletedEvent` to `BuildingCatalogData.CampCoreId`, matching its existing repair-flow meaning.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Unity EditMode tests were not run because agents must not launch Unity or run project build checks.

## Next Step
Phase 2 complete, ready for `03-01-PLAN.md`.

---
*Phase: 02-building-catalog-and-construction*
*Completed: 2026-05-20*
