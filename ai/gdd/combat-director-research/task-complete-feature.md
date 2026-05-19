# Task Complete Feature - Combat Director Completion

## Goal

Finalize the Combat Director implementation against `02-combat-director.md` and ensure it is stable enough to treat as production-ready foundation.

This task is a completion pass: it fixes known gaps, enforces architecture boundaries, and records follow-up work.

## Current State

The preceding tasks (1–9) add:

- Combat Director feature boundary with split Contracts/Logic assemblies.
- Director phase, enemy role, and spawn source contracts.
- Encounter config and enemy spawn catalog.
- Combat cell tracking and threat input systems.
- Budget accumulation and phase machine.
- Spawn source selection and spawn request build.
- Authoritative enemy entity creation on the server.
- Client presentation, telegraph, and view binding.
- Debug overlay.
- Integration tests.

The work is directionally aligned with the Design Lock, but several gaps and boundary risks remain.

## Blocking Fixes

### 1. Ensure spawn request single-consume

`SpawnRequest` components must not be processed twice.

Required behavior:

- A valid spawn request is consumed exactly once.
- After spawn apply, delete the `SpawnRequest` component or mark it processed.
- If spawn fails validation, delete the request and do not retry automatically.

### 2. Validate alive enemy cap server-side

The alive enemy cap must be enforced authoritatively on the server.

Required behavior:

- `EnemySpawnApplySystem` queries current alive enemies in the cell before creating new ones.
- If the cap would be exceeded, reject the request or clamp the count.
- Do not rely on the client to enforce the cap.

### 3. Keep server free of view paths

Server recipes and factory code must not include `ViewPath` or prefab references.

Fix direction:

- `EnemySpawnApplySystem` must not assign view-related components.
- `ClientProjection` or view-sync systems in `Presentation` own view binding.
- Confirm that `NetEntityFactory` server recipes exclude view metadata.

## Architecture Rework

### 4. Clarify combat feature boundary

If `EnemySpawnApplySystem` needs to attach health or combat state components that belong to another feature, use one of these patterns:

- Emit `EnemySpawnedEvent` and let the owning feature attach its components.
- Add a narrow spawner contract in the owning feature that Combat Director may call.

Do not attach foreign components directly inside Combat Director unless the project explicitly assigns ownership to Combat Director.

### 5. Clarify AI behavior ownership

Combat Director assigns `EnemyArchetype.Role`, but AI behavior initialization belongs to `AiBots`.

Fix direction:

- Combat Director should not set AI state, brain ids, or task state.
- If `AiBots` needs role hints, expose a read-only contract and let `AiBots` systems react to `EnemySpawnedEvent` or query `EnemyArchetype`.

### 6. Keep request/result contracts in the correct boundary

If client intent systems need to send commands to the director (for example "force cooldown" or designer cheats), those typed events must live in `CombatDirector.Contracts`, not hidden inside `Logic`.

Evaluate moving network-facing request/result events to:

```text
Runtime/Contracts/Events/
```

Keep handlers in `Runtime/Logic/Requests/`.

## Missing Design Lock Scope

### 7. Visibility-based spawn source selection

MVP uses distance and directional heuristics. A true visibility check ("not in player line of sight") is out of scope for the initial tasks.

Completion options:

- Add a follow-up task to integrate the project visibility service when it exists, or
- Document that MVP spawn selection is heuristic-only.

### 8. Multi-cell support

The initial implementation targets a single combat cell.

Follow-up needed:

- Define how multiple player groups create multiple cells.
- Ensure `CellId` is used consistently across systems.
- Prevent double-counting players who overlap multiple cells.

### 9. Loot/value integration

`CarriedLootValue` is referenced in the design but may not have an existing project component.

Follow-up needed:

- If the project has an inventory or loot feature, align `CarriedLootValue` with its contracts.
- If no such feature exists, add a minimal placeholder component inside Combat Director and document the integration point.

### 10. Biome and time-of-day spawn rules

The design mentions source activity per biome. This is not in the MVP.

Follow-up needed:

- Add `BiomeId` to `SpawnSource` when the open-world biome system is ready.
- Add time-of-day modifiers to `EncounterDirectorConfig` when the world time system is ready.

### 11. Death/despawn integration

The director tracks alive enemies for the cap, but enemy death/despawn may be owned by the combat feature.

Follow-up needed:

- Define how Combat Director observes death (event vs component deletion query).
- Ensure the alive enemy count is accurate.

## Tests To Add Or Strengthen

Add or update tests for:

- Spawn request is consumed exactly once.
- Alive enemy cap is enforced server-side under concurrency.
- Server recipe does not contain view paths.
- Full Calm -> BuildUp -> Peak -> Relief -> Cooldown cycle in a single test.
- Multiple players correctly update cell center and threat inputs.
- Client view binding cleans up after entity despawn.

## Acceptance Criteria

- Clean checkout compiles after Unity codegen/import.
- Spawn requests are single-consume and server-authoritative.
- Alive enemy cap is enforced on the server.
- Server entity creation contains no view or prefab metadata.
- Combat Director does not own AI behavior or damage rules.
- All original `02-combat-director.md` acceptance criteria are met:
  - threat grows with player activity
  - spawn happens only through Burrow/Rift sources
  - relief and cooldown phases exist
  - infinite spawn without cap is impossible
  - all combat entities are authoritative on the server
  - client View is client-only
  - `EnemyRole` can be extended without rewriting the director
