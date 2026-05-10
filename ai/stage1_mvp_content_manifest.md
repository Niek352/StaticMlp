# Stage 1 MVP Content Manifest

This manifest locks the Stage 1 vertical slice to the smallest content set that fits the current project foundations and avoids premature domain expansion.

The lock is based on:

- `AGENTS.md`
- `ai/MVP_plan.md`
- `ai/GDD_Frontier_Buildlords.md`
- `ai/stage1_architecture_boundary_audit.md`
- `ai/stage1_vertical_slice_loop_spec.md`
- current feature/code and `prefabxml` scan under `Assets/Scripts/StaticMlp` and `Assets/Resources/Views`

## Locked MVP Content Table

| Category | Locked Stage 1 content | Lock rationale |
| --- | --- | --- |
| Buildings | `Camp Core` only. Reuse the current `Wooden Hut` implementation as the single repairable/completable camp anchor. No second building family in Stage 1. | The codebase only has one real building definition and one complete view family. Adding storage, barracks, farm, tower, or crafting stations would force new settlement systems before the loop is proven. |
| Resources | `Wood`, `Stone` only. | These are the only implemented resource fields in runtime state. Do not add food, metal, sulfur, mana, ammo, or population resources in Stage 1. |
| Worker roles | `Camp Builder` only. One role that can deliver materials and perform build work. | Existing AI foundations already support `DeliveryResourceToBuilding` and `BuildConstruction`. Splitting into hauler/builder/gatherer/soldier roles would create new assignment and logistics scope. |
| Build archetypes | `Poison Archer`, `Fire Bomber`. Shared fallback baseline is `BasicMeleeAuto`. | Two distinct combat choices are the minimum needed to prove that base progress changes combat feel. More than two archetypes is wishlist scope at this stage. |
| Build modules | `Poison Arrow Module`, `Fire Flask Module`. `BasicMeleeAuto` stays baseline, not a separate unlock lane. | These map directly to existing combat abilities. Do not add passive trees, relic grids, companion slots, or multi-slot loadout logic in Stage 1. |
| Enemy types | `Raider Grunt`, `Raider Elite`, `Raider Chief`. All are stat/tuning variants on the current monster bot runtime. | The project has one hostile AI family today. Reusing one family across expedition, counterattack, and boss keeps combat content honest and cheap. |
| Regions/objectives | `Home Camp` for repair and defense, `Nearby Raider Camp` for the single expedition clear, `Chief Stand` for the boss encounter. | One home anchor and one external lane are enough. Do not add capture chains, branching regions, outposts, or faction map layers in Stage 1. |
| Rewards | `Recovered War Cache` as the single progression reward package that unlocks final boss prep. `Raider Banner` can exist as end-of-slice completion flavor only, not as a new system lane. | One real reward type is enough to prove return-to-base progression. More reward classes would immediately drag in inventory, unlock routing, and UI complexity. |

## Reuse vs New Work

### Content already partially supported by the current codebase

| Content | Existing support | Notes |
| --- | --- | --- |
| `Camp Core` building | `Features/BuildingCatalog/Runtime/BuildingCatalogData.cs` defines only `WoodenHut`. `Features/Buildings/*` already handles placement, site state, deposit, build work, completion, and building menu presentation. `Assets/Resources/Views/Buildings/WoodenHut*.prefabxml` already exist. | Best Stage 1 move is to relabel/reframe this as the camp core instead of inventing new building content. |
| `Wood` and `Stone` | `Features/ResourcesInventoryMinimal/Runtime/Components/ResourcesInventory.cs` contains only `Wood` and `Stone`. `DepositConstructionResourcesHandler.cs` already spends them into construction. | This is still player-owner inventory, not shared settlement storage. |
| `Camp Builder` worker skeleton | `AiBehaviorIds.PeacefulBuilder`, `AiTaskType.BuildConstruction`, `AiTaskType.DeliveryResourceToBuilding`, `BuildConstructionExecutor.cs`, `DeliveryBuildResourcesExecutor.cs`. | The role exists as AI behavior scaffolding, but not as a proper worker domain. |
| `Poison Archer` and `Fire Bomber` combat seeds | `CombatAbilityId.cs` exposes `BasicMeleeAuto`, `PoisonArrow`, `FireFlask`. `CombatConfig.cs` and `ServerHitToEffectSystem.cs` already route their damage behavior. | This is combat-side support only. Stage 1 still needs a build-selection wrapper. |
| Status/effect presentation layer | `PoisonStatus`, `BurningStatus`, `OiledStatus`, `StatusesConfig.cs`, `ServerSynergyTriggerSystem.cs`, `CombatProjectileView.prefabxml`, `CombatEffectView.prefabxml`, `CharacterView.prefabxml`, `BotCharacterView.prefabxml`. | Important constraint: poison is directly reachable today; burning/oiled synergy plumbing exists, but no current player-facing content actually applies `OiledStatus`. |
| Enemy family seed | `AiBotSpawns.cs`, `ServerAiPerceptionSystem.cs`, `AttackEnemyExecutor.cs`, bootstrap initial bot spawns in `Composition/Runtime/Bootstrap/StaticMlpMultiplayerBootstrap.cs`. | Good enough for grunt/elite/boss stat variants; not good enough for several enemy factions or bespoke bosses. |
| View/prefabxml surface | Existing `prefabxml` content covers the hut building set, building menu, player/bot character views, combat projectile/effect views, and status/combat overlays. | There is no equivalent Stage 1 content surface yet for world map, reward return, progression, or expedition prep UI. |

### Content that requires net-new systems

| Content | Why it is net-new |
| --- | --- |
| Real resource gathering | There are no harvest nodes, gather interactions, hauling rules, or world resource points. `GatherWood` exists only as an enum entry today. |
| Shared camp resource ownership | Current resource state is owner-only player inventory, not a settlement pool. Any true camp economy needs an explicit settlement-owned resource model. |
| Worker assignment and worker identity | The AI can execute builder-like tasks, but there is no worker spawn/setup contract, assignment state, worker UI, or worker-specific authority model. |
| Build/loadout domain | There is no module inventory, slot model, archetype selection flow, or exported build snapshot between settlement and combat. |
| Expedition/world wrapper | Regions, objectives, encounter activation, return flow, counterattack scheduling, and boss unlock state do not exist as owned runtime domains yet. |
| Reward application/progression | There is no authoritative place to apply `Recovered War Cache`, unlock boss prep, or persist slice milestones. |
| Burning/oiled as promised player content | The backend status chain exists, but a real fire/oil build would need an actual oil applicator, module ownership, and player-facing enablement. That is expansion, not free reuse. |

## Smallest Viable Content Set

The smallest viable Stage 1 set that still proves the hypothesis is:

1. One repairable `Camp Core` building, implemented on the current hut pipeline.
2. Two economy resources only: `Wood` and `Stone`.
3. One worker role only: `Camp Builder`.
4. Two player build choices only: `Poison Arrow Module` or `Fire Flask Module`, both on top of shared `BasicMeleeAuto`.
5. One enemy family only: raiders as `Grunt`, `Elite`, and `Chief` stat variants.
6. One external objective lane only: clear `Nearby Raider Camp`, return `Recovered War Cache`, survive one raid, then fight the `Raider Chief`.

This is the minimum set that can answer the real hypothesis:

`Does the camp change what combat build the player can bring, and does combat return meaningful power back into the same camp?`

Anything smaller, especially a single combat build, stops proving the build-creation claim. Anything larger starts building the full game before the loop is validated.

## Dependency Notes

| Must be defined first | Unblocks | Why order matters |
| --- | --- | --- |
| `Camp Core`, `Wood`, `Stone` data contract | construction flow, UI copy, worker tasks, reward spend target | These are the only concrete settlement-side nouns already close to implementation. If they stay fuzzy, every later system invents its own assumptions. |
| Single camp progression chain | worker assignment, build prep, expedition availability, raid trigger, boss unlock | The loop spec only works if the same repaired camp is the authority anchor for all later states. |
| Two module ids and two archetype names | build prep UI/state, combat snapshot export, reward usage | Without this lock, combat ability selection will keep leaking into ad hoc client input instead of becoming build content. |
| One enemy family with three tuned variants | expedition encounter, raid encounter, boss encounter | Reusing one family keeps tuning and view work cheap and prevents premature AI branching. |
| `Recovered War Cache` reward schema | return flow, final prep, boss gating | Reward content must be defined before raid/boss logic, or the loop falls back into disconnected combat wins. |

Implementation should stay in this order:

1. settlement anchor and resource contract
2. camp progression state chain
3. worker assignment on the same camp
4. build snapshot with two module choices
5. expedition wrapper around existing combat
6. reward application into camp state
7. counterattack trigger
8. boss unlock and boss encounter

## Scope Risks

- Adding a second real building family in Stage 1 forces shared storage, unlock routing, more UI states, and more build dependencies immediately.
- Adding any resource beyond `Wood` and `Stone` forces node content, gathering logic, storage semantics, balance work, and presentation debt.
- Splitting workers into separate gatherer/hauler/builder/combat roles forces job assignment, job priority, and inventory ownership systems that do not exist yet.
- Promising burning/oiled as a core archetype now is misleading. The backend status chain exists, but there is no current player-facing oil application content.
- Adding multiple regions, capture points, or outposts before one expedition lane works will force the `World` domain to expand before the slice is validated.
- Making the boss mechanically unique instead of a tuned monster-family variant will pull in bespoke AI and encounter scripting too early.
- Building reward variety before the return loop is wired will create a fake progression surface with no authoritative owner.
- Expanding `prefabxml` content broadly for settlement/world/progression UI before the runtime states exist will produce presentational dead ends.
