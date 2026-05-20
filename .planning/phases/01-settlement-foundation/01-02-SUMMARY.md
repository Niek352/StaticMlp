# Phase 1 Plan 2: Building Metadata Summary

**Building definitions now carry typed category, capability, interaction, NPC, and operation metadata without presentation coupling.**

## Accomplishments
- Added immutable building metadata definition types under the existing BuildingCatalog `Definitions` bucket.
- Extended `BuildingDefinition` with code, display name, category, capabilities, interactions, NPC profile, and operation profile fields.
- Updated the existing Wooden Hut catalog entry with construction interactions and typed metadata while keeping view paths in the presentation catalog.
- Added catalog validation for duplicate ids, missing code/display/category, invalid footprint/build work, construction costs, interaction consistency, and profile mismatches.
- Added editor tests for current catalog validation, duplicate ids, missing metadata, and storage capability/profile mismatch.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Definitions/BuildingCategory.cs` - Building category enum.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Definitions/BuildingCapabilityFlags.cs` - Building capability flags.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Definitions/BuildingInteractionKind.cs` - Interaction kind enum.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Definitions/BuildingInteractionDefinition.cs` - Immutable interaction metadata.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Definitions/BuildingNpcProfileDefinition.cs` - Immutable NPC profile metadata.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Definitions/BuildingOperationDefinition.cs` - Immutable operation profile metadata.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Definitions/BuildingDefinition.cs` - Adds gameplay metadata fields.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Catalogs/BuildingCatalogData.cs` - Populates metadata and validates catalog data.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Validation/BuildingCatalogValidator.cs` - Fails fast on invalid building definitions.
- `Assets/Tests/Editor/Settlement/BuildingCatalogValidatorTests.cs` - Covers catalog validation.
- `Assets/Tests/Editor/Settlement/StaticMlp.Tests.Settlement.asmdef` - References BuildingCatalog for validation tests.

## Decisions Made
- Used static catalog initialization for validation, matching the existing resource catalog style.
- Kept presentation view paths in `BuildingPresentationDefinition`; gameplay definitions only describe domain metadata.
- Left the current Wooden Hut id and presentation entry intact so existing construction flow remains compatible.

## Deviations from Plan
None - plan executed as written.

## Issues Encountered
Unity EditMode tests were not run because agents must not launch Unity or run project build checks.

## Next Phase Readiness
Ready for `01-03-PLAN.md`.

---
*Phase: 01-settlement-foundation*
*Completed: 2026-05-20*
