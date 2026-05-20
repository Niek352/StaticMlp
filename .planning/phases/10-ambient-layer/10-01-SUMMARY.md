# Phase 10 Plan 01: Ambient Layer Summary

**Server-authoritative ambient encounter requests with source cooldowns, cell caps, and non-escalating solo/small-group recovery**

## Accomplishments
- Added ambient spawn contracts for marker kind, per-marker cooldown, and director-cell alive caps.
- Added ambient scan/request systems that create solo or small requests only from ambient-enabled sources and reuse the validated spawn request/apply pipeline.
- Extended validation/apply logic so ambient requests do not consume pressure budget, respect the 1-4 ambient cap, and create non-escalating encounter state.
- Added tests for ambient contract defaults, source filtering, cooldown, cap enforcement, and solo ambient recovery without wave creation.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Contracts/Definitions/AmbientSpawnKind.cs` - Ambient encounter composition kind contract.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Contracts/Components/AmbientSpawnMarker.cs` - Source-local ambient count and cooldown state.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Contracts/Components/CellAliveEnemyCaps.cs` - Director-cell cap state derived from config.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/WorldResources/EncounterDirectorConfig.cs` - Conservative ambient, encounter, and escalation cap defaults.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/AmbientWorldInterestScanSystem.cs` - Selects eligible ambient sources and ticks marker cooldowns.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/AmbientEncounterSpawnRequestSystem.cs` - Creates capped ambient spawn requests and starts source cooldowns.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/CombatDirectorGameplayFeature.cs` - Registers ambient scan/request systems before pressure-event selection/building.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Events/SpawnRequest.cs` - Carries explicit ambient kind on server-only spawn requests.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Components/SelectedSpawnSource.cs` - Carries ambient marker count metadata for selected sources.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/SpawnRequestValidationSystem.cs` - Validates ambient requests by ambient phase, source flag, cap, and distance.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/EnemySpawnApplySystem.cs` - Applies ambient requests without pressure budget consumption and creates non-escalating encounter state.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/CombatCellTrackingSystem.cs` - Initializes director-cell cap state.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Systems/SpawnSourcePlacementSeedSystem.cs` - Adds ambient markers to ambient open-world placement sources.
- `Assets/Scripts/StaticMlp/Features/CombatDirector/Runtime/Logic/Domain/SpawnSourcePlacementRules.cs` - Maps ambient placement kind to conservative ambient marker defaults.
- `Assets/Tests/Editor/CombatDirector/CombatDirectorContractsTests.cs` - Covers ambient kind ordering, marker fields, and conservative config defaults.
- `Assets/Tests/Editor/CombatDirector/CombatDirectorRuntimeSystemsTests.cs` - Covers ambient request creation, pressure-only rejection, cooldown, cap enforcement, and non-escalating recovery.
- `.planning/ROADMAP.md` - Marked Phase 10 complete.
- `.planning/phases/10-ambient-layer/10-01-SUMMARY.md` - Execution summary.

## Decisions Made
- Ambient source cooldowns live on source entities as `AmbientSpawnMarker` state, avoiding global mutable state.
- Cell caps live on the director cell as `CellAliveEnemyCaps` and are refreshed from `EncounterDirectorConfig`.
- Ambient requests reuse `SpawnRequestValidationSystem` and `EnemySpawnApplySystem`, but are distinguished by `AmbientSpawnKind` so they do not consume pressure budget or require pressure-event phase.
- Current ambient enemy role uses the existing `EnemyRole.Swarmer` catalog entry because no separate animal/bandit role catalog exists yet; composition is constrained by `AmbientSpawnKind` and request counts.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None.

## Next Phase Readiness
Phase 10 complete; future planning may continue with escalation reasons and pressure-event gating.

---
*Phase: 10-ambient-layer*
*Completed: 2026-05-20*
