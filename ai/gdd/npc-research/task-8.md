# Task 8 - Incubation Contracts And Server Job Skeleton

## Goal

Add the minimal ECS contracts for incubation without pretending the full station/economy pipeline already exists.

Design Lock requires:

```text
egg/core/biome heart/fragment + station job + time/resources/station tier -> new companion/specialist
```

This task should create the incubation job shape and validation rules. Full resource spending and station production can be integrated later when the economy/station APIs are ready.

## Dependencies

Requires:

- `task-1.md`
- `task-2.md`
- `task-4.md`
- `task-5.md`

## Architecture

Incubation belongs to the NPC feature, but it reads settlement resource and station contracts.

Do not put station tier, resource storage, or recipe unlock ownership inside `Npc` if those are owned by settlement/economy/progression features. Use events or explicit public contracts.

Use `SimulationTime.ServerTick` for authoritative job deadlines.

## Code Scope

Add definitions:

```text
NpcIncubationRecipeId
NpcIncubationRecipeDefinition
NpcIncubationRecipeCatalog
NpcIncubationRecipeCatalogValidator
```

Suggested recipe fields:

```text
NpcIncubationRecipeId Id
NpcDefinitionId ResultNpc
ResourceAmount[] Costs
byte RequiredStationTier
float DurationSeconds
```

If arrays are not suitable for replicated fields, keep them in domain definitions only. Do not replicate recipe definitions.

Add job state:

```text
NpcIncubationJobState
```

Suggested fields:

```text
ushort RecipeId
NpcRosterState State
uint StartedAtServerTick
uint CompletesAtServerTick
```

Add internal events:

```text
NpcIncubationJobStartedEvent
NpcIncubationJobCompletedEvent
```

Add systems only as a skeleton if all required dependencies are available:

```text
ServerNpcIncubationJobCompleteSystem
```

This system may complete existing valid jobs and create roster records. Starting jobs from player commands can be a later task if settlement resource spending/station validation is not ready.

## Validation Scope

Fail fast on catalog/config errors:

- duplicate recipe ids
- missing result NPC definition
- result NPC definition does not allow `Incubation`
- empty cost list if the recipe is intended to cost resources
- zero/negative duration
- zero station tier if tiers are required
- missing resource ids in `ResourceCatalog`

Do not silently allow missing station/resource dependencies.

## Out Of Scope

- Full station job queue.
- Player start-incubation UI.
- Resource spending apply step unless a clean settlement resource API already exists.
- Incubation VFX.
- Spawned companion actor.

## Tests

Cover:

- current recipe catalog validates
- invalid result NPC fails
- invalid resource id fails
- completing a ready job creates a roster record if the job system is implemented
- incomplete job does not complete before `CompletesAtServerTick`

## Acceptance Criteria

- Incubation has explicit domain contracts.
- Server time is tick-based.
- No fake fallback station/resource behavior is introduced.
- Full economy/station integration remains a clear follow-up, not hidden in this task.

