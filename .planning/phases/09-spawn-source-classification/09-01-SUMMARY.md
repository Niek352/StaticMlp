# Phase 09 Plan 01: Spawn Source Classification Summary

**Open-world spawn sources with explicit kind, biome/faction metadata, and ambient/escalation/pressure usage flags**

## Accomplishments
- Added `SpawnSourceKind` and extended `SpawnSource` with source role, faction id, biome id, and explicit usage flags.
- Mapped current open-world spawn placements into explicit source records: wildlife placements become ambient-only points, and highland placements become rift sources usable for escalation and pressure events.
- Updated pressure source selection and request validation so ambient-only sources cannot be selected or validated for pressure-event spawning.
- Preserved the AiNavigation boundary; navigation consumers still read source position, active state, and radius without deciding director phase, encounter kind, or enemy composition.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Contracts/Definitions/SpawnSourceKind.cs` - Open-world source kind contract.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Contracts/Definitions/SpawnSourceKind.cs.meta` - Unity metadata for the new contract file.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Contracts/Components/SpawnSource.cs` - Added kind, faction id, biome id, and usage flags.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Domain/SpawnSourcePlacementRules.cs` - Centralized placement-to-source mapping and kept unknown placement kinds fail-fast.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/SpawnSourcePlacementSeedSystem.cs` - Seeds full source classification from placement rules.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/SpawnSourceSelectionSystem.cs` - Filters pressure selection by `AllowsPressureEvent`.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/SpawnRequestBuildSystem.cs` - Carries source kind into spawn requests.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/SpawnRequestValidationSystem.cs` - Revalidates source kind and pressure usage before accepting requests.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Components/SelectedSpawnSource.cs` - Stores selected source kind.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Events/SpawnRequest.cs` - Stores request source kind.
- `Assets/Tests/Editor/CombatDirector/CombatDirectorContractsTests.cs` - Added source kind ordering and default flag tests.
- `Assets/Tests/Editor/CombatDirector/CombatDirectorRuntimeSystemsTests.cs` - Added placement mapping, unknown placement, pressure selection, and pressure validation coverage.
- `.planning/ROADMAP.md` - Marked Phase 09 complete.
- `.planning/phases/09-spawn-source-classification/09-01-SUMMARY.md` - Execution summary.

## Decisions Made
- Kept `SpawnSourceType` as compatibility metadata for current spawn VFX and enemy spawn source presentation while adding `SpawnSourceKind` as the open-world gameplay classification.
- Existing wildlife placement kind `1` is ambient-only. Existing highland placement kind `2` maps to a rift that allows escalation and pressure events.
- AiNavigation code did not need changes because the `SpawnSource` fields it reads remain intact.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None.

## Next Phase Readiness
Ready for `10-01-PLAN.md`.

Phase 10 can build ambient requests from `AllowsAmbient` sources without reusing pressure-only rift sources.

---
*Phase: 09-spawn-source-classification*
*Completed: 2026-05-20*
