# Agent Guide

Project rules for AI coding agents. Keep this file focused on non-negotiable architecture constraints; use the linked skills and `ai/*.md` files for implementation workflow.

## Project Skills

Load the relevant project skill before changing code:

- [StaticMlp code writing](.agents/skills/staticmlp-code-writing/SKILL.md): use for ordinary C# gameplay, ECS, module, and system changes.
- [StaticMlp networked feature](.agents/skills/staticmlp-networked-feature/SKILL.md): use for replicated components, replicated events, ownership, networking, or generated replication code.
- [StaticMlp UI/MVC](.agents/skills/staticmlp-ui-mvc/SKILL.md): use for UI, MVC, presentation, view sync, MonoBehaviour, PrefabXML, or Canvas-facing changes.

## Read More

- [StaticEcs + Unity Transport architecture](ai/static_ecs_multiplayer_architecture.md)
- [StaticEcs client view feature](ai/static_ecs_view_feature.md)
- [Networked gameplay feature recipes](ai/networked_feature_recipes.md)
- [Gameplay systems memo](ai/gameplay_systems_memo.md)
- [StaticEcs quick reference](ai/static_ecs_reference.md)
- [SimulationTime and ServerTick guide](ai/simulation_time_server_tick.md)
- [Input feature guide](ai/input_feature.md)
- [MVC usage guidelines](ai/mvc_usage_guidelines.md)
- [MVC package review](ai/mvc_package_review.md)
- [ECS feature architecture layout](ai/ECS_Feature_Architecture_Layout_StaticEcs.md)
- [Replication codegen notes](ai/replication_codegen_notes.md)
- [Refactor backlog](ai/refactor/README.md)
- [PrefabXML skill](ai/prefabxml/SKILL.md)
- [Full StaticEcs documentation](ai/static-ecs%20FULL.txt)

## Core Model

```text
StaticEcs = game state
Unity Transport = byte delivery
Replication layer = components/events <-> packets
Gameplay systems = normal ECS logic
```

Do not put networking, packet serialization, raw inbox/outbox access, UI composition, or transport calls into gameplay systems.

Gameplay systems should only:

1. Query entities by ownership tags.
2. Modify owned replicated components through `Mut<T>()`.
3. Send gameplay actions through typed replicated events when direct ownership or feature ownership is absent.

## Architecture Rules

- Architecture and correctness are more important than quick code. If the requested implementation conflicts with these boundaries, stop and explain the safer path before coding.
- Keep gameplay, replication, transport, ownership, and presentation code in separate modules.
- Split feature modules into `Runtime/Logic` and `Runtime/Presentation` when both concerns exist.
- `Runtime/Logic` owns gameplay state, replicated contracts, simulation, validation, and server/client-core systems.
- `Runtime/Presentation` owns client-only view state, view sync, visual systems, MVC, and Unity-facing presentation code.
- Do not place Unity presentation code, view components, or client-only visuals in `Runtime/Logic`.
- Do not place gameplay rules, replicated state mutation, or server authority logic in `Runtime/Presentation`.
- Put shared gameplay/bootstrap contracts in `Game.Core`; put ordinary gameplay features in their own `StaticMlp.Features.FeatureX` asmdef.
- Add feature systems through `GameplayFeature`, not by editing `MultiplayerSystemBootstrap`.
- Register typed network commands through `GameplayFeature.RegisterNetworkEvents`.
- Systems communicate through event components, not hidden direct calls across feature boundaries.
- A feature must not write another feature's component or tag state through `Mut<T>()`, `ReplicationMut.Mut<T>()`, or `ClientProjection.Mut<T>()`.
- Cross-feature writes must go through `IEvent`: the foreign feature sends the request or fact, and the owner feature system applies the state mutation.
- Composite UI may aggregate multiple feature sections, but it must read owner-provided contracts or read models. Do not add dependencies on foreign `*.Logic` assemblies to make UI composition convenient.
- Keep `*State` and `IResource` types cohesive: one owner, authority, lifecycle, writer, and UI section. If a new field crosses one of those boundaries, split the state or add an owner-provided read model instead.
- Do not create vague extraction buckets such as `Helper`, `Utility`, or generic static orchestration classes.
- Allowed extracted logic intents are explicit `Domain/Rules` and explicit `Spawner` abstractions.
- `StaticMlp.Features.Settlement` owns settlement resource ids, resource families, resource catalog validation, and settlement storage; other features may read stable resource contracts from `Settlement.Contracts` but must not create a parallel `Resources` or `DesignLock` feature.

## ECS And Replication

- Register every component, tag, event, and link type between `Create()` and `Initialize()`.
- Use `W.Types().RegisterAll(...)` when appropriate.
- Call `W.Tick()` once after systems update.
- `Multi<T>` indexers return by reference. When reading indexed rows, bind with `ref readonly var row = ref rows[i]` or `ref var row = ref rows[i]`; do not copy with `var row = rows[i]` or consume fields through `rows[i].Field`.
- Do not store `Entity` across frames; use `EntityGID` for persistent references.
- Do not expose entity references as raw `ulong`, `*Raw`, or similar gameplay/replication contract fields. Use `EntityGID`; touch `.Raw` only at explicit serialization/codegen boundaries.
- Default query mode is Strict. Do not modify filtered component/tag types on other entities while iterating.
- During `ForParallel`, only modify the current entity and do not perform structural changes.
- Use `[ReplicatedComponent]` for replicated components with stable GUIDs.
- Enable `trackChanged` for replicated state deltas.
- Use `Mut<T>()` for replicated state mutation and `Read<T>()` for read-only access.
- Read-only access to another feature's public contracts is allowed; mutable access is owned by the feature that defines the state.
- Prefer quantization for floats.
- Use `UnreliableSequenced` for frequent movement/state updates.
- Use `ReliableSequenced` for spawn, despawn, ownership, inventory, quests, and important events.
- Do not manually edit `.Generated.cs` files. Change the source contracts/codegen pipeline instead.

## Ownership

Client tags:

- `LocalOwned`: this client can write gameplay state.
- `RemoteOwned`: this client cannot write gameplay state; it applies network state and smooths visuals.

Server tags:

- `ServerOwned`: server simulates this entity.
- `ClientOwned`: server accepts state from a specific owning client.

Do not replicate ownership tags directly. Replicate only `NetworkIdentity`, then derive local tags through `OwnershipTags.ApplyForClient` or `OwnershipTags.ApplyForServer`.

## Fail Fast

- Unexpected runtime states are bugs. Throw explicit exceptions instead of hiding invalid architecture with silent early returns.
- Do not add defensive `Has`, null, or resource existence checks by default.
- Do not write `Ensure*`, `Require*`, or similar helpers that probe with `Has<T>()`/`HasResource<T>()`, lazily create required state, or mask missing components/resources.
- Use direct `Get`, `Read`, `Mut`, `GetResource`, and `Unpack` when architecture guarantees the entity, component, event, or resource exists.
- Validate data only at trust boundaries: client network requests, user input, external files/configs, optional gameplay states, and actual gameplay rules.

## Unity Authoring

- AI agents must not create prefab assets.
- Do not generate `.prefab` files through code, YAML patches, editor scripts, or batchmode.
- Do not launch Unity in BatchMode.
- Do not run `dotnet build` on your own; ask the user to run Unity/build checks when verification is needed.
- Prefabs, Canvas hierarchies, and inspector references are assembled manually by a human in the Unity Editor.
- When using PrefabXML, serialize enum values as numbers.
- Null UI references are bugs. Fail fast instead of using defensive `if (x != null)` guards.
- Do not write `ValidateReferences` or scene/hierarchy search helpers instead of explicit inspector wiring.
- Feature `MonoBehaviour` classes must stay view-only: inspector references, Unity callbacks, passive rendering, and forwarding UI intent.
- Feature UI composition and MVC lifecycle must be owned by ECS/bootstrap systems, not by feature `MonoBehaviour` classes.

## Code Style

- One top-level class, struct, interface, or enum per `.cs` file.
- File name must match the top-level type name.
- Private fields use `_camelCase`.
- `const` members use `CAPS_UNDER`.
- Do not create `static class` types to store mutable runtime data, commands, or state.
- Create `NetEntity` only through `NetEntityFactory`. The API/composition role must be named `Factory`, using local casing such as `factory`, `_factory`, or `Factory`.
- Temporary workarounds must be explicitly marked with `[Obsolete("Temp")]`.
- All `.md` files must stay UTF-8 without BOM.

## Frame Order

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
