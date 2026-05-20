# Phase 07 Plan 01: Cell Attention Summary

**Reason-based cell attention with explicit cause fields, decay, and temporary threat-budget compatibility**

## Accomplishments
- Added `CellAttention` to director cells with named noise, trespass, combat, loot, and faction alarm fields.
- Replaced passive threat accumulation with explicit player input into `CellAttentionInputSystem`; standing or walking in a cell now adds zero pressure.
- Added `CellAttentionDecaySystem` so attention returns toward calm after causes stop.
- Updated CombatDirector tests around passive calm, named attention causes, clamping, decay, and deterministic results.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Contracts/Components/CellAttention.cs` - Director-cell attention contract.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Contracts/Components/PlayerCombatAttention.cs` - Explicit per-player combat attention input.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Contracts/Components/ThreatBudget.cs` - Marked temporary compatibility state with `[Obsolete("Temp")]`.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/CellAttentionInputSystem.cs` - Aggregates explicit player causes into attention and mirrors to temporary budget.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/CellAttentionDecaySystem.cs` - Decays attention fields toward zero.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/ThreatBudgetAccumulationSystem.cs` - Removed old passive threat accumulator.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/CombatCellTrackingSystem.cs` - Seeds `CellAttention` on the director entity.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/PlayerThreatInputSystem.cs` - Removed passive time-in-cell/source-proximity noise and split combat from noise.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/WorldResources/EncounterDirectorConfig.cs` - Replaced passive threat config with attention multipliers and decay.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/CombatDirectorGameplayFeature.cs` - Registered attention input and decay systems in the server pipeline.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/AGENTS.md` - Updated current CombatDirector flow and debug checklist.
- `Assets/Tests/Editor/CombatDirector/CombatDirectorRuntimeSystemsTests.cs` - Replaced passive budget assumptions with attention behavior tests.
- `Assets/Tests/Editor/CombatDirector/CombatDirectorContractsTests.cs` - Added compactness coverage for `CellAttention`.
- `.planning/ROADMAP.md` - Marked Phase 07 complete.
- `.planning/phases/07-cell-attention/07-01-SUMMARY.md` - Execution summary.

## Decisions Made
- Kept `ThreatBudget` only as temporary compatibility because existing Phase 08-dependent phase, selection, validation, spawn build, and spawn apply systems still consume it.
- Treated attack input as combat attention, harvest request input as noise attention, and carried loot as continuous loot attention scaled by fixed timestep.
- Left trespass and faction alarm as explicit `CellAttention` fields with config multipliers but zero producers until the owning future systems exist.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Preserved temporary budget bridge for existing phase/spawn systems**
- **Found during:** Task 2 (Replace passive accumulation with reason-based input and decay)
- **Issue:** `DirectorPhaseSystem`, spawn selection/build/validation, and enemy spawn apply still depend on `ThreatBudget`; removing it completely would pull Phase 08 pressure-event migration into this plan.
- **Fix:** Removed `ThreatBudgetAccumulationSystem`, marked `ThreatBudget` `[Obsolete("Temp")]`, and mirrored `CellAttention.Current` into the temporary budget until the next phase migrates the remaining systems.
- **Files modified:** `ThreatBudget.cs`, `CellAttentionInputSystem.cs`, `CellAttentionDecaySystem.cs`, existing phase/spawn tests.
- **Verification:** Search confirms `ThreatBudgetAccumulationSystem` runtime references are gone and attention tests cover the new source of pressure.

---

**Total deviations:** 1 auto-fixed (1 blocking), 0 deferred
**Impact on plan:** The bridge keeps current phase/spawn behavior compiling while making `CellAttention` the pressure source for Phase 08.

## Issues Encountered
Unity editor tests were not run by the agent, per project rule. Please run the CombatDirector editor tests in Unity.

## Next Phase Readiness
Ready for `08-01-PLAN.md`.

Phase 08 can migrate `DirectorPhaseSystem`, spawn request construction, validation, and enemy spawn apply away from temporary `ThreatBudget` and onto open-world phase/encounter contracts driven by `CellAttention`.

---
*Phase: 07-cell-attention*
*Completed: 2026-05-20*
