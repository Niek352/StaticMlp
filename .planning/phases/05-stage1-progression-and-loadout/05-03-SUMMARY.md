# Phase 5 Plan 3: Settlement Loadout Modules Summary

**Loadout now has first-pass settlement-facing module definitions.**

## Accomplishments
- Added concrete settlement-facing loadout modules for Utility, BuildSignal, and BaseInfrastructure slots.
- Split combat ability binding from generic loadout modules through explicit `HasGrantedAbility`, `HasArchetype`, and `LoadoutModuleEffectKind` metadata.
- Tightened module catalog validation so combat modules require combat bindings and non-combat modules reject combat ability/archetype bindings.
- Added loadout tests for settlement module slot acceptance, combat slot limits, non-combat catalog validity, and combat-only Stage1 prepared snapshots.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Loadout/Runtime/Logic/Catalogs/LoadoutModuleCatalog.cs` - Adds settlement Utility, BuildSignal, and BaseInfrastructure module ids and definitions.
- `Assets/Scripts/StaticMlp/Features/Loadout/Runtime/Logic/Definitions/LoadoutModuleDefinition.cs` - Adds optional combat binding flags and non-combat module construction.
- `Assets/Scripts/StaticMlp/Features/Loadout/Runtime/Logic/Definitions/LoadoutModuleEffectKind.cs` - Defines semantic module effect categories for combat and settlement modules.
- `Assets/Scripts/StaticMlp/Features/Loadout/Runtime/Logic/Domain/SlotRules.cs` - Validates module ids, effect kinds, combat bindings, non-combat bindings, and duplicate ids.
- `Assets/Scripts/StaticMlp/Features/Loadout/Runtime/Logic/Domain/Stage1LoadoutRules.cs` - Fails fast when a non-combat module is used as the Stage1 primary combat build.
- `Assets/Tests/Editor/Loadout/SlotRulesTests.cs` - Covers settlement module definitions, slot-kind enforcement, catalog validation, and combat slot limits.
- `.planning/ROADMAP.md` - Marks Phase 5 and plan `05-03` complete.
- `.planning/phases/05-stage1-progression-and-loadout/05-03-SUMMARY.md` - Records this plan result.

## Decisions Made
- Settlement-facing modules are represented as loadout module definitions with `LoadoutModuleEffectKind`, not as fake `CombatAbilityId` values.
- Modifier application is deferred. This plan only defines catalog content and validation because no existing safe owner path applies settlement modifiers yet.
- Stage1 build preparation remains combat-only; utility, build signal, and base infrastructure modules can be active loadout entries but cannot produce combat prepared snapshots.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added `LoadoutModuleEffectKind` as a separate definition file**
- **Found during:** Task 1 (Add settlement-facing module definitions)
- **Issue:** The project requires one top-level type per `.cs` file, and settlement modules needed explicit semantics without faking combat abilities.
- **Fix:** Added `LoadoutModuleEffectKind.cs` in the existing `Definitions` bucket and referenced it from `LoadoutModuleDefinition`.
- **Files modified:** `Assets/Scripts/StaticMlp/Features/Loadout/Runtime/Logic/Definitions/LoadoutModuleEffectKind.cs`, `Assets/Scripts/StaticMlp/Features/Loadout/Runtime/Logic/Definitions/LoadoutModuleDefinition.cs`
- **Verification:** `git diff --check` passed for touched files.

**2. [Rule 1 - Bug] Rejected non-combat modules in Stage1 combat prepared snapshots**
- **Found during:** Task 2 (Update loadout slot rules and tests for non-combat modules)
- **Issue:** `Stage1LoadoutRules.CreatePreparedSnapshot` read combat archetype and ability fields directly. A non-combat module id would have produced default combat fields if it reached that path.
- **Fix:** Added an explicit fail-fast combat module check before preparing the Stage1 combat snapshot.
- **Files modified:** `Assets/Scripts/StaticMlp/Features/Loadout/Runtime/Logic/Domain/Stage1LoadoutRules.cs`, `Assets/Tests/Editor/Loadout/SlotRulesTests.cs`
- **Verification:** `git diff --check` passed for touched files.

---

**Total deviations:** 2 auto-fixed (1 blocking consistency issue, 1 bug), 0 deferred
**Impact on plan:** Both changes keep non-combat modules explicit and prevent invalid combat state. No modifier application systems were added.

## Issues Encountered
- Unity EditMode tests were not run because agents must not launch Unity or run project build checks. Please run `StaticMlp.Tests.Loadout` in Unity EditMode tests.
- Git continues to report pre-existing permission warnings when reading `C:\Users\pavel\.config\git\ignore`; this did not block local diff checks.

## Next Step
Phase 5 complete, ready for `06-01-PLAN.md`.

---
*Phase: 05-stage1-progression-and-loadout*
*Completed: 2026-05-21*
