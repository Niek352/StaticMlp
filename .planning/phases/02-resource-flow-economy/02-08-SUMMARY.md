# Phase 2 Research 8: Barter Boundary Summary

**Barter runtime deferred behind merchant/faction and item ownership, with Settlement resource contracts preserved as the future resource leg**

## Accomplishments
- Researched the current Settlement, NPC, Progression, and Loadout ownership boundaries for barter.
- Created findings that reject Phase 2 barter runtime and static placeholder offer contracts.
- Updated the roadmap with the Phase 2 deferral boundary and future write-path constraint.

## Files Created/Modified
- `.planning/phases/02-resource-flow-economy/02-08-FINDINGS.md` - Research recommendation and architecture boundary for barter.
- `docs/Roadmap по GDD.md` - Adds the Research 02-08 barter deferral note.
- `.planning/phases/02-resource-flow-economy/02-08-SUMMARY.md` - Execution summary for this research plan.

## Decisions Made
- Merchant identity and offer lifecycle should not be owned by Settlement, NPC, Progression, or Loadout alone.
- A future dedicated commerce/barter feature should own offer definitions and lifecycle after merchant/faction and item contracts exist.
- Settlement storage remains owner-mutated by Settlement-owned request/event handlers.
- No Phase 2 `BarterOfferDefinition` should be created while merchant, faction, and item ids would be placeholders.

## Deviations from Plan

Used the repository's numbered summary convention, `02-08-SUMMARY.md`, instead of creating a directory-level `SUMMARY.md`. Existing Phase 2 executions use per-plan summaries in the same directory, and a generic `SUMMARY.md` would incorrectly mark the whole phase directory as already executed.

## Issues Encountered
- None.

## Next Phase Readiness
- Phase 2 economy remains closed around settlement production, storage, construction, and unlock gates.
- Barter is ready to be reconsidered after Phase 3 merchant/faction facts and Phase 4 item ownership contracts exist.

---
*Phase: 02-resource-flow-economy*
*Completed: 2026-05-27*
