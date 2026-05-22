# Combat Director

Combat Director is the server-authoritative encounter pressure feature.

Its job is to observe player activity, maintain one runtime combat cell for the current vertical slice, accumulate threat budget, choose active spawn sources, create validated spawn requests, and turn those requests into networked AI enemy entities.

It does not own AI behavior, combat damage rules, transport, prefab creation, or UI composition.

## Runtime Ownership

`StaticMlp.Features.CombatDirector.Contracts` owns shared state that other features may read:

- `CombatCell`: current combat area around the active player group.
- `ThreatBudget`: accumulated encounter budget and per-second accumulation.
- `DirectorState`: replicated phase state for clients.
- `PlayerNoise`: server-side pressure input derived from player activity.
- `CarriedLootValue`: server-side pressure input derived from carried resources.
- `SpawnSource`: server-side spawn source definition.
- `EnemyTag`: marker for enemies created by the director.
- `EnemyArchetype`: replicated enemy role metadata.
- `EnemySpawnSource`: replicated source metadata attached to spawned enemies for client presentation.

`Runtime/Logic` owns server simulation, spawn request validation, and authoritative enemy creation.

`Runtime/Presentation` owns client-only VFX/audio/view state. It may read replicated Combat Director contracts, but must not mutate gameplay truth.

## Current Spawn Flow

The server system order is:

```text
SpawnSourcePlacementSeedSystem
CombatCellTrackingSystem
PlayerThreatInputSystem
ThreatBudgetAccumulationSystem
DirectorPhaseSystem
SpawnSourceSelectionSystem
SpawnRequestBuildSystem
SpawnRequestValidationSystem
EnemySpawnApplySystem
```

Flow:

1. `SpawnSourcePlacementSeedSystem` consumes `OpenWorldChunkGenerationCompleted.SpawnPlacements` and creates active `SpawnSource` entities.
2. `CombatCellTrackingSystem` creates or updates the single `CombatCell` entity and seeds `ThreatBudget` plus `DirectorState`.
3. `PlayerThreatInputSystem` writes `PlayerNoise` and `CarriedLootValue` on server player entities.
4. `ThreatBudgetAccumulationSystem` adds budget from base threat, noise, and carried loot for players inside the cell.
5. `DirectorPhaseSystem` advances `Calm -> BuildUp -> Peak -> Relief -> Cooldown`.
6. `SpawnSourceSelectionSystem` selects one active `SpawnSource` in the configured distance band.
7. `SpawnRequestBuildSystem` creates one or more `SpawnRequest` entities while in `BuildUp` or `Peak`, if there is budget and enemy capacity.
8. `SpawnRequestValidationSystem` rejects invalid requests or marks them with `ValidSpawnRequestTag`.
9. `EnemySpawnApplySystem` creates enemies through `AiBotFactory`, passes the nearest player inside the combat cell as the wave's initial AI target, attaches `EnemyTag`, `EnemyArchetype`, and `EnemySpawnSource`, emits `EnemySpawnedEvent`, consumes budget, and destroys the request.

## Why Nothing Spawns Yet

Combat Director requires runtime `SpawnSource` entities.

The production path seeds them from open-world `SpawnPlacement` facts emitted by `OpenWorldChunkGenerationCompleted`. If no generated chunk event with spawn placements reaches the server, or if no entity has `SpawnSource { IsActive = true }`, then:

- `SpawnSourceSelectionSystem` cannot select a source;
- `SpawnRequestBuildSystem` cannot build spawn requests;
- `EnemySpawnApplySystem` has nothing to spawn.

For enemies to appear in play mode, the open-world server generation pipeline must generate chunks around the active area and publish `OpenWorldChunkGenerationCompleted` with non-empty `SpawnPlacements`. Seeded sources must still be within `EncounterDirectorConfig.MinSpawnSourceDistance..MaxSpawnSourceDistance` from the current cell center.

## Config

`EncounterDirectorConfig` is a server world resource registered by `CombatDirectorGameplayFeature.RegisterServerResources()`.

Default values:

| Field | Default | Meaning |
| --- | ---: | --- |
| `CellRadius` | `35f` | Radius of the active combat cell around the current player group. |
| `BaseThreatPerSecond` | `2.5f` | Threat budget gained every server tick before player modifiers. |
| `NoiseThreatMultiplier` | `1.25f` | Multiplier applied to accumulated `PlayerNoise`. |
| `LootThreatMultiplier` | `1.5f` | Multiplier applied to accumulated `CarriedLootValue`. |
| `MinReliefSeconds` | `20f` | Minimum duration before `Relief` can become `Cooldown`. |
| `MinCooldownSeconds` | `15f` | Minimum duration before `Cooldown` can become `Calm`. |
| `MaxAliveEnemiesPerCell` | `24` | Alive enemy cap inside the combat cell. |
| `MinSpawnSourceDistance` | `10f` | Minimum allowed source distance from cell center. |
| `MaxSpawnSourceDistance` | `90f` | Maximum allowed source distance from cell center. |
| `BuildUpThreshold` | `30f` | Budget threshold for `Calm -> BuildUp`. |
| `PeakThreshold` | `75f` | Budget threshold for high-intensity wave composition and `BuildUp -> Peak`. |

`EnemySpawnCatalog` is also a server world resource.

Default definitions:

| Role | Budget Cost | Min Count | Max Count |
| --- | ---: | ---: | ---: |
| `Swarmer` | `1f` | `6` | `18` |
| `Marker` | `4f` | `1` | `1` |
| `AnchorElite` | `10f` | `1` | `1` |

The current wave builder uses hardcoded wave composition bands derived from budget:

- low budget: `Swarmer`;
- medium budget: `Marker` plus `Swarmer`;
- peak budget: `AnchorElite`, `Marker`, and `Swarmer`.

Open-world spawn placement mapping is currently code-defined in `SpawnSourcePlacementRules`:

- `SpawnPlacementKindId(1)` creates a `SpawnSourceType.Burrow`;
- `SpawnPlacementKindId(2)` creates a `SpawnSourceType.Rift`;
- source radius is `5f * SpawnPlacement.Scale`.

Unknown placement kinds or non-positive scales are invalid integration data and should fail fast.

## Spawn Source Requirements

A `SpawnSource` is valid for selection when:

- `IsActive == true`;
- `Position` is finite;
- distance from `CombatCell.Center` is between `MinSpawnSourceDistance` and `MaxSpawnSourceDistance`;
- it wins the current behind/side directional heuristic.

Validation later rechecks that:

- the source entity still exists;
- it still has `SpawnSource`;
- type and position still match the request;
- it is still active and in the valid distance band.

When the request is applied, enemies are spawned at the selected source position, but their AI blackboard receives an initial target player from the current combat cell. This is required because open-world sources can be farther than the normal AI perception acquisition radius.

At the moment, the director does not require `SpawnSourceNavState.Status == Reachable` in `SpawnSourceSelectionSystem`. `AiNavigation` already owns reachability contracts, but the selector has not been tightened to require reachable cached sources yet.

## Client Presentation

Presentation is client-only:

- `DirectorTelegraphReceiveSystem` infers a source pulse from newly replicated `EnemySpawnSource`.
- `SpawnSourceVfxSystem` spawns a generic combat effect at the replicated spawn position.
- `EnemyViewBindSystem` fails fast if a replicated enemy lacks `ViewPath` or `ViewTransform`, then writes `EnemyRoleViewState`.
- `EnemySpawnAudioSystem` plays procedural stingers for source activation, enemy appearance, and Peak entry.

Presentation does not create server entities and does not write replicated gameplay state.

## Manual Verification Checklist

To verify spawning in play mode:

1. Run replication codegen after adding or changing replicated contracts.
2. Ensure the server has `EncounterDirectorConfig`, `EnemySpawnCatalog`, and `AiBotFactory` resources.
3. Ensure there is at least one server player with `PlayerTag` and `CharacterNetState`.
4. Ensure at least one server entity has active `SpawnSource` at a valid distance from the combat cell.
5. Watch `ThreatBudget.Current` pass `BuildUpThreshold`.
6. Confirm `SelectedSpawnSource` appears on the director entity.
7. Confirm `SpawnRequest` appears, then gets `ValidSpawnRequestTag`, then is consumed by `EnemySpawnApplySystem`.
8. Confirm spawned entities have `EnemyTag`, `EnemyArchetype`, `EnemySpawnSource`, `NetworkIdentity`, and `CharacterNetState`.

If any earlier condition is missing, the director should do nothing or fail fast depending on whether the missing state is optional gameplay input or required architecture state.
