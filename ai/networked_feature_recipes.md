# Networked Gameplay Feature Recipes

Examples and checklists for adding replicated gameplay.

## Replicated Component

Use `[ReplicatedComponent]`.

```csharp
[ReplicatedComponent(
    authority: ReplicationAuthority.Owner,
    delivery: NetDelivery.UnreliableSequenced,
    sendRate: 20
)]
public struct CharacterNetState : IComponent, IComponentConfig<CharacterNetState> {
    [ReplicatedField(Quantize = 0.01f)]
    public Vector3 Position;

    [ReplicatedField(Quantize = 0.01f)]
    public Vector3 Velocity;

    [ReplicatedField(Compress = true)]
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

Remote entities should not simulate gameplay. They should smooth replicated state.

```csharp
public sealed class RemoteSmoothingSystem : ISystem {
    public void Update() {
        foreach (var e in CW.Query<All<RemoteOwned, CharacterNetState, ViewTransform>>().Entities()) {
            ref readonly var state = ref e.Read<CharacterNetState>();
            ref var view = ref e.Mut<ViewTransform>();

            var t = 1f - MathF.Exp(-12f * Time.deltaTime);
            view.RenderPosition = Vector3.Lerp(view.RenderPosition, state.Position, t);
            view.RenderRotation = Quaternion.Slerp(view.RenderRotation, state.Rotation, t);
        }
    }
}
```

## Spawning Networked Entities

Server:

```csharp
var e = SW.NewEntity<Default>();

e.Set(new NetworkIdentity {
    Owner = ownerPeer,
    Authority = NetworkAuthority.Owner,
    PrefabId = Prefabs.Player
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
    PrefabId = spawn.PrefabId
});

e.Set<NetworkedTag>();

PrefabRegistry.Apply(spawn.PrefabId, e);
ReplicationRegistry.ApplyInitialState(e, spawn.Components);
OwnershipTags.ApplyForClient(e, spawn.Owner, spawn.Authority);
```

Do not use `NewEntity()` for network spawn on clients. Use `NewEntityByGID`.

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

No manual door sync system is needed.

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
12. `RemoteSmoothingSystem` displays movement.
