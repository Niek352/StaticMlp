# Phase 1: Core Combat and Gathering Summary

**Combat can now describe actor and static-placement targets while OpenWorldResources publishes targetable proxy read models.**

## Current State

- `01-01-PLAN.md` planned: combat target contracts and resource target read models.
- `01-02-PLAN.md` planned: active target selection and server validation.
- `01-03-PLAN.md` planned: attack-to-gather overlay mutation.
- `01-04-PLAN.md` planned: raw inventory and magnetic pickup flow.
- `01-05-PLAN.md` planned: resource destruction hazards and feedback.
- `01-06-PLAN.md` planned: enemy swarm/elite baseline and phase integration verification.

## Completed Work

- Planning structure initialized under `.planning/`.
- Phase 1 split into six atomic execution plans.
- Architecture decision recorded: resources are attacked through combat target facts, but OpenWorldResources remains the only owner of resource overlay mutation.
- `01-01-PLAN.md` completed: combat target refs, combat hit facts, and OpenWorldResources target read models are in source.

## Implementation Progress

| Plan | Status | Summary |
| --- | --- | --- |
| `01-01-PLAN.md` | Done | `01-01-SUMMARY.md` |
| `01-02-PLAN.md` | Not started | Pending |
| `01-03-PLAN.md` | Not started | Pending |
| `01-04-PLAN.md` | Not started | Pending |
| `01-05-PLAN.md` | Not started | Pending |
| `01-06-PLAN.md` | Not started | Pending |

## Update Rule

After each plan is executed:

1. Create that plan's required `01-0X-SUMMARY.md`.
2. Update this file's table row from `Not started` to `Done` or `Blocked`.
3. Add a short bullet under Completed Work describing what shipped.
4. Record blockers under Open Issues if execution cannot continue.

## Open Issues

- Unity replication/codegen and compile checks are pending after the `UseAbilityCommand` and `PassiveAutoAttackRequestEvent` source contract changes.

---
*Phase: 01-core-combat-and-gathering*
*Last updated: 2026-05-26*
