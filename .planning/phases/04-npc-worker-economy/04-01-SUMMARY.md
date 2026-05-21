# Phase 4 Plan 1: Worker Role Catalog Summary

**Settlement workers now have Stage1 role, runtime, and NPC definition mappings for Builder, Gatherer, Hauler, Processor, and Guard.**

## Accomplishments
- Added Stage1 worker role ids and allowed job flags for building, gathering, hauling, processing, guarding, and maintenance.
- Added runtime profiles for each Stage1 worker role with stable behavior ids and a shared settlement worker network archetype.
- Added seeded NPC definitions and Settlement.Workers-to-Npc mappings for each Stage1 worker role without adding worker runtime fields to `NpcIdentity`.
- Expanded editor tests around worker role flags, runtime profiles, NPC mappings, and seeded NPC definition role flags.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/WorkerJobFlags.cs` - Adds Stage1 economy job flags.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/WorkerRoleCatalog.cs` - Defines Builder, Gatherer, Hauler, Processor, and Guard role data.
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/SettlementWorkerRuntimeProfileCatalog.cs` - Maps each worker role to runtime archetype, behavior id, and health.
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/SettlementWorkerBehaviorIds.cs` - Adds behavior ids for the new worker runtime profiles.
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/SettlementWorkerBehaviorContributionSource.cs` - Registers minimal idle behavior entries for new role behavior ids.
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/SettlementWorkerNetworkArchetypeIds.cs` - Adds a generic settlement worker archetype alias while preserving camp-builder compatibility.
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/SettlementWorkersGameplayFeature.cs` - Registers the shared settlement worker archetype.
- `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Catalogs/SettlementWorkerNpcProfileCatalog.cs` - Maps worker roles to seeded NPC definitions.
- `Assets/Scripts/StaticMlp/Features/Npc/Runtime/Logic/Catalogs/NpcDefinitionCatalog.cs` - Adds seeded NPC definitions for Stage1 worker product identities.
- `Assets/Tests/Editor/Ai/SettlementWorkersSystemsTests.cs` - Covers worker role flags, runtime profiles, behavior catalog ids, and worker-to-NPC mappings.
- `Assets/Tests/Editor/Npc/NpcDefinitionCatalogTests.cs` - Covers seeded worker NPC definition role flags.
- `.planning/ROADMAP.md` - Marks Phase 4 Plan 1 complete.

## Decisions Made
- Kept `CampBuilderId` as an alias of `BuilderId` to preserve existing camp repair seed data and tests while making Builder the Stage1 catalog role.
- Reused a shared settlement worker network archetype for all Stage1 roles because no role-specific prefab or presentation asset exists yet.
- Added distinct behavior ids per role, but only registered idle behavior for non-builder roles so gather, haul, process, and guard task logic remains owned by later demand-discovery plans.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Registered behavior catalog entries for new runtime profile ids**
- **Found during:** Task 2 (runtime profiles and NPC profile mappings)
- **Issue:** `ServerAiUtilityDecisionSystem` throws when an active AI agent uses a behavior id missing from the runtime AI catalog. New worker runtime profiles introduced new behavior ids.
- **Fix:** Added idle behavior entries for Gatherer, Hauler, Processor, and Guard behavior ids without adding demand-specific AI task logic.
- **Files modified:** `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/SettlementWorkerBehaviorContributionSource.cs`, `Assets/Tests/Editor/Ai/SettlementWorkersSystemsTests.cs`
- **Verification:** Added a test assertion that the discovered AI action catalog contains every Stage1 settlement worker behavior id.

---

**Total deviations:** 1 auto-fixed missing critical issue, 0 deferred
**Impact on plan:** The added behavior registry entries are required to keep the new runtime profiles valid under the existing fail-fast AI decision system. No gather, haul, process, or guard behavior logic was added ahead of later plans.

## Issues Encountered
- Unity EditMode tests were not run because agents must not launch Unity or run project build checks. Please run the targeted Unity EditMode tests for `StaticMlp.Tests.Npc` and the settlement worker tests.

## Next Phase Readiness
Ready for `04-02-PLAN.md`.

---
*Phase: 04-npc-worker-economy*
*Completed: 2026-05-21*
