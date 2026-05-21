# Phase 5 Plan 2: Stage1 Objective UI Summary

**Stage1 presentation now guides the player through settlement economy gates before loadout preparation.**

## Accomplishments
- Added client HUD objective kinds and labels for stockpile placement, shelter placement, extraction startup, and workbench startup.
- Mapped the new replicated Stage1 flow objectives and hints into `Stage1HudState` without moving gameplay rules into presentation.
- Tightened the server flow view state so loadout preparation opens only after `WorkbenchOnline`.
- Updated presentation test fixtures and tests to cover every new economy objective, hint, and loadout lock state.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Stage1/Runtime/Contracts/Stage1FlowObjective.cs` - Adds the settlement economy objective contract values.
- `Assets/Scripts/StaticMlp/Features/Stage1/Runtime/Contracts/Stage1FlowHint.cs` - Adds the settlement economy hint contract values.
- `Assets/Scripts/StaticMlp/Features/Stage1/Runtime/Logic/Systems/Server/ServerStage1FlowViewStateSystem.cs` - Resolves new economy objectives/hints and locks loadout preparation until `WorkbenchOnline`.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Stage1ObjectiveKind.cs` - Adds client HUD objective kinds for the new settlement gates.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/ClientStage1HudStateSystem.cs` - Maps server flow objectives/hints into HUD state strings.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Stage1HudController.cs` - Adds concise HUD objective labels for the new stages.
- `Assets/Tests/Editor/Combat/Stage1PresentationStateSystemsTests.cs` - Covers each economy objective and loadout preparation availability.
- `Assets/Tests/Editor/Combat/Stage1PresentationClientWorldScope.cs` - Mirrors the server flow read model for client presentation tests.
- `Assets/Tests/Editor/Combat/CombatTestServerWorldScope.cs` - Keeps server test fixture flow state aligned with the new objective/hint chain.
- `.planning/ROADMAP.md` - Marks Phase 5 plans 1 and 2 complete and Phase 5 in progress.
- `.planning/phases/05-stage1-progression-and-loadout/05-02-SUMMARY.md` - Records this plan result.

## Decisions Made
- `CanOpenLoadoutPreparation` now requires `Stage1SettlementProgressStage.WorkbenchOnline`. This resolves the current plan's loadout lock requirement and prevents the UI from opening during stockpile, shelter, and extraction gates.
- HUD presentation remains a passive read model: it maps replicated flow state to concise strings and does not validate placement, mutate operations, or own gameplay truth.
- Combat priority stays above settlement guidance: defeated/active/available boss states, raid states, and active expedition state still resolve before economy objectives.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Aligned test helper flow state outside the listed task files**
- **Found during:** Task 2 (Update HUD presentation state and tests)
- **Issue:** `CombatTestServerWorldScope` still synthesized the old objective/hint chain and old loadout gate, which could leave server-side tests with stale initial flow state.
- **Fix:** Updated the helper's synthesized `Stage1FlowViewState` to match the new economy objectives, hints, and `WorkbenchOnline` loadout gate.
- **Files modified:** `Assets/Tests/Editor/Combat/CombatTestServerWorldScope.cs`
- **Verification:** `git diff --check` passed for touched files.

---

**Total deviations:** 1 auto-fixed blocking consistency issue, 0 deferred
**Impact on plan:** Supporting test fixture alignment only; no new architecture or gameplay surface was introduced.

## Issues Encountered
- Unity EditMode tests were not run because agents must not launch Unity or run project build checks. Please run `Stage1PresentationStateSystemsTests` through Unity EditMode tests.
- Git continues to report pre-existing permission warnings when reading `C:\Users\pavel\.config\git\ignore`; this did not block local diff checks.
- Unrelated deleted files under `Packages/com.staticmlp.layer-proc-lite/` were already present and were not modified by this plan.

## Next Step
Ready for `05-03-PLAN.md`.

---
*Phase: 05-stage1-progression-and-loadout*
*Completed: 2026-05-21*
