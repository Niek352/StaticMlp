# Task 2 - Encounter Director Config And Enemy Spawn Catalog

## Goal

Add authoritative config and enemy spawn catalog that the director uses to translate threat budget into concrete enemy counts.

This task prepares spawn composition rules, but it must not implement runtime spawn logic.

## Current Context

The Combat Director design requires:

- A config for cell radius, threat rates, phase timings, and spawn caps.
- A catalog that maps `EnemyRole` to spawn data (cost, count ranges).

These are pure domain definitions.

## Dependencies

Requires:

- `task-1.md`

## Architecture

`CombatDirector` owns encounter config and spawn catalog validation.

Do not put spawn catalog ownership inside `AiBots` or a global `Enemy` feature unless the project already has one that owns enemy definitions.

## Code Scope

Add config resource:

```text
Runtime/Logic/Configs/EncounterDirectorConfig.cs
```

Suggested fields:

```text
float CellRadius
float BaseThreatPerSecond
float NoiseThreatMultiplier
float LootThreatMultiplier
float MinReliefSeconds
float MinCooldownSeconds
int MaxAliveEnemiesPerCell
float BuildUpThreshold
float PeakThreshold
```

Add domain definitions:

```text
Runtime/Logic/Definitions/EnemySpawnDefinition.cs
Runtime/Logic/Catalogs/EnemySpawnCatalog.cs
Runtime/Logic/Validation/EnemySpawnCatalogValidator.cs
```

Suggested `EnemySpawnDefinition` fields:

```text
EnemyRole Role
float BudgetCost
int MinCountPerWave
int MaxCountPerWave
```

If arrays of definitions are needed, keep them in domain catalog code only. Do not replicate the catalog itself.

Minimum catalog state for this task:

- `Swarmer` -> low cost, count 6–18
- `Marker` -> medium cost, count 1
- `AnchorElite` -> high cost, count 1

## Validation Scope

Add fail-fast validation for the catalog:

- no duplicate `EnemyRole`
- every definition has a non-zero `EnemyRole`
- `BudgetCost` is positive
- `MinCountPerWave` <= `MaxCountPerWave`
- `MaxCountPerWave` > 0

Do not add silent defaults for missing roles.

## Out Of Scope

- Runtime spawn logic.
- View paths or prefab references.
- AI behavior mapping.
- Health or damage stats.

## Tests

Add editor tests:

- current catalog validates
- duplicate role throws
- zero budget cost throws
- min/max count inversion throws

## Acceptance Criteria

- Config and catalog are available as gameplay resources.
- Invalid catalog data fails fast.
- Domain definitions stay free of view/presentation/network metadata.
