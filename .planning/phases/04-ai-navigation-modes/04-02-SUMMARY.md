# Phase 04 Plan 02: Far-To-Local Navigation Handoff Summary

**Generic local-navigation handoff events with owner-side `AiMoveRequest` projection and stale-position reconciliation for `AiBots`**

## Accomplishments
- Added `LocalNavAttachState` and `LocalNavAttachStateSystem` so navigation can decide when a far-simulated entity is eligible to attach to a ready local nav area.
- Added `FarToLocalAiNavigationHandoffSystem` plus `LocalNavigationHandoffRequestEvent` so `AiNavigation` can request local movement handoff without mutating `AiBots` state directly.
- Added `ServerAiLocalNavigationHandoffApplySystem` in `AiBots` so the move-intent owner projects logical position into `CharacterNetState`, writes `AiMoveRequest`, and preserves `ServerAiNavigationSystem` as the only local navigation application system.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/LocalNavAttachState.cs` - Navigation-owned readiness fact for local-nav handoff.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/LocalNavigationHandoffRequestEvent.cs` - Cross-feature handoff request event emitted by navigation and consumed by movement owners.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Systems/Server/LocalNavAttachStateSystem.cs` - Resolves current local-nav readiness from nav-area state.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Systems/Server/FarToLocalAiNavigationHandoffSystem.cs` - Emits handoff requests when local nav becomes available.
- `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/AiNavigationLogicFeature.cs` - Registers attach-state and handoff systems before local bot navigation.
- `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/Systems/Server/ServerAiLocalNavigationHandoffApplySystem.cs` - Owner-side bridge that applies logical handoff into `CharacterNetState` and `AiMoveRequest`.
- `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/AiBotsGameplayFeature.cs` - Registers the owner-side handoff apply system before `ServerAiNavigationSystem`.
- `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/StaticMlp.Features.AiBots.asmdef` - Adds the dependency on `StaticMlp.Features.AiNavigation.Contracts` for the handoff event contract.
- `.planning/ROADMAP.md` - Marks Phase 04 complete with both plans finished.

## Decisions Made
- Used ready `CombatCellNavArea` plus `RuntimeNavMeshZoneState` as the concrete local-nav availability fact because the current server gameplay layer exposes that state directly and does not provide a separate chunk-attach contract for AI handoff.
- Kept the cross-feature boundary explicit: `AiNavigation` emits a generic handoff request, while `AiBots` remains the owner of `AiMoveRequest` and local replicated movement projection.
- Reconciled far logical position into `CharacterNetState` on the `AiBots` side before local navigation resumes so local backend movement never starts from stale replicated coordinates.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 4 - Architectural] Replaced direct `AiMoveRequest` mutation with an explicit handoff event boundary**
- **Found during:** Task 2 (Create handoff system through `AiMoveRequest`)
- **Issue:** The original plan path required `AiNavigation` to write `AiBots`-owned `AiMoveRequest` directly, which violated the project rule that cross-feature writes must go through typed events.
- **Fix:** Added `LocalNavigationHandoffRequestEvent` in `AiNavigation` and an owner-side apply system in `AiBots` that performs the actual movement-intent mutation.
- **Files modified:** `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Contracts/LocalNavigationHandoffRequestEvent.cs`, `Assets/Scripts/StaticMlp/Features/AiNavigation/Runtime/Logic/Systems/Server/FarToLocalAiNavigationHandoffSystem.cs`, `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/Systems/Server/ServerAiLocalNavigationHandoffApplySystem.cs`, `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/AiBotsGameplayFeature.cs`, `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/StaticMlp.Features.AiBots.asmdef`
- **Verification:** `AiNavigation` contains no `AiMoveRequest` mutation and the move-intent write happens only inside `AiBots`.
- **Commit:** Included in the plan completion commit for `feat(04-02)`.

**2. [Rule 2 - Missing Critical] Added owner-side `CharacterNetState` projection during handoff**
- **Found during:** Task 2 (Create handoff system through `AiMoveRequest`)
- **Issue:** Far simulation advances only logical navigation position, so local navigation would otherwise resume from stale replicated coordinates when the handoff event fired.
- **Fix:** The `AiBots` owner-side apply system now projects the far logical position into `CharacterNetState` before setting or clearing `AiMoveRequest`.
- **Files modified:** `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/Systems/Server/ServerAiLocalNavigationHandoffApplySystem.cs`
- **Verification:** The owner-side handoff system writes `ReplicationMut.Mut<CharacterNetState>(entity)` before applying `AiMoveRequest`.
- **Commit:** Included in the plan completion commit for `feat(04-02)`.

---

**Total deviations:** 2 auto-fixed (1 architectural with approval, 1 missing critical), 0 deferred
**Impact on plan:** Both deviations were necessary to preserve feature ownership and to ensure far-to-local handoff resumes from the correct logical position without bypassing the existing local movement path.

## Issues Encountered
- `04-01-SUMMARY.md` was missing even though the roadmap already implied Phase 04 work had started, so `04-01` was executed and documented first as a prerequisite to the handoff work.

## Next Phase Readiness
Far AI now has explicit navigation mode state, sparse logical movement, attach readiness, and an owner-safe handoff into the existing `AiBots` local navigation path.
Ready for `05-01-PLAN.md`.

---
*Phase: 04-ai-navigation-modes*
*Completed: 2026-05-19*
