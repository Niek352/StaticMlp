# LayerProcLite Agent Guide

LayerProcLite is our Unity-specific port of a lightweight procedural layer dependency runtime.

This package is intentionally built for Unity and already references low-level Unity packages. It is not a pure .NET or engine-agnostic library. The architecture goal is narrower: keep the package game-agnostic and presentation-free while allowing Unity Burst, Jobs, Collections, and Mathematics as core implementation dependencies.

Its responsibility is to:

- describe layer ids, chunk ids, dependency windows, chunk keys, and request keys;
- resolve requested top-level layer chunks into dependency chunks;
- retain and release generated chunk data by dependency reference count;
- combine Unity Job handles between dependency chunks and consumer chunks;
- expose Burst/job-friendly value types and minimal native collection helpers.

LayerProcLite must not know about StaticMlp gameplay features.

Forbidden in this package:

- `StaticMlp.Features.*` references;
- OpenWorld, terrain, biome, water, resource, spawn, settlement, frontier, or other game-specific concepts;
- Unity presentation concepts such as `GameObject`, `MonoBehaviour`, prefab paths, materials, views, UI, or scene objects;
- transport, networking, ECS world mutation, ownership tags, replication contracts, or packet serialization;
- mesh output semantics such as visual mesh, physics mesh, nav mesh source, collider mesh, or feature-specific vertex coloring.

If a type is only useful for one feature, keep it in that feature. For example, terrain mesh buffer layouts belong in `OpenWorldGeneration`, not in LayerProcLite.

Allowed Unity dependencies are low-level data/job packages only. These references are part of this Unity port's design:

- `Unity.Collections`
- `Unity.Jobs`
- `Unity.Mathematics`
- `Unity.Burst`

Keep `noEngineReferences: false` for the package asmdef. This Unity-port package must be able to resolve the allowed Unity packages above. Do not add `UnityEngine` object/presentation references unless the package's architecture is deliberately changed and this file is updated in the same change.

## Data Model

LayerProcLite owns graph/runtime semantics, not generated domain semantics.

- `LayerProcLiteRuntime` owns registered layers, active top dependencies, generated chunks, ref counts, and job completion state.
- `ILayerProcLiteLayerScheduler` is the boundary where a game feature schedules concrete generation work.
- `ILayerProcLiteChunkData` is an opaque disposable handle owned by the scheduler and retained by the runtime.
- `LayerProcLiteProviderSet` exposes dependency chunk data declared by the layer graph.
- `LayerProcLiteWorldBounds` and `LayerProcLiteChunkId` describe 2D chunk coverage only; they do not imply terrain or world gameplay.

Prefer fail-fast behavior for invalid graph/request state. Missing layers, mismatched chunk data, invalid dependency bounds, and dependency cycles are architecture errors.

## Performance Direction

This package is expected to stay allocation-conscious because it can run on streaming/generation hot paths.

- Keep ids, keys, masks, bounds, windows, and small immutable records as `readonly struct`.
- Avoid turning chunk data into structs while it is passed through `ILayerProcLiteChunkData`; that would introduce boxing and make disposal ownership less clear.
- Avoid feature-specific wrappers in the package to save one allocation. Put those wrappers in the owning feature or remove them.
- Prefer APIs that let callers avoid per-query `List<T>` and `ToArray()` allocations when reading provider chunks.
- Be careful when converting dictionary values from classes to structs: mutation must write the value back into the dictionary.

## Maintenance Rule

When changing LayerProcLite's architecture, public semantics, allowed dependencies, package boundary, or ownership model, update this `CLAUDE.md` in the same change.

If a requested change conflicts with this guide, stop and explain the architectural concern before editing code.
