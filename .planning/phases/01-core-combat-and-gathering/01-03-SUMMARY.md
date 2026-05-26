# Phase 1 Plan 3: Attack-to-Gather Overlay Summary

**Combat hit facts now reduce OpenWorldResources overlay amounts, mark depleted placements, and emit harvest facts using Settlement resource ids.**

## Accomplishments
- Added OpenWorldResources harvest definitions for wood and stone placements using `Settlement.Contracts` resource ids.
- Added a server resource-hit system that consumes `CombatTargetHitEvent`, resolves `PlacementId`, applies sparse overlay state changes, and marks dirty chunks through `OpenWorldChunkOverlayStore`.
- Added `OpenWorldResourceHarvestedEvent` as the owner-provided fact for future pickup and inventory handling.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Definitions/OpenWorldResourceHarvestDefinition.cs` - Defines immutable hit/harvest data for a resource placement kind.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Catalogs/OpenWorldResourceHarvestCatalog.cs` - Maps open-world resource placement kinds to Settlement wood/stone output.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Domain/OpenWorldResourceNodeRules.cs` - Applies resource hit rules and depletion flagging.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Systems/Server/ServerOpenWorldResourceHitSystem.cs` - Handles static-placement combat hit facts and emits harvest facts.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Contracts/Events/OpenWorldResourceHarvestedEvent.cs` - Public harvest fact for pickup/inventory plans.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/OpenWorldResourcesGameplayFeature.cs` - Registers the hit system after combat emits hit facts and before overlay send.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/StaticMlp.Features.OpenWorldResources.asmdef` - Adds Combat contracts and Settlement contracts references.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Contracts/StaticMlp.Features.OpenWorldResources.Contracts.asmdef` - Adds Settlement contracts reference.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Definitions.meta` - Unity folder metadata.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Catalogs.meta` - Unity folder metadata.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Contracts/Events.meta` - Unity folder metadata.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Definitions/OpenWorldResourceHarvestDefinition.cs.meta` - Unity script metadata.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Catalogs/OpenWorldResourceHarvestCatalog.cs.meta` - Unity script metadata.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Contracts/Events/OpenWorldResourceHarvestedEvent.cs.meta` - Unity script metadata.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Systems/Server/ServerOpenWorldResourceHitSystem.cs.meta` - Unity script metadata.
- `.planning/ROADMAP.md` - Marks Phase 1 progress as three of six plans complete.
- `.planning/phases/01-core-combat-and-gathering/01-PHASE-SUMMARY.md` - Updates Phase 1 progress.

## Decisions Made
- Kept wood and stone resource id mapping in `OpenWorldResourceHarvestCatalog`, not in the server system.
- Treated unknown indexed placements and inconsistent overlay state as fail-fast architecture errors.
- Treated hits against already depleted placements as gameplay rejections with no overlay mutation or harvest fact.
- Used `ResourceAmount` in the harvest event so the next inventory/pickup plan consumes Settlement-owned resource contracts.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None during source implementation.

## Next Step
Run Unity replication/codegen if still pending from earlier Combat contract changes, then run Unity compile/play checks. Ready for `01-04-PLAN.md`.

---
*Phase: 01-core-combat-and-gathering*
*Completed: 2026-05-26*
