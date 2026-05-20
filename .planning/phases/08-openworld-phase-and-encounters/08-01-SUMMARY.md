# Phase 08 Plan 01: OpenWorld Phase And Encounters Summary

**Conservative open-world director phases with explicit encounter state and pressure-only wave request gating**

## Accomplishments
- Replaced old wave-first director phases with `Dormant`, `Ambient`, `Contact`, `Suspicion`, `Escalation`, `PressureEvent`, `Recovery`, and `Cooldown`.
- Added `EncounterState`, `EncounterKind`, and `EncounterIntensity` plus server systems for contact detection, lifetime updates, and recovery.
- Gated source selection, request building, and request validation behind `DirectorPhase.PressureEvent` so ambient/contact/recovery states do not produce wave requests.
- Updated tests for deterministic phase serialization, passive ambient behavior, suspicion decay, solo encounter resolution, and blocked non-pressure spawn requests.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Contracts/Definitions/DirectorPhase.cs` - Open-world phase enum values.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Contracts/Definitions/EncounterKind.cs` - Encounter kind contract.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Contracts/Definitions/EncounterIntensity.cs` - Encounter intensity contract.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Contracts/Components/EncounterState.cs` - Runtime encounter state contract.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Contracts/Components/DirectorState.cs` - Preserved byte/float/float serialization shape while renaming the pressure-event timer.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/DirectorPhaseSystem.cs` - Conservative open-world phase transitions.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/EncounterContactDetectionSystem.cs` - Creates small encounter state from existing alive enemies in the combat cell.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/EncounterLifetimeSystem.cs` - Advances encounter time and alive counts.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/EncounterRecoverySystem.cs` - Resolves completed encounters into recovery.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/CombatDirectorGameplayFeature.cs` - Registers encounter systems in the server pipeline.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/SpawnSourceSelectionSystem.cs` - Limits source selection to pressure events.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/SpawnRequestBuildSystem.cs` - Limits wave request building to pressure events.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/SpawnRequestValidationSystem.cs` - Rejects requests outside pressure events.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Presentation/Systems/Client/EnemySpawnAudioSystem.cs` - Renamed peak-entry audio behavior to pressure-event entry.
- `Assets/Tests/Editor/CombatDirector/CombatDirectorContractsTests.cs` - Contract ordering and compactness coverage for new phase/encounter contracts.
- `Assets/Tests/Editor/CombatDirector/CombatDirectorRuntimeSystemsTests.cs` - Runtime coverage for ambient, contact, recovery, and pressure-event request behavior.
- `.planning/ROADMAP.md` - Marked Phase 08 complete.
- `.planning/phases/08-openworld-phase-and-encounters/08-01-SUMMARY.md` - Execution summary.

## Decisions Made
- `PressureEvent` is not reached automatically in this plan because explicit pressure eligibility/reason contracts are not available yet.
- Current encounter detection uses existing alive enemies inside the director cell. Camp, lair, patrol, territory, and guarded-resource facts are deferred inputs for later phases rather than mocked fallback facts.
- The old wave builder remains only behind `DirectorPhase.PressureEvent`; no temporary compatibility path was added.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Updated compile-facing presentation and contract tests after enum replacement**
- **Found during:** Task 1 (Replace wave-first phase semantics)
- **Issue:** Replacing `DirectorPhase.Peak` and the old enum names left stale references in client audio presentation and contract tests outside the plan's named implementation files.
- **Fix:** Updated the audio cue to trigger on `PressureEvent` and updated contract tests to assert the new deterministic enum ordering.
- **Files modified:** `EnemySpawnAudioSystem.cs`, `CombatDirectorContractsTests.cs`
- **Verification:** `rg` confirms no stale `DirectorPhase.Calm`, `BuildUp`, `Peak`, `Relief`, or `TimeSinceLastPeak` C# references remain in CombatDirector source/tests.

---

**Total deviations:** 1 auto-fixed (1 blocking), 0 deferred
**Impact on plan:** The extra edits were required to keep the enum replacement coherent. No pressure eligibility or source classification scope was pulled forward.

## Issues Encountered
Unity editor tests were not run by the agent, per project rule. Please run the CombatDirector editor tests in Unity.

## Next Phase Readiness
Ready for `09-01-PLAN.md`.

Phase 09 can add source classification and explicit pressure eligibility/reason inputs. `PressureEvent` is now isolated as the only phase that can use the old wave request builder.

---
*Phase: 08-openworld-phase-and-encounters*
*Completed: 2026-05-20*
