# Phase 2 Plan 11: Unlock-Aware Building And Recipe UX Summary

**Building cards and Workbench recipe presentation now read Settlement unlock availability before offering player actions**

## Accomplishments
- Added availability and locked-reason fields to building menu card presentation.
- Disabled locked building cards in the view and blocked stale/programmatic locked selection before placement state changes.
- Added a Settlement-owned client unlock read-model resource shared by building and recipe availability queries.
- Aligned production summaries with `AvailableRecipesQuery` so locked recipes are not listed as normal options.
- Added focused editor tests for building-card lock presentation and recipe availability filtering.
- Updated Settlement UX documentation with locked card, locked recipe, and Phase 3 NPC-specific unlock reason notes.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/WorldResources/ClientSettlementUnlockState.cs` - Client-core unlock read model over projected settlement level and constructed buildings.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Systems/Client/ClientSettlementProgressionBootstrapSystem.cs` - Registers the client unlock read-model resource.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Domain/AvailableBuildingsQuery.cs` - Uses the shared client unlock read model.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Domain/AvailableRecipesQuery.cs` - Uses the shared client unlock read model.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Presentation/BuildingMenuCardPresentation.cs` - Carries card availability and lock reason.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Presentation/BuildingMenuPresentation.cs` - Evaluates unlock requirements while building cards.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Presentation/BuildingMenuView.cs` - Disables locked card buttons and shows locked reasons in the card label area.
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Presentation/BuildingMenuController.cs` - Supplies the Settlement unlock read model and rejects locked selections.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Definitions/BuildingProductionSummaryProvider.cs` - Lists only unlocked station recipes in summaries.
- `Assets/Tests/Editor/Combat/BuildingMenuPresentationTests.cs` - Covers available and locked building card models.
- `Assets/Tests/Editor/Settlement/UnlockAvailabilityPresentationTests.cs` - Covers recipe filtering through `AvailableRecipesQuery`.
- `Assets/Tests/Editor/Settlement/StaticMlp.Tests.Settlement.asmdef` - Adds the Settlement presentation reference for availability presentation tests.
- `docs/ux/settlement-stage1.md` - Documents locked card and recipe UX, with NPC-specific reasons deferred to Phase 3.

## Decisions Made
- Kept unlock truth in Settlement by adding `ClientSettlementUnlockState` as an owner-provided read model instead of making Buildings depend on Settlement presentation internals.
- Kept locked buildings visible rather than filtered out, because the plan calls for visible locked states and card-level reasons.
- Hid locked recipes from the existing Workbench action cycle because the current UI only has a compact action-cycling control, not a full disabled recipe list.

## Deviations from Plan

None - plan executed as written.

## Issues Encountered
None.

## Next Phase Readiness
- Building and recipe availability are player-visible through existing UI surfaces.
- Server-side recipe validation remains the authority for invalid recipe requests.
- Unity compile/play verification is still required in the Editor; no `dotnet build` or Unity batchmode verification was run.

---
*Phase: 02-resource-flow-economy*
*Completed: 2026-05-29*
