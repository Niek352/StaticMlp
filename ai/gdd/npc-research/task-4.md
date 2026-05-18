# Task 4 - NPC Definition Catalog And Fail-Fast Validation

## Goal

Add a domain catalog for NPC definitions and validate it at startup/tests.

This gives later extraction, rescue, incubation, and worker migration tasks a stable `NpcDefinitionId` source of truth.

## Dependencies

Requires:

- `task-1.md`

Can run before:

- extraction
- rescue
- incubation
- worker integration

## Architecture

NPC definitions are pure domain data.

They must not contain:

- Unity prefab paths
- EntityView paths
- network archetype ids
- AI behavior ids
- settlement worker role ids
- station ids
- UI strings
- transport or request metadata

If later tasks need those mappings, add adapter catalogs in the owning feature, for example:

- `NpcRuntimeProfileCatalog` in `Npc.Logic` for AI/network spawn data
- `SettlementWorkerNpcProfileCatalog` in `Settlement.Workers` for worker-specific mapping

## Code Scope

Add:

```text
Runtime/Logic/Definitions/NpcDefinition.cs
Runtime/Logic/Catalogs/NpcDefinitionCatalog.cs
Runtime/Logic/Validation/NpcDefinitionCatalogValidator.cs
```

Suggested `NpcDefinition` fields:

```text
NpcDefinitionId Id
NpcClass Class
NpcRoleFlags Roles
NpcAcquisitionPath AllowedAcquisitionPaths
```

If `NpcAcquisitionPath` is not flags-capable, add a dedicated flags enum such as `NpcAcquisitionPathFlags`. Do not misuse numeric OR on a non-flags enum.

Add initial definitions only for code-verifiable vertical-slice concepts. Suggested minimum:

- one `Companion` definition allowed by `Extraction`
- one `Specialist` definition allowed by `Rescue`
- one `Companion` or `Specialist` definition allowed by `Incubation`

Keep ids stable.

## Validation Scope

Fail fast on:

- duplicate `NpcDefinitionId`
- zero/missing class
- zero/missing allowed acquisition path
- no roles for a definition that is meant to be usable by economy/worker tasks
- Specialist with only raw labor roles if such roles are modeled in `NpcRoleFlags`

Do not add silent defaults.

## Out Of Scope

- Spawning entities.
- Roster records.
- Acquisition commands.
- Settlement worker migration.
- Presentation.

## Tests

Add tests for:

- current catalog validates
- duplicate id throws
- missing class throws
- missing acquisition path throws
- invalid Specialist role setup throws if role semantics are modeled

## Acceptance Criteria

- Code can resolve `NpcDefinition` by `NpcDefinitionId`.
- Invalid catalog data fails fast.
- Domain definitions stay free of network/presentation/settlement-worker metadata.

