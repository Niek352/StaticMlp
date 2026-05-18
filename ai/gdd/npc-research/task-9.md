# Task 9 - Settlement Worker Integration With NPC Contracts

## Goal

Align the existing Stage 1 settlement worker implementation with the new NPC contracts without rewriting the worker economy.

This task should make current workers identifiable as NPCs while preserving existing worker behavior.

## Dependencies

Requires:

- `task-1.md`
- `task-4.md`

Recommended after:

- `task-5.md` if workers should also create/acquire roster records

## Current Context

Existing worker code:

```text
Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/
  SettlementWorkerIdentity.cs
  SettlementWorkerAssignment.cs
  SettlementWorkerFactory.cs
  SettlementWorkerRuntimeProfileCatalog.cs
  SettlementWorkersGameplayFeature.cs
```

Existing worker spawn uses:

```text
SettlementWorkerFactory : NetEntityFactory<AiBotNetworkEntity>
```

and applies generic AI state through `AiBotFactory`.

## Architecture

Keep this separation:

- `NpcIdentity` = general NPC class/definition/acquisition path.
- `SettlementWorkerIdentity` = settlement-specific home anchor and worker role.
- `AiBrain`/`AiTaskState` = AI behavior execution.

Do not merge `WorkerRoleId` into `NpcClass`.

Do not make `Npc` depend on `Settlement.Workers`.

`Settlement.Workers` may depend on `Npc.Contracts` and, if needed, `Npc.Logic`.

## Code Scope

Add a worker-to-NPC adapter catalog in `Settlement.Workers`, for example:

```text
Runtime/Logic/Catalogs/SettlementWorkerNpcProfileCatalog.cs
```

It should map:

```text
WorkerRoleId -> NpcDefinitionId
```

Update `SettlementWorkerFactory.Configure(...)` so spawned workers also receive:

```text
NpcTag
NpcIdentity
```

Use the mapped NPC definition to fill:

```text
DefinitionId
Class
Roles
AcquisitionPath
```

For the current seeded camp builder, acquisition path should be whatever is architecturally true for seeded Stage 1 content. If it is not actually acquired through extraction/rescue/incubation, add an explicit enum value only if Design Lock is updated to allow seeded/tutorial NPCs. Otherwise keep this task blocked until the product decision is made.

Do not fake a rescue/incubation path just to satisfy the enum.

## Validation Scope

Validate:

- every current worker role maps to an NPC definition
- mapped NPC definition exists
- mapped NPC class is compatible with worker behavior

Fail fast on missing mapping.

## Out Of Scope

- New worker roles.
- Economy priority board.
- Food/fuel/wear leaks.
- Worker UI redesign.
- Active companion combat support.
- Changing AI action execution.

## Tests

Update/add tests near existing AI tests:

```text
Assets/Tests/Editor/Ai/SettlementWorkersSystemsTests.cs
```

Cover:

- spawned settlement worker has `NpcTag`
- spawned settlement worker has `NpcIdentity`
- missing worker-to-NPC mapping throws
- existing camp builder assignment tests still pass

## Acceptance Criteria

- Existing settlement workers remain functional.
- Workers now carry general NPC identity.
- No broad AI/economy rewrite happens in this task.
- Any seeded/tutorial acquisition ambiguity is resolved explicitly, not hidden with a fallback.

