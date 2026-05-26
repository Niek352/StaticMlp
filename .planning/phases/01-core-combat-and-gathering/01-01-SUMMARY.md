# Phase 1 Plan 1: Combat Target Contracts Summary

**Combat target references now represent actor entities and deterministic static placements, with OpenWorldResources publishing targetable proxy read models.**

## Accomplishments
- Added shared Combat target contracts for actor and static-placement targets.
- Converted combat command, request, hit, passive intent, and passive target state to carry `CombatTargetRef`.
- Added `CombatTargetHitEvent` as the cross-feature combat fact emitted for validated hits, without letting Combat mutate resource overlays.
- Added OpenWorldResources contracts for client-readable target state on resource proxy entities.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Contracts/CombatTargetKind.cs` - Target discriminator for none, actor entity, and static placement.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Contracts/CombatTargetRef.cs` - Shared target reference with `EntityGID`, placement id, and quantized hit point fields.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Contracts/CombatTargetHitEvent.cs` - Combat fact emitted after a validated hit.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Contracts/PassiveAutoAttackRequestEvent.cs` - Updated passive attack request target shape.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Events/UseAbilityCommand.cs` - Updated client ability command target shape.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Components/CombatAbilityRequest.cs` - Updated server request state target shape.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Components/CombatHit.cs` - Updated hit state target shape.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Components/PassiveAutoAttackIntent.cs` - Updated local attack intent target shape.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Components/PassiveAutoAttackState.cs` - Updated local current target state.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Systems/Client/ClientPassiveAutoAttackTargetingSystem.cs` - Wraps existing monster targets as actor target refs.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Systems/Client/ClientPassiveAutoAttackIntentSystem.cs` - Validates actor and static target refs before creating local intent.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Systems/Server/ServerReceiveCombatCommandsSystem.cs` - Creates server requests with the updated target ref.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Systems/Server/ServerValidateCombatCommandsSystem.cs` - Preserves actor validation and adds static-placement range validation by hit point.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Systems/Server/ServerHitToEffectSystem.cs` - Emits combat target hit facts and only creates damage/status effects for actor targets.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Presentation/Systems/Client/ClientPassiveAutoAttackPresentationSystem.cs` - Keeps actor highlight behavior while allowing static hit-point tracer endpoints.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Contracts/StaticMlp.Features.OpenWorldResources.Contracts.asmdef` - New contracts assembly for resource target read models.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Contracts/Components/OpenWorldResourceTargetable.cs` - Targetable marker for active resource proxies.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Contracts/Components/OpenWorldResourceTargetState.cs` - Resource proxy target read model.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Contracts/Components/OpenWorldResourceOverlayFlags.cs` - Moved owner overlay flags into contracts so read models do not depend on Logic internals.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Logic/StaticMlp.Features.OpenWorldResources.asmdef` - References the new contracts assembly.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Presentation/StaticMlp.Features.OpenWorldResources.Presentation.asmdef` - References the new contracts assembly.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Presentation/Systems/Client/ClientOpenWorldResourceProxySpawnSystem.cs` - Publishes targetable read models when resource proxies spawn.
- `Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime/Presentation/Systems/Client/ClientOpenWorldChunkOverlayApplySystem.cs` - Updates target read models and targetable tags when overlays arrive.
- `.planning/ROADMAP.md` - Marks Phase 1 as in progress with one plan complete.
- `.planning/phases/01-core-combat-and-gathering/01-PHASE-SUMMARY.md` - Updates Phase 1 progress.

## Decisions Made
- Static placements are represented by `PlacementId` plus quantized hit point, not by `EntityGID`.
- Combat emits `CombatTargetHitEvent` for validated hits, and resource mutation remains owned by OpenWorldResources.
- OpenWorldResource overlay flags moved to the contracts assembly to avoid making target read-model consumers depend on OpenWorldResources Logic or Presentation.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Moved overlay flags into OpenWorldResources.Contracts**
- **Found during:** Task 3 (OpenWorldResources target read-model contracts)
- **Issue:** `OpenWorldResourceTargetState` needed to expose owner flags, but the existing flag enum lived in Logic. Referencing Logic from Contracts would invert the intended dependency.
- **Fix:** Moved `OpenWorldResourceOverlayFlags` into the new contracts assembly and referenced that assembly from Logic and Presentation.
- **Files modified:** `OpenWorldResourceOverlayFlags.cs`, `StaticMlp.Features.OpenWorldResources.asmdef`, `StaticMlp.Features.OpenWorldResources.Presentation.asmdef`
- **Verification:** `rg -n "OpenWorldResourceTarget" Assets/Scripts/StaticMlp/Features/OpenWorldResources/Runtime` shows contracts under `Runtime/Contracts` and presentation systems updating them.

---

**Total deviations:** 1 auto-fixed blocking dependency issue, 0 deferred.
**Impact on plan:** The dependency direction is safer than exposing target flags through a Logic dependency.

## Issues Encountered
- Generated network-event code is now stale by source-contract design: `UseAbilityCommand` and `PassiveAutoAttackRequestEvent` changed from `EntityGID` targets to `CombatTargetRef`, and `.Generated.cs` files were not manually edited.

## Next Phase Readiness
Run the Unity replication/codegen workflow and Unity compile checks, then proceed to `01-02-PLAN.md`.

---
*Phase: 01-core-combat-and-gathering*
*Completed: 2026-05-26*
