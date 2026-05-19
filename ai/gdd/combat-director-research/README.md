# Combat Director Research Tasks

Source: `ai/gdd/02-combat-director.md`.

Purpose: turn the Combat Director design into scoped code-writing tasks for encounter pressure, spawn, and enemy lifecycle runtime work.

## Architecture Decision

Do not implement a monolithic `CombatDirectorFeature` that owns AI behavior, damage rules, and presentation.

Runtime ownership stays feature-based:

- `StaticMlp.Features.CombatDirector` owns encounter pressure, combat cells, threat budget, director phases, spawn sources, and spawn requests.
- `StaticMlp.Features.AiBots` owns generic AI behavior, navigation, and task execution. Combat Director should not rewrite it unless a task explicitly says so.
- `StaticMlp.Features.Combat` (or the existing damage/health owner) owns health, damage, and combat state components. Combat Director may reference public combat contracts but must not mutate foreign health/damage state directly.
- `StaticMlp.Features.Settlement` owns resource ids and resource families if economy integration is needed later.

This split keeps gameplay, replication, transport, and presentation boundaries intact.

## Task Order

1. `task-1.md` - Combat Director contracts and feature boundary.
2. `task-2.md` - Encounter Director config and Enemy Spawn catalog.
3. `task-3.md` - Combat cell tracking and threat input systems.
4. `task-4.md` - Threat budget accumulation and phase machine.
5. `task-5.md` - Spawn source selection and spawn request build.
6. `task-6.md` - Enemy spawn apply and authoritative entity creation.
7. `task-7.md` - Client presentation, telegraph, and view systems.
8. `task-8.md` - Debug overlay and director monitoring.
9. `task-9.md` - Integration tests and acceptance validation.

## Non-Negotiables For Code-Writing

- Do not create prefab assets.
- Do not edit `.Generated.cs` files manually.
- Do not run `dotnet build`.
- Do not add gameplay state to MonoBehaviours.
- Do not read/write raw networking inbox/outbox from gameplay systems.
- Do not put Unity view paths, prefab paths, VFX, UI, or network archetype ids in pure domain definitions.
- Use typed replicated events for client-to-server requests.
- Mutate replicated state with `Mut<T>()` or `ReplicationMut.Mut<T>()`.
- Use `EntityGID`, not raw `ulong`, in gameplay contracts.
- Add tests for rules and validation, but leave Unity compile/test execution to the Unity workflow.
