# NPC Feature Guide

Short operational rules for `StaticMlp.Features.Npc`. This feature is the domain boundary for NPC identity, acquisition, and future roster/economy work.

## Purpose

- `Npc` owns global NPC product identity: `Companion`, `Specialist`, acquisition paths, role flags, definition ids, roster states, and durable NPC identity state.
- `Npc` is not an AI behavior feature. It does not decide movement, utility actions, navigation, combat actions, or task execution.
- Active NPC actors may also be AI agents, but that composition is expressed by adding both NPC contracts and `AiBots` contracts to the same entity.

## Current State

Implemented foundation:

- `StaticMlp.Features.Npc.Contracts` asmdef.
- `StaticMlp.Features.Npc.Logic` asmdef.
- `NpcGameplayFeature` as the discoverable feature entry point.
- `NpcClass : byte` with `None = 0`, `Companion = 1`, `Specialist = 2`.
- `NpcAcquisitionPath : byte` with `None = 0`, `Extraction = 1`, `Rescue = 2`, `Incubation = 3`.
- `NpcRoleFlags : ushort` with `Gatherer`, `Hauler`, `Processor`, `Guard`, `Builder`, and `Researcher`.
- `NpcDefinitionId` value id.
- `NpcRosterState : byte`.
- `NpcTag : ITag`.
- `NpcIdentity : IComponent`, server-authoritative reliable replicated durable identity state.

`NpcIdentity` currently contains only primitive/codegen-safe fields:

```text
ushort DefinitionId
NpcClass Class
NpcAcquisitionPath AcquisitionPath
NpcRoleFlags Roles
```

Do not add settlement worker role ids, view paths, prefab paths, network archetype ids, station ids, AI behavior ids, UI text, or transport metadata to `NpcIdentity`.

## Relationship With AiBots

`Npc` and `AiBots` are different layers:

```text
Npc = what the actor is in the product/domain model
AiBots = how an active AI-controlled actor behaves in the world
```

`Npc` owns:

- NPC class and definition identity.
- Acquisition source: extraction, rescue, incubation.
- NPC role capability flags used by future economy and roster features.
- Future NPC definition catalogs, acquisition records, roster records, incubation contracts, and NPC-owned validation.

`AiBots` owns:

- AI brain, blackboard, utility decision inputs, navigation requests, action/task-facing runtime state, and thin replicated AI presentation state.
- Server-side behavior execution for generic AI agents.
- Client presentation summaries for AI behavior.

Do not make `AiBots` depend on `Npc`. `AiBots` is the lower-level generic behavior layer. `Npc` may depend on or compose with public `AiBots` contracts only when a concrete NPC task needs active AI-agent behavior.

## Boundaries

- Keep NPC domain contracts free from Unity presentation, prefab resources, raw networking, settlement-worker-specific ids, and AI implementation details.
- Do not move AI decision logic, navigation backend logic, or task execution into `Npc`.
- Do not use `NpcTag` or `NpcIdentity` as a replacement for `AiAgentTag` when behavior systems need to identify active AI agents.
- Do not use `AiAgentTag` as proof that an entity is a product-level NPC. Require `NpcTag` or `NpcIdentity` when querying NPC identity or roster/acquisition state.
- Cross-feature writes still go through events. `Npc` must not directly mutate settlement worker, AI, combat, or station state unless that state is owned by `Npc`.

## Where To Start

- For identity contracts, read `Runtime/Contracts/Components` and `Runtime/Contracts/Domain`.
- For future definition catalogs, use `Runtime/Logic/Definitions`, `Runtime/Logic/Catalogs`, and `Runtime/Logic/Validation`.
- For future roster/acquisition state, use NPC-owned `Components`, `Events`, `Factories`, `NetworkEntityTypes`, and `Systems/Server` buckets under `Runtime/Logic`.
- For active AI behavior integration, read `../AiBots/CLAUDE.md` and compose through public contracts instead of moving AI behavior into `Npc`.
