# OpenWorldGeneration Agent Guide

Read this before changing `StaticMlp.Features.OpenWorldGeneration`.

This feature owns deterministic chunk generation, spatial chunk ids, server chunk interest, and local client terrain presentation. It does not own resource gameplay state; `OpenWorldResources` consumes generation output, indexes deterministic resource placements, and owns sparse chunk overlay state plus client-only resource proxies.

## Runtime Shape

- `Runtime/Contracts` contains stable public contracts: chunk ids, bounds, generation requests, output masks, terrain/placement DTOs, samplers, and chunk events.
- `Runtime/Logic` contains deterministic generation, LayerProcLite schedulers/jobs, server interest/snapshot systems, and client-core generation.
- `Runtime/Presentation` contains client-only terrain views, mesh application, LOD selection, debug gizmos, and terrain streaming runtime.
- Keep LayerProcLite package generic. Game-specific concepts such as water, surface samples, placements, meshes, and OpenWorld ids stay in this feature.

## Generation Pipeline

Generation requests enter through `OpenWorldChunkGenerationRequested`.

`OpenWorldChunkGenerationSystemBase<TWorld>` is the only bridge from ECS events to `LayerProcLiteRuntime`:

1. Validate that the request matches the registered `OpenWorldChunkGenerationRuntime`.
2. Convert `GenerationOutputMask` into output layers.
3. Add LayerProcLite top dependencies for the chunk bounds and LOD.
4. Tick `LayerProcLiteRuntime`.
5. When all top dependencies are ready, build managed output and emit `OpenWorldChunkGenerationCompleted`.
6. Remove top dependencies after completion.

Do not schedule generation jobs directly from ECS systems. Add or modify layer behavior through `OpenWorldGenerationLayerCatalog`, `LayerSchedulers`, `Jobs`, `Definitions`, or pure `Domain` rules.

## Outputs

- `VisualMesh` is client presentation terrain.
- `PhysicsMesh` is server-side authoritative collision/query geometry.
- `NavMeshSourceMesh` is reserved for server navigation source geometry.
- `Placements` are deterministic spawn/resource placement facts, not spawned gameplay entities.
- `ServerGeometry` means `PhysicsMesh | NavMeshSourceMesh`.

One mesh data layer currently serves all mesh outputs. Keep the output mask explicit so future server and visual mesh generation can diverge without changing call sites.

## Server Flow

`OpenWorldGenerationGameplayFeature.RegisterServerResources` creates:

- `OpenWorldChunkGenerationRuntime`: deterministic generation runtime and LayerProcLite graph.
- `OpenWorldGenerationServerRuntime`: server budgets, default request, streaming radius, and server geometry LOD.
- `OpenWorldChunkStreamingState`: per-peer loaded chunks, server loaded/loading chunk sets, and pending snapshot queue.
- `OpenWorldServerChunkGeometryRuntime`: generated server geometry cache.

Server systems run in this order:

1. `ServerOpenWorldChunkInterestSystem` reads connected peers and their player positions, computes desired chunks, registers/activates spatial clusters, queues server chunk loads, queues snapshots for already-loaded chunks, and sends `OpenWorldChunkUnloadEvent` when a peer leaves a chunk.
2. `ServerOpenWorldChunkGenerationBridgeSystem` converts internal `OpenWorldChunkLoadRequested` events into `OpenWorldChunkGenerationRequested` with server outputs.
3. `ServerOpenWorldChunkGenerationSystem` runs the shared generation bridge on the server world.
4. `OpenWorldResources.ServerOpenWorldResourcePlacementIndexSystem` consumes `OpenWorldChunkGenerationCompleted` and registers `ResourcePlacements` into placement/overlay stores.
5. `ServerOpenWorldChunkGeometryStoreSystem` caches physics/nav geometry for server-side consumers.
6. `ServerOpenWorldChunkGenerationCompleteSystem` marks chunks loaded and queues snapshots for every peer waiting on that chunk.
7. `ServerOpenWorldChunkSnapshotSystem` builds one public cluster snapshot packet per cluster per frame budget and sends it to waiting peers.

Server interest owns which replicated ECS clusters a peer should have. It does not send terrain render meshes to clients.

## Client Flow

Client-core logic registers `ClientOpenWorldChunkGenerationSystem`. On a host, `OpenWorldGenerationGameplayFeature` reuses the server `OpenWorldChunkGenerationRuntime` for the client world so server and client requests share the same LayerProcLite graph. On a client-only process it creates its own default generation runtime.

Client presentation systems run as follows:

1. `ClientOpenWorldChunkUnloadSystem` consumes `NetworkEventFromServer<OpenWorldChunkUnloadEvent>` and destroys replicated entities in the target cluster unless a newer snapshot for the same cluster is already queued in the inbox.
2. `ClientOpenWorldTerrainStreamingSystem` finds the `LocalOwned` player, updates local terrain streaming around that position, and sends local `OpenWorldChunkGenerationRequested` events for `VisualMesh` output.
3. `ClientOpenWorldTerrainApplySystem` consumes local `OpenWorldChunkGenerationCompleted` events and applies visual meshes to `TerrainChunkView` through `OpenWorldTerrainRuntime`.

Client terrain chunks are local presentation objects. They are created and destroyed by `OpenWorldTerrainRuntime` according to local view radius and LOD, not by server cluster snapshots.

## Client-Server Interaction

There are two separate data paths:

- Replicated gameplay entities: server owns spatial cluster activation, cluster snapshots, and unload events for real dynamic actors. Static resource placements use `OpenWorldResources` placement index + chunk overlay instead of ordinary replicated entities.
- Visual terrain: clients generate deterministic `VisualMesh` locally from the same seed/request settings. Visual terrain meshes are not replicated over the network.

Important rules:

- `OpenWorldChunkUnloadEvent` is a replicated event with `ReliableSequenced` delivery. Keep its id and GUID stable.
- Do not add raw transport calls, raw inbox/outbox access, or packet serialization to OpenWorldGeneration systems.
- Do not replicate ownership tags. Spatial clusters are addressed by `OpenWorldSpatialClusterIds`, and entity ownership remains under the normal networking/ownership layer.
- Do not turn client visual terrain into authoritative gameplay state. Authoritative collision, navigation, resources, and other gameplay consumers must use server-generated data or replicated entities.
- Server validation happens at the interest/request boundary: peer positions come from server-owned peer/player state, chunk ids are clamped to configured bounds, and generation requests must match the registered runtime.
- Cross-feature spawning stays event-based. OpenWorldGeneration emits `OpenWorldChunkGenerationCompleted`; `OpenWorldResources` owns resource node factories, state, replication, and deltas.

## Host Spike Notes

Host mode runs server and client work in the same Unity process, so an open-world chunk load can produce a single visible spike made from multiple systems.

What this spike means architecturally:

- This is primarily replicated object synchronization, not terrain height/mesh generation.
- Server chunk generation emits deterministic placement facts. `OpenWorldResources` now registers those facts into placement/overlay stores instead of turning each static placement into a server-authoritative network entity.
- When a peer becomes interested in a chunk, generic cluster snapshots should contain only real dynamic actors. Static resource depletion/amount state is sent separately as sparse overlay events keyed by `PlacementId`.
- `ServerOpenWorldResourceNodeDeltaCaptureSystem` is resource-node persistence/dirty bookkeeping. A high cost there means the server is spending time finding changed existing resource node states and recording depleted placement ids, or the query is touching a large resource-node population.
- `ServerOpenWorldChunkSnapshotSystem` is the initial cluster synchronization burst. It serializes the public replicated state for the whole cluster, builds a snapshot payload, gzip-compresses it, and queues it to peers.
- On a host, the same frame also runs the client side. After the server snapshot is received locally, client systems create or patch replicated ECS entities and then presentation systems project those ECS components into Unity views.
- Client view markers are render-side fan-out from replicated ECS state into MonoBehaviour views. They are not authoritative gameplay and should not feed gameplay decisions back into the server.

Observed profiler samples from the current host spike:

- `ServerWT_ServerOpenWorldResourceNodeDeltaCaptureSystem`: 43.90 ms, plus another observed sample at 26.46 ms.
- `ServerWT_ServerOpenWorldResourceNodeDeltaCaptureSystem`: current frame accumulated 67.59 ms for 2 instances on Main Thread.
- `ServerWT_ServerOpenWorldChunkSnapshotSystem`: 36.10 ms.
- `ServerWT_ServerOpenWorldChunkSnapshotSystem`: current frame accumulated 77.81 ms for 2 instances on Main Thread.
- `ClientCoreWT_ApplyComponentToViewSystem<PlacementPreviewViewState>`: 1.90 ms.
- `ClientCoreWT_RemoteInterpolatedViewSyncSystem`: 1.91 ms.
- `ClientCoreWT_ClientOpenWorldTerrainStreamingSystem`: 1.65 ms.

Current high-risk chain:

1. `OpenWorldResources.ServerOpenWorldResourcePlacementIndexSystem` should do bounded placement/index work and create zero `OpenWorldResourceNodeNetworkEntity` instances by default.
2. `OpenWorldResources.ServerOpenWorldChunkOverlaySendSystem` sends absolute/delta overlay events only for chunks with touched overlay state.
3. `ServerOpenWorldChunkSnapshotSystem` is still the expected heavier server marker when pending chunk snapshots exist, but static resource placements should no longer dominate it. For each unique cluster allowed by `MaxClusterSnapshotsPerFrame`, it calls `CopyClusterChunks`, `ReplicationRegistry.CreatePublicClusterStateSnapshot(clusterId, gzip: true)`, and `PacketCodec.EncodeSnapshot`. That means querying public `NetworkedTag + NetworkIdentity` entities in the cluster, serializing public replicated components, allocating snapshot/packet arrays, and gzip-compressing the payload.
4. The same prebuilt snapshot packet is reused for multiple peers in the same frame; sending it to each peer is not the expensive part.
5. On the host client, `ClientSnapshotApplySystem` upserts the snapshot and initializes client archetype/view state, then presentation systems and generic view sync apply the changes.

When profiling this flow, compare these markers first:

- `ServerWT_ServerOpenWorldResourceNodeDeltaCaptureSystem`
- `ServerWT_ServerOpenWorldChunkSnapshotSystem`
- `ClientCoreWT_ClientSnapshotApplySystem`
- `ClientCoreWT_ClientOpenWorldResourceNodeViewStateSystem`
- `ClientCoreWT_ApplyComponentToViewSystem<OpenWorldResourceNodeViewState>`
- `ClientCoreWT_ApplyComponentToViewSystem<ViewTransform>`

The observed `2 instances on Main Thread` for both server markers must be explained before optimizing internals. Verify whether the server pipeline is updated twice in one Unity frame, or whether the same feature system was registered twice. If two server ticks per Unity frame are intentional, measure each instance separately.

With the overlay model, first confirm that chunk entry creates zero static resource network entities. If `ServerOpenWorldChunkSnapshotSystem` still dominates, investigate real dynamic entity count per spatial cluster, snapshot payload size, packet bytes, and gzip cost before changing generation.

The reported client markers around placement preview, remote interpolation, and terrain streaming are currently secondary compared with the server spike. If client `ApplyComponentToViewSystem<OpenWorldResourceNodeViewState>` dominates in a later capture, the likely cost is first-time resource-node view application, especially Unity primitive/material creation in `OpenWorldResourceNodeViewPart`, not terrain mesh generation.

## Spatial Clusters

`OpenWorldSpatialClusterIds` maps `WorldChunkId` to a row-major `ushort` cluster id inside `WorldChunkBounds`.

- Keep one spatial cluster per world chunk.
- Do not reuse logical fixed cluster constants for open-world chunks.
- Keep bounds small enough to fit in `ushort` cluster ids.
- Register and activate clusters on the server through server interest code before snapshotting or spawning entities into them.

## Presentation Rules

- `TerrainChunkView` and debug gizmos are passive Unity-facing views.
- `OpenWorldTerrainRuntime` may own GameObjects and meshes because it is presentation runtime state.
- Do not move UnityEngine presentation objects into `Runtime/Logic`.
- Null mesh input to `ApplyChunkMesh` is invalid and should fail fast.
- LOD and collider decisions are local presentation concerns unless server gameplay explicitly needs them.

## Tests And Architecture Guards

Relevant editor tests live in:

- `Assets/Tests/Editor/OpenWorldGeneration/OpenWorldGenerationTests.cs`
- `Assets/Tests/Editor/Architecture/OpenWorldGenerationArchitectureTests.cs`

Architecture tests enforce:

- contracts do not depend on LayerProcLite internals;
- no LayerProcGen references remain;
- LayerProcLite stays package-generic;
- generation systems remain thin LayerProcLite runtime bridges;
- OpenWorldGeneration scope contains no generated code or prefabs;
- spatial streaming does not use fixed logical cluster constants.

Do not edit `.Generated.cs` files manually. Ask the user to run Unity/editor tests when verification is needed.
