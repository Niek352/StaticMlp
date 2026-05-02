# StaticEcs + Unity Transport Architecture

Detailed architecture notes moved out of `AGENTS.md`.

## Mental Model

```text
StaticEcs = game state
Unity Transport = byte delivery
Replication Layer = components/events <-> packets
Gameplay Systems = normal ECS logic
```

Networking logic belongs in replication and transport layers. Gameplay systems should not manually serialize packets or touch Unity Transport directly.

## Ownership Rules

Client:

```text
LocalOwned:
    this client can write gameplay state

RemoteOwned:
    this client cannot write gameplay state;
    it only applies network state and smooths visuals
```

Server:

```text
ServerOwned:
    server simulates this entity

ClientOwned:
    server accepts state from a specific owning client
```

Ownership tags are local derived state. Replicate `NetworkIdentity` only:

```csharp
public struct NetworkIdentity : IComponent {
    public NetworkPeerId Owner;
    public NetworkAuthority Authority;
    public ushort PrefabId;
}
```

Then apply tags locally with `OwnershipTags.ApplyForClient` or `OwnershipTags.ApplyForServer`.

## Transport Rules

Allowed places to touch Unity Transport:

```text
ClientTransportCompleteSystem
ClientTransportScheduleSystem
ClientTransportSendSystem
ServerTransportCompleteSystem
ServerTransportScheduleSystem
ServerTransportSendSystem
Raw receive jobs
```

Gameplay code must never call:

```text
NetworkDriver.BeginSend
NetworkDriver.EndSend
NetworkConnection.PopEvent
```

## Packet Delivery Rules

```text
Spawn                reliable
Despawn              reliable
OwnershipChanged     reliable
ComponentBatch/move  unreliable sequenced
Inventory/quest      reliable
Events               depends on importance
```

## System Ordering

Always follow:

```text
Complete transport jobs
Drain raw inbox
Apply network state
Run gameplay
Collect dirty replication
Send packets
Schedule transport jobs
Tick world
```

Do not collect replication before gameplay mutates state. Do not apply network state after local gameplay in the same frame unless that is explicitly required.

## Unity Physics for Coop

The owner simulates physics. Everyone else renders kinematic replicated state.

Owner captures:

```csharp
state.Position = rigidbody.position;
state.Rotation = rigidbody.rotation;
state.Velocity = rigidbody.linearVelocity;
```

Remote applies:

```csharp
rigidbody.isKinematic = true;
rigidbody.MovePosition(state.Position);
rigidbody.MoveRotation(state.Rotation);
```

Do not try to make Unity Physics deterministic across clients.

## Standard Module Layout

```text
/Networking
    NetworkIdentity.cs
    NetworkAuthority.cs
    NetworkPeerId.cs
    NetPacketType.cs
    NetDelivery.cs
    NetInbox.cs
    NetOutbox.cs
    UtpTransportContext.cs

/Networking/Transport
    ClientTransportStartup.cs
    ServerTransportStartup.cs
    ClientReceiveJob.cs
    ServerReceiveJob.cs
    ServerUpdateConnectionsJob.cs
    ClientTransportSystems.cs
    ServerTransportSystems.cs

/Networking/Replication
    ReplicatedComponentAttribute.cs
    ReplicatedFieldAttribute.cs
    ReplicationRegistry.cs
    ReplicationCollectSystem.cs
    ReplicationApplySystem.cs
    SpawnPackets.cs
    ComponentDeltaPackets.cs
    OwnershipPackets.cs

/Networking/Ownership
    LocalOwned.cs
    RemoteOwned.cs
    ServerOwned.cs
    ClientOwned.cs
    OwnershipTags.cs

/Game/Components
    CharacterNetState.cs
    DoorState.cs
    Health.cs

/Game/Systems/Client
    LocalPlayerMovementSystem.cs
    RemoteSmoothingSystem.cs

/Game/Systems/Server
    ServerAiSystem.cs
    ServerDoorSystem.cs
    ServerLootSystem.cs

/Game/Presentation
    ViewTransform.cs
    CameraFollowSystem.cs
    AnimationBindingSystem.cs
```
