# Task 5 - NPC Roster And Acquisition Records

## Goal

Represent captured/recruited/incubating NPC records as authoritative ECS state.

This is the durable result that extraction, rescue, and incubation pipelines will create.

## Dependencies

Requires:

- `task-1.md`
- `task-4.md`

## Architecture

The roster record belongs to the NPC feature.

Do not store captured/recruited NPC records inside UI state, MonoBehaviours, or settlement worker-only components.

Use ECS entity state so later features can query acquired NPCs without calling hidden services.

## Code Scope

Add:

```text
NpcRosterRecord
NpcRosterRecordTag
NpcRosterRecordFactory
NpcRosterRecordSpawnSpec
NpcRosterRecordNetworkEntity
```

Use `NetEntityFactory` for replicated roster record entities if clients need to observe the records.

`NpcRosterRecord` should be server-authoritative reliable replicated state.

Suggested fields:

```text
ushort DefinitionId
NpcClass Class
NpcAcquisitionPath AcquisitionPath
NpcRosterState State
uint CreatedServerTick
```

Use `SimulationTime.ServerTick` for server-authored timestamps.

If ownership/player attribution is needed and the project still lacks `PlayerId`, use the existing project-owned identity type after inspection. Do not invent a raw `ulong` owner field.

## Events

Add internal ECS events for accepted acquisition facts:

```text
NpcAcquisitionAcceptedEvent
NpcAcquisitionRejectedEvent
```

These are normal ECS events inside the feature, not raw transport packets.

Client-originated requests are out of scope for this task and belong to extraction/rescue/incubation tasks.

## Registration

`NpcGameplayFeature` should:

- register projections/replication for `NpcRosterRecord`
- register network archetype client/server recipes if roster records are replicated as network entities
- register server resources needed by the factory

Server recipe must not include `ViewPath`.

## Out Of Scope

- Extraction/rescue/incubation command validation.
- Spawning active companion/specialist actors.
- Worker assignment.
- Roster UI.

## Tests

Cover:

- factory creates a roster record with correct definition/class/path/state
- created tick comes from `SimulationTime.ServerTick`
- invalid definition id fails through direct catalog lookup/validation, not fallback

## Acceptance Criteria

- NPC feature has an authoritative durable record for acquired NPCs.
- Records are queryable through ECS.
- Replication is reliable if records are client-visible.
- No gameplay behavior is implemented yet.

