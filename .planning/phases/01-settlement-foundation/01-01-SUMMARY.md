# Phase 1 Plan 1: Resource Foundation Summary

**Settlement resource contracts now cover typed Stage1 storage for raw, flow, refined, progression, and stability resources.**

## Accomplishments
- Replaced placeholder resources with Wood, Stone, Planks, Simple Parts, Repair Kits, Food, Fuel, Research Data, and Medicine.
- Expanded `SettlementSharedResources` with explicit replicated integer fields and fail-fast typed access for all stored Stage1 resources.
- Updated Stage1 seed creation and settlement storage spawning so every stored resource has a configured starting amount.
- Routed existing progression reward grants through the shared storage `Add` method.
- Updated resource catalog tests to cover concrete Stage1 ids, families, stored flags, usage flags, and starting amounts.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Contracts/Catalogs/ResourceCatalog.cs` - Defines the concrete Stage1 resource ids and catalog entries.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Contracts/Definitions/ResourceUsageFlags.cs` - Adds the Stage1 usage flags.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/SettlementSharedResources.cs` - Adds explicit replicated storage fields and typed accessors.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/SettlementSharedResourcesFactory.cs` - Initializes all Stage1 stored resource fields.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Stage1SettlementSeedManifest.cs` - Emits starting resource entries from the resource catalog.
- `Assets/Scripts/StaticMlp/Features/Progression/Runtime/Logic/ServerStage1RewardApplicationSystem.cs` - Uses the shared storage grant accessor for reward resources.
- `Assets/Tests/Editor/Settlement/ResourceCatalogValidatorTests.cs` - Covers the concrete Stage1 catalog.

## Decisions Made
- Kept all Stage1 resources settlement-stored because the current phase is establishing the shared storage baseline for later stockpile, production, repair, and progression work.
- Kept storage as explicit fields instead of a dictionary or dynamic array because the component is a replicated contract.
- Added `SettlementSharedResources.Add` because the current reward application caller already grants settlement resources.

## Deviations from Plan
None - plan executed as written.

## Issues Encountered
Unity EditMode tests and replication codegen checks were not run because agents must not launch Unity or run project build checks.

## Next Phase Readiness
Ready for `01-02-PLAN.md`.

---
*Phase: 01-settlement-foundation*
*Completed: 2026-05-20*
