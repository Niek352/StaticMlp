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

## Current Runtime Layout

```text
Assets/Scripts/StaticMlp/Networking
    Ecs.Networking
    StaticEcs worlds, ownership tags, packet types, transport runtime,
    replication runtime contracts and transport systems.

Assets/Scripts/StaticMlp/Networking/Unity
    Ecs.Networking.Unity
    MonoBehaviour bootstrap for Unity play mode.

Assets/Scripts/StaticMlp/Game
    Game.Core
    Gameplay bootstrap, feature discovery, built-in demo gameplay,
    prefab registry, replication collect/apply systems, generated replication code,
    presentation-only components.

Assets/Scripts/StaticMlp/Game/Features/EcsViews
    Game.Ecs.Views
    Client-only EntityView binding, ViewPath/View runtime components,
    Resources-based view factory, and generic view-state apply systems.

Future feature assemblies
    Game.FeatureA
    Feature-local components, tags, systems, presentation state and a small
    GameplayFeature class that registers systems/prefabs.
```

`Game.Core` owns the composition points. A new feature assembly should reference `Game.Core` and implement `GameplayFeature`; the bootstrap discovers it through loaded assemblies. This keeps the main startup code free from per-feature `using` statements and manual `Add(new FeatureSystem())` calls.

Feature discovery currently provides:

- ECS type assembly discovery before `RegisterAll(...)`.
- Prefab factory registration through `RegisterPrefabs()`.
- Server systems registration.
- Client core systems registration.
- Client view-state apply registration through `RegisterClientViewSync(ViewSyncBuilder)`.

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
    public ushort NetworkArchetypeId;
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

Current core order:

```text
Server:
    -1000 complete transport jobs
     -900 drain raw inbox
     -850 connection lifecycle
     -830 feature connection gameplay, for example player spawn
     -780 receive client-owned state
        0 feature gameplay
      500 collect server-owned dirty state
      550 relay client-owned state
      700 send packets
     1000 schedule transport jobs
          SW.Tick()

Client core:
    -1000 complete transport jobs
     -900 drain raw inbox
     -810 apply snapshots
     -800 apply spawns
     -790 apply despawns
     -780 apply ownership
     -770 apply component deltas
        0 feature local gameplay
      200 bind EntityView prefabs for client ViewPath entities
      250 feature presentation sync
      320 apply changed view-state components to EntityView parts
      490 explicit EntityView cleanup
      500 collect local-owned dirty state
      700 send packets
     1000 schedule transport jobs
          CW.Tick()
```

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
/Networking                 asmdef: Ecs.Networking
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

/Game                         asmdef: Game.Core
    Bootstrap/
        GameplayFeature.cs
        GameplayFeatureDiscovery.cs
        MultiplayerSystemBootstrap.cs
        ViewSyncBuilder.cs
    Replication/
    ReplicationGenerated/
    Presentation/

/Game/Features/EcsViews        asmdef: Game.Ecs.Views
    Components/
        ViewPath.cs
        View.cs
        DestroyViewRequest.cs
    Contracts/
        IEntityView.cs
        IEntityViewPart.cs
        IViewComponent.cs
    Factory/
    Systems/
    Unity/

/Game/Features/FeatureA        asmdef: Game.FeatureA
    FeatureAGameplayFeature.cs
    Components/
    Tags/
    Systems/Client/
    Systems/Server/
    Presentation/
```

Recommended feature asmdef:

```json
{
    "name": "Game.FeatureA",
    "rootNamespace": "StaticMlp.Game.FeatureA",
    "references": [
        "Game.Core",
        "Ecs.Networking",
        "FFS.StaticEcs",
        "FFS.StaticPack",
        "FFS.StaticEcs.Unity"
    ],
    "autoReferenced": true
}
```

Minimal feature entry point:

```csharp
using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Game.FeatureA {
    public sealed class FeatureAGameplayFeature : GameplayFeature {
        public override void RegisterServerSystems(ServerSystemsBuilder systems) {
            systems.Add(new FeatureAServerSystem(), GameplaySystemOrder.Gameplay);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems) {
            systems.Add(new FeatureAClientSystem(), GameplaySystemOrder.Gameplay);
        }
    }
}
```
