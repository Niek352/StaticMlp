# Phase 2 Plan 2: Open-World Node Expansion Summary

**Deterministic open-world resource generation now produces tree, ore, spore pod, and chest nodes with harvest, hazard, and client visual support**

## Accomplishments
- Expanded both open-world resource placement paths to choose Phase 2 node kinds deterministically from world seed, chunk, and surface data.
- Added harvest and depletion hazard coverage for spore pod and chest nodes while keeping overlay state as the mutable state path.
- Added code-built runtime visuals for spore pod and chest nodes without generating prefab assets.
- Centralized WorldGeneration tuning values and kind ids under `Configs` so placement, terrain defaults, surface rules, and related tests use one shared source.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Contracts/Configs/OpenWorldGenerationConfig.cs` - Shared WorldGeneration constants, defaults, placement tuning, surface thresholds, kind ids, and default request creation.
- `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Presentation/Configs/OpenWorldTerrainStreamingConfig.cs` - Moved terrain streaming config into a `Configs` folder and wired defaults to the shared config.
- `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/Jobs/ResourcePlacementGenerationJob.cs` - Added deterministic Phase 2 resource kind selection in the native generation path.
- `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/Domain/OpenWorldPlacementGenerator.cs` - Added matching deterministic Phase 2 resource kind selection in the managed generation path.
- `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/Catalogs/OpenWorldGenerationLayerCatalog.cs` - Routed heightmap sizing constants through the shared config.
- `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/Domain/OpenWorldSurfaceRules.cs` - Routed native surface thresholds, noise, and vertex color values through the shared config.
- `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/Domain/SimpleSurfaceSampler.cs` - Routed managed surface thresholds and noise values through the shared config.
- `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/Domain/TerrainMeshBuilder.cs` - Routed mesh vertex color values through the shared config.
- `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/Jobs/OpenWorldSurfaceSamplingJob.cs` - Routed wetness sampling constants through the shared config.
- `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/WorldResources/OpenWorldChunkGenerationRuntime.cs` - Routed default chunk generation settings through the shared config.
- `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/Runtime/Logic/WorldResources/OpenWorldGenerationServerRuntime.cs` - Routed server generation defaults through the shared config.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Catalogs/OpenWorldResourceHarvestCatalog.cs` - Added Phase 2 harvest definitions and switched placement kind ids to shared constants.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/Catalogs/OpenWorldResourceHazardCatalog.cs` - Added poison and explosion depletion hazards for spore pod and chest nodes.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Presentation/ViewParts/OpenWorldResourceNodeViewPart.cs` - Added spore pod and chest visuals and routed kind ids through shared constants.
- `Assets/Tests/Editor/OpenWorldGeneration/OpenWorldPlacementGeneratorTests.cs` - Added deterministic placement coverage for all four Phase 2 node kinds.
- `Assets/Tests/Editor/OpenWorldResources/OpenWorldResourceHarvestCommandSystemTests.cs` - Added harvest and depletion hazard coverage for kinds 1 through 4.
- `Assets/Tests/Editor/OpenWorldResources/OpenWorldResourceNodeViewPartTests.cs` - Added runtime visual coverage for tree, ore, spore pod, chest, and unknown kind fail-fast behavior.
- `Assets/Tests/Editor/OpenWorldResources/ClientOpenWorldResourceProxyPresentationTests.cs` - Routed resource kind and chunk size test values through the shared config.
- `docs/Roadmap по GDD.md` - Updated Phase 2 status to reflect completed resource node expansion.

## Decisions Made
- Kept static resource nodes as deterministic placement data plus overlay state, not replicated network entities.
- Checked chest selection first using a rare deterministic hash roll, then selected spore pod on wet/moist surfaces, ore on highland or rocky surfaces, and tree as the default valid land node.
- Put the shared config in `OpenWorldGeneration.Runtime.Contracts/Configs` so logic, presentation, tests, and OpenWorldResources catalog/view code can use the same kind ids without cross-boundary mutation.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Centralized WorldGeneration config values during checkpoint completion**
- **Found during:** User follow-up after the Phase 2 node expansion implementation.
- **Issue:** WorldGeneration tuning and kind ids were spread across generation, presentation, catalogs, and tests, so changing values in one place did not reliably update all paths.
- **Fix:** Added `OpenWorldGenerationConfig`, moved the existing terrain streaming config under `Configs`, and routed generation, surface, placement, catalog, view, and test constants through the shared config.
- **Files modified:** `Assets/Scripts/StaticMlp/Features/OpenWorldGeneration/**`, `Assets/Scripts/StaticMlp/Features/OpenWorldResources/**`, `Assets/Tests/Editor/OpenWorldGeneration/**`, `Assets/Tests/Editor/OpenWorldResources/**`.
- **Verification:** Source checks confirmed Phase 2 kind ids and placement constants now route through config; `git diff --check` passed.

---

**Total deviations:** 1 auto-fixed blocking follow-up, 0 deferred.
**Impact on plan:** The deviation supports the same resource node expansion and keeps the existing architecture boundaries intact.

## Issues Encountered
None

## Next Phase Readiness
Ready for `02-03-PLAN.md`.

---
*Phase: 02-resource-flow-economy*
*Completed: 2026-05-27*
