---
name: staticmlp-networked-feature
description: Use when changing replicated components, replicated events, network commands, ownership tags, transport boundaries, codegen inputs, or networked gameplay features in StaticMlp.
---

# StaticMlp Networked Feature

Use this skill for any change that affects multiplayer gameplay state, replicated events, network identity, ownership, replication codegen, or transport-facing boundaries.

## Required Context

Read these first when relevant:

- `AGENTS.md`
- `.agents/skills/staticmlp-code-writing/SKILL.md`
- `ai/static_ecs_multiplayer_architecture.md`
- `ai/networked_feature_recipes.md`
- `ai/replication_codegen_notes.md`
- `ai/simulation_time_server_tick.md`
- `ai/static_ecs_reference.md`

## Boundary Model

Keep the layers separate:

```text
Gameplay systems -> components/events
Replication layer -> components/events <-> packets
Unity Transport -> byte delivery
```

Gameplay code must never call:

```text
NetworkDriver.BeginSend
NetworkDriver.EndSend
NetworkConnection.PopEvent
```

Raw transport access belongs only in transport startup/schedule/send/complete systems and raw receive jobs.

## Choosing State Versus Event

Use replicated component state when the current value matters after packet loss or late arrival:

- position/state summaries
- health/resources/inventory
- ownership/identity-affecting data
- durable quest/progression/building state

Use replicated events when an action or command matters:

- client requests build/attack/interact
- server announces accepted important action
- gameplay action without direct ownership

Do not use raw inbox/outbox reads from feature logic. Use typed `NetworkEvents` registered by the feature.

## Ownership Rules

Client:

- `LocalOwned`: the client may write gameplay state for that entity.
- `RemoteOwned`: the client may only apply network state and smooth/render visuals.

Server:

- `ServerOwned`: the server simulates the entity.
- `ClientOwned`: the server accepts validated owner input for the entity.

Replicate only `NetworkIdentity`, not ownership tags:

```csharp
public struct NetworkIdentity : IComponent
{
    public NetworkPeerId Owner;
    public NetworkAuthority Authority;
    public ushort NetworkArchetypeId;
}
```

Then derive tags through `OwnershipTags.ApplyForClient` or `OwnershipTags.ApplyForServer`.

## Replicated Components

When adding a replicated component:

1. Place the contract in the feature `Runtime/Logic/Components`, legacy `Runtime/Components`, or `Runtime/Contracts/Components` bucket according to `.agents/skills/staticmlp-code-writing/SKILL.md`.
2. Add `[ReplicatedComponent]` with a stable GUID.
3. Enable `trackChanged` for state deltas unless there is a clear reason not to.
4. Prefer quantized float fields for high-frequency movement/state data.
5. Register the component/tag type during world creation.
6. Mutate replicated state through `Mut<T>()`, not `Ref<T>()`.
7. Read replicated state through `Read<T>()`.
8. Update codegen inputs and run the project codegen workflow if required.

Do not expose entity references as raw `ulong`, `*Raw`, or similar fields. Use `EntityGID` directly in gameplay and replication contracts. Touch `.Raw` only at explicit serialization/codegen boundaries.

## Delivery Selection

Use `UnreliableSequenced` for frequent state where newer packets replace older packets:

- movement
- aim/look
- short-lived animation/view state summaries
- frequently refreshed AI/network state

Use `ReliableSequenced` for important durable state or commands:

- spawn/despawn
- ownership changes
- inventory/resource/progression changes
- build/quest decisions
- client-to-server gameplay commands that must arrive once in order

If losing the packet creates a permanent gameplay inconsistency, it is not `UnreliableSequenced`.

## Client-To-Server Commands

For a client action on state it does not directly own:

1. Define a typed replicated event/request in an approved `Events/` bucket.
2. Register it in `GameplayFeature.RegisterNetworkEvents`.
3. Send from client UX/client-core code at the boundary where user intent becomes gameplay intent.
4. Validate on the server as a trust boundary.
5. Convert accepted commands into normal ECS components/events/state mutations.
6. Mutate replicated state only in the owning server/client authority system.

Do not call feature systems directly from UI or another feature to bypass validation.

## Server Validation

Validate client requests for:

- ownership/authority
- entity existence through guaranteed network identity mapping
- distance/range/cooldown/resource costs
- command target legality
- config/catalog ids from external/user-controlled input

Fail fast for invalid architecture state, but reject invalid client gameplay requests as normal validation failures.

## Generated Code

Do not manually edit `.Generated.cs` files.

When generated output is wrong:

- fix the source contract attributes/types;
- fix the codegen pipeline or template;
- regenerate through the intended Unity/editor/codegen workflow;
- keep generated diffs separate enough to review.

Check `ai/replication_codegen_notes.md` before modifying codegen inputs or serialization boundaries.

## Simulation Time

Use the project simulation time/server tick abstractions for deterministic networked logic.

Do not mix presentation frame time into server authority or replicated gameplay decisions. Client visual smoothing may use presentation time in `Runtime/Presentation`.

## Before Finishing

Check:

- Gameplay systems do not call Unity Transport APIs.
- Ownership tags are derived, not replicated.
- Mutations of replicated components use `Mut<T>()`.
- Entity references use `EntityGID`.
- Delivery mode matches loss tolerance.
- Server validates client-originated commands.
- Network commands are registered through the feature entry point.
- Generated files were not edited manually.
