# Agent Guide

Short operational rules for this Unity project. Keep this file small; put details in `ai/*.md`.

## Read More

- [StaticEcs + Unity Transport architecture](ai/static_ecs_multiplayer_architecture.md)
- [Networked gameplay feature recipes](ai/networked_feature_recipes.md)
- [StaticEcs quick reference](ai/static_ecs_reference.md)
- [Original multiplayer implementation plan](ai/static_ecs_unity_transport_multiplayer_plan.md)

## Core Model

```text
StaticEcs = game state
Unity Transport = byte delivery
Replication layer = components/events <-> packets
Gameplay systems = normal ECS logic
```

Do not put networking or packet serialization into gameplay systems.

Gameplay systems should only:

1. Query entities by ownership tags.
2. Modify replicated components through `Mut<T>()`.
3. Send gameplay actions through replicated events when direct ownership is absent.

## Code Organization

- One top-level class/struct/interface/enum per `.cs` file.
- File name must match the top-level type name.
- Do not collect many unrelated classes in one file.
- Keep gameplay, replication, transport, ownership, and presentation code in separate folders/modules.
- Write code inside explicit modules with clear boundaries. Treat modules as separate packages.
- Do not cross module boundaries with hidden dependencies or direct calls when an event/component boundary belongs there.
- Do not store `Entity` across frames; use `EntityGID` for persistent references.

## Unity Authoring

- AI agents must not create prefab assets.
- Do not generate `.prefab` files through code, YAML patches, editor scripts, or batchmode.
- Prefabs, Canvas hierarchies, and inspector references are assembled manually by a human in the Unity Editor.
- Null UI references are bugs. Fail fast instead of using defensive `if (x != null)` guards.
- Do not write `ValidateReferences` or helper methods that search the scene/hierarchy instead of explicit inspector wiring.
- All `.md` files must stay UTF-8 without BOM. Do not convert them to CP1251/ANSI.

## Ownership

Client tags:

- `LocalOwned`: this client can write gameplay state.
- `RemoteOwned`: this client cannot write gameplay state; it applies network state and smooths visuals.

Server tags:

- `ServerOwned`: server simulates this entity.
- `ClientOwned`: server accepts state from a specific owning client.

Do not replicate ownership tags directly. Replicate only `NetworkIdentity`:

```csharp
public struct NetworkIdentity : IComponent {
    public NetworkPeerId Owner;
    public NetworkAuthority Authority;
    public ushort PrefabId;
}
```

Then derive local tags through `OwnershipTags.ApplyForClient` or `OwnershipTags.ApplyForServer`.

## Replicated State

- Use `[ReplicatedComponent]` for replicated components.
- Use stable GUIDs for serialized/replicated types.
- Enable `trackChanged` for state deltas.
- Prefer quantization for floats.
- Use `UnreliableSequenced` for frequent movement/state updates.
- Use `ReliableSequenced` for spawn, despawn, ownership, inventory, quests, and important events.
- When changing replicated state, use `Mut<T>()`, not `Ref<T>()`.
- For read-only access, use `Read<T>()`.

## Systems

- Systems communicate through event components, not direct calls.
- Local gameplay queries `LocalOwned`.
- Remote presentation queries `RemoteOwned` and smooths/render-applies state; it does not simulate gameplay.
- Server gameplay queries `ServerOwned` or validated `ClientOwned`.
- Client-to-server interactions that are not owned state changes should be replicated events.
- Camera, UI, selection, and local UX state are not replicated core state.

Frame order:

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

## Transport Boundary

Gameplay code must never call:

```text
NetworkDriver.BeginSend
NetworkDriver.EndSend
NetworkConnection.PopEvent
```

Unity Transport access belongs only in transport startup/schedule/send/complete systems and raw receive jobs.

## StaticEcs Essentials

- Register every component/tag/event/link type between `Create()` and `Initialize()`.
- Prefer `W.Types().RegisterAll(...)` when appropriate.
- Call `W.Tick()` once after systems update.
- `Entity` is a short-lived handle; use `EntityGID` for persistent references.
- Default query mode is Strict; do not modify filtered component/tag types on other entities while iterating.
- During `ForParallel`, only modify the current entity and do not perform structural changes.

## Before Writing Code

Ask:

1. Is this server, client core, or client UX?
2. Is the entity local-owned, remote-owned, server-owned, or client-owned?
3. Is this replicated state or a replicated event?
4. Should delivery be unreliable sequenced or reliable?
5. Does the component need tracking?
6. Does replicated state mutate through `Mut<T>()`?
7. Does this belong in gameplay, replication, transport, ownership, or presentation?
