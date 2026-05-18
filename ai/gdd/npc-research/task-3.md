# Task 3 - Build Equipment Slot Contracts And Limits

## Goal

Add the Design Lock equipment slot vocabulary and slot limits that NPC/build/economy work will depend on.

This task should only establish contracts and validation rules. It should not implement full module gameplay effects.

## Current Context

Existing build module code has:

```text
Assets/Scripts/StaticMlp/Features/Build/Runtime/Logic/Definitions/BuildModuleSlotType.cs
```

Current enum has only:

```text
PrimaryAbility = 1
```

Design Lock requires slot-limited activation:

- 3 Combat modules
- 2 Utility modules
- 2 BuildSignal modules
- 4 BaseInfrastructure effects

## Architecture

`Build` owns equipment/module slot rules.

Do not put slot rules in `Npc` or a global `DesignLock` feature.

The slot vocabulary should be visible to features that need to ask whether a module can be active. Prefer `Build.Contracts` if other features must reference the enum.

## Code Scope

Add a stable enum:

```text
EquipmentSlotKind : byte
  Combat = 1
  Utility = 2
  BuildSignal = 3
  BaseInfrastructure = 4
```

Add a slot limit definition:

```text
SlotRules
SlotRuleCatalog
```

Suggested baseline limits:

```text
Combat: 3
Utility: 2
BuildSignal: 2
BaseInfrastructure: 4
```

Update build module definitions to expose the Design Lock slot kind.

If existing `BuildModuleSlotType.PrimaryAbility` is still required by current build selection tests, do not delete it in this task. Add a deliberate compatibility mapping or migrate current catalog entries to `EquipmentSlotKind.Combat`, whichever is smaller and cleaner after inspecting references.

Do not mark old code `[Obsolete("Temp")]` unless the implementation truly introduces a temporary workaround.

## Validation Scope

Add pure rules for:

- slot kind is valid/non-zero
- slot index is within configured limit
- module supports requested slot kind

Put rules in an approved bucket, for example:

```text
Runtime/Logic/Domain/SlotRules.cs
Runtime/Logic/Catalogs/SlotRuleCatalog.cs
```

## Out Of Scope

- Equip/unequip network commands.
- Active loadout replicated state.
- Module effects.
- UI preview.
- NPC-specific module behavior.

## Tests

Cover:

- current baseline limits match Design Lock
- invalid slot kind fails
- index equal/above limit fails
- current build module catalog validates against slot rules

## Acceptance Criteria

- Build feature exposes the Design Lock slot kinds.
- Slot limits are centralized and test-covered.
- Existing build selection still compiles.
- No module effect pipeline added.

