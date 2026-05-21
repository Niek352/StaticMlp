# Phase 4 Plan 3: Extraction Hauling Summary

**Lumber Camp and Stone Mine output now accumulates in building buffers and moves to stockpile through hauler-driven Settlement events.**

## Accomplishments
- Added server-side extraction operation state for Lumber Camp and Stone Mine with typed output resources, buffer capacity, enabled state, and worker slot count.
- Added Settlement-owned extraction bootstrap, buffer fill, and transfer systems so extracted raw resources do not enter shared storage until hauled.
- Added a HaulResources action package in Settlement.Workers that moves to extraction buildings and emits a transfer intent instead of mutating Settlement state.
- Expanded Settlement and AI worker tests for buffer fill, full-buffer pause, resource id mapping, demand selection, executor intent emission, and stockpile capacity clamping.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Components/ExtractionOperationState.cs` - Server-side extraction building buffer state.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Domain/ExtractionRules.cs` - Resource mapping and buffer clamp/remove rules.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Events/TransferExtractionOutputToStockpileEvent.cs` - Worker-to-Settlement transfer intent event.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Systems/Server/ServerExtractionBootstrapSystem.cs` - Adds extraction state to finished Lumber Camp and Stone Mine buildings.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Systems/Server/ServerExtractionOperationSystem.cs` - Fills enabled extraction buffers up to capacity.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Systems/Server/ServerTransferExtractionOutputToStockpileSystem.cs` - Applies transfer events by adding to stockpile-limited shared storage and subtracting accepted output from the extraction buffer.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/SettlementSharedResourcesGameplayFeature.cs` - Registers the extraction bootstrap, fill, and transfer systems.
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Actions/HaulExtractionOutput/*` - Adds the worker haul action package, collector, executor, and utility binding.
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Queries/SettlementWorkerDemandQuery.cs` - Adds extraction output as the first haul demand when stockpile capacity is available.
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Systems/Server/ServerSettlementWorkerTaskSyncSystem.cs` - Pushes HaulResources targets into the haul extraction blackboard slot.
- `Assets/Tests/Editor/Settlement/ExtractionOperationTests.cs` - Covers extraction rules, bootstrap, fill, and stockpile transfer capacity.
- `Assets/Tests/Editor/Ai/SettlementWorkersSystemsTests.cs` - Covers extraction haul demand, task sync, executor event emission, and action catalog discovery.
- `.planning/ROADMAP.md` - Marks Phase 4 Plan 3 and Phase 4 complete.

## Decisions Made
- Kept extraction output as Settlement-owned operation state. Settlement.Workers reads the state for demand/execution context and only emits `TransferExtractionOutputToStockpileEvent`.
- Filled extraction buffers at the Settlement gameplay stage before worker job selection so haulers can discover same-frame output; transfer application runs after AI task execution.
- Prioritized extraction output over workbench output for generic haul demand because this plan establishes the first raw resource chain.

## Deviations from Plan

None - plan executed as written. The plan-listed `SettlementSharedResources.cs` path had moved to `Runtime/Logic/Components`, and the implementation used the current file location.

## Issues Encountered
- Unity MCP validation could not run because no Unity Editor instance with MCP for Unity was connected.
- Unity EditMode tests were not run because agents must not launch Unity or run project build checks. Please run targeted Unity EditMode tests for `StaticMlp.Tests.Settlement` and `StaticMlp.Tests.Ai`.
- Local `git diff --check` completed with no whitespace errors, but Git reported it could not read `C:\Users\pavel\.config\git\ignore` due to permission denial.

## Next Step
Phase 4 complete, ready for `05-01-PLAN.md`.

---
*Phase: 04-npc-worker-economy*
*Completed: 2026-05-21*
