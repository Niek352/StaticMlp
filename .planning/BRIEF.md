# Brief: Settlement + NPC Economy Improvements

## Objective

Turn the current Stage1 camp repair prototype into a small but real settlement economy slice.

The target is not a cosmetic building list. Settlement must support an ECS-first flow where the player selects, places, builds, and operates early base buildings, while NPC workers consume typed settlement contracts instead of hardcoded UI/controller branches.

## Research Source

Primary research:

- `ai/gdd/Settelment + Npc Improvements.md`

Supporting architecture rules:

- `AGENTS.md`
- `Assets/Scripts/StaticMlp/Features/AGENTS.md`
- `ai/ECS_Feature_Architecture_Layout_StaticEcs.md`
- `ai/static_ecs_multiplayer_architecture.md`
- `ai/networked_feature_recipes.md`
- `ai/mvc_usage_guidelines.md`

## Current State

- `Settlement` already owns settlement resource ids, resource families, shared storage, construction state, Stage1 settlement seed data, and Stage1 progression state.
- `Buildings` already owns placement, construction requests, construction lifecycle, network archetype registration, build menu presentation, and construction preview/view state.
- `BuildingCatalog` already provides a code catalog, but it currently only defines `WoodenHut`.
- `Settlement.Workers` already owns settlement worker runtime profiles, assignment requests, camp builder task sync, and construction delivery/build actions.
- `Npc` already owns product-level NPC identity and role flags. It must remain separate from AI behavior and settlement worker runtime execution.
- `Loadout` already has combat slots and reserved slot rules for utility, build signal, and base infrastructure, but settlement-related modules are not filled out yet.
- Stage1 currently advances from camp repair to one worker assignment and then loadout preparation. It does not require stockpile, shelter, extraction, or production to come online.

## Architecture Direction

Evolve the existing feature boundaries instead of creating a new monolithic settlement layer.

Approved direction:

- Keep authoritative settlement state in ECS components under existing feature owners.
- Add typed contracts and domain definitions before adding content.
- Prefer explicit Stage1 fields for replicated resource/storage ledgers over dynamic dictionaries until the replication/codegen path proves a generic representation is safe.
- Use events and requests for cross-feature writes.
- Keep `MonoBehaviour` classes passive and view-only.
- Do not edit `.Generated.cs` files manually.
- Do not create prefab assets, prefab YAML, Unity batchmode scripts, or Canvas hierarchy generation.

Rejected direction:

- Do not add more `if (buildingId == ...)` controller/view branches as the primary scaling path.
- Do not put gameplay truth in `MonoBehaviour`.
- Do not create a parallel `Resources` or `DesignLock` feature.
- Do not make `Npc` own worker movement, AI behavior, station execution, or settlement storage.
- Do not migrate the whole project to ScriptableObject/Addressables in the first pass. That can come after typed runtime contracts are stable.

## Success Criteria

- New Stage1 buildings can be added through catalog definitions and presentation catalog entries without gameplay state in `MonoBehaviour`.
- Settlement storage and construction no longer hardcode only wood/stone at the domain boundary.
- Stockpile, shelter, extraction, and workbench have typed operation/service contracts.
- NPC worker jobs discover typed settlement demand instead of only the camp repair target.
- Stage1 cannot reach loadout preparation until the first settlement economy chain is online.
- UI focus and building actions are driven by typed presentation/action state rather than nearest repair-site assumptions.

