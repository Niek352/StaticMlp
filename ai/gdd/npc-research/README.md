# NPC Code Research Tasks

Source: `ai/gdd/01-design-lock.md`.

Purpose: turn the Design Lock into scoped code-writing tasks for NPC-related runtime work.

## Architecture Decision

Do not implement a monolithic `DesignLockFeature`.

The Design Lock describes shared product rules, but runtime ownership should stay feature-based:

- `StaticMlp.Features.Npc` owns general NPC identity, definitions, acquisition state, and captured/recruited roster records.
- `StaticMlp.Features.Settlement` owns resource ids, resource families, settlement storage, and settlement resource validation.
- `StaticMlp.Features.Build` owns equipment/module slots and loadout validation.
- `StaticMlp.Features.Settlement.Workers` remains the current Stage 1 worker implementation and should consume NPC contracts only after the general NPC foundation exists.
- `StaticMlp.Features.AiBots` remains generic AI behavior/execution. NPC tasks should not rewrite it unless a task explicitly says so.

This split keeps gameplay, replication, transport, and presentation boundaries intact.

## Task Order

1. `task-1.md` - NPC contracts and feature boundary.
2. `task-2.md` - Resource families in the existing settlement resource model.
3. `task-3.md` - Build/equipment slot contracts and limits.
4. `task-4.md` - NPC definition catalog and fail-fast validation.
5. `task-5.md` - NPC roster/acquisition records.
6. `task-6.md` - Extraction capture pipeline.
7. `task-7.md` - Rescue/recruit pipeline.
8. `task-8.md` - Incubation contracts and server job skeleton.
9. `task-9.md` - Settlement worker integration with NPC contracts.

## Non-Negotiables For Code-Writing

- Do not create prefab assets.
- Do not edit `.Generated.cs` files manually.
- Do not run `dotnet build`.
- Do not add gameplay state to MonoBehaviours.
- Do not read/write raw networking inbox/outbox from gameplay systems.
- Do not put Unity view paths, prefab paths, VFX, UI, or network archetype ids in pure domain definitions.
- Use typed replicated events for client-to-server requests.
- Mutate replicated state with `ReplicationMut.Mut<T>()`.
- Use `EntityGID`, not raw `ulong`, in gameplay contracts.
- Add tests for rules and validation, but leave Unity compile/test execution to the Unity workflow.

