# Multiplayer Architecture Plan: StaticEcs + Unity Transport

## 0. Цель

Создать coop-friendly multiplayer-архитектуру для Unity, где:

- StaticEcs хранит game state.
- Unity Transport доставляет bytes.
- Replication layer автоматически синхронизирует `[ReplicatedComponent]`.
- Gameplay-код не пишет сетевой boilerplate.
- Ownership выражается ECS-тегами: `LocalOwned`, `RemoteOwned`, `ServerOwned`, `ClientOwned`.
- Для coop используется owner-authoritative state replication, без rollback/reconciliation.
- Unity Physics не синхронизируется детерминированно: владелец симулирует объект, остальные отображают replicated state.

## 1. Базовая модель

### 1.1 Authority

```text
ServerOwned:
    сервер пишет state
    клиенты принимают state

LocalOwned:
    локальный клиент пишет state
    сервер принимает и relay-ит observers

RemoteOwned:
    локальная машина не пишет state
    только применяет network state + smoothing

ClientOwned:
    серверная сторона entity, чьё состояние приходит от client-owner
```

Важно:

```text
LocalOwned / RemoteOwned / ServerOwned / ClientOwned — локальные runtime-теги.
Они не реплицируются напрямую.
Реплицируется metadata-компонент NetworkIdentity.
```

### 1.2 NetworkIdentity

```csharp
public enum NetworkAuthority : byte {
    Server = 0,
    Owner = 1,
    LocalOnly = 2
}

public readonly struct NetworkPeerId {
    public readonly ushort Value;
    public NetworkPeerId(ushort value) => Value = value;
}

public struct NetworkIdentity : IComponent {
    public NetworkPeerId Owner;
    public NetworkAuthority Authority;
    public ushort PrefabId;
}
```

### 1.3 Ownership tags

```csharp
public struct LocalOwned : ITag { }
public struct RemoteOwned : ITag { }
public struct ServerOwned : ITag { }
public struct ClientOwned : ITag { }
public struct NetworkedTag : ITag { }
```

На клиенте:

```text
Owner == LocalPeerId && Authority == Owner → LocalOwned
Owner != LocalPeerId && Authority == Owner → RemoteOwned
Authority == Server                         → RemoteOwned или ServerOwnedView
```

На сервере:

```text
Authority == Server → ServerOwned
Authority == Owner  → ClientOwned
```

## 2. Миры

### 2.1 Server

```csharp
public struct ServerWT : IWorldType { }
public abstract class SW : World<ServerWT> { }

public struct ServerSystemsT : ISystemsType { }
public abstract class ServerSys : SW.Systems<ServerSystemsT> { }
```

### 2.2 Client

```csharp
public struct ClientCoreWT : IWorldType { }
public abstract class CW : World<ClientCoreWT> { }

public struct ClientUxWT : IWorldType { }
public abstract class UXW : World<ClientUxWT> { }

public struct ClientCoreSystemsT : ISystemsType { }
public abstract class ClientCoreSys : CW.Systems<ClientCoreSystemsT> { }

public struct ClientUxSystemsT : ISystemsType { }
public abstract class ClientUxSys : UXW.Systems<ClientUxSystemsT> { }
```

`ClientCoreWorld` содержит replicated gameplay state.  
`ClientUxWorld` содержит input, camera, UI, cursor, selection, render smoothing.

## 3. StaticEcs требования

Использовать `EntityGID` для сетевых ссылок и spawn-by-id. Обычный `Entity` нельзя хранить как persistent reference.

Использовать `Mut<T>()`, когда изменение компонента должно попасть в change tracking. `Ref<T>()` не помечает компонент changed.

Включить tracking для реплицируемых компонентов:

```csharp
public struct CharacterNetState : IComponent, IComponentConfig<CharacterNetState> {
    public Vector3 Position;
    public Vector3 Velocity;
    public Quaternion Rotation;

    public ComponentTypeConfig<CharacterNetState> Config() => new(
        guid: new Guid("11111111-1111-1111-1111-111111111111"),
        trackAdded: true,
        trackDeleted: true,
        trackChanged: true
    );
}
```

После `Systems.Update()` обязательно делать:

```csharp
W.Tick();
```

## 4. Serialization strategy

### 4.1 StaticEcs serializer

Использовать для:

- spawn snapshot;
- full entity resync;
- late join;
- scene/zone streaming;
- debug save/load.

```csharp
using var writer = SW.Serializer.CreateEntitiesSnapshotWriter();
writer.Write(entity);
byte[] payload = writer.CreateSnapshot();
```

На клиенте:

```csharp
CW.Serializer.LoadEntitiesSnapshot(payload, entitiesAsNew: false);
```

### 4.2 Generated component replication

Использовать для частых deltas:

- transform;
- rigidbody state;
- animation;
- health;
- owner-controlled state.

Компонент:

```csharp
[ReplicatedComponent(
    authority: ReplicationAuthority.Owner,
    delivery: NetDelivery.UnreliableSequenced,
    sendRate: 20
)]
public struct CharacterNetState : IComponent {
    [ReplicatedField(Quantize = 0.01f)]
    public Vector3 Position;

    [ReplicatedField(Quantize = 0.01f)]
    public Vector3 Velocity;

    [ReplicatedField(Compress = true)]
    public Quaternion Rotation;
}
```

Генератор должен создать:

```text
CharacterNetStateReplicator
CharacterNetStateSerializer
CharacterNetStateDirtyComparer
CharacterNetStateApplyHandler
ComponentTypeId
```

## 5. Unity Transport layer

Unity Transport отвечает только за bytes.

### 5.1 Delivery

```csharp
public enum NetDelivery : byte {
    Unreliable = 0,
    UnreliableSequenced = 1,
    ReliableSequenced = 2
}
```

Mapping:

```text
UnreliableSequenced:
    CharacterNetState
    RigidbodyNetState
    AnimationNetState

ReliableSequenced:
    Spawn
    Despawn
    OwnershipChanged
    Inventory
    QuestState
    DoorState
    PickupItemEvent
```

### 5.2 Packet types

```csharp
public enum NetPacketType : byte {
    Hello = 1,
    Welcome = 2,

    Spawn = 10,
    Despawn = 11,
    OwnershipChanged = 12,

    ComponentBatch = 20,
    NetworkEvent = 30,

    Ping = 40,
    Pong = 41
}
```

### 5.3 Transport context

```csharp
public sealed class UtpTransportContext {
    public NetworkDriver Driver;

    public NetworkPipeline UnreliablePipeline;
    public NetworkPipeline UnreliableSequencedPipeline;
    public NetworkPipeline ReliableSequencedPipeline;

    public NativeArray<NetworkConnection> ServerConnection; // client side
    public NativeList<NetworkConnection> ClientConnections; // server side

    public JobHandle TransportJobHandle;

    public NativeQueue<RawNetworkPacket> RawInbox;
    public NetInbox Inbox = new();
    public NetOutbox Outbox = new();

    public bool IsServer;
    public NetworkPeerId LocalPeerId;
}
```

### 5.4 Transport abstraction

```csharp
public interface INetworkTransport {
    bool IsServer { get; }

    void Send(NetworkPeerId peer, ReadOnlySpan<byte> payload, NetDelivery delivery);

    bool TryReceive(out NetworkPeerId peer, out ReadOnlySpan<byte> payload);

    void Poll();
}
```

Replication layer зависит от `INetworkTransport`, а не от Unity Transport напрямую.

## 6. Server systems

Рекомендуемый порядок:

```text
-1000 ServerTransportCompleteSystem
 -900 ServerRawInboxDrainSystem
 -850 ServerConnectionLifecycleSystem
 -840 ServerHelloWelcomeSystem

 -800 ServerSpawnRequestSystem
 -790 ServerOwnershipRequestSystem
 -780 ServerReceiveClientOwnedStateSystem
 -770 ServerReceiveClientNetworkEventsSystem

    0 ServerAiSystem
   10 ServerDoorSystem
   20 ServerLootSystem
   30 ServerQuestSystem

  400 ServerInterestSystem
  500 ServerOwnedReplicationCollectSystem
  550 ServerRelayClientOwnedStateSystem
  600 ServerReplicationPacketBuildSystem
  700 ServerTransportSendSystem

 1000 ServerTransportScheduleSystem
```

### 6.1 ServerTransportCompleteSystem

```csharp
public sealed class ServerTransportCompleteSystem : ISystem {
    public void Update() {
        var ctx = SW.GetResource<UtpTransportContext>();
        ctx.TransportJobHandle.Complete();
    }
}
```

### 6.2 ServerConnectionLifecycleSystem

Принимает новые connections, удаляет closed connections, выдаёт peer id.

### 6.3 ServerReceiveClientOwnedStateSystem

```csharp
public sealed class ServerReceiveClientOwnedStateSystem : ISystem {
    public void Update() {
        ref var inbox = ref SW.GetResource<NetInbox>();

        foreach (var batch in inbox.ComponentBatches) {
            foreach (var delta in batch.Deltas) {
                if (!delta.Gid.TryUnpack<ServerWT>(out var e))
                    continue;

                if (!e.Has<ClientOwned>())
                    continue;

                ref readonly var net = ref e.Read<NetworkIdentity>();

                if (net.Owner.Value != batch.SourcePeer.Value)
                    continue;

                ReplicationRegistry.ApplyDelta(e, delta);
                ServerRelayBuffer.Add(batch.SourcePeer, delta);
            }
        }
    }
}
```

### 6.4 ServerOwnedReplicationCollectSystem

```csharp
public sealed class ServerOwnedReplicationCollectSystem : ISystem {
    public void Update() {
        foreach (var e in SW.Query<All<ServerOwned, NetworkedTag, NetworkIdentity>>().Entities()) {
            ReplicationRegistry.CollectDirty(e, Outbox);
        }
    }
}
```

### 6.5 ServerRelayClientOwnedStateSystem

```csharp
public sealed class ServerRelayClientOwnedStateSystem : ISystem {
    public void Update() {
        foreach (var item in ServerRelayBuffer.Items) {
            foreach (var observer in Interest.GetObservers(item.Delta.Gid)) {
                if (observer.Value == item.SourcePeer.Value)
                    continue;

                Outbox.EnqueueComponentDelta(
                    observer,
                    item.Delta,
                    NetDelivery.UnreliableSequenced
                );
            }
        }

        ServerRelayBuffer.Clear();
    }
}
```

## 7. Client systems

Рекомендуемый порядок:

```text
-1000 ClientTransportCompleteSystem
 -900 ClientRawInboxDrainSystem

 -800 ClientSpawnApplySystem
 -790 ClientDespawnApplySystem
 -780 ClientOwnershipApplySystem
 -770 ClientComponentDeltaApplySystem
 -760 ClientNetworkEventApplySystem

 -100 ClientUxToCoreBridgeSystem
    0 LocalPlayerMovementSystem
   10 OwnedRigidbodyCaptureSystem

  300 RemoteSmoothingSystem
  400 CameraFollowSystem
  410 AnimationBindingSystem
  420 UiBindingSystem

  500 ClientReplicationCollectSystem
  600 ClientReplicationPacketBuildSystem
  700 ClientTransportSendSystem

 1000 ClientTransportScheduleSystem
```

### 7.1 ClientSpawnApplySystem

```csharp
public sealed class ClientSpawnApplySystem : ISystem {
    public void Update() {
        ref var inbox = ref CW.GetResource<NetInbox>();

        foreach (var spawn in inbox.Spawns) {
            if (spawn.Gid.TryUnpack<ClientCoreWT>(out _))
                continue;

            var e = CW.NewEntityByGID<Default>(spawn.Gid);

            e.Set(new NetworkIdentity {
                Owner = spawn.Owner,
                Authority = spawn.Authority,
                PrefabId = spawn.PrefabId
            });

            e.Set<NetworkedTag>();

            PrefabRegistry.Apply(spawn.PrefabId, e);
            ReplicationRegistry.ApplyInitialState(e, spawn.Components);

            OwnershipTags.ApplyForClient(e, spawn.Owner, spawn.Authority);
        }
    }
}
```

### 7.2 ClientOwnershipApplySystem

```csharp
public sealed class ClientOwnershipApplySystem : ISystem {
    public void Update() {
        ref var inbox = ref CW.GetResource<NetInbox>();

        foreach (var msg in inbox.OwnershipChanges) {
            if (!msg.Gid.TryUnpack<ClientCoreWT>(out var e))
                continue;

            ref var net = ref e.Mut<NetworkIdentity>();
            net.Owner = msg.NewOwner;
            net.Authority = msg.Authority;

            OwnershipTags.ApplyForClient(e, msg.NewOwner, msg.Authority);
        }
    }
}
```

### 7.3 ClientComponentDeltaApplySystem

```csharp
public sealed class ClientComponentDeltaApplySystem : ISystem {
    public void Update() {
        ref var inbox = ref CW.GetResource<NetInbox>();

        foreach (var batch in inbox.ComponentBatches) {
            foreach (var delta in batch.Deltas) {
                if (!delta.Gid.TryUnpack<ClientCoreWT>(out var e)) {
                    PendingDeltas.Add(delta);
                    continue;
                }

                if (e.Has<LocalOwned>())
                    continue;

                ReplicationRegistry.ApplyDelta(e, delta);
            }
        }
    }
}
```

### 7.4 ClientReplicationCollectSystem

```csharp
public sealed class ClientReplicationCollectSystem : ISystem {
    public void Update() {
        foreach (var e in CW.Query<All<LocalOwned, NetworkedTag, NetworkIdentity>>().Entities()) {
            ReplicationRegistry.CollectDirty(e, Outbox);
        }
    }
}
```

## 8. Ownership tag helper

```csharp
public static class OwnershipTags {
    public static void ApplyForClient(CW.Entity e, NetworkPeerId owner, NetworkAuthority authority) {
        if (e.Has<LocalOwned>()) e.Delete<LocalOwned>();
        if (e.Has<RemoteOwned>()) e.Delete<RemoteOwned>();
        if (e.Has<ServerOwned>()) e.Delete<ServerOwned>();

        if (authority == NetworkAuthority.Owner && owner.Value == NetworkRuntime.LocalPeerId.Value) {
            e.Set<LocalOwned>();
            return;
        }

        e.Set<RemoteOwned>();
    }

    public static void ApplyForServer(SW.Entity e, NetworkPeerId owner, NetworkAuthority authority) {
        if (e.Has<ClientOwned>()) e.Delete<ClientOwned>();
        if (e.Has<ServerOwned>()) e.Delete<ServerOwned>();

        if (authority == NetworkAuthority.Server) {
            e.Set<ServerOwned>();
            return;
        }

        if (authority == NetworkAuthority.Owner) {
            e.Set<ClientOwned>();
            return;
        }
    }
}
```

## 9. Pipeline

### 9.1 Client-owned player movement

```text
Client UX:
    Read input

Client Core:
    Query<LocalOwned, PlayerTag, CharacterNetState>
    Mut<CharacterNetState>()

Replication:
    tracking detects CharacterNetState changed
    generated replicator writes component delta
    packet sent via Unity Transport UnreliableSequenced

Server:
    receives ComponentBatch
    checks entity has ClientOwned
    checks NetworkIdentity.Owner == sourcePeer
    applies delta to server copy
    relays delta to observers

Remote clients:
    receive delta
    apply to RemoteOwned entity
    RemoteSmoothingSystem updates view transform
```

### 9.2 Server-owned monster

```text
Server:
    Query<ServerOwned, MonsterTag, CharacterNetState>
    AI mutates CharacterNetState
    replication collects dirty state
    sends delta to observers

Clients:
    apply delta
    smooth remote view
```

### 9.3 Ownership transfer

```text
Client:
    sends OwnershipRequestEvent

Server:
    validates
    changes NetworkIdentity.Owner
    updates ownership tags on server
    sends OwnershipChanged reliable packet

Clients:
    update NetworkIdentity
    recalculate LocalOwned/RemoteOwned tags
```

## 10. Unity Transport job flow

### Client

```text
Begin frame:
    TransportJobHandle.Complete()

Main thread:
    drain raw packets
    apply ECS packets
    run gameplay
    build outbox
    send outbox

End frame:
    Driver.ScheduleUpdate()
    ClientReceiveJob.Schedule(driverHandle)
```

### Server

```text
Begin frame:
    TransportJobHandle.Complete()

Main thread:
    drain raw packets
    apply ECS packets
    run server gameplay
    build outbox
    send outbox

End frame:
    Driver.ScheduleUpdate()
    ServerUpdateConnectionsJob.Schedule(driverHandle)
    ServerReceiveJob.Schedule(connections, dependency)
```

## 11. Unity Transport startup

### 11.1 Client startup

```csharp
public static void StartClient(string host, ushort port) {
    var ctx = new UtpTransportContext();
    ctx.IsServer = false;

    ctx.Driver = NetworkDriver.Create();
    ctx.ServerConnection = new NativeArray<NetworkConnection>(1, Allocator.Persistent);
    ctx.RawInbox = new NativeQueue<RawNetworkPacket>(Allocator.Persistent);

    ctx.UnreliableSequencedPipeline =
        ctx.Driver.CreatePipeline(typeof(UnreliableSequencedPipelineStage));

    ctx.ReliableSequencedPipeline =
        ctx.Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

    var endpoint = NetworkEndpoint.Parse(host, port);
    ctx.ServerConnection[0] = ctx.Driver.Connect(endpoint);

    CW.SetResource(ctx);
    CW.SetResource(new NetInbox());
    CW.SetResource(new NetOutbox());
}
```

### 11.2 Server startup

```csharp
public static void StartServer(ushort port) {
    var ctx = new UtpTransportContext();
    ctx.IsServer = true;

    ctx.Driver = NetworkDriver.Create();
    ctx.ClientConnections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    ctx.RawInbox = new NativeQueue<RawNetworkPacket>(Allocator.Persistent);

    ctx.UnreliableSequencedPipeline =
        ctx.Driver.CreatePipeline(typeof(UnreliableSequencedPipelineStage));

    ctx.ReliableSequencedPipeline =
        ctx.Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

    var endpoint = NetworkEndpoint.AnyIpv4.WithPort(port);

    if (ctx.Driver.Bind(endpoint) != 0)
        throw new Exception($"Failed to bind port {port}");

    ctx.Driver.Listen();

    SW.SetResource(ctx);
    SW.SetResource(new NetInbox());
    SW.SetResource(new NetOutbox());
}
```

## 12. Basic movement example

### 12.1 Components

```csharp
[ReplicatedComponent(
    authority: ReplicationAuthority.Owner,
    delivery: NetDelivery.UnreliableSequenced,
    sendRate: 20
)]
public struct CharacterNetState : IComponent, IComponentConfig<CharacterNetState> {
    public Vector3 Position;
    public Vector3 Velocity;
    public Quaternion Rotation;

    public ComponentTypeConfig<CharacterNetState> Config() => new(
        guid: new Guid("5f52be22-6d13-4f7a-9d2c-111111111111"),
        trackAdded: true,
        trackDeleted: true,
        trackChanged: true
    );
}

public struct PlayerTag : ITag { }

public struct ViewTransform : IComponent {
    public Vector3 RenderPosition;
    public Quaternion RenderRotation;
}
```

### 12.2 Client local movement

```csharp
public sealed class LocalPlayerMovementSystem : ISystem {
    public void Update() {
        foreach (var e in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState>>().Entities()) {
            ref var state = ref e.Mut<CharacterNetState>();

            var input = Input.ReadMove();
            var move = new Vector3(input.X, 0f, input.Y);

            if (move.sqrMagnitude > 1f)
                move.Normalize();

            state.Velocity = move * 5f;
            state.Position += state.Velocity * Time.deltaTime;

            if (state.Velocity.sqrMagnitude > 0.0001f)
                state.Rotation = Quaternion.LookRotation(state.Velocity);
        }
    }
}
```

### 12.3 Remote smoothing

```csharp
public sealed class RemoteSmoothingSystem : ISystem {
    public void Update() {
        foreach (var e in CW.Query<All<RemoteOwned, CharacterNetState, ViewTransform>>().Entities()) {
            ref readonly var net = ref e.Read<CharacterNetState>();
            ref var view = ref e.Mut<ViewTransform>();

            var t = 1f - MathF.Exp(-12f * Time.deltaTime);

            view.RenderPosition = Vector3.Lerp(view.RenderPosition, net.Position, t);
            view.RenderRotation = Quaternion.Slerp(view.RenderRotation, net.Rotation, t);
        }
    }
}
```

### 12.4 Server spawn player

```csharp
public static EntityGID ServerSpawnPlayer(NetworkPeerId owner, Vector3 spawnPosition) {
    var e = SW.NewEntity<Default>();

    e.Set(new NetworkIdentity {
        Owner = owner,
        Authority = NetworkAuthority.Owner,
        PrefabId = Prefabs.Player
    });

    e.Set<NetworkedTag>();
    e.Set<PlayerTag>();
    e.Set(new CharacterNetState {
        Position = spawnPosition,
        Velocity = Vector3.zero,
        Rotation = Quaternion.identity
    });

    OwnershipTags.ApplyForServer(e, owner, NetworkAuthority.Owner);

    SpawnBroadcaster.SendSpawn(e);

    return e.GID;
}
```

## 13. Start guide

1. Install packages:
   - Unity Transport.
   - StaticEcs.
   - StaticPack.

2. Define worlds:
   - `ServerWT`, `ClientCoreWT`, `ClientUxWT`.

3. Register types:
   - components: `NetworkIdentity`, replicated state components, view components;
   - tags: `LocalOwned`, `RemoteOwned`, `ServerOwned`, `ClientOwned`, gameplay tags;
   - events if needed.

4. Enable tracking:
   - `WorldConfig { TrackCreated = true, TrackingBufferSize = 32 }`;
   - `trackChanged: true` for replicated state components.

5. Create transport:
   - server: `NetworkDriver.Create()`, `Bind()`, `Listen()`;
   - client: `NetworkDriver.Create()`, `Connect()`.

6. Create pipelines:
   - `UnreliableSequencedPipelineStage`;
   - `ReliableSequencedPipelineStage`.

7. Create inbox/outbox resources:
   - `NetInbox`;
   - `NetOutbox`;
   - `RawNetworkPacket` queue.

8. Register systems in order:
   - complete transport jobs;
   - drain packets;
   - apply network;
   - run gameplay;
   - collect replication;
   - send;
   - schedule transport jobs.

9. Spawn player on server:
   - create entity;
   - set `NetworkIdentity`;
   - set replicated components;
   - apply ownership tags;
   - send spawn packet.

10. Client receives spawn:
    - `NewEntityByGID`;
    - apply prefab;
    - apply initial components;
    - apply ownership tags.

11. Gameplay writes only:
    - `Query<All<LocalOwned, ...>>()`;
    - `Mut<T>()`.

12. Replication layer sends deltas automatically.

13. Call every frame:
    - `Systems.Update()`;
    - `World.Tick()`.

## 14. Non-goals

Do not implement for coop baseline:

- deterministic physics;
- rollback;
- input prediction/reconciliation;
- server correction of every transform;
- per-component manual sync systems.

Those are useful for competitive games, but unnecessary for this coop owner-authoritative design.
