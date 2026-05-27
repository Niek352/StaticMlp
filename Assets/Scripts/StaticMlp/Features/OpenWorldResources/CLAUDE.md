# OpenWorldResources Agent Guide

Read this before changing `StaticMlp.Features.OpenWorldResources`.

This feature owns mutable static-resource gameplay state for deterministic open-world placements. `OpenWorldGeneration` emits `ResourcePlacement` facts; this feature indexes those facts, stores sparse per-chunk overlay state, sends overlay events, and creates client-only visual proxies.

Default rule:

```text
Generated ResourcePlacement != replicated NetworkEntity.
PlacementId is the authoritative reference.
OpenWorldChunkOverlayStore is the mutable source of truth.
```

`OpenWorldResourceNodeNetworkEntity`, `OpenWorldResourceNodeFactory`, and `OpenWorldResourceNodeDeltaStore` are legacy compatibility pieces only. They must stay behind `OpenWorldResourcesCompatibility.UseLegacyReplicatedResourceNodes`, which defaults to `false`.

## Runtime Shape

- `Runtime/Logic/Domain` owns overlay value/model types and resource rules.
- `Runtime/Logic/WorldResources` owns `OpenWorldPlacementIndexStore`, `OpenWorldChunkOverlayStore`, peer ack state, and overlay dirty queues.
- `Runtime/Logic/Events` owns typed overlay network events and codecs.
- `Runtime/Logic/Systems/Server` owns placement indexing and server overlay request/send systems.
- `Runtime/Presentation` owns client-only proxy components, proxy index, overlay apply, proxy spawn/unload, and passive view parts.
- Do not put chunk generation or terrain rendering logic here.

## Server Flow

`OpenWorldResourcesGameplayFeature` registers:

- overlay network events through `OpenWorldChunkOverlayEventCodec`;
- `OpenWorldPlacementIndexStore` for deterministic placement facts;
- `OpenWorldChunkOverlayStore` for mutable resource state;
- `OpenWorldPeerChunkOverlayState` for peer acked revisions;
- `OpenWorldChunkOverlayDirtyQueue` for touched chunks;
- `ServerOpenWorldResourcePlacementIndexSystem`;
- `ServerOpenWorldChunkOverlayRequestSystem`;
- `ServerOpenWorldChunkOverlaySendSystem`.

Server systems:

1. `ServerOpenWorldResourcePlacementIndexSystem` consumes `OpenWorldChunkGenerationCompleted`, registers `ResourcePlacement[]`, and registers `PlacementId -> WorldChunkId` in the overlay store.
2. Server gameplay that changes static resource state must resolve `PlacementId` through `OpenWorldPlacementIndexStore`, validate the command, then apply `OpenWorldResourceOverlayState` to `OpenWorldChunkOverlayStore`.
3. `ServerOpenWorldChunkOverlayRequestSystem` records client known/acked chunk overlay revisions.
4. `ServerOpenWorldChunkOverlaySendSystem` sends absolute overlays for unknown baselines and deltas when the server has a valid basis.

Do not spawn `OpenWorldResourceNodeNetworkEntity` for ordinary static trees, rocks, ore, or bushes.

## Client Flow

Clients generate visual terrain and deterministic `ResourcePlacement` facts locally.

Presentation flow:

1. `ClientOpenWorldResourceProxySpawnSystem` consumes local `OpenWorldChunkGenerationCompleted`, registers placements, and creates `OpenWorldResourceProxyTag` entities with `ViewPath`, `ViewTransform`, and `OpenWorldResourceNodeViewState`.
2. Proxy entities live in `CW`, do not have `NetworkIdentity`, and are not part of generic replication.
3. `ClientOpenWorldChunkOverlayApplySystem` applies absolute/delta overlay events to the client overlay store and existing proxies, then sends `OpenWorldChunkOverlayAck`.
4. `ClientOpenWorldResourceProxyUnloadSystem` removes proxies when `OpenWorldChunkUnloadEvent` arrives.
5. `OpenWorldResourceNodeViewPart` remains view-only and can render proxy view state without owning gameplay lifecycle.

## Dynamic Exceptions

A static placement may become a network entity only after a real gameplay transition turns it into a dynamic actor, for example:

- falling physics tree;
- dropped moving loot;
- special interactable with continuous dynamic state;
- construction object that needs ordinary replicated entity lifecycle.

That transition must be explicit. Do not use the legacy replicated node path as the default wilderness resource model.

## Networking Rules

- Overlay events use reliable sequenced delivery.
- Absolute overlay is the fallback for first enter, missing ack, or delta basis mismatch.
- Deltas are keyed by `PlacementId`, not `EntityGID`.
- Client harvest or interaction commands must send `PlacementId`; the server resolves and validates against placement index and overlay state.
- Unknown placement after the authority chunk has generated is an invalid server state and should fail fast.
- Invalid tool, range, or line-of-sight is gameplay rejection, not an architecture crash.

## Performance Notes

The old host spike came from treating static resources as replicated objects:

- server spawned many resource node entities from one chunk;
- cluster snapshot serialized each node's `NetworkIdentity`, state, and transform;
- host client materialized replicated entities and projected them into views.

The target model removes static resource nodes from the generic snapshot by default. When profiling open-world chunk entry now, compare:

- `ServerOpenWorldResourcePlacementIndexSystem`;
- `ServerOpenWorldChunkOverlaySendSystem`;
- `ServerOpenWorldChunkSnapshotSystem`;
- `ClientOpenWorldResourceProxySpawnSystem`;
- `ClientOpenWorldChunkOverlayApplySystem`;
- `ApplyComponentToViewSystem<OpenWorldResourceNodeViewState>`.

If `ServerOpenWorldChunkSnapshotSystem` is still high, inspect real dynamic actors in the cluster, not static resource placements first.

## Boundaries

- `Settlement.Contracts owns resource ids` and settlement storage contracts.
- `OpenWorldResources` owns deterministic placement indexing, overlay state, and harvest profile metadata.
- `ResourcesInventoryMinimal` owns carried raw inventory and pickup collection.
- `OpenWorldGeneration` owns deterministic generation and spatial chunk interest.
- `OpenWorldResources` may read chunk interest/loading state to send overlays, but must not take ownership of terrain generation.
- Other features must not mutate resource overlay state directly. Send typed gameplay events/commands to this feature.
- Settlement, inventory, and open-world resource cross-feature mutations must go through typed events or requests.
- Do not manually edit generated replication files.
- Do not add fallbacks that silently spawn replicated resource nodes when overlay state is missing.
