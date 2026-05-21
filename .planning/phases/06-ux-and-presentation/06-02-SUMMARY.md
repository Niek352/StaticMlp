# Phase 6 Plan 2: Generic Building Actions Summary

**The Stage1 context panel now renders prepared building actions instead of fixed Deposit/Build branches.**

## Accomplishments
- Added a presentation-only building action model carrying interaction kind, label, enabled state, disabled reason, and target entity.
- Updated Stage1 building context state to derive primary/secondary actions from construction phase, construction resources, and building catalog operation interactions.
- Updated the context panel controller and view so the view renders prepared action labels while the controller dispatches actions through typed construction requests or a local operation-open intent.
- Added presentation tests for Deposit, Build, disabled Build, finished Workbench operation actions, and operation-open intent forwarding.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Definitions.meta` - Unity metadata for the new presentation definitions bucket.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Definitions/BuildingAvailableActionPresentation.cs` - Defines generic building action presentation data.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Definitions/BuildingAvailableActionPresentation.cs.meta` - Unity metadata for the new action model script.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/WorldResources/Stage1BuildingOperationOpenIntent.cs` - Adds a client presentation resource for finished-building open/action intent.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/WorldResources/Stage1BuildingOperationOpenIntent.cs.meta` - Unity metadata for the new operation intent script.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Stage1ContextPanelState.cs` - Carries building display name and generic building actions.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/ClientStage1ContextPanelStateSystem.cs` - Builds action state from projected construction state and catalog interactions.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/ClientStage1PresentationBootstrapSystem.cs` - Registers the operation-open intent presentation resource.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Stage1ContextPanelController.cs` - Dispatches generic building actions to typed requests or local operation-open intent.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Stage1ContextPanelView.cs` - Renders prepared action labels and disabled reasons.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/StaticMlp.Features.Settlement.Presentation.asmdef` - Adds the building catalog reference needed by presentation state construction.
- `Assets/Tests/Editor/Combat/Stage1PresentationClientWorldScope.cs` - Lets tests create focused construction sites with explicit building ids.
- `Assets/Tests/Editor/Combat/Stage1PresentationStateSystemsTests.cs` - Covers generic building action state and operation-open controller intent.
- `.planning/ROADMAP.md` - Marks `06-02` complete and Phase 6 as 2/3 plans complete.
- `.planning/phases/06-ux-and-presentation/06-02-SUMMARY.md` - Records this plan result.

## Decisions Made
- Kept construction gameplay mutations on existing `DepositConstructionResourcesRequestEvent` and `BuildConstructionRequestEvent` paths.
- Added `Stage1BuildingOperationOpenIntent` as local presentation intent because finished-building open/details/queue actions are not authoritative gameplay mutations.
- Kept the MonoBehaviour view passive: it renders prepared labels, interactable state, and disabled reason without deciding which gameplay action is valid.

## Deviations from Plan

### Path Adjustment

**1. Placed new presentation files in approved buckets**
- **Found during:** Task 1 (Add building available action presentation model)
- **Issue:** The plan listed flat `Runtime/Presentation` paths, but current project layout rules require new presentation definitions/resources to live in approved role buckets.
- **Fix:** Placed `BuildingAvailableActionPresentation` under `Runtime/Presentation/Definitions` and `Stage1BuildingOperationOpenIntent` under `Runtime/Presentation/WorldResources`.
- **Files modified:** `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Definitions/*`, `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/WorldResources/Stage1BuildingOperationOpenIntent.cs`
- **Verification:** `git diff --check` passes apart from pre-existing Git ignore permission warnings.
- **Commit:** pending

---

**Total deviations:** 1 file-placement adjustment, 0 deferred
**Impact on plan:** Behavior and public namespaces match the plan; file placement follows the active architecture rules.

## Issues Encountered
- Unity EditMode tests were not run because agents must not launch Unity or run project build checks. Please run `Stage1PresentationStateSystemsTests` in Unity EditMode tests.
- Git continues to report pre-existing permission warnings when reading `C:\Users\pavel\.config\git\ignore`; this did not block local diff checks.

## Next Step
Ready for `06-03-PLAN.md`.

---
*Phase: 06-ux-and-presentation*
*Completed: 2026-05-21*
