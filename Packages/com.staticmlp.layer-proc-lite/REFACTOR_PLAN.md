# LayerProcLite Refactor Plan

This plan is based on the current LayerProcLite audit. The goal is to keep LayerProcLite as a Unity-specific, game-agnostic procedural layer runtime while removing OpenWorld-specific semantics and reducing avoidable allocations.

Status: phases 1, 2, 3, and the LayerProcLite API-surface cleanup from phase 6 have been applied. Phase 4 remains a separate OpenWorldGeneration legacy-test migration task. Phase 5 has started with the low-risk allocation/struct changes listed below.

## Goals

- Keep LayerProcLite focused on layer graph resolution, chunk dependency ownership, job dependency composition, and opaque chunk data lifetime.
- Keep OpenWorld terrain, mesh, placement, visual, physics, and nav mesh concepts outside the package.
- Preserve fail-fast graph validation.
- Reduce per-request and per-provider allocations on generation hot paths.
- Avoid refactors that only make code look generic while hiding ownership or disposal rules.

## Phase 1: Lock Package Boundary

Status: applied.

1. `noEngineReferences: false` is kept in `Runtime/StaticMlp.LayerProcLite.asmdef` so the Unity-port package can resolve the allowed Unity packages.
2. The package keeps only these Unity references:
   - `Unity.Burst`
   - `Unity.Collections`
   - `Unity.Jobs`
   - `Unity.Mathematics`
3. Keep architecture tests that reject:
   - `StaticMlp.Features.*`
   - `UnityEngine`
   - OpenWorld, terrain, biome, water, resource, spawn, visual mesh, physics mesh, nav mesh source semantics.
4. Update `AGENTS.md` in the same change whenever the package boundary or public semantics change.

## Phase 2: Move Game-Specific Native Mesh Types Out

Status: applied.

1. The native mesh buffer type was moved out of LayerProcLite.
2. OpenWorldGeneration owns `OpenWorldNativeMeshBuffers`.
3. `OpenWorldTerrainMeshGenerationJob` and `OpenWorldMeshDataLayerScheduler` use the OpenWorld-owned buffer type.
4. LayerProcLite runtime code is free from vertex, triangle, tangent, color, collider, visual mesh, physics mesh, and nav mesh source terminology.

## Phase 3: Simplify OpenWorld Layer Outputs

Status: applied.

OpenWorld previously modeled visual, physics, and nav mesh source outputs as separate LayerProcLite layers even though they all pointed at the same generated mesh data.

Preferred direction:

1. OpenWorld keeps a single generated geometry/data layer: `MeshData`.
2. Visual, physics, and nav mesh source outputs are OpenWorld output flags, not LayerProcLite graph nodes.
3. The mesh output scheduler wrapper was removed.
4. The mesh output chunk-data wrapper was removed.
5. Conversion from native generated data to managed `TerrainMeshData` stays in OpenWorldGeneration, not LayerProcLite.

## Phase 4: Remove Legacy Managed Generation Path

Status: deferred.

The obsolete managed path duplicates the LayerProcLite job pipeline and keeps old abstractions alive.

Candidates to remove after tests are migrated:

- `SimpleWorldGenerationService`
- `TerrainMeshBuilder`
- `OpenWorldPlacementGenerator`
- `SimpleSurfaceSampler`
- `GeneratedChunkData`
- `IWorldGenerationService`, if no current production code needs the interface
- old tests that only validate the obsolete path

Do not replace these with a new generic service layer unless there is a current production caller that needs it.

This phase is intentionally deferred because the remaining usage is test/legacy contract migration, not LayerProcLite package cleanup. Do it as a separate OpenWorldGeneration cleanup so tests are rewritten against the job pipeline instead of silently deleting coverage.

## Phase 5: Allocation and Struct Pass

Status: started.

Prioritize hot-path allocations before cosmetic type changes.

1. Done: `GetSingleOverlapping` no longer allocates through `GetOverlapping`.
2. Done: OpenWorld `PendingChunkGeneration` is a readonly struct.
3. Remaining: reduce temporary collection creation in `LayerProcLiteRuntime.ResolveChunk` when dependency count and provider chunk count are known or small.
4. Remaining: consider making `TopDependencyEntry` a readonly struct.
5. Do carefully: if `ChunkEntry` becomes a struct, every mutation of `State` or `RefCount` must write back into the dictionary.

Do not convert `ILayerProcLiteChunkData` implementations to structs while they are stored through the interface. That would cause boxing and make native collection disposal ownership less obvious.

## Phase 6: Review Package API Surface

Status: partially applied.

1. Done: the generation request key moved to OpenWorldGeneration.
2. Done: `LayerProcLitePlanBuilder` and `LayerProcLiteLayerDescriptor` were removed because runtime validation covers the graph semantics.
3. Remaining: periodically review generic grid/math/hash helpers and keep only helpers broadly useful for Unity procedural layer jobs.
4. If a helper starts encoding terrain/noise/material assumptions, move it to the owning feature.

## Verification

After each phase:

1. Run Unity editor tests for OpenWorldGeneration and architecture tests.
2. Confirm no `.Generated.cs`, prefab, or Unity asset files were created.
3. Confirm LayerProcLite still contains no feature-specific terms.
4. Confirm public API changes are reflected in this file and `AGENTS.md`.
