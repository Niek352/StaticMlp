# Task 2 - Resource Families In Settlement Resource Model

## Goal

Add the Design Lock resource family source of truth to the existing resource model.

This task prepares NPC economy and incubation costs, but it must not implement economy tasks or incubation processing.

## Current Context

Existing resource code lives in:

```text
Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/
  ResourceId.cs
  ResourceAmount.cs
  ResourceUsageFlags.cs
  ResourceDefinition.cs
  ResourceCatalog.cs
```

Current resources:

- Wood
- Stone

Design Lock requires families:

- Raw
- Flow
- Refined
- Progression
- Stability

## Architecture

`Settlement` should remain the owner of settlement storage and resource catalog rules.

If NPC/Build/Progression features need to read resource ids or families, shared resource contracts should move to:

```text
Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Contracts/
```

Keep the existing namespace `StaticMlp.Features.Settlement` so downstream code does not need namespace churn.

Do not create a separate vague `Resources` or `DesignLock` feature for this task.

## Code Scope

Add:

- `ResourceFamily : byte`

Extend:

- `ResourceDefinition` with `ResourceFamily Family`
- `ResourceCatalog` entries with explicit family

Minimum catalog state for this task:

- `WoodId` -> `Raw`
- `StoneId` -> `Raw`

If moving files into `Runtime/Contracts`, move only stable shared value types and definitions:

- `ResourceId`
- `ResourceAmount`
- `ResourceUsageFlags`
- `ResourceFamily`
- `ResourceDefinition`

Leave mutable settlement storage and systems in `Runtime/Logic`.

## Validation Scope

Add a fail-fast validation path for resource definitions.

Required rules:

- no duplicate `ResourceId`
- every definition has a non-zero `ResourceFamily`
- all current settlement stored resources have non-negative starting amounts

Use explicit validation/domain code such as:

```text
Runtime/Logic/Validation/ResourceCatalogValidator.cs
```

or a similarly named approved bucket.

Do not add lazy `Ensure*` helpers or silent fallback families.

## Out Of Scope

- Adding all future resources from the Design Lock list.
- Resource UI.
- Economy consumption.
- Settlement production chains.
- Open-world resource node integration.
- Inventory migration outside compile-required references.

## Tests

Add editor tests near existing settlement/resource test ownership, or add a focused test asmdef if needed.

Cover:

- duplicate ids fail
- missing/zero family fails
- current catalog validates

## Acceptance Criteria

- Resource families are available from the resource catalog.
- Current resources have explicit families.
- Invalid resource catalogs fail fast in tests.
- No MonoBehaviour state or UI changes.
- No economy task pipeline added in this task.

