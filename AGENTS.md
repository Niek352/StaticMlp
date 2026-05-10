# Agent Guide

Short operational rules for this Unity project. Keep this file small; put details in `ai/*.md`.

## Read More

- [StaticEcs + Unity Transport architecture](ai/static_ecs_multiplayer_architecture.md)
- [StaticEcs client view feature](ai/static_ecs_view_feature.md)
- [Networked gameplay feature recipes](ai/networked_feature_recipes.md)
- [Gameplay systems memo](ai/gameplay_systems_memo.md)
- [StaticEcs quick reference](ai/static_ecs_reference.md)
- [Input feature guide](ai/input_feature.md)
- [MVC usage guidelines](ai/mvc_usage_guidelines.md)
- [MVC package review](ai/mvc_package_review.md)
- [ECS feature architecture layout](ai/ECS_Feature_Architecture_Layout_StaticEcs.md)
- [Replication codegen notes](ai/replication_codegen_notes.md)
- [Prefab XML skill](ai/prefabxml)
- Full StaticEcs documentation is available at [static-ecs FULL.txt](ai/static-ecs%20FULL.txt) when deeper API/reference details are needed.

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
3. Send gameplay actions through typed replicated events when direct ownership is absent.

## Code Organization

- One top-level class/struct/interface/enum per `.cs` file.
- File name must match the top-level type name.
- Do not collect many unrelated classes in one file.
- Keep gameplay, replication, transport, ownership, and presentation code in separate folders/modules.
- Split feature modules into `Runtime/Logic` and `Runtime/Presentation` when both concerns exist.
- `Runtime/Logic` owns gameplay state, replicated contracts, simulation, validation, and server/client-core systems.
- `Runtime/Presentation` owns client-only view state, view sync, visual systems, and Unity-facing presentation code.
- Do not place Unity presentation code, view components, or client-only visuals in `Runtime/Logic`.
- Do not place gameplay rules, replicated state mutation, or server authority logic in `Runtime/Presentation`.
- Keep domain definitions/rules free from network ids, prefab/view paths, transport state, and concrete UI concerns.
- Put shared gameplay/bootstrap contracts in `Game.Core`; put ordinary gameplay features in their own `StaticMlp.Features.FeatureX` asmdef.
- Add feature systems through `GameplayFeature`, not by editing `MultiplayerSystemBootstrap`.
- Register typed network commands through `GameplayFeature.RegisterNetworkEvents`.
- Write code inside explicit modules with clear boundaries. Treat modules as separate packages.
- Do not cross module boundaries with hidden dependencies or direct calls when an event/component boundary belongs there.
- Do not store `Entity` across frames; use `EntityGID` for persistent references.
- Do not expose entity references as raw `ulong`, `*Raw`, or similar surrogate fields in gameplay/replication contracts. Use `EntityGID` directly, and only touch `.Raw` at explicit serialization/codegen boundaries.
- Do not create `static class` types to store mutable runtime data, commands, or state.
- Private fields use `_camelCase` naming, for example `_mvcManager`.
- `const` members use `CAPS_UNDER` naming.
- Do not extract logic into a `static class` by default. Use a static class only when its role is explicit and obvious from the call site; otherwise keep the logic in the owning system/module so readers do not need to open hidden helpers to understand behavior.
- Allowed extracted-logic intent: `Domain/Rules` for shared domain rules that stay generic and can be tested separately from presentation/transport.
- Allowed extracted-logic intent: `Spawner` for explicit entity spawn abstractions whose purpose is visible from the call site.
- Do not introduce vague extraction buckets such as `Helper`, `Utility`, or generic static orchestration classes when the name does not clearly communicate the feature intent.

## Unity Authoring

- AI agents must not create prefab assets.
- Do not generate `.prefab` files through code, YAML patches, editor scripts, or batchmode.
- Do not launch Unity in BatchMode.
- Do not run `dotnet build` on your own; ask the user to run Unity/build checks when verification is needed.
- Prefabs, Canvas hierarchies, and inspector references are assembled manually by a human in the Unity Editor.
- When using PrefabXML, serialize enum values as numbers.
- Null UI references are bugs. Fail fast instead of using defensive `if (x != null)` guards.
- Do not write `ValidateReferences` or helper methods that search the scene/hierarchy instead of explicit inspector wiring.
- Unity `MonoBehaviour` classes used by gameplay features should stay view-only: inspector references, Unity callbacks, passive rendering, and forwarding UI intent. Do not let them become bootstrap, composition root, MVC host, ECS host, or gameplay/presentation orchestrator. Feature UI composition and MVC lifecycle must be owned by ECS/bootstrap systems, not by feature `MonoBehaviour` classes.
- Do not store feature-specific Unity objects, controllers, or bridge systems inside ordinary gameplay/resources by default. If a project-wide composition service such as a shared UI manager must be exposed to systems, provide it explicitly as a composition-owned resource with a clear contract.
- Do not hide missing required runtime state behind silent early `return` paths. If a client world, resource, controller binding, catalog entry, or required dependency is mandatory for the code path, fail fast with an explicit exception that explains what is missing and why it should exist.
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
    public ushort NetworkArchetypeId;
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
- Use `NetworkEvents` for client-to-server gameplay actions; do not read/write raw network inbox/outbox in feature logic.
- Local gameplay queries `LocalOwned`.
- Remote presentation queries `RemoteOwned` and smooths/render-applies state; it does not simulate gameplay.
- Server gameplay queries `ServerOwned` or validated `ClientOwned`.
- Client-to-server interactions that are not owned state changes should be replicated events.
- Camera, UI, selection, and local UX state are not replicated core state.
- Unexpected runtime states are bugs. Fail fast with exceptions instead of silently skipping missing required resources, configuration, or references. If a system should not run in some context, do not register that system in that context.
- Do not add defensive `Has`, null, or resource existence checks by default.
- Do not write `Ensure*`, `Require*`, or similar helpers that probe with `Has<T>()`/`HasResource<T>()`, lazily create missing state, or manually re-check required resources/components for null/existence before access.
- Prefer fail-fast code. Use direct `Get`, `Read`, `GetResource`, and `Unpack` when the entity, resource, or event is guaranteed by the architecture.
- For required mutable component state, use direct `Mut<T>()`; do not hide architecture bugs behind `Ensure` methods that silently `Set(...)` missing components.
- If you see existing `Ensure`/`Require`/`Has`-guarded access of this kind, rewrite it to normal direct access through `Get`, `Mut`, `Read`, `GetResource`, or `Unpack`.
- Validate data only at trust boundaries: client network requests, user input, external files/configs, optional gameplay states, and actual gameplay rules.
- Invalid ECS architecture state must crash loudly instead of being silently ignored.

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
