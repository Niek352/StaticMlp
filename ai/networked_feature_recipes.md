# Networked Gameplay Feature Recipes

Examples and checklists for adding replicated gameplay.

## New Feature Assembly

Create a new folder under `Assets/Scripts/StaticMlp/Features/FeatureA` with its own asmdef:

```json
{
    "name": "StaticMlp.Features.FeatureA",
    "rootNamespace": "StaticMlp.Features.FeatureA",
    "references": [
        "Game.Core",
        "StaticMlp.Features.EcsViews",
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

namespace StaticMlp.Features.FeatureA {
public sealed class FeatureAGameplayFeature : GameplayFeature {
    public override void RegisterNetworkEvents() {
        // Optional: NetworkEventRegistry.Register<MyCommand>(...)
    }

    public override void RegisterPrefabs() {
        // Optional: NetArchetypeRegistry.RegisterClient/RegisterServer(...)
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

## Feature Shape

Prefer this split inside a networked feature:

- `Domain`: pure gameplay rules and value decisions. No transport, no inbox/outbox, no prefab paths, no network archetype ids.
- `Networking`: typed event codecs, event ids, and feature-local protocol adapters.
- `Presentation`: view paths, preview/view state, and client-only visual mapping.
- `Systems/Server` and `Systems/Client`: ECS queries, ownership checks, validation against world state, and calls into domain rules.

When domain data needs network or presentation metadata, create a separate adapter catalog keyed by the domain id instead of putting those fields on the domain definition.

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
NetworkEntitySpawner.SpawnServerEntity(
    ownerPeer,
    NetworkAuthority.Owner,
    Prefabs.Player,
    e => {
        e.Set<PlayerTag>();
        e.Set(new CharacterNetState { Position = spawnPosition });
    });
```

Feature gameplay should not create `NetworkIdentity`, apply ownership tags, or call `SpawnBroadcaster` directly. The replication layer handles the client-side `NewEntityByGID`, initial state, and ownership tag application.

Feature-local network archetypes should be registered from the feature entry point:

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

For an action, use a typed replicated event, not direct state mutation, unless the client owns the entity.

```csharp
[ReplicatedEvent(delivery: NetDelivery.ReliableSequenced)]
public readonly struct UseDoorEvent {
    public EntityGID Door;
}
```

Register the event from the feature entry point:

```csharp
public override void RegisterNetworkEvents() {
    NetworkEventRegistry.Register<UseDoorEvent>(
        FeatureNetworkEventTypeIds.UseDoor,
        NetDelivery.ReliableSequenced,
        UseDoorEventCodec.Write,
        UseDoorEventCodec.TryRead);
}
```

Client sends through the feature-facing helper:

```csharp
NetworkEvents.TrySendToServer(new UseDoorEvent {
    Door = doorGid
});
```

Server receives typed events:

```csharp
public void Update() {
    NetworkEvents.ForEachServer<UseDoorEvent>(HandleUseDoor);
}

private static void HandleUseDoor(NetworkPeerId sourcePeer, in UseDoorEvent evt) {
    if (!evt.Door.TryUnpack<ServerWT>(out var door))
        return;

    if (CanUseDoor(sourcePeer, door)) {
        ref var state = ref door.Mut<DoorState>();
        state.IsOpen = !state.IsOpen;
    }
}
```

Feature gameplay systems should not scan `NetInbox.Events`, compare raw event type ids, call `NetOutbox.EnqueueNetworkEvent`, or hardcode `new NetworkPeerId(0)`. Keep byte serialization in a small feature networking adapter or generated codec.

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
