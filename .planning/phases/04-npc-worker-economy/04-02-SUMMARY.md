# Phase 4 Plan 2: Worker Demand Discovery Summary

**Settlement workers now discover typed economy demands by role capability.**

## Accomplishments
- Added typed worker demand facts for construction delivery/build, gathering, hauling, and processing with target entity, resource id, amount, and task type data.
- Added a read-only `SettlementWorkerDemandQuery` that reads construction, stockpile, and workbench operation state without mutating Settlement-owned components.
- Updated the worker job system to select assigned workers by `WorkerJobFlags` instead of only camp-builder role ids while preserving construction delivery/build behavior.
- Added AI task ids and blocking reasons for gather, haul, and process demand selection.
- Expanded settlement worker tests for demand facts, role-gated selection, existing construction behavior, and task sync for non-construction demand tasks.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Definitions/SettlementWorkerDemand.cs` - Adds the typed demand fact returned by worker demand queries.
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Queries/SettlementWorkerDemandQuery.cs` - Adds read-only demand discovery over construction, Stockpile, and Workbench state.
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/SettlementWorkerBlockingReason.cs` - Adds no-demand reasons for gather, haul, process, and unsupported role capabilities.
- `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/Components/AiTaskType.cs` - Adds gather, haul, and process task ids for worker demand sync.
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Systems/Server/ServerSettlementWorkerCampBuilderJobSystem.cs` - Refactors role selection through worker demand facts while retaining the existing component name.
- `Assets/Tests/Editor/Ai/AiTestServerWorldScope.cs` - Allows test workers to be created with any Stage1 worker role.
- `Assets/Tests/Editor/Ai/SettlementWorkersSystemsTests.cs` - Adds demand-query, role-selection, guard-ignore, and economy-task-sync coverage.
- `.planning/ROADMAP.md` - Marks Phase 4 Plan 2 complete.

## Decisions Made
- Kept `SettlementCampBuilderJobState` and `SettlementWorkerSummary` names for this plan. A rename would touch generated replication registration and presentation projections, so it should be handled as a complete rename in a dedicated follow-up if desired.
- Kept the replicated job state shape unchanged. Resource id and amount live in `SettlementWorkerDemand`, avoiding manual edits to generated replication metadata.
- Treated workbench input shortage as gather demand for raw resources only, workbench output as haul demand when an enabled stockpile exists, and ready workbench inputs as process demand.

## Deviations from Plan

None - plan executed as written. The retained component names were allowed by the plan's rename guidance and documented above.

## Issues Encountered
- Unity MCP validation could not run because no Unity Editor instance with MCP for Unity was connected.
- Unity EditMode tests were not run because agents must not launch Unity or run project build checks. Please run the targeted Unity EditMode tests for `StaticMlp.Tests.Ai` settlement worker coverage.

## Next Step
Ready for `04-03-PLAN.md`.

---
*Phase: 04-npc-worker-economy*
*Completed: 2026-05-21*
