# CRDT Research For Coop StaticEcs Networking

## Goal

This note collects the current research context about using CRDT ideas in this project.

Primary project constraints:

- the game is `coop-first`
- trusted environment is acceptable
- speed of development matters more than perfect competitive netcode
- architecture should stay as simple as possible
- gameplay code should remain ECS-first, not transport-first

This is a research note, not an implementation spec.

## Current Project Shape

Current networking model is:

```text
StaticEcs = gameplay state
Unity Transport = byte delivery
Replication layer = components/events <-> packets
Gameplay systems = normal ECS logic
```

Important existing architecture points:

- replicated state is described with `[ReplicatedComponent]`
- client-to-server actions are described with `[ReplicatedEvent]`
- spawn/despawn and lifecycle are already handled by the current replication stack
- `EntityGID` already exists and is the correct persistent entity reference type
- ownership is derived locally from `NetworkIdentity`

Useful code references:

- [ReplicatedComponentAttribute.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Networking/ReplicationContracts/ReplicatedComponentAttribute.cs>)
- [ReplicatedEventAttribute.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Networking/ReplicationContracts/ReplicatedEventAttribute.cs>)
- [ReplicationRegistry.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Game/Replication/ReplicationRegistry.cs>)
- [NetworkEventRegistry.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Networking/Runtime/Replication/NetworkEventRegistry.cs>)
- [NetworkEntitySpawner.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Networking/Replication/NetworkEntitySpawner.cs>)
- [ClientSpawnApplySystem.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Game/Replication/ClientSpawnApplySystem.cs>)
- [EntityGID.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Library/PackageCache/com.felid-force-studios.static-ecs@b9ab907789ec/Src/EntityGID.cs>)

## What CRDT Means Here

For this project, CRDT should not be treated as "replace networking".

Better mental model:

```text
CRDT = merge/convergence model for replicated data
transport = packet delivery
authority = gameplay validation policy
prediction = latency-hiding UX layer
```

This distinction is important.

CRDT is helpful when multiple peers may update shared state and all replicas should converge despite:

- duplicate packets
- out-of-order packets
- dropped packets
- reconnect/resync

CRDT does not automatically solve:

- latency hiding
- anti-cheat
- transactional invariants
- multi-entity rule enforcement
- deterministic simulation

## Decentraland-Inspired Direction

The most promising CRDT direction for this project is not separate CRDT classes per gameplay component.

Not this:

```text
CrdtTransform
CrdtInventory
CrdtDoor
CrdtHealth
```

Prefer this:

```text
generic CRDT store
key = EntityId + ComponentId
value = serialized bytes + version/timestamp + op kind
```

This matches the Decentraland-style idea:

```text
ComponentId -> EntityId -> replicated entry
```

That direction keeps the merge logic generic and leaves component-specific knowledge in serializers/bindings.

## Why CRDT Is More Interesting In Coop

In a coop game, CRDT becomes more attractive because:

- we can tolerate more trust between peers
- small temporary inconsistencies are less catastrophic
- local-first UX is more valuable than strict server arbitration everywhere
- host migration and reconnect become easier to reason about
- shared scene/object state is often a better fit for convergence than pure RPC chains

However, coop does not mean "CRDT everywhere".

If used without scope control, CRDT can still increase complexity:

- difficult to reason about invariants
- hard to debug eventual consistency issues
- tombstones and cleanup are easy to forget
- not every gameplay state should be multi-writer

## Main Conclusion

For this project, the best direction is:

```text
one replication model
with multiple merge strategies
instead of
two separate networking stacks
```

Do not build:

- one "base replication" path
- one completely separate "CRDT path"

Instead build one system where ordinary authoritative replication is treated as one merge mode, and CRDT-style convergence is another merge mode.

## Recommended Unification Model

Use current `[ReplicatedComponent]` and `[ReplicatedEvent]` as the single public API surface, then extend them with merge semantics.

Conceptually:

```csharp
public enum ReplicatedMergeMode : byte
{
    AuthorityReplace = 0,
    LwwRegister = 1,
    AppendCommand = 2
}

public enum ReplicatedPredictionMode : byte
{
    None = 0,
    ClientOverlay = 1
}
```

Then:

- ordinary current replication = `AuthorityReplace`
- shared-object convergent state = `LwwRegister`
- commands/events = `AppendCommand`
- local-first UX = `ClientOverlay`

This keeps one mental model for the team.

## Entity-Level CRDT vs Component-Level CRDT

Question considered:

- should CRDT be marked on the entire entity
- or should it be marked per replicated component/event

Conclusion:

- core semantics should live at `ReplicatedComponent` and `ReplicatedEvent` level
- entity-level CRDT should exist only as a profile/defaults mechanism

Why:

- one entity usually contains mixed semantics
- some fields are authoritative snapshots
- some are append-style commands
- some are local-only presentation state
- some are predicted overlays

Entity-level marking is too coarse as the primary rule.

Better pattern:

- component/event metadata defines real replication semantics
- entity profile can provide defaults for shared-object categories

Example conceptual preset:

```text
SharedObject entity profile:
default component merge = LwwRegister
default event merge = AppendCommand
default writer policy = host or lease-based
```

## Recommended CRDT Scope

Best first scope for CRDT:

- shared scene objects
- interactable props
- placed world objects
- carryable objects
- doors, buttons, levers
- low-frequency shared object transform/state

Good candidates:

- `DoorState`
- `GrabbedBy`
- `PlacedState`
- `SharedObjectTransform`
- `ToggleState`
- shared world variables

Bad first candidates:

- player movement
- combat truth
- AI internal simulation
- inventory economy truth
- projectile simulation
- complex deterministic physics

## SharedObject Strategy

For shared objects, CRDT is promising if combined with a writer policy.

Important simplification:

- do not allow true unrestricted multi-writer by default
- prefer `single active writer + convergent state distribution`

Example writer policies:

- `HostWriter`
- `LeaseWriter`

Typical approach:

- host owns idle object truth
- when player grabs object, host grants temporary lease
- current writer updates object state
- everyone else converges to it

This is much simpler than unrestricted multi-writer object movement.

## Snapshot vs Append

This is the most important classification for implementation.

`Snapshot state`:

- long-lived object state
- usually one current value
- best fit for `AuthorityReplace` or `LwwRegister`

Examples:

- transform snapshot
- door open/closed
- grabbed by player X
- placed/not placed

`Append state`:

- actions, intents, commands, transient events
- best fit for `AppendCommand`

Examples:

- interact request
- grab request
- release request
- use request
- deposit request

## SendDeposit Case Study

Current code path:

- client sends [DepositConstructionResourcesRequestEvent.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Events/DepositConstructionResourcesRequestEvent.cs>)
- client trigger is in [ClientConstructionInteractionSystem.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Client/ClientConstructionInteractionSystem.cs>)
- server validates and applies in [ServerDepositConstructionResourcesSystem.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Server/ServerDepositConstructionResourcesSystem.cs>)
- gameplay rules are in [ConstructionRules.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Domain/ConstructionRules.cs>)
- affected state includes:
  - [ResourcesInventory.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Features/ResourcesInventoryMinimal/Runtime/Components/ResourcesInventory.cs>)
  - [ConstructionResources.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Game/Components/Buildings/ConstructionResources.cs>)
  - [ConstructionSiteState.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Game/Components/Buildings/ConstructionSiteState.cs>)

Important observation:

`SendDeposit` is not just shared object state.

It is a command that mutates:

- player inventory
- building delivered resources
- building phase

This is a multi-entity validated operation.

Because of that, `SendDeposit` is not a good candidate for "pure CRDT state".

Better model:

- `DepositConstructionResourcesRequestEvent` = append-style command
- resulting authoritative state remains ordinary replicated components
- client UX improvement comes from local anticipation/reconciliation

## Recommended Implementation Direction For SendDeposit

Do not try to convert `DepositConstructionResourcesRequestEvent` into a free multi-writer CRDT state mutation.

Recommended direction:

- keep it as a replicated event/command
- extend replicated events with prediction metadata
- extend affected replicated components with predicted overlay support

Conceptually:

- `DepositConstructionResourcesRequestEvent` = `AppendCommand + ClientOverlay`
- `ResourcesInventory` = `AuthorityReplace + ClientOverlay`
- `ConstructionResources` = `AuthorityReplace + ClientOverlay`
- `ConstructionSiteState` = `AuthorityReplace + ClientOverlay`

This preserves one unified replication architecture.

## How Prediction Would Fit Without A Second Stack

The current replication registry already has extension points:

- custom `clientApply`
- client-only type registration
- client-only systems registration
- client state initialization

See:

- [ReplicationRegistry.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Game/Replication/ReplicationRegistry.cs>)
- [PhysicsCubeNetState.Replication.Generated.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Generated/ReplicationComponents/PhysicsCubeNetState.Replication.Generated.cs>)
- [ReplicatedComponentRegistration.Generated.cs](</C:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Generated/ReplicationComponents/ReplicatedComponentRegistration.Generated.cs>)

This means prediction/reconciliation can be integrated into the same generated replication path, not built as a separate networking layer.

Possible model:

- server truth is stored as authoritative state
- client keeps pending predicted commands
- effective client state is recomputed as:

```text
effective = authoritative + pending overlays
```

This allows the feature gameplay and UI to keep reading ordinary ECS components, while the generated client-apply path handles reconciliation.

## Why Full CRDT Does Not Solve SendDeposit By Itself

Even in a coop game, CRDT does not automatically solve the deposit problem because:

- inventory spending must not go below zero
- deposit amount is capped by remaining required resources
- multiple entities are changed together
- range validation is required
- gameplay phase transition is part of the transaction

These are gameplay invariants, not just convergence mechanics.

So for `SendDeposit`, the real win comes from:

- append-style command transport
- local anticipation
- authoritative confirmation
- reconciliation

Not from replacing the rule system with generic CRDT merge.

## Practical Architecture Recommendation

If this project continues CRDT research, use this layering:

```text
Gameplay ECS
-> ReplicatedComponent / ReplicatedEvent metadata
-> generated serializers + generated merge behavior
-> unified replication registry
-> transport
```

And support these semantic modes inside that one system:

- `AuthorityReplace`
- `LwwRegister`
- `AppendCommand`
- `ClientOverlay`

This avoids maintaining two fundamentally different networking paradigms.

## Greenfield Guideline

If starting a new feature:

1. decide whether the thing is state or command
2. decide whether it is single-writer or shared-object
3. decide whether it needs local prediction
4. decide whether host validation is still required

Decision examples:

- player movement:
  - command/input + prediction + authority
- shared door:
  - snapshot state + optional append interaction request
- carried cube:
  - lease-based writer + convergent transform
- construction deposit:
  - append command + local anticipation + server reconciliation

## Recommended Next Research Questions

If continuing the CRDT study in a new thread, useful follow-up prompts are:

1. design a unified `ReplicatedMergeMode` API that extends current `[ReplicatedComponent]` and `[ReplicatedEvent]`
2. design generated client overlay reconciliation for authoritative components
3. design a `SharedObject` profile using `LwwRegister + AppendCommand + writer lease`
4. design command sequencing/ack flow for predicted events such as `DepositConstructionResourcesRequestEvent`
5. design cleanup/snapshot rules for append logs and tombstones

## External References

- [Habr: CRDT](https://habr.com/ru/articles/418897/)
- [CRDT-Based Game State Synchronization in Peer-to-Peer VR](https://arxiv.org/html/2503.17826v1)
- [Decentraland Unity Explorer Architecture Overview](https://github.com/decentraland/unity-explorer/blob/27b999ed9ea6cd7678ae77276e7309b27b9bb63f/docs/architecture-overview.md)
- [Decentraland ADR-117](https://adr.decentraland.org/adr/ADR-117)

## Bottom Line

Short version:

- CRDT is worth studying for this coop project
- CRDT is best aimed at shared objects and convergent world state
- `SendDeposit` should stay command-driven, not become pure CRDT state
- do not build a second parallel networking stack
- unify everything under one replication model with different merge strategies
