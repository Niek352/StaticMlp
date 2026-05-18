# Task 1 - NPC Contracts And Feature Boundary

## Goal

Create the general NPC feature boundary that future NPC acquisition, economy, and presentation work can depend on.

This task is foundation-only. It must not implement extraction, rescue, incubation, NPC economy, UI, views, VFX, or station jobs.

## Current Context

The project already has:

- `StaticMlp.Features.AiBots` for generic AI agents/tasks.
- `StaticMlp.Features.Settlement.Workers` for Stage 1 settlement workers.
- `SettlementWorkerIdentity` and worker role data, but these are settlement-specific and should not become the global NPC source of truth.

Design Lock requires global NPC classes:

- `Companion`
- `Specialist`

And global acquisition paths:

- `Extraction`
- `Rescue`
- `Incubation`

## Architecture

Add a new feature:

```text
Assets/Scripts/StaticMlp/Features/Npc/
  Runtime/
    Contracts/
    Logic/
```

Use split asmdefs:

- `StaticMlp.Features.Npc.Contracts`
- `StaticMlp.Features.Npc.Logic`

Keep namespace:

```csharp
namespace StaticMlp.Features.Npc
```

`Npc.Logic` may reference:

- `StaticMlp.Features.Npc.Contracts`
- `StaticMlp.Features.AiBots`
- `StaticMlp.Features.Settlement.Contracts` only if a concrete task needs settlement anchors.
- `Game.Core`
- `Ecs.Networking`
- `FFS.StaticEcs`
- `FFS.StaticPack`

Do not make `AiBots` depend on `Npc`.

## Code Scope

Create contract/domain types:

- `NpcClass : byte`
- `NpcAcquisitionPath : byte`
- `NpcRoleFlags : ushort`
- `NpcDefinitionId`
- `NpcRosterState : byte`
- `NpcTag : ITag`
- `NpcIdentity : IComponent`

`NpcIdentity` should be server-authoritative replicated state:

- `[ReplicatedComponent(authority: ReplicationAuthority.Server, delivery: NetDelivery.ReliableSequenced, ...)]`
- stable GUID through `IComponentConfig<NpcIdentity>`
- `ITrackableAdded`, `ITrackableChanged`, `ITrackableDeleted`
- replicated fields should stay primitive/enums supported by codegen

Suggested fields:

```text
ushort DefinitionId
NpcClass Class
NpcAcquisitionPath AcquisitionPath
NpcRoleFlags Roles
```

Do not include settlement worker role, view path, network archetype id, prefab path, station id, or UI text in `NpcIdentity`.

Create `NpcGameplayFeature : GameplayFeature` in `Runtime/Logic`.

For this task it should only register replication/projection for NPC components if the current project pattern requires it. No systems are required yet unless registration cannot happen otherwise.

## Out Of Scope

- Spawning NPCs.
- Captured/recruited roster records.
- Extraction/rescue/incubation commands.
- Worker migration.
- AI behavior changes.
- Presentation/UI.

## Tests

Add editor tests covering pure contract behavior if useful:

- value id equality/hash behavior for `NpcDefinitionId`
- enum values match Design Lock numeric ordering

Do not manually edit generated replication files. The code-writing agent should ask for Unity replication codegen/compile verification after adding replicated components.

## Acceptance Criteria

- New NPC feature folders and asmdefs follow project layout.
- Contracts compile in isolation from settlement worker logic.
- `NpcIdentity` is suitable for replicated durable NPC identity.
- No existing worker behavior changes in this task.
- No MonoBehaviour, prefab, UI, or generated code changes.

