# Task 7 - Rescue And Recruit Pipeline

## Goal

Implement the rescue/recruit acquisition path from Design Lock:

```text
rescue site -> player rescue request -> specialist or pending recruit roster record
```

This task should make rescue a server-authoritative gameplay command and produce NPC roster records.

## Dependencies

Requires:

- `task-1.md`
- `task-4.md`
- `task-5.md`

Can run independently from extraction after common roster state exists.

## Architecture

The rescue source may be a networked entity or a deterministic open-world placement.

For this first scoped task, prefer a networked ECS entity with `EntityGID` if that matches existing Stage 1 content. Do not introduce raw `ulong` ids. If rescue sites must be deterministic open-world placements, split that into a follow-up using the open-world feature's stable placement id type.

NPC feature owns the acquisition record. The feature owning the rescue site owns site state mutation.

If the rescue site component lives in `Npc`, then `Npc` may mutate it. If it belongs to another feature, use events.

## Code Scope

Add:

```text
NpcRescueSite
NpcRescueSiteState
RescueNpcRequestEvent
RescueNpcResultEvent
```

Suggested `NpcRescueSite` fields:

```text
ushort NpcDefinitionId
NpcRescueSiteState State
```

Suggested states:

```text
Locked = 1
Rescuable = 2
Resolved = 3
```

Request field:

```text
EntityGID RescueSite
```

Result fields:

```text
EntityGID RescueSite
EntityGID RosterRecord
NpcAcquisitionResult Status
```

Use reliable replicated request/result events.

## Server Validation

Reject when:

- rescue site does not resolve
- site is not `Rescuable`
- NPC definition is missing
- NPC definition is not allowed by `Rescue`
- requesting peer/player cannot interact with the site
- site was already resolved

Specialist definitions should be allowed here. Companion definitions may be allowed only if catalog data explicitly says rescue is valid.

## Server Apply

On accepted request:

- mark site `Resolved` through its owner component
- create `NpcRosterRecord`
- state should be `Recruited` or `PendingRecruit`, depending on the enum created in `task-1.md`
- path should be `Rescue`
- emit `NpcAcquisitionAcceptedEvent`

Do not unlock recipes directly in this task. Recipe unlock should be a later progression/economy task consuming the accepted acquisition fact.

## Out Of Scope

- Camps/cages/capsules art prefabs.
- Open-world deterministic placement integration unless already trivial.
- Specialist recipe unlocks.
- UI panels.
- Worker assignment.

## Tests

Cover:

- locked site rejected
- resolved site rejected
- missing/invalid definition rejected
- valid rescue creates roster record
- site becomes resolved once

## Acceptance Criteria

- Rescue/recruit is represented as a typed server-authoritative request.
- Accepted rescue creates a durable NPC roster record.
- No recipe/progression mutation is hidden inside the rescue handler.

