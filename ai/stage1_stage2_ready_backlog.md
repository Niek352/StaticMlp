# Stage 1 -> Stage 2 Ready Backlog

This backlog turns the Stage 1 decisions into the next implementation sequence for the current project layout under `Assets/Scripts/StaticMlp`.

The ordering below treats the Stage 1 architecture decisions as hard constraints:

- `Settlement` owns camp-local authoritative state.
- `Build` owns prepared combat build state.
- `World` owns expeditions, raids, and encounter activation.
- `Progression` owns rewards, unlocks, and long-lived flags.
- `Combat` consumes a prepared build snapshot; it does not query settlement or UI state directly.
- bootstrap owns wiring and seed injection only.
- AI workers and combat enemies must not keep growing inside the same mixed feature boundary.

## Ordered backlog

### Sequence 1. Freeze contracts before feature expansion

#### 1. Stable ids and catalog cleanup

- Goal: freeze the typed content ids and code-first catalog pattern that Stage 2+ work will build on, and remove the remaining raw-content-id assumptions from gameplay-facing contracts.
- Owning feature/module: existing `Features/BuildingCatalog`, plus new `Features/Settlement/Runtime/Logic/Data`, `Features/Build/Runtime/Logic/Data`, `Features/World/Runtime/Logic/Data`, and `Features/Progression/Runtime/Logic/Data`.
- Prerequisites: approved Stage 1 docs only.
- Architectural dependency: this must land first because later replicated contracts, seed manifests, and cross-feature gates need stable ids and domain-owned catalogs to avoid rewrites.
- Expected output artifact: typed ids for `ResourceId`, `WorkerRoleId`, `BuildModuleId`, `BuildArchetypeId`, `RegionId`, `ExpeditionId`, `RewardPackageId`, `RaidId`, `ProgressFlagId`, and `EncounterProfileId`; cleaned domain definitions; `All/Get/TryGet` catalogs; `BuildingDefinition` no longer carrying presentation-only fields as the long-term pattern.

#### 2. Stage seed extraction from bootstrap

- Goal: move hardcoded Stage 1 slice setup out of `StaticMlpMultiplayerBootstrap` and into owned seed/manifests.
- Owning feature/module: `Composition/Runtime/Bootstrap` plus new seed resources in `Settlement`, `World`, and `Progression`.
- Prerequisites: item 1.
- Architectural dependency: bootstrap must stop owning gameplay constants before more slice state is added, or the composition root becomes the hidden owner of settlement/world/progression rules.
- Expected output artifact: code-first `Stage1SettlementSeed`, `Stage1WorldSeed`, and `Stage1ProgressionSeed` resources; bootstrap updated to inject seeds instead of defining initial construction sites, bots, and progression literals inline.

#### 3. Move building gameplay contracts out of `Game.Core`

- Goal: stop the `Game/Components/Buildings/*` area from remaining the de facto owner of settlement gameplay contracts.
- Owning feature/module: transitional work in `Features/Buildings`, with target ownership under `Features/Settlement/Runtime/Logic`.
- Prerequisites: item 1.
- Architectural dependency: this must happen before camp progression, worker logic, or reward routing starts depending on building state, otherwise `Game.Core` becomes a long-term feature bucket.
- Expected output artifact: settlement-owned locations for `ConstructionSiteState`, `ConstructionResources`, `ConstructionProgress`, `ConstructionPhase`, `ConstructionTransform`, and related tags; updated asmdef references.

### Sequence 2. Establish the settlement authority baseline

#### 4. Introduce the `Settlement` feature shell and camp anchor

- Goal: create the authoritative camp owner that later build prep, reward return, raid scheduling, and boss gating can all point to.
- Owning feature/module: new `Features/Settlement/Runtime/Logic`.
- Prerequisites: items 1-3.
- Architectural dependency: this is the first real domain owner in the MVP loop and must exist before shared storage, worker assignment, or world/progression gates are added.
- Expected output artifact: `StaticMlp.Features.Settlement` asmdef(s), settlement gameplay feature registration, camp anchor component/resource contracts, and one authoritative settlement summary contract.

#### 5. Replace owner-only prototype inventory with shared settlement resources

- Goal: replace `ResourcesInventoryMinimal` as the gameplay truth for camp progress with a settlement-owned shared resource store keyed by `ResourceId`.
- Owning feature/module: new `Features/Settlement/Runtime/Logic`, with updates in `Features/Buildings`; `ResourcesInventoryMinimal` becomes legacy or compatibility-only.
- Prerequisites: items 1, 3, and 4.
- Architectural dependency: this must happen before worker production, reward return, base HUD summaries, and construction all rely on the wrong inventory model.
- Expected output artifact: settlement resource storage contract, seed/init system, updated construction deposit flow, and an explicit migration path away from `ResourcesInventory.Wood` and `ResourcesInventory.Stone` as the loop authority.

#### 6. Add the explicit camp progression chain and active objective state

- Goal: encode the Stage 1 forward-only loop state around the same camp anchor instead of letting UI flow and feature-local booleans imply progression.
- Owning feature/module: `Features/Settlement/Runtime/Logic`, with public contracts consumable by `Build`, `World`, and `Progression`.
- Prerequisites: items 4 and 5.
- Architectural dependency: this must remain sequential with the settlement baseline because expedition availability, build readiness, reward return, and threat escalation all depend on one camp-owned chain.
- Expected output artifact: stable progression flags or settlement milestone contracts for at least `DamagedCampStart`, `RepairObjectiveActive`, `RepairResourcesReady`, `CampRepaired`, `WorkerAssigned`, and `BuildPrepared`; active objective summary resource/read model.

### Sequence 3. Add the thin vertical-slice bridges without collapsing boundaries

#### 7. Introduce `Settlement.Workers` worker identity and assignment contracts

- Goal: promote the existing builder-like AI path into a real worker domain with one Stage 1 role: `CampBuilder`.
- Owning feature/module: new `Features/Settlement.Workers/Runtime/Logic`.
- Prerequisites: items 2, 4, 5, and 6.
- Architectural dependency: worker assignment must depend on the camp anchor and settlement resource truth, not on bootstrap bot spawns or mixed AI assumptions.
- Expected output artifact: worker role ids/catalog, worker entity seed/setup, assignment events/components, camp job slot or demand contracts, and one authoritative worker summary contract.

#### 8. Split worker behavior ownership out of the mixed AI feature surface

- Goal: keep `AiTaskExecution` generic while moving worker-owned behavior selection and mappings under settlement worker ownership.
- Owning feature/module: `Features/Settlement.Workers`, with targeted edits to `Features/AiBots`, `Features/AiActions`, and `Features/AiTaskExecution`.
- Prerequisites: item 7.
- Architectural dependency: this should happen before more worker jobs or more enemy variants are added, otherwise `AiActions` and shared AI catalogs keep accumulating unrelated domain rules.
- Expected output artifact: a worker-owned behavior/profile mapping layer, narrowed shared AI runtime contracts, and clear separation between worker job execution and enemy combat behavior ownership.

#### 9. Introduce the `Build` feature and prepared build snapshot

- Goal: create the thin Stage 1 build domain that turns camp state into one prepared combat build without letting combat or UI own selection rules.
- Owning feature/module: new `Features/Build/Runtime/Logic`.
- Prerequisites: items 1, 2, 4, and 6.
- Architectural dependency: expedition commit and boss prep must depend on a stable exported build snapshot, not on direct reads of building completion, progression flags, or client-side ability selection.
- Expected output artifact: `BuildModuleId` and `BuildArchetypeId` catalogs, build availability rules, authoritative selected/prepared build state, and a combat-consumable `PreparedBuildSnapshot` contract that maps to the existing `CombatAbilityId` set.

### Sequence 4. Add world and progression ownership around the existing combat slice

#### 10. Introduce the `World` feature for expeditions, threat, and raids

- Goal: make one explicit owner for expedition availability, active encounter state, raid scheduling, and threat progression.
- Owning feature/module: new `Features/World/Runtime/Logic`.
- Prerequisites: items 1, 2, 4, 6, and 9.
- Architectural dependency: expedition activation must come from `World`, not from settlement UI/controllers or combat bootstrap shortcuts.
- Expected output artifact: region/expedition/raid catalogs, active expedition state, threat summary state, and encounter activation contracts that wrap the existing combat runtime.

#### 11. Introduce the `Progression` feature for reward application and unlock routing

- Goal: make one authoritative place to apply `RecoveredWarCache`, raise unlock flags, and expose downstream effects to settlement/build/world.
- Owning feature/module: new `Features/Progression/Runtime/Logic`.
- Prerequisites: items 1, 2, 4, 6, and 9.
- Architectural dependency: rewards must be applied into persistent progression state before the UI or `World` promises new readiness, raids, or boss access.
- Expected output artifact: reward package catalog, progress flag catalog, reward application systems/contracts, and explicit unlock-routing outputs for settlement/build/world consumers.

#### 12. Wire expedition completion into reward return and threat escalation

- Goal: connect the expedition result to persistent progression writes and the next camp/world state instead of leaving combat victory as an isolated output.
- Owning feature/module: public contracts between `World`, `Progression`, `Settlement`, and `Build`.
- Prerequisites: items 9-11.
- Architectural dependency: this is the first unavoidable cross-domain integration point and must stay sequential because reward application order determines raid gating and boss prep legality.
- Expected output artifact: expedition completion event/result contract, reward application trigger, threat escalation trigger, and updated summary read-model inputs for the camp loop.

### Sequence 5. Close the Stage 1 loop with raid and boss wrappers

#### 13. Implement the counterattack/raid activation contract

- Goal: instantiate the Stage 1 raid against the same camp anchor as a `World`-owned combat scenario with a stable defense outcome contract.
- Owning feature/module: `Features/World/Runtime/Logic` and existing `Features/Combat`, with settlement summary values as inputs.
- Prerequisites: items 7, 10, 11, and 12.
- Architectural dependency: raid activation depends on prior reward application and world-owned threat state; it cannot be safely built as a parallel side path.
- Expected output artifact: raid activation state, raid encounter wrapper, defense success/failure result contract, and settlement/world progression updates for `CampDefended`.

#### 14. Implement the boss prep gate and boss encounter wrapper

- Goal: gate the final preparation spend and boss encounter through the existing `Build`, `World`, and `Progression` seams instead of special-case combat launch code.
- Owning feature/module: `Features/Build`, `Features/Progression`, `Features/World`, and existing `Features/Combat`.
- Prerequisites: items 12 and 13.
- Architectural dependency: this must remain last in the gameplay chain because boss access is defined as a consequence of expedition success plus raid survival.
- Expected output artifact: boss-prep spend contract, boss unlock flag/state, boss encounter definition/wrapper, and `VerticalSliceComplete` completion contract.

### Sequence 6. Layer presentation on top of stable gameplay owners

#### 15. Build the Stage 1 read models and MVC surfaces

- Goal: implement the locked HUD/panel/screens only after the authoritative contracts exist, so presentation reflects the real domain owners instead of inventing them.
- Owning feature/module: `Settlement.Runtime.Presentation`, `Settlement.Workers.Runtime.Presentation`, `Build.Runtime.Presentation`, `World.Runtime.Presentation`, and `Progression.Runtime.Presentation`.
- Prerequisites: item 6 for camp overview, item 7 for worker UI, item 9 for build prep UI, items 10-12 for expedition/reward/threat UI, and items 13-14 for late-loop messaging.
- Architectural dependency: presentation can branch by feature once the underlying read-model contracts are frozen, but it must not lead the gameplay architecture.
- Expected output artifact: persistent base HUD read model and controller, focused context-panel read model, build prep screen/controller, expedition selection screen/controller, reward result panel, and threat banner logic. No prefab authoring is included here; this task stops at code, read models, and controller/view contracts for manual Unity wiring.

#### 16. Remove remaining prototype shortcuts from the gameplay path

- Goal: retire or isolate the prototype seams that would otherwise keep fighting the new architecture.
- Owning feature/module: cross-cutting cleanup in `Composition`, `Buildings`, `Combat`, `ResourcesInventoryMinimal`, and newly introduced features.
- Prerequisites: replacement paths from items 5, 9, and 15.
- Architectural dependency: this should happen after the replacement seams exist so cleanup does not block feature implementation, but it should happen before broader Stage 2 content growth.
- Expected output artifact: gameplay path no longer depending on owner-only `ResourcesInventoryMinimal`, direct bootstrap content literals, debug composition UI as gameplay UI, or ad hoc combat ability selection as the build-selection owner.

## Parallelization notes

### Safe to parallelize

- After item 1, items 2 and 3 can run in parallel.
- After item 6, item 7 and item 9 can run in parallel because worker assignment and build snapshoting share the same camp progression baseline but do not own each other's rules.
- After item 7 starts, item 8 can run as a focused cleanup track for AI ownership while item 9 continues independently.
- After item 9, items 10 and 11 can run in parallel if the shared `ProgressFlagId` and reward/build/world catalog contracts from item 1 are already frozen.
- Item 15 can be split by feature once each logic owner has produced stable read-model contracts. `Settlement` HUD work, `Build` prep UI, and `World` threat/expedition UI do not need to be one serial Codex task.
- Item 16 is a cleanup track that can overlap with late presentation work after the replacement architecture is in place.

### Must stay sequential

- Item 1 must happen before any new domain catalog, seed manifest, or replicated contract work.
- Item 4 must happen before shared settlement resources, worker assignment, and camp progression because those systems need one camp authority source.
- Item 5 must happen before any task that treats construction cost, worker production, reward return, or HUD summaries as shared camp truth.
- Item 6 must happen before expedition readiness, raid gating, and boss unlock logic because Stage 1 locked one explicit forward-only chain.
- Item 12 must stay after items 10 and 11 because the order of expedition completion, reward application, and threat escalation is itself part of the architecture.
- Item 13 must stay after item 12 because the raid is defined as a consequence of the expedition reward return.
- Item 14 must stay after item 13 because boss access is defined as a consequence of surviving the counterattack.

## Critical path

The minimum architecture-safe path is:

1. Item 1. Stable ids and catalog cleanup
2. Item 4. `Settlement` feature shell and camp anchor
3. Item 5. Shared settlement resources
4. Item 6. Camp progression chain and active objective
5. Item 9. `Build` feature and prepared build snapshot
6. Item 10. `World` feature for expeditions/threat/raids
7. Item 11. `Progression` feature for reward application/unlocks
8. Item 12. Expedition completion -> reward return -> threat escalation
9. Item 13. Counterattack/raid activation
10. Item 14. Boss prep gate and boss encounter wrapper
11. Item 15. Presentation on top of the stable contracts

Items 2, 3, 7, 8, and 16 matter, but they are supporting tracks around that core sequence rather than the main dependency spine.

## First implementation tasks

These are the first tasks that should be handed to Codex after Stage 1 approval:

1. Item 1. Stable ids and catalog cleanup.
   This is the highest-leverage task because every later feature needs the ids, catalogs, and domain/adapter split to stay stable.

2. Item 2. Stage seed extraction from bootstrap.
   This removes the current gameplay-literal ownership from `StaticMlpMultiplayerBootstrap` before more slice state gets embedded there.

3. Item 3. Move building gameplay contracts out of `Game.Core`.
   This stops the most obvious existing boundary leak before settlement/world/progression start depending on those contracts.

4. Item 4. Introduce the `Settlement` feature shell and camp anchor.
   This gives Stage 2 one authoritative owner for camp-local state and unblocks shared resources, workers, and objective state.

5. Item 5. Replace owner-only prototype inventory with shared settlement resources.
   This is the first true gameplay foundation task for the MVP loop because construction, worker output, reward return, and HUD summaries all need the same camp resource truth.

If you want the backlog converted into Codex-sized implementation tickets next, the clean split is:

- Ticket A: items 1-2
- Ticket B: items 3-4
- Ticket C: item 5
- Ticket D: item 6
- Ticket E: items 7-8 or item 9, depending on whether you want workers or build prep to branch first after the settlement baseline
