# Phase 1 Plan 2: Active Targeting Summary

**Active aim targeting now selects enemy actors or resource placements through one Combat command path, with static placement hits emitted as cross-feature facts.**

## Accomplishments
- Updated client passive auto-attack targeting to prefer `ClientInputState.TryGetAimRay` targets before falling back to nearest valid targets.
- Added resource placement target selection through OpenWorldResources target contracts without depending on OpenWorldResources Presentation.
- Kept passive attack intent and send behavior on the existing `UseAbilityCommand` path with `CombatTargetRef`.
- Updated server validation so actor targets keep authoritative combat checks while static-placement requests only validate Combat-owned command facts.
- Split server casting so actor targets still produce `CombatHit`, while static placements emit `CombatTargetHitEvent` for OpenWorldResources-owned handling.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Systems/Client/ClientPassiveAutoAttackTargetingSystem.cs` - Adds aim-ray target selection across actor and static-placement target families.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Systems/Client/ClientPassiveAutoAttackIntentSystem.cs` - Keeps local intent validation aligned with static-placement target refs.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Systems/Server/ServerValidateCombatCommandsSystem.cs` - Preserves actor validation and limits static-placement validation to Combat-owned command facts.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Systems/Server/ServerAbilityCastSystem.cs` - Emits static-placement hit facts directly and only creates `CombatHit` for actor targets.
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/StaticMlp.Features.Combat.Logic.asmdef` - References `StaticMlp.Features.OpenWorldResources.Contracts`.
- `.planning/ROADMAP.md` - Marks Phase 1 progress as two of six plans complete.
- `.planning/phases/01-core-combat-and-gathering/01-PHASE-SUMMARY.md` - Updates Phase 1 progress.

## Decisions Made
- Aim selection uses a small cone around the input aim ray, then tie-breaks by ray proximity, source distance, and stable target id.
- Resource placement hit points are quantized from the resource target read model position until OpenWorldResources adds authoritative placement/range handling in Plan 3.
- Static placement requests do not perform server-side placement existence, depletion, or authoritative resource range checks in Combat.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Unity replication/codegen output was already pending from Plan 1 because `UseAbilityCommand` and `PassiveAutoAttackRequestEvent` source contracts changed to `CombatTargetRef`; generated files were not manually edited.
- Unity compile/play checks were not run, following the project rule that agents must ask the user to run Unity/build verification.

## Next Phase Readiness
Run the Unity replication/codegen workflow and Unity compile checks, then proceed to `01-03-PLAN.md`.

Ready for `01-03-PLAN.md`.

---
*Phase: 01-core-combat-and-gathering*
*Completed: 2026-05-26*
