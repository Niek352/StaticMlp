# ProcLayerGenLite: capabilities and criteria

Source reviewed: `Z:\_UnityProjects\LayerProcGen\Documentation` plus core source files from `Z:\_UnityProjects\LayerProcGen\Src\LayerProcGen`.

## Why This Document Exists

The current `StaticMlp.LayerProcLite` is not equivalent to LayerProcGen. It currently provides ids, masks, native buffers, a simple dependency order, and padding/window propagation. That is useful, but it does not provide the architectural boundary that made LayerProcGen valuable.

The immediate architectural problem in StaticMlp is that `OpenWorldChunkGenerationSystemBase` became a centralized pipeline that knows every OpenWorld layer, allocates every buffer, schedules every job, combines every dependency, and builds every output. That is a god system. A proper ProcLayerGenLite must move orchestration rules into the generic package and move concrete generation into independent game-owned layers.

## What Original LayerProcGen Gives

### 1. Layer/chunk ownership model

LayerProcGen is built around explicit layer and chunk pairs:

- A layer owns chunks of one chunk type.
- A chunk owns the generated data for one spatial cell of that layer.
- Each layer defines its chunk world size.
- Chunk size is an implementation detail of that layer.
- Different layers can use different chunk sizes.
- A layer exposes typed APIs for other layers to query its data.
- Generation code lives in the chunk's `Create(level, destroy)` method, not in one global generation system.

This is the core modularity we are missing.

### 2. Top-level dependencies

Generation starts from one or more top-level dependencies.

A top-level dependency says:

- this layer is needed;
- at this level;
- around this focus point;
- within this size/bounds.

Typical top dependencies:

- player area;
- map viewport;
- debug camera area;
- fast travel target;
- separate radii for terrain, props, NPCs, resources, etc.

Multiple top dependencies can overlap. The original system deduplicates overlapping provider chunks so the same chunk is not generated twice.

### 3. Recursive provider dependency resolution

Layer dependencies are declared once by the user layer:

- user layer;
- provider layer;
- provider level;
- padding in world units.

Before a user chunk generates, the framework recursively ensures that provider chunks exist for the user chunk bounds plus dependency padding.

Important consequence: chunk code can assume declared provider data exists. If it requests more data than declared, LayerProcGen reports a missing dependency/padding error.

This is much stronger than our current `PlanStep` array. The original does not just sort layers. It maps user bounds to provider chunk indices, loads providers recursively, tracks those relationships, and only then calls the user chunk generation.

### 4. Deterministic contextual generation

LayerProcGen's purpose is deterministic contextual generation:

- same chunk output independent of generation order;
- no missing surrounding context for algorithms that need context;
- input and output separated by layer boundaries;
- enough provider padding based on effect distance.

Examples from docs:

- blurs and kernel filters;
- point relaxation;
- pathfinding across chunk boundaries;
- terrain deformation from paths or locations;
- large-scale planning feeding smaller-scale layers.

The framework exists because chunked generation is otherwise easy to make deterministic but not contextually correct.

### 5. Effect distance and padding model

Effect distance means how far away input data can influence output at a point.

LayerProcGen makes this explicit:

- each layer dependency declares padding;
- chunk requests must stay within declared padding;
- missing/deeper request produces a diagnostic with required padding;
- algorithms should not special-case chunk edges when padding is correct;
- iterative algorithms accumulate effect distance per iteration.

For ProcLayerGenLite, this means padding/effect distance is not just metadata for tests. It must drive provider bounds and buffer windows.

### 6. Internal layer levels

Original layers may have multiple internal levels:

- one chunk object can progress through multiple generation levels;
- level N depends on neighbor chunks at level N-1;
- external dependencies can target a specific provider level;
- dependencies can be attached to a specific user level.

This supports staged generation inside one layer when chunk size must be shared and adjacent lower-level data must be available.

### 7. Chunk lifecycle and dependency reference tracking

Original LayerProcGen tracks:

- which provider chunk levels each generated chunk depends on;
- how many users depend on each provider chunk level;
- when a top dependency changes or is removed;
- when old provider chunks can be destroyed;
- chunk object reuse through pools.

This is a major missing piece. Our current implementation only schedules one request and disposes its buffers after completion. It does not own a persistent layer/chunk cache or dependency graph lifetime.

### 8. Rolling infinite chunk storage

Original layers store chunks in a `RollingGrid`:

- supports pseudo-infinite integer chunk coordinates;
- stores only currently needed chunks;
- allows old chunks to be released;
- supports bounded memory around active top dependencies.

ProcLayerGenLite does not need to copy `RollingGrid` exactly, but it needs equivalent chunk cache semantics.

### 9. Data access helpers

Original layer base class provides helpers such as:

- handle all loaded chunks;
- handle chunks overlapping world bounds;
- handle grid points across chunk boundaries;
- map world/grid points to provider chunks;
- check if a position is loaded at a given level.

These helpers are the layer API substrate. Without them, every game layer reimplements bounds math and provider access inconsistently.

### 10. Overlapping vs owned-within-bounds patterns

LayerProcGen documents two important query semantics:

- overlapping bounds: return all provider data overlapping bounds, useful for deformation and rendering;
- owned within bounds: return only data whose ownership anchor is inside bounds, useful for avoiding duplicate decorations/placements.

This is not a package feature in the original, but it is an architectural pattern ProcLayerGenLite should make easy and testable.

### 11. Top-down planning at scale

Original LayerProcGen supports layers operating at very different spatial scales:

- coarse world/region planning chunks;
- mid-scale path/location chunks;
- fine terrain/object chunks.

Large chunks can plan intent, connectivity, progression, routes, named places, and region state. Smaller chunks consume that plan. This is the difference between local functional generation and planned open-world generation.

Our current OpenWorld setup has no such generic support. It is a local chunk pipeline.

### 12. Scheduling and threading

Original LayerProcGen uses a manager:

- background thread;
- optional parallel chunk generation;
- top dependency change detection;
- progress/work tracking;
- main-thread action queue for engine objects.

ProcLayerGenLite should not copy the thread model literally because StaticMlp wants Unity Jobs/Burst/NativeArray-friendly scheduling. But it must provide equivalent orchestration concepts:

- schedule provider jobs before user jobs;
- dedupe jobs for the same layer chunk;
- track pending/ready/failed states;
- expose completion to game systems.

### 13. Unity separation

Original LayerProcGen has a Unity-independent core and separate Unity wrappers. For StaticMlp, our boundary is different:

- `StaticMlp.LayerProcLite` may depend on low-level Unity packages: Burst, Collections, Jobs, Mathematics;
- it must not depend on UnityEngine object APIs;
- it must not know OpenWorld, terrain, biome, water, resources, prefabs, navmesh, or game output names.

## Criteria For StaticMlp ProcLayerGenLite

### Package boundary criteria

`StaticMlp.LayerProcLite` must:

- depend only on `Unity.Burst`, `Unity.Collections`, `Unity.Jobs`, and `Unity.Mathematics`;
- not use `UnityEngine.Object`, `Mesh`, `Bounds`, `Color32`, `GameObject`, `NavMeshSurface`, or scene/prefab APIs;
- not contain game names such as OpenWorld, biome, water, wetness, resource, spawn, terrain mesh, visual mesh, physics mesh, navmesh source;
- provide generic native/job orchestration primitives only.

### Layer model criteria

The package needs a first-class layer abstraction, not just ids:

- layer id;
- layer chunk world size/layout;
- optional level count;
- dependencies per user level;
- provider layer id;
- provider level;
- padding/effect distance;
- layer-owned scheduling/generation callback;
- layer-owned chunk data type or opaque native chunk state handle.

OpenWorld should define `HeightLayer`, `SurfaceLayer`, `VisualMeshLayer`, `PhysicsMeshLayer`, `NavMeshSourceLayer`, `PlacementLayer`, etc. The generic package should not know what those mean.

### Chunk model criteria

The package needs chunk keys and states:

- `LayerProcLiteChunkKey`: layer id, level, chunk id, possibly lod/variant;
- chunk bounds in world coordinates;
- chunk state: unloaded, scheduled, generating, ready, releasing, failed;
- provider list per generated chunk level;
- user/ref count per chunk level;
- deterministic stable request key/hash;
- native buffer ownership/disposal hooks.

### Top dependency criteria

The package needs top dependencies:

- request a layer/level over focus + size or explicit bounds;
- update focus/size over time;
- deactivate/remove;
- support multiple active top dependencies;
- dedupe overlapping chunk needs;
- release chunks no longer reachable from active top dependencies.

This is what prevents every feature system from inventing its own pending dictionary and lifetime rules.

### Dependency resolver criteria

The package must resolve dependencies spatially:

- convert user chunk bounds to provider bounds using padding/effect distance;
- convert provider bounds to provider chunk keys using provider chunk size;
- recurse through provider dependencies;
- detect cycles before scheduling;
- support dependencies on specific provider levels;
- support internal layer levels or an equivalent explicit staged layer model;
- fail fast or report a precise padding error if a layer requests provider data outside declared dependency bounds.

Current `LayerProcLitePlanBuilder` only sorts layers and computes windows. That is not enough.

### Scheduling criteria

The package must own orchestration while game layers own concrete jobs:

- package decides which chunk jobs are needed;
- package combines provider `JobHandle`s;
- package calls the layer-owned scheduler for each chunk/level;
- scheduler receives a context with provider handles and provider data views;
- scheduler returns output handles and native chunk state;
- same chunk request is scheduled once even if requested by multiple top dependencies;
- completion and disposal are tracked centrally.

OpenWorld systems should not manually combine `HeightJob`, `SurfaceJob`, `PlacementJob`, `MeshJob` in one class.

### Data access criteria

The package should provide bounded provider access:

- provider data by world bounds;
- provider data by grid bounds;
- provider chunk views;
- generic native grid/buffer views;
- helpers for overlapping-bounds queries;
- helpers or patterns for owned-within-bounds queries;
- no access to provider chunks that are not ready for the declared dependency.

### Runtime integration criteria for StaticMlp

OpenWorld should integrate through a thin ECS/system boundary:

- systems create/update top dependencies from gameplay/streaming requests;
- systems tick/poll the generic layer runtime;
- systems emit completed events or store completed chunk outputs;
- concrete generation logic lives in layer classes/jobs, not in the ECS system;
- presentation remains separate from logic and receives generated output through OpenWorld-owned events/resources.

`OpenWorldChunkGenerationSystemBase` should shrink to coordination only, or disappear behind a small OpenWorld generation runtime.

### Diagnostics criteria

ProcLayerGenLite should expose:

- plan dump / dependency graph dump;
- active top dependencies;
- loaded/scheduled/ready chunks by layer;
- work/progress counters;
- cycle errors;
- missing dependency or insufficient padding diagnostics;
- leak diagnostics for native chunk buffers still retained after top dependency release.

### Test criteria

Package tests should cover:

- dependency order;
- cycle detection;
- provider bounds expansion from padding/effect distance;
- provider chunk index calculation across negative coordinates;
- top dependency overlap dedupe;
- top dependency movement releases old chunks;
- chunk ref counts across multiple users;
- dependency on specific provider level;
- internal/staged level neighbor availability or explicit replacement;
- native grid/buffer layout;
- package contains no OpenWorld/game semantics;
- package references only allowed low-level Unity assemblies.

OpenWorld tests should cover:

- OpenWorld layers are separate layer definitions/schedulers;
- height/surface/mesh/placement scheduling goes through ProcLayerGenLite runtime;
- surface normals use padded height provider data;
- placements use OpenWorld-owned surface samples;
- visual/physics/nav outputs are OpenWorld layer ids;
- requesting visual mesh and physics mesh dedupes shared height/surface providers;
- moving/removing generation interest releases unused native buffers;
- `OpenWorldChunkGenerationSystemBase` is not a god pipeline.

## What We Are Missing Right Now

Current `StaticMlp.LayerProcLite` has:

- ids;
- layer mask;
- dependencies;
- topological plan builder;
- padding/window propagation;
- native grid/buffer/mesh buffer wrappers;
- request key/hash utilities.

Current `StaticMlp.LayerProcLite` is missing:

- first-class layer definitions with chunk size and scheduler ownership;
- first-class layer chunks;
- persistent chunk cache;
- top dependencies;
- recursive provider chunk resolution;
- provider bounds to provider chunk key mapping;
- multiple top dependency dedupe;
- ref-counted chunk lifetime;
- release/destruction lifecycle;
- internal layer levels/provider levels;
- bounded provider data query API;
- missing-padding diagnostics;
- runtime scheduler that calls layer-owned jobs;
- completion/progress/debug state;
- memory ownership model for generated native chunk data.

Current OpenWorld implementation is missing:

- OpenWorld-owned layer classes/schedulers;
- one chunk output type per layer;
- layer-local pending/chunk state;
- thin ECS bridge;
- generic runtime-owned dependency scheduling;
- output dedupe between visual/physics/nav requests;
- release of no-longer-needed provider chunks based on generation interest.

## Minimum Acceptable ProcLayerGenLite V1

V1 should not try to clone all of LayerProcGen. It should be the smallest architecture that prevents another god system.

Minimum V1:

- generic `LayerProcLiteRuntime`;
- generic layer registration with chunk size, dependencies, and scheduler;
- top dependency requests over bounds;
- recursive provider chunk key resolution;
- per-layer chunk state cache;
- deduped scheduling by chunk key;
- `JobHandle` dependency propagation;
- explicit disposal/release when top dependencies move or deactivate;
- diagnostics for cycles and missing/invalid dependency declarations;
- OpenWorld implemented as separate layer schedulers.

Anything less is just helper structs around a hand-written OpenWorld pipeline.

