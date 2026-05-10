# Stage 1 Architecture Boundary Audit

This audit focuses on the current runtime architecture under `Assets/Scripts/StaticMlp/Features`, `Assets/Scripts/StaticMlp/Game`, and `Assets/Scripts/StaticMlp/Composition`, evaluated against the MVP loop in `ai/MVP_plan.md` and the target domain split in `ai/GDD_Frontier_Buildlords.md`.

## Findings

- The project already has a strong composition baseline. `Composition/Runtime/Bootstrap/GameplayFeatureDiscovery.cs` and `MultiplayerSystemBootstrap.cs` give automatic feature discovery, explicit server/client pipelines, and a clean composition root instead of feature registration scattered across startup code.
- The current runtime order respects the intended boundary between transport, replication, gameplay, and presentation. Network apply happens before gameplay, replication collect happens after gameplay, and client view sync is a separate phase.
- `Combat`, `Effects`, and `Statuses` are the clearest existing MVP-ready runtime slice. They already use `Runtime/Logic` and `Runtime/Presentation`, typed network event registration, and server-authoritative gameplay.
- `Buildings` has a good internal shape for future settlement work. It already has explicit domain rules (`ConstructionRules`), request handlers/projectors, client presentation state, and prefab/network adapter registration.
- `BuildingCatalog` is currently data-only and very small. It contains one building definition and separate network/presentation catalogs, which is the right direction, but it is not yet a full settlement domain.
- `ResourcesInventoryMinimal` is not yet a settlement economy module. It seeds owner-only per-player wood and stone inventories, which is useful for prototype construction flow, but not for shared base storage, worker production, or raid recovery.
- `World`, `Build`, and `Progression` are not explicit runtime domains yet. There is no dedicated feature module owning regions, expeditions, captures, outposts, raid pressure, loadout assembly, unlocks, blueprints, or reward routing.
- `Game.Core` still contains feature-specific gameplay contracts, especially `Game/Components/Buildings/*`. That is already a boundary leak. Those contracts belong to the owning feature unless they are genuinely shared by multiple domains.
- The AI stack is reusable only partially. `AiTaskExecution` is narrow enough to stay generic, but `AiBots` and `AiActions` currently mix combat-facing agents with construction-facing behaviors. That will become expensive once workers and enemies diverge.
- `AiActions` is already a catch-all module. It hides ownership of behavior instead of clarifying it. For MVP growth, this is a risk.
- `BuiltinGameplayFeature` and physics-cube demo pieces are bootstrap/demo support, not a good place to grow MVP gameplay.

## Recommended Feature/Module Boundaries

### MVP domain split

- `Settlement` owns base-local gameplay state: buildings, construction, shared storage, worker jobs, production queues, housing/food/security state, and settlement-side defense readiness.
- `Build` owns the bridge from economy to combat: loadout slots, equipped modules, available archetypes, synergy rules, expedition preparation, and the final combat-ready build snapshot.
- `Combat` owns encounter-local simulation: player/enemy combat actors, abilities, attacks, hits, damage, statuses, effects, encounter rules, and combat result output.
- `World` owns map-scale state: regions, capture points, expeditions, outposts, resource streams, raid pressure, threat state, and encounter destination selection.
- `Progression` owns long-lived unlock state: blueprints, research, trophies, unlock flags, reward application, and domain unlock routing.

### What should remain in existing modules

- `Composition/Runtime/Bootstrap/*` should remain the composition root and feature discovery/bootstrap layer.
- `Assets/Scripts/StaticMlp/Networking/*` should remain the only home for transport, raw inbox/outbox, packet delivery, ownership tag application, and replication internals.
- `Features/EcsViews` should remain the shared EntityView binding and view-apply infrastructure.
- `Features/Mvc` should remain shared UI infrastructure only, not feature gameplay.
- `Features/Input` should remain the local input capture/publish feature.
- `Game/Bootstrap/*`, `Game/Replication/*`, `Game/SimulationTime.cs`, and `Game/GameTime.cs` should remain shared bootstrap/runtime infrastructure.
- `Game/Features/Lifecycle` should remain shared lifecycle cleanup.
- `Features/Player` can remain the player/avatar feature for MVP movement and local player camera input.
- `Features/Combat`, `Features/Effects`, and `Features/Statuses` should remain the combat runtime slice. They already align with the current architecture better than the settlement side.

### What should be refactored into clearer ownership

- `Features/Buildings` should become the start of `Settlement`, not a standalone prototype island.
- `Features/BuildingCatalog` should stay, but as settlement-owned data plus explicit adapter catalogs, not as a detached feature with no domain owner.
- `Features/ResourcesInventoryMinimal` should evolve into settlement resource ownership. The current per-player inventory model is not enough for the MVP loop.
- `Game/Components/Buildings/*` should move under the owning settlement feature runtime. `Game.Core` should stop absorbing feature-specific gameplay contracts.
- `AiTaskExecution` may remain shared only if it stays domain-agnostic and contains no building/combat assumptions.

### What should become new feature modules

- `Features/Settlement`
  Owns buildings, construction, storage, worker assignment, production, settlement state, and settlement-side presentation.
- `Features/Build`
  Owns loadout assembly, build slots, module availability, synergy activation, expedition preparation UI/state, and the exported build snapshot consumed by combat.
- `Features/World`
  Owns regions, capture points, outposts, expedition destinations, threat escalation, raid scheduling, and world-facing presentation state.
- `Features/Progression`
  Owns unlock state, blueprints, research, trophies, reward application, and the rules that open new settlement/build/world content.
- `Features/Settlement.Workers` or equivalent
  Owns settlers/workers, job assignment, execution state, and settlement-specific agent behavior.
- `Features/Combat.Enemies` or equivalent
  Owns hostile AI behavior if enemy logic grows beyond the current bot prototype.

### AI module recommendation

- Do not keep `AiActions` as the main extensibility bucket for MVP.
- Split generic agent runtime from domain-owned behaviors.
- Construction delivery/build actions should belong to settlement-owned worker logic.
- Combat attack/chase/flee behaviors should belong to combat/world-owned enemy logic.
- If `AiBots` remains shared, narrow it to generic navigation, task state, and replicated agent presentation only.

### Boundary rules

- Gameplay logic lives in `Runtime/Logic`.
  It owns ECS state, domain rules, validation, authoritative simulation, and typed gameplay events/requests. It may depend on public contracts from other features, but not on foreign systems or UI classes.
- Presentation lives in `Runtime/Presentation`.
  It owns client-only view state, EntityView parts, MVC controllers, previews, local UX state, and rendering glue. It must not own authoritative gameplay mutation or server rules.
- Replication is a boundary layer, not domain logic.
  It owns `[ReplicatedComponent]`, network event registration, request/projector plumbing, `NetArchetypeRegistry`, and client projection. Gameplay asks for these seams; it should not serialize bytes or create transport payloads itself.
- Networking owns byte delivery only.
  Transport systems, raw inbox/outbox access, and packet-level concerns stay in `Networking/*` and never move into feature gameplay systems.
- Composition owns wiring only.
  It creates worlds, registers systems, injects shared resources/services, and controls startup order. It must not become a gameplay rule layer.
- Cross-feature communication must use explicit public contracts.
  Use components, tags, typed events, typed requests, and stable catalogs. Do not introduce hidden cross-feature calls, cross-module orchestrator helpers, or direct system-to-system calls.
- Domain definitions must stay clean.
  Building/loadout/world/progression definitions should not contain prefab paths, `NetworkArchetypeId`, transport state, or concrete UI references. Keep domain, network, and presentation catalogs separate.
- Fail fast on required architecture state.
  Missing required catalogs, resources, controller bindings, or required composition state should throw, not silently no-op.

## Decisions To Lock Now

- Lock `Settlement` as the owner of shared base state. Shared storage must be modeled explicitly and must not be approximated forever with per-player owner-only inventories.
- Lock `Build` as a first-class domain between settlement/world/progression and combat. Combat should consume a finalized build snapshot, not query buildings, unlocks, or UI state directly.
- Lock `World` as the owner of regions, captures, expeditions, outposts, and raid pressure. Do not scatter those rules across settlement and combat.
- Lock `Progression` as the single owner of blueprints, unlocks, research, trophies, and reward application.
- Lock worker architecture now. Workers should be server-owned ECS agents with explicit jobs/contracts, not hidden service queues or UI-driven side effects.
- Lock the AI split now. Enemy AI and worker AI must not continue to grow inside the same mixed feature unless the shared part is purely generic runtime.
- Lock `Game.Core` scope now. No new feature-specific gameplay contracts should be added there unless at least two domains genuinely need them.
- Lock catalog structure now. Each domain should have a clean domain catalog plus explicit network and presentation adapter catalogs where needed.
- Lock authority rules now. Settlement, world, progression, and encounter-authoritative combat state should be server authoritative; clients should send intent, not raw state ownership writes.
- Lock raid modeling now. Base defense should be a combat scenario instantiated from settlement/world state, not a special-case UI mode or a custom transport path.
- Lock demo isolation now. Builtin physics-cube/demo features should not become the foundation for MVP gameplay growth.

## Risks If Delayed

- If storage ownership is delayed, construction, production, workers, loot return, and raid recovery will all be built on incompatible assumptions and require rework.
- If `Build` is not separated before more combat content lands, loadout logic will leak into `Combat`, `Buildings`, and unlock code, making later extraction expensive.
- If `World` is not the owner of captures/outposts/raids, those rules will be duplicated across settlement and combat flows with no stable authority source.
- If AI remains mixed in `AiBots` and `AiActions`, settlement workers will inherit combat dependencies and asmdef coupling that will be painful to unwind.
- If feature contracts keep moving into `Game.Core`, module isolation will collapse and the MVP domains will become a shared-assembly tangle.
- If presentation begins owning more gameplay orchestration, multiplayer debugging and authority reasoning will become much harder.
- If domain catalogs start carrying prefab paths, archetype ids, or UI references, data-driven iteration will slow down and every content change will touch the wrong layer.
