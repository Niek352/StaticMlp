# AiCombatDirector OpenWorld Audit

This audit maps the existing `StaticMlp.Features.CombatDirector` implementation before the open-world refactor. It is grounded in:

- `.planning/BRIEF.md`
- `.planning/ROADMAP.md`
- `ai/gdd/AiCombatDirector_OpenWorld_Plan.md`
- `ai/combat_director.md`
- `ai/CombatDirector_AiNavigation_Contract.md`
- `Assets/Scripts/StaticMlp/Features/CombatDirector/AGENTS.md`
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/CombatDirectorGameplayFeature.cs`
- `Assets/Tests/Editor/CombatDirector/CombatDirectorRuntimeSystemsTests.cs`
- `rg -n "ThreatBudget|DirectorPhase|SpawnSource|SpawnRequest|EnemySpawnApplySystem" Assets/Scripts/StaticMlp/Features/CombatDirector Assets/Tests/Editor/CombatDirector ai`

## Current Shape

The current feature is the right feature boundary to keep. It already separates contracts, server logic, and client presentation, and it already creates enemies only through `AiBotFactory` from `EnemySpawnApplySystem`.

The current gameplay model is wave-first:

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

The open-world target keeps the server-authoritative spawn/apply boundary, but replaces the always-growing pressure loop with reason-based attention, encounter state, source classification, and explicit pressure-event eligibility.

## Runtime File Map

| File | Responsibility | Open-world disposition |
| --- | --- | --- |
| `Runtime/Logic/CombatDirectorGameplayFeature.cs` | Registers replicated contracts, resources, and server system order. | Refactor. Keep feature boundary and registration pattern, replace the wave-first systems in phases 07-10. |
| `Runtime/Contracts/Definitions/DirectorPhase.cs` | Current `Calm`, `BuildUp`, `Peak`, `Relief`, `Cooldown` enum. | Replace or supersede in phase 08 with open-world phase semantics. Preserve stable replicated ordering rules when migrating. |
| `Runtime/Contracts/Definitions/EnemyRole.cs` | `Swarmer`, `Marker`, `AnchorElite` role ids. | Keep for existing enemy archetype metadata. Defer broader creature/faction role work unless an implementation phase requires it. |
| `Runtime/Contracts/Definitions/SpawnSourceType.cs` | Current `Burrow`, `Rift` source ids. | Refactor in phase 09 into source classification or a compatibility source type plus `SpawnSourceKind`. |
| `Runtime/Contracts/Components/CombatCell.cs` | Single active cell around the player group. | Keep and extend meaning as an open-world attention cell. Multi-cell support is deferred. |
| `Runtime/Contracts/Components/ThreatBudget.cs` | Accumulating wave budget. | Replace in phase 07 with `CellAttention`; do not retain a hidden compatibility budget. |
| `Runtime/Contracts/Components/DirectorState.cs` | Replicated phase, phase timer, and time since peak. | Refactor in phase 08 for open-world phases and encounter presentation state. |
| `Runtime/Contracts/Components/PlayerNoise.cs` | Per-player threat input. | Refactor in phase 07 into explicit attention reason inputs. |
| `Runtime/Contracts/Components/CarriedLootValue.cs` | Loot-derived pressure input. | Keep concept, route through attention reason input instead of global threat growth. |
| `Runtime/Contracts/Components/SpawnSource.cs` | Active world source with type, position, radius, active flag. | Refactor in phase 09 with source kind and allowed usage flags. |
| `Runtime/Contracts/Components/EnemyTag.cs` | Tags director-spawned enemies. | Keep. Useful for alive caps and encounter accounting. |
| `Runtime/Contracts/Components/EnemyArchetype.cs` | Replicated enemy role metadata. | Keep. |
| `Runtime/Contracts/Components/EnemySpawnSource.cs` | Replicated source metadata for spawned enemies. | Refactor only if presentation needs reason/source-kind data; keep as presentation metadata boundary. |
| `Runtime/Logic/Components/PlayerThreatInputState.cs` | Tracks accepted attack sequence for noise deltas. | Refactor in phase 07 into reason input bookkeeping. |
| `Runtime/Logic/Components/SelectedSpawnSource.cs` | Server-only selected source snapshot. | Refactor in phase 09 to include source kind, reason/usage, and cached reachability constraints. |
| `Runtime/Logic/Components/SpawnSourcePlacementRef.cs` | Links spawned source entities to open-world chunk placements. | Keep. Extend only if source classification needs placement metadata. |
| `Runtime/Logic/Components/ValidSpawnRequestTag.cs` | Marks server-only validated spawn requests. | Keep. |
| `Runtime/Logic/Events/SpawnRequest.cs` | Server-only request consumed by spawn apply. | Refactor in phases 09-10 with encounter/reason/source classification. Keep server-only. |
| `Runtime/Logic/Events/EnemySpawnedEvent.cs` | Internal event after enemy creation. | Keep. May add reason/kind later if presentation/debug needs it. |
| `Runtime/Logic/WorldResources/EncounterDirectorConfig.cs` | Wave thresholds, budget multipliers, alive cap, distance band. | Refactor. Split attention, encounter, ambient, pressure-event, and source-distance config as needed. |
| `Runtime/Logic/Catalogs/EnemySpawnCatalog.cs` | Role definitions and wave counts. | Refactor for encounter composition. Keep catalog validation pattern. |
| `Runtime/Logic/Definitions/EnemySpawnDefinition.cs` | Role budget cost and min/max wave counts. | Refactor because wave count semantics do not map to ambient/encounter pressure directly. |
| `Runtime/Logic/Validation/EnemySpawnCatalogValidator.cs` | Catalog fail-fast validation. | Keep validation style, update rules with new catalog fields. |
| `Runtime/Logic/Domain/SpawnSourcePlacementRules.cs` | Maps open-world placement kind ids to `Burrow`/`Rift`. | Refactor in phase 09. Unknown kinds should still fail fast. |
| `Runtime/Logic/Systems/SpawnSourcePlacementSeedSystem.cs` | Seeds active `SpawnSource` entities from `OpenWorldChunkGenerationCompleted.SpawnPlacements`. | Keep the event-to-source boundary; refactor seeded data to source kind and usage flags. |
| `Runtime/Logic/Systems/CombatCellTrackingSystem.cs` | Creates/updates the single `CombatCell` entity and seeds `ThreatBudget`/`DirectorState`. | Refactor in phase 07 to seed `CellAttention`; phase 08 to seed open-world state. |
| `Runtime/Logic/Systems/PlayerThreatInputSystem.cs` | Converts attacks, harvest requests, time in cell, proximity, and loot into `PlayerNoise`/`CarriedLootValue`. | Replace in phase 07 with reason-based attention input. Time-in-cell and passive proximity must not create dangerous pressure by default. |
| `Runtime/Logic/Systems/ThreatBudgetAccumulationSystem.cs` | Adds base threat, noise, and loot into `ThreatBudget.Current` every tick. | Replace in phase 07 with attention accumulation and decay. |
| `Runtime/Logic/Systems/DirectorPhaseSystem.cs` | Drives `Calm -> BuildUp -> Peak -> Relief -> Cooldown`. | Replace in phase 08 with open-world phase transitions. |
| `Runtime/Logic/Systems/SpawnSourceSelectionSystem.cs` | Picks active source by distance and directional score during spawn phases. | Refactor in phase 09 to require source usage kind and cached reachability where applicable. |
| `Runtime/Logic/Systems/SpawnRequestBuildSystem.cs` | Builds hardcoded low/medium/high wave requests from budget bands. | Replace for ambient/encounter request builders. Keep only the idea of server-only request construction. |
| `Runtime/Logic/Systems/SpawnRequestValidationSystem.cs` | Rechecks phase, source, distance, alive cap, role, budget. | Refactor. Add reason, source usage, reachability, encounter cap, and cooldown validation. |
| `Runtime/Logic/Systems/EnemySpawnApplySystem.cs` | Creates enemies through `AiBotFactory`, attaches metadata, consumes budget, destroys request. | Keep as the only enemy creation boundary; refactor budget consumption into attention/encounter accounting. |

## Presentation File Map

| File | Responsibility | Open-world disposition |
| --- | --- | --- |
| `Runtime/Presentation/CombatDirectorPresentationFeature.cs` | Registers client-only presentation systems and view sync. | Keep. |
| `Runtime/Presentation/Components/DirectorAudioPresentationState.cs` | Tracks last phase for audio. | Refactor with open-world phases. |
| `Runtime/Presentation/Components/SpawnSourceTelegraphState.cs` | Client-only source pulse state. | Keep/refactor for source kind and reason. |
| `Runtime/Presentation/Components/EnemyRoleViewState.cs` | Client view role state. | Keep. |
| `Runtime/Presentation/Systems/Client/DirectorTelegraphReceiveSystem.cs` | Builds telegraph state from replicated spawn source metadata. | Refactor if spawn reasons/source kinds become visible. |
| `Runtime/Presentation/Systems/Client/SpawnSourceVfxSystem.cs` | Plays source VFX from source type. | Refactor source-type mapping after phase 09. |
| `Runtime/Presentation/Systems/Client/EnemySpawnAudioSystem.cs` | Plays source, spawn, and phase audio. | Refactor phase audio for ambient/contact/escalation/pressure. |
| `Runtime/Presentation/Systems/Client/EnemyViewBindSystem.cs` | Binds enemy role view state and fails fast on missing view data. | Keep. |

Presentation remains correctly client-only. No open-world gameplay state should move here.

## Test File Map

| File | Responsibility | Open-world disposition |
| --- | --- | --- |
| `Assets/Tests/Editor/CombatDirector/CombatDirectorContractsTests.cs` | Stable enum values, replicated GUIDs, contract size limits. | Refactor with new/changed contracts. Keep stable GUID and compactness checks. |
| `Assets/Tests/Editor/CombatDirector/CombatDirectorRuntimeSystemsTests.cs` | End-to-end runtime coverage for cell tracking, threat, phase, source selection, spawn request build/validation/apply. | Refactor heavily across phases 07-10. Tests currently encode wave-first behavior. |
| `Assets/Tests/Editor/CombatDirector/EnemySpawnCatalogTests.cs` | Catalog validation and default role definitions. | Refactor with encounter/ambient catalog semantics. |

## Wave-first Contracts And Systems

The current wave-first surface is:

- `ThreatBudget`: `Current`, `Max`, `AccumulationPerSecond`.
- `DirectorPhase`: `Calm`, `BuildUp`, `Peak`, `Relief`, `Cooldown`.
- `EncounterDirectorConfig`: `BaseThreatPerSecond`, noise/loot multipliers, `BuildUpThreshold`, `PeakThreshold`, relief/cooldown seconds, max alive enemies, spawn distance band.
- `PlayerThreatInputSystem`: action noise, harvest noise, time-in-cell noise, spawn-source proximity noise, carried loot value.
- `ThreatBudgetAccumulationSystem`: budget always grows from base threat plus current inputs.
- `DirectorPhaseSystem`: budget threshold transitions into wave phases.
- `SpawnSourceSelectionSystem`: source selection is allowed in `BuildUp`/`Peak`.
- `SpawnRequestBuildSystem`: low/medium/high budget bands create swarmer/marker/anchor wave requests.
- `SpawnRequestValidationSystem`: accepts only current wave phases and checks budget/cap/source validity.
- `EnemySpawnApplySystem`: consumes threat budget after spawning.
- Tests named around `ThreatBudgetAccumulationSystem`, `DirectorPhaseSystem`, low/medium/high budgets, and immediate spawn apply.

These are the exact files phase 07 must treat as the first replacement target.

## Architecture Conflicts

### ThreatBudget

Conflict: `ThreatBudgetAccumulationSystem` adds `BaseThreatPerSecond` every tick and `PlayerThreatInputSystem` adds time-in-cell noise. This means mere player presence trends toward pressure. The open-world plan requires calm exploration to remain calm unless explicit causes exist.

Safe path: replace `ThreatBudget` with `CellAttention` in phase 07. Attention should grow only from explicit reasons such as combat, noise, trespass, guarded loot, faction alarm, or high-value resource actions, and should decay after causes stop.

### DirectorPhase

Conflict: current phase semantics are pressure-wave phases. `BuildUp` and `Peak` are not open-world encounter phases, and `Peak` is entered from budget plus any active source.

Safe path: phase 08 should introduce open-world states: `Ambient`, `Contact`, `Suspicion`, `Escalation`, `PressureEvent`, `Recovery`, `Cooldown`. Do not preserve old phase names as compatibility wrappers unless they are explicitly deprecated and removed quickly.

### Source Selection

Conflict: source selection currently accepts any active `SpawnSource` in the distance band and uses directional scoring. It does not require source role, allowed usage, safe-zone constraints, cooldown, or cached `AiNavigation` reachability.

Safe path: phase 09 should classify sources first, then selection/build systems should select only sources whose usage allows the current encounter type. Pressure events should require cached reachable sources from `AiNavigation`; do not add fallback positions.

### Spawn Request Build

Conflict: `SpawnRequestBuildSystem` turns budget bands into low/medium/high wave compositions. This hardcodes swarmers as the default and makes large groups a normal consequence of threshold crossing.

Safe path: phase 10 should add separate ambient/encounter request builders. Pressure-event request building should remain gated behind explicit reason, source kind, cooldown, alive caps, and reachability.

### Alive Caps

Conflict: current cap is a single `MaxAliveEnemiesPerCell`. It prevents over-spawn, but it cannot distinguish ambient solo/small encounters from pressure events or per-source cooldowns.

Safe path: keep the existing cap as a technical safety limit, but add encounter/ambient caps and per-marker/source cooldowns before enabling ambient spawns.

### Reachability

Conflict: docs require `AiNavigation` to own reachability and the contract says selection should use cached reachable sources, but the current selector does not read reachability state.

Safe path: do not move navigation logic into `CombatDirector`. Phase 09 should read public cached reachability/resolved-source contracts from `AiNavigation` where pressure or escalation requires reachability.

### Presentation

Conflict: presentation is structurally safe, but it currently keys audio/VFX from old `DirectorPhase` and `SpawnSourceType` only. It cannot explain why an enemy appeared or why pressure was blocked.

Safe path: keep presentation client-only and add reason/source-kind state only through replicated/server-owned metadata.

### Tests

Conflict: tests intentionally lock old behavior: passive budget growth, old phase ordering, low/medium/high role counts, and budget consumption.

Safe path: refactor tests phase by phase. Do not delete coverage without replacing it with attention decay, open-world phase, source classification, ambient no-wave, and pressure eligibility tests.

## Safe Migration Order

1. Phase 07, `07-01-PLAN.md`: Replace `ThreatBudget` and `ThreatBudgetAccumulationSystem` with `CellAttention` and attention decay. Refactor `CombatCellTrackingSystem`, `PlayerThreatInputSystem`, `EncounterDirectorConfig`, contract tests, and runtime tests around reason-based attention. The first invariant is: passive exploration does not create pressure.
2. Phase 08: Replace `DirectorPhase` semantics and add encounter state. Update `DirectorState`, `DirectorPhaseSystem`, presentation phase reads, and tests. The invariant is: ambient/contact/suspicion can resolve without becoming a wave.
3. Phase 09: Extend/refactor `SpawnSource` classification and selection. Update `SpawnSourcePlacementRules`, `SpawnSourcePlacementSeedSystem`, `SelectedSpawnSource`, request validation, and tests. Pressure-capable sources must be explicit and reachability must be consumed as cached `AiNavigation` state.
4. Phase 10: Add ambient solo/small-group spawning, cooldowns, and caps. Refactor spawn request building and validation so ambient encounters cannot implicitly become pressure events.
5. After phase 10: Keep `EnemySpawnApplySystem` as the spawn boundary, but remove budget consumption and attach encounter/source/reason metadata needed by debug and presentation.

## Phase 07 Starting Point

Start with these files:

- `Runtime/Contracts/Components/ThreatBudget.cs`
- `Runtime/Logic/Systems/ThreatBudgetAccumulationSystem.cs`
- `Runtime/Logic/Systems/PlayerThreatInputSystem.cs`
- `Runtime/Logic/Systems/CombatCellTrackingSystem.cs`
- `Runtime/Logic/WorldResources/EncounterDirectorConfig.cs`
- `Runtime/Logic/CombatDirectorGameplayFeature.cs`
- `Assets/Tests/Editor/CombatDirector/CombatDirectorContractsTests.cs`
- `Assets/Tests/Editor/CombatDirector/CombatDirectorRuntimeSystemsTests.cs`

Do not change spawn source classification or ambient spawning in phase 07 unless the phase plan explicitly expands scope. Those are phases 09 and 10.

## Verification Notes

- The file map was grounded with the required `rg` command listed at the top of this audit.
- No runtime `.cs` files should be modified by this audit phase.
- Next step: `07-01-PLAN.md`.
