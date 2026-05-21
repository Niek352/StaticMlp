# Phase 3 Plan 2: Bedroll Shelter Operation Summary

**Bedroll Shelter now exposes finite bed service slots for future NPC rest behavior.**

## Accomplishments
- Added typed Bedroll Shelter operation state with enabled state and finite slot count.
- Added per-slot `BedSlotState` rows with explicit `Free`, `Reserved`, `Occupied`, and `Blocked` statuses.
- Added pure bed-slot transition rules and a Settlement-owned bootstrap system for completed Bedroll Shelter buildings.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Components/BedrollShelterState.cs` - Stores Shelter service state on finished buildings.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Components/BedSlotState.cs` - Stores individual bed slot status and `EntityGID` occupant/reservation data.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Components/BedSlotStatus.cs` - Defines the explicit slot lifecycle states.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Domain/BedSlotRules.cs` - Provides fail-fast reservation, occupy, release, and block transitions.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Systems/Server/ServerBedrollShelterBootstrapSystem.cs` - Consumes Bedroll Shelter completion facts and initializes bed slots.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/SettlementSharedResourcesGameplayFeature.cs` - Registers the shelter bootstrap system.
- `Assets/Tests/Editor/Settlement/BedrollShelterOperationTests.cs` - Covers slot transitions, blocked rejection, and Shelter bootstrap state.
- `.planning/ROADMAP.md` - Marks Phase 3 as 2/3 plans complete.

## Decisions Made
- Stored slots as `SW.Multi<BedSlotState>` on the finished Shelter building so each building owns its own finite service surface.
- Used `EntityGID` for worker/NPC reservation references and avoided raw ids in gameplay contracts.
- Kept Shelter bootstrap independent of NPC and Settlement.Workers state; future worker behavior can consume the typed slots by request/event.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Unity EditMode tests were not run because agents must not launch Unity or run project build checks.

## Next Step
Ready for `03-03-PLAN.md`.
