# Request API Implementation Plan

## Goal

This note describes the implementation plan for a `RequestApi` layer on top of the current networking stack.

Target outcome:

- feature code sends request-style gameplay actions through a simple API
- client gets zero-ping style local UX through projected state
- authoritative replication remains the source of truth
- feature authors do not write manual rollback systems for each action

The intended feature-facing usage is:

```csharp
RequestApi.Send(new DepositConstructionResourcesRequestEvent(...));
```

Feature authors should ideally define only:

- request
- result
- server handler
- optional client projector

## Design Direction

Do not build a second networking stack.

Build a request/result layer on top of the current:

- `ReplicatedEvent`
- typed network event registry
- authoritative component replication

Core rule:

```text
authoritative state stays authoritative
pending requests stay separate
effective client state = authoritative + pending projections
```

This avoids manual rollback of authoritative ECS components on the client.

## Runtime Model

Planned abstractions:

```csharp
public interface IRequest<TResult> : IEvent
    where TResult : struct, IRequestResult
{
    RequestId RequestId { get; set; }
}

public interface IRequestResult : IEvent
{
    RequestId RequestId { get; set; }
    RequestStatus Status { get; set; }
}

public interface IRequestHandler<TRequest, TResult>
    where TRequest : struct, IRequest<TResult>
    where TResult : struct, IRequestResult
{
    TResult Handle(NetworkPeerId sourcePeer, in TRequest request);
}

public interface IRequestProjector<TRequest, TResult>
    where TRequest : struct, IRequest<TResult>
    where TResult : struct, IRequestResult
{
    void Project(in TRequest request);
    void OnResolved(in TRequest request, in TResult result);
}
```

Supporting types:

- `RequestId`
- `RequestStatus`
- `RequestApi`
- `RequestRegistry`
- `ClientPendingRequests`
- `ClientProjection`
- `Projected<T>`

## Architecture Principles

1. Keep transport and packet routing unchanged.
2. Keep authoritative replication unchanged as much as possible.
3. Add typed `server -> client` event flow so results can be delivered cleanly.
4. Do not reconcile by mutating authoritative ECS state and rolling it back later.
5. Rebuild projected client state from authoritative state plus pending requests.
6. Use one vertical slice first before adding auto-magic or declarative sugar.

## Implementation Phases

## Phase 1: Typed Server-To-Client Events

### Goal

Support typed network events from server to client, symmetric to the existing client-to-server path.

### Changes

- Extend `NetworkEventRegistry` with `TryApplyToClient(in NetworkEventPacket packet)`.
- Add client-side typed event registration alongside the existing server-side typed event registration.
- Add `ClientNetworkEventApplySystem`.
- Add `SW.SendToPeerEvent<TEvent>(NetworkPeerId peer, in TEvent evt)`.

### Files To Change

- `Assets/Scripts/StaticMlp/Networking/Runtime/Replication/NetworkEventRegistry.cs`
- `Assets/Scripts/StaticMlp/Networking/CW.cs`
- add `Assets/Scripts/StaticMlp/Networking/SW.cs` helper if needed
- `Assets/Scripts/StaticMlp/Composition/Runtime/Bootstrap/MultiplayerSystemBootstrap.cs`
- `Assets/Scripts/StaticMlp/Composition/Runtime/Bootstrap/StaticMlpMultiplayerBootstrap.cs`

### System Order

Add `ClientNetworkEventApplySystem` after component delta apply:

```text
-810 ClientSnapshotApplySystem
-800 ClientSpawnApplySystem
-790 ClientDespawnApplySystem
-780 ClientOwnershipApplySystem
-770 ClientComponentDeltaApplySystem
-760 ClientNetworkEventApplySystem
```

### Expected Result

- server can send typed `result` events back to the owning client
- feature code no longer needs raw network packet logic for results

## Phase 2: Request Contracts And Registry

### Goal

Introduce the generic request/result contracts and registration runtime.

### New Folder

```text
Assets/Scripts/StaticMlp/Networking/Runtime/Requests
```

### New Types

- `RequestId.cs`
- `RequestStatus.cs`
- `IRequest.cs`
- `IRequestResult.cs`
- `IRequestHandler.cs`
- `IRequestProjector.cs`
- `RequestRegistry.cs`
- `RequestApi.cs`
- `ClientPendingRequests.cs`

### Notes

- Requests and results remain ordinary `[ReplicatedEvent]` types.
- `IRequest<TResult>` and `IRequestResult` add semantics, not a separate transport path.
- Do not add attribute-driven projection metadata yet.

### Expected Result

- one consistent request model exists
- `RequestApi.Send(...)` becomes the feature-facing entry point

## Phase 3: Generic Pending Request Flow

### Goal

Track pending requests on the client and resolve them when `result` arrives.

### New Systems

- `ClientRequestResultApplySystem`

### Responsibilities

`RequestApi.Send(...)`:

- allocates `RequestId`
- stores request in `ClientPendingRequests`
- sends request through the existing event path

`ClientRequestResultApplySystem`:

- receives typed request result events
- looks up the matching pending request by `RequestId`
- removes the pending request entry
- invokes `projector.OnResolved(...)`

### Important Rule

This layer should not do manual world repair.

It should:

- manage pending lifecycle
- invoke optional UX hooks

It should not:

- hand-edit authoritative replicated components to undo prediction

### Suggested Order

```text
200 ClientRequestResultApplySystem
```

### Expected Result

- request resolution becomes generic
- feature-specific reconcile systems are no longer needed

## Phase 4: Projected Client Read Model

### Goal

Allow zero-ping local UX without mutating authoritative ECS state directly.

### New Types

- `Projected<T>.cs`
- `ProjectionRegistry.cs`
- `ClientProjection.cs`
- `ClientProjectionRebuildSystem.cs`

### Model

Projected state should be rebuilt as:

```text
projected = authoritative copy
projected += all matching pending request projections
```

### Responsibilities

`ProjectionRegistry`:

- tracks which component types participate in projection

`ClientProjectionRebuildSystem`:

- copies authoritative component values into projected values
- runs all pending request projectors over the projected layer

`ClientProjection` helpers:

- `Read<T>(entity)`
- `Mut<T>(entity)`

### Rules

- do not project every component in the world by default
- start only with components needed by predicted actions
- presentation and local UX should read projected state when relevant

### Suggested Order

```text
200 ClientRequestResultApplySystem
210 ClientProjectionRebuildSystem
250 ClientPresentation systems
```

### Expected Result

- no manual rollback for reject/mismatch
- projected state naturally changes when pending requests appear or disappear

## Phase 5: First Vertical Slice With Deposit

### Goal

Validate the whole architecture on one real feature before expanding it.

### Feature Types

- `DepositConstructionResourcesRequestEvent`
- `DepositConstructionResourcesResultEvent`
- `DepositConstructionResourcesHandler`
- `DepositConstructionResourcesProjector`

### Client Entry Point

Update:

- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Client/ClientConstructionInteractionSystem.cs`

Change:

- replace `CW.SendToServerEvent(...)`
- use `RequestApi.Send(...)`

### Server Logic

Refactor current deposit server logic from:

- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Server/ServerDepositConstructionResourcesSystem.cs`

Into:

- `DepositConstructionResourcesHandler`

The domain rules in:

- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Domain/ConstructionRules.cs`

should stay authoritative and remain the single validation source.

### Projection Scope For Deposit

Start with projection for:

- `ResourcesInventory`
- `ConstructionResources`
- `ConstructionSiteState`

The projector should:

- inspect projected current state
- compute accepted predicted values with existing domain rules
- write only to projected state

### Success Criteria

- local client sees immediate response on deposit
- server still owns gameplay truth
- result event resolves pending request cleanly
- no custom rollback system is needed

## Phase 6: Registration And Authoring Simplification

### Goal

Reduce manual setup after the first vertical slice proves the architecture.

### Options

- reflection-based request discovery
- code-generated request registration

Preferred direction:

- codegen, because the project already uses generated registration for replicated components and events

### Desired Outcome

Feature authors should only write:

- request
- result
- handler
- optional projector

And the runtime registration should happen automatically.

## Phase 7: Declarative Sugar

### Goal

Further reduce feature boilerplate only after the core model is stable.

### Possible Additions

- `RequestProjectionProfile`
- declarative projection rules for simple add/subtract cases
- default no-op `OnResolved`
- helper base types for common predicted request patterns

### Important Constraint

Do not implement this before the first real feature slice works.

## Files Likely To Be Touched

### Existing Files

- `Assets/Scripts/StaticMlp/Networking/Runtime/Replication/NetworkEventRegistry.cs`
- `Assets/Scripts/StaticMlp/Composition/Runtime/Bootstrap/MultiplayerSystemBootstrap.cs`
- `Assets/Scripts/StaticMlp/Composition/Runtime/Bootstrap/StaticMlpMultiplayerBootstrap.cs`
- `Assets/Scripts/StaticMlp/Networking/CW.cs`
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Client/ClientConstructionInteractionSystem.cs`
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Server/ServerDepositConstructionResourcesSystem.cs`

### New Runtime Area

- `Assets/Scripts/StaticMlp/Networking/Runtime/Requests/*`

## What Not To Do Yet

- do not add CRDT merge logic here
- do not create a second networking stack
- do not build manual rollback systems per request
- do not auto-register everything before the first vertical slice works
- do not project the full world by default

## Recommended Delivery Order

1. Add typed `server -> client` event application.
2. Add request contracts and `RequestApi`.
3. Add generic pending result resolution.
4. Add projected read-model support.
5. Migrate `deposit` as the first full vertical slice.
6. Add registration automation.
7. Add declarative sugar only after the architecture proves itself.

## Bottom Line

The right simplification is not automatic rollback of authoritative ECS state.

The right simplification is:

```text
authoritative state
+ pending requests
+ projected read model
= easy fast iteration
```

That keeps gameplay rules authoritative while giving feature authors a much smaller surface area for local-first UX.
