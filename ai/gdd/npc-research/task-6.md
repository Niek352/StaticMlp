# Task 6 - Extraction Capture Pipeline

## Goal

Implement the server-authoritative extraction path from Design Lock:

```text
weakened creature -> ExtractableState -> player extraction request -> captured companion record
```

This task should produce captured roster records. It should not spawn a usable companion actor yet.

## Dependencies

Requires:

- `task-1.md`
- `task-4.md`
- `task-5.md`

Reads from existing combat/AI state as needed.

## Architecture

Extraction is a gameplay request from a client to the server.

Use typed replicated request/result events. Do not read raw network inbox/outbox and do not mutate another feature's state directly outside owned boundaries.

The NPC feature may read public combat/AI contracts to validate target state. If it needs to change combat/enemy-owned state, it should emit an event for the owning feature or limit this task to adding NPC-owned state only.

## Code Scope

Add components:

```text
ExtractableState
ExtractionTargetTag
```

`ExtractableState` should be server-authored. Replicate it only if client UI/prompt systems need to observe it in this task. Otherwise keep it server-only and add replication later with presentation work.

Suggested fields:

```text
ushort NpcDefinitionId
uint ExpiresAtServerTick
```

Add replicated events:

```text
ExtractNpcRequestEvent
ExtractNpcResultEvent
```

Suggested request fields:

```text
EntityGID Target
```

Suggested result fields:

```text
EntityGID Target
EntityGID RosterRecord
NpcAcquisitionResult Status
```

Use `ReliableSequenced`.

## Server Validation

Reject as normal gameplay validation failure when:

- target entity does not resolve on server
- target lacks `ExtractableState`
- target is expired by `SimulationTime.ServerTick`
- target definition is not allowed by `Extraction`
- target is dead/destroyed or otherwise invalid per existing combat state
- requesting peer/player is not allowed to interact with the target

Fail fast only for architecture bugs such as missing required catalog/resources.

## Server Apply

On accepted request:

- create `NpcRosterRecord` with `NpcRosterState.Captured`
- set acquisition path `Extraction`
- emit `NpcAcquisitionAcceptedEvent`
- return result to requester if request/result infrastructure is used

Do not directly grant worker assignment, recipes, or modules here.

## Out Of Scope

- How enemies become extractable from combat director pressure rules, unless a minimal test fixture needs it.
- Extraction VFX/UI.
- Companion actor spawn.
- Inventory costs/tools.
- Incubation of captured companion.

## Tests

Cover:

- non-extractable target is rejected
- expired extractable target is rejected
- definition not allowed by extraction is rejected
- valid extraction creates exactly one captured roster record
- request uses `EntityGID`, not raw ids

## Acceptance Criteria

- Extraction command path is server-authoritative.
- Accepted extraction creates durable captured NPC state.
- No MonoBehaviour gameplay state.
- No direct raw networking access.

