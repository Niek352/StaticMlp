# Phase 2 Plan 1: Resource Contracts And Node Profiles Summary

**Settlement-owned resource ids for ore/resin/spores/ingots plus OpenWorldResources harvest profiles for tree, ore, spore pod, and chest nodes**

## Accomplishments
- Added the minimal Phase 2 economy resource set to `Settlement.Contracts`.
- Added server-authoritative harvest profile metadata for Phase 2 node types.
- Documented economy ownership boundaries for future Phase 2 plans.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Contracts/Catalogs/ResourceCatalog.cs` - Added ore, resin, spores, and ingots resource definitions.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Contracts/Validation/ResourceCatalogValidator.cs` - Added fail-fast validation for missing resource usage flags.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Definitions/OpenWorldResourceHarvestTag.cs` - Added preferred harvest tag metadata flags.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Definitions/OpenWorldResourceHarvestDefinition.cs` - Added armor, resistance, and preferred harvest tag profile fields.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Catalogs/OpenWorldResourceHarvestCatalog.cs` - Added tree, ore, spore pod, and chest harvest profiles.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Domain/OpenWorldResourceNodeRules.cs` - Added profile metadata accessors.
- `Assets/Tests/Editor/Settlement/ResourceCatalogValidatorTests.cs` - Updated catalog coverage for the Phase 2 resources.
- `Assets/Tests/Editor/OpenWorldResources/OpenWorldResourceHarvestCommandSystemTests.cs` - Added harvest resource assertions for all profile kinds.
- `Assets/Tests/Editor/OpenWorldResources/OpenWorldResourceNodeRulesTests.cs` - Added profile metadata coverage.
- `Assets/Tests/Editor/OpenWorldResources/StaticMlp.Tests.OpenWorldResources.asmdef` - Added the direct Settlement contracts test reference.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/AGENTS.md` - Documented feature ownership boundaries.
- `docs/Roadmap по GDD.md` - Documented Phase 2 economy ownership boundaries.

## Decisions Made
- Used the minimal approved Phase 2 resource set: ore, resin, spores, and ingots.
- Kept preferred harvest tags as OpenWorldResources-local metadata until combat/loadout systems own concrete tool and passive semantics.
- Left resource placement generation and presentation visuals unchanged; kind ids 3 and 4 are profile-ready but not spawned yet.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## Next Phase Readiness
Ready for `02-02-PLAN.md`.
