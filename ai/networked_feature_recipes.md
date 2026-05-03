# Networked Gameplay Feature Recipes

Examples and checklists for adding replicated gameplay.

## New Feature Assembly

Create a new folder under `Assets/Scripts/StaticMlp/Game/Features/FeatureA` with its own asmdef:

```json
{
    "name": "Game.FeatureA",
    "rootNamespace": "StaticMlp.Game.FeatureA",
    "references": [
        "Game.Core",
        "Game.Ecs.Views",
        "Ecs.Networking",
        "FFS.StaticEcs",
        "FFS.StaticPack",
        "FFS.StaticEcs.Unity"
    ],
    "autoReferenced": true,
    "noEngineReferences": false
}
```

Then add one feature entry point:

```csharp
using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Game.FeatureA {
    public sealed class FeatureAGameplayFeature : GameplayFeature {
        public override void RegisterPrefabs() {
            // Optional: PrefabRegistry.RegisterClient/RegisterServer(...)
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems) {
            systems.Add(new FeatureAServerSystem(), GameplaySystemOrder.Gameplay);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems) {
            systems.Add(new FeatureAClientSystem(), GameplaySystemOrder.Gameplay);
        }

        public override void RegisterClientViewSync(ViewSyncBuilder views) {
            views.Register<FeatureAViewState>();
        }
    }
}
```

No central bootstrap edit is needed for ordinary feature systems. `GameplayFeatureDiscovery` finds `GameplayFeature` classes in loaded assemblies, passes their assemblies to StaticEcs `RegisterAll(...)`, and lets each feature register server/client/UX systems.

Use these order constants first:

- `GameplaySystemOrder.ServerConnectionGameplay`: server logic that reacts to new peers, such as spawning a player.
- `GameplaySystemOrder.Gameplay`: normal simulation.
- `GameplaySystemOrder.ClientPresentation`: local view sync and generated remote interpolation.
- `GameplaySystemOrder.CollectReplication`: boundary where dirty state collection starts; gameplay should normally run before it.

Use `ViewSystemOrder` for EntityView-specific work:

- `ViewSystemOrder.BindViews`: bind client `ViewPath` entities to `EntityView` prefabs.
- `ViewSystemOrder.BuildPresentationState`: copy replicated/interpolated state into client-only view state.
- `ViewSystemOrder.ApplyPresentationState`: generic `ApplyComponentToViewSystem<T>` calls view parts.
- `ViewSystemOrder.DestroyViews`: explicit view cleanup before replication collection.

## Replicated Component

Use `[ReplicatedComponent]`.

```csharp
[ReplicatedComponent(
    authority: ReplicationAuthority.Owner,
    delivery: NetDelivery.UnreliableSequenced,
    sendRate: 20
)]
public struct CharacterNetState : IComponent, IComponentConfig<CharacterNetState> {
    [ReplicatedField(Quantize = 0.01f, Interpolation = ReplicatedFieldInterpolation.Auto)]
    public Vector3 Position;

    [ReplicatedField(Quantize = 0.01f)]
    public Vector3 Velocity;

    [ReplicatedField(Compress = true, Interpolation = ReplicatedFieldInterpolation.Auto)]
    public Quaternion Rotation;

    public ComponentTypeConfig<CharacterNetState> Config() => new(
        guid: new Guid("PUT-STABLE-GUID-HERE"),
        trackAdded: true,
        trackDeleted: true,
        trackChanged: true
    );
}
```

Rules:

- Always use stable GUIDs for serialized/replicated types.
- Enable `trackChanged` for state deltas.
- Prefer quantization for floats.
- Use `UnreliableSequenced` for frequently updated state.
- Use `ReliableSequenced` for spawn/despawn/ownership/inventory/quest state.
- Add `Interpolation = ReplicatedFieldInterpolation.Auto` for remote presentation fields such as position and rotation.
- `Auto` supports `float`, `Vector2`, `Vector3`, and `Quaternion`.

## Gameplay System

Correct:

```csharp
public sealed class LocalPlayerMovementSystem : ISystem {
    public void Update() {
        foreach (var e in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState>>().Entities()) {
            ref var state = ref e.Mut<CharacterNetState>();

            var input = Input.ReadMove();
            state.Position += new Vector3(input.X, 0f, input.Y) * 5f * Time.deltaTime;
        }
    }
}
```

Avoid runtime network permission checks in gameplay queries:

```csharp
foreach (var e in CW.Query<All<PlayerTag, CharacterNetState>>().Entities()) {
    if (!Network.CanWrite(e)) continue;
}
```

Avoid mutating replicated state through `Ref<T>()`:

```csharp
ref var state = ref e.Ref<CharacterNetState>();
state.Position += delta;
```

Use `Mut<T>()` for replicated changes.

## Remote Visual System

Remote entities should not simulate gameplay. For fields marked with `Interpolation = Auto`, read `Interpolated<T>` and copy it into presentation state.

```csharp
public sealed class RemoteInterpolatedViewSyncSystem : ISystem {
    public void Update() {
        foreach (var e in CW.Query<All<RemoteOwned, Interpolated<CharacterNetState>, ViewTransform>>().Entities()) {
            var state = e.Read<Interpolated<CharacterNetState>>().Value;
            ref var view = ref e.Mut<ViewTransform>();

            view.RenderPosition = state.Position;
            view.RenderRotation = state.Rotation;
        }
    }
}
```

Do not write custom lerp systems for every replicated component. The generated `ReplicatedInterpolationSystem<T>` updates `Interpolated<T>` from `InterpolatedPrevious<T>` and the latest network value using the component `sendRate`.

## Spawning Networked Entities

Server:

```csharp
var e = SW.NewEntity<Default>();

e.Set(new NetworkIdentity {
    Owner = ownerPeer,
    Authority = NetworkAuthority.Owner,
    NetworkArchetypeId = Prefabs.Player
});

e.Set<NetworkedTag>();
e.Set<PlayerTag>();
e.Set(new CharacterNetState { Position = spawnPosition });

OwnershipTags.ApplyForServer(e, ownerPeer, NetworkAuthority.Owner);
SpawnBroadcaster.SendSpawn(e);
```

Client:

```csharp
var e = CW.NewEntityByGID<Default>(spawn.Gid);

e.Set(new NetworkIdentity {
    Owner = spawn.Owner,
    Authority = spawn.Authority,
    NetworkArchetypeId = spawn.NetworkArchetypeId
});

e.Set<NetworkedTag>();

PrefabRegistry.Apply(spawn.NetworkArchetypeId, e);
ReplicationRegistry.ApplyInitialState(e, spawn.Components);
OwnershipTags.ApplyForClient(e, spawn.Owner, spawn.Authority);
```

Do not use `NewEntity()` for network spawn on clients. Use `NewEntityByGID`.

Feature-local prefabs should be registered from the feature entry point:

```csharp
public override void RegisterPrefabs() {
    PrefabRegistry.RegisterClient(MyPrefabs.Door, e => {
        e.Set<DoorTag>();
        e.Set(new ViewPath("Views/Doors/DoorView"));
        e.Set(new DoorViewState());
    });

    PrefabRegistry.RegisterServer(MyPrefabs.Door, e => e.Set<DoorTag>());
}
```

Keep `NetworkArchetypeId` values stable. Treat them as protocol ids, not as scene or prefab instance ids.
Do not put `ViewPath`, `View`, or `IViewComponent` state in server recipes.

## Server-Owned Door Example

State:

```csharp
[ReplicatedComponent(
    authority: ReplicationAuthority.Server,
    delivery: NetDelivery.ReliableSequenced,
    sendRate: 10
)]
public struct DoorState : IComponent, IComponentConfig<DoorState> {
    public bool IsOpen;
    public float OpenAmount;

    public ComponentTypeConfig<DoorState> Config() => new(
        guid: new Guid("PUT-STABLE-GUID-HERE"),
        trackAdded: true,
        trackDeleted: true,
        trackChanged: true
    );
}
```

Server system:

```csharp
public sealed class ServerDoorSystem : ISystem {
    public void Update() {
        foreach (var e in SW.Query<All<ServerOwned, DoorTag, DoorState>>().Entities()) {
            ref var door = ref e.Mut<DoorState>();
            door.OpenAmount = Mathf.MoveTowards(
                door.OpenAmount,
                door.IsOpen ? 1f : 0f,
                Time.deltaTime
            );
        }
    }
}
```

Client visual system:

```csharp
public sealed class DoorViewSystem : ISystem {
    public void Update() {
        foreach (var e in CW.Query<All<DoorState, DoorView>>().Entities()) {
            ref readonly var state = ref e.Read<DoorState>();
            ref var view = ref e.Mut<DoorView>();

            view.ApplyOpenAmount(state.OpenAmount);
        }
    }
}
```

Then register the view apply system:

```csharp
public override void RegisterClientViewSync(ViewSyncBuilder views) {
    views.Register<DoorView>();
}
```

The Unity prefab should have `EntityView` and one or more `IEntityViewPart<DoorView>` components.

## Client-to-Server Action

For an action, use a network event, not direct state mutation, unless the client owns the entity.

```csharp
[ReplicatedEvent(delivery: NetDelivery.ReliableSequenced)]
public struct UseDoorEvent : IEvent {
    public EntityGID Door;
}
```

Client sends:

```csharp
NetworkEvents.Send(new UseDoorEvent {
    Door = doorGid
});
```

Server receives:

```csharp
if (evt.Door.TryUnpack<ServerWT>(out var door)) {
    if (CanUseDoor(peer, door)) {
        ref var state = ref door.Mut<DoorState>();
        state.IsOpen = !state.IsOpen;
    }
}
```

## Minimal Movement Checklist

1. Create `CharacterNetState`.
2. Mark it `[ReplicatedComponent(authority: Owner, delivery: UnreliableSequenced)]`.
3. Enable `trackChanged`.
4. Add `PlayerTag`.
5. Server spawns player with `NetworkIdentity.Owner = peer`.
6. Client receives spawn using `NewEntityByGID`.
7. Client applies ownership tags.
8. `LocalPlayerMovementSystem` queries `LocalOwned`.
9. Replication collect sends dirty `CharacterNetState`.
10. Server validates `ClientOwned` and relays.
11. Remote clients apply deltas.
12. Generated interpolation updates `Interpolated<CharacterNetState>`.
13. `RemoteInterpolatedViewSyncSystem` displays movement.
