# StaticMlp Planning Brief

Source documents:
- `docs/combined_gdd.md`
- `docs/Roadmap по GDD.md`

## Vision

StaticMlp is an attack-to-gather base-building roguelite. The core loop is:

```text
Attack the world to harvest resources
Build and upgrade a settlement
Recruit companions
Unlock items and assemble a build
Defeat the biome boss
Unlock the next biome
```

Phase 1 focuses only on the first playable combat and gathering slice:

- third-person or isometric movement with active aiming;
- enemies and static resource placements targetable during combat;
- resource nodes taking attack damage through the open-world overlay model;
- small carried raw-material inventory and magnetic pickups;
- simple enemy pressure with swarm and elite archetypes.

## Current Repository Baseline

- Player movement already exists in `StaticMlp.Features.Player`.
- Combat already has passive auto-attack, ability commands, server validation, damage effects, health, death marking, and presentation feedback.
- OpenWorldResources already owns deterministic placement indexing, chunk overlay state, overlay network events, and client-only resource proxy views.
- ResourcesInventoryMinimal exists, but it is a two-resource minimal component and is not yet the GDD raw-material slot inventory.
- AiBots, AiActions, AiTaskExecution, AiNavigation, and CombatDirector already provide the enemy spawn/AI foundation.

## Non-Negotiable Architecture

- Combat may validate attacks and emit combat facts, but it must not mutate OpenWorldResources overlay state directly.
- OpenWorldResources owns resource placement ids, resource HP/amount overlay state, depletion, and resource-specific reactions.
- Inventory/pickup code must use `StaticMlp.Features.Settlement.Contracts` resource ids/catalog contracts. Do not create a parallel resource id catalog.
- Static resource placements remain deterministic placements keyed by `PlacementId`; do not convert ordinary trees/rocks/ores into replicated `NetworkIdentity` entities.
- Presentation and Unity-facing view code stay in `Runtime/Presentation`; gameplay mutation and validation stay in `Runtime/Logic`.
- Replicated command/component changes require source contract changes and the project's replication generation workflow. Do not manually edit `.Generated.cs` files.
- Agents must not run `dotnet build`, create prefabs, generate prefab YAML, or launch Unity BatchMode. Ask the user to run Unity compile/play checks where needed.

