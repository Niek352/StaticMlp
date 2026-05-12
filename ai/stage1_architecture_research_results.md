# Stage1 Architecture Research Results

Source plan: `ai/Research.md`

This audit found that the current Stage1 bugs are not isolated defects. They come from one deeper issue: camp-global state, per-building state, UI read models, and replicated spawn lifecycle are sharing the same transient entities and ad-hoc initialization paths.

## Decisions

1. Stage1 camp-global authority must move to a dedicated server-owned replicated camp anchor entity.
2. Critical replicated state must be present in the initial spawn snapshot of the entity that owns it.
3. Stage1 flow transitions must be owned by one domain flow owner instead of being advanced opportunistically by Settlement, Workers, Build, Frontier, and Progression systems.
4. User-facing construction intent must be normalized before it reaches networking requests.
5. Regression tests that claim runtime behavior must use shared runtime bootstrap and real spawn/apply lifecycle.
6. Presentation systems may build client-only view state, but must not invent gameplay gates or hardcode ability/domain semantics.
7. `SpawnServerEntity(..., Action<SW.Entity>)` must be treated as a low-level replication primitive, not as a feature-facing factory API.
8. Feature-owned components must not be mutated by other features except through explicit contracts owned by the component owner.

## A. Stable Authoritative Anchor Model

### Status

- Done: added a dedicated `Stage1CampAnchorNetworkEntity` and one-time server spawn system for camp anchors.
- Done: moved `Stage1SettlementProgression` off construction-site and finished-building network manifests.
- Done: added replicated `SettlementAnchorRef` for construction/finished building entities.
- Done: server repair progression, construction completion, client repair focus lookup, and raid origin lookup now resolve through the stable anchor instead of treating the building entity as the camp.
- Done: the spawned anchor is initialized with Stage1 camp-global runtime state currently used by Settlement, Workers, Build, Frontier, and Progression systems.
- Done: `Stage1ProgressionState` is now part of the generated replicated component contract and the camp anchor manifest.
- Done: removed lazy anchor init systems for Frontier, Build boss preparation, and Progression state.
- Not done: Stage1 flow ownership is still split across existing feature systems; that is part of Stage C.
- Not done: runtime parity tests have not yet been converted to prove spawn/apply/despawn behavior through the real replication loop.

### Problem

`Stage1SettlementProgression` is treated as the camp anchor, but it is attached to construction/building entities:

- `ConstructionSiteNetworkEntity` includes `Stage1SettlementProgression` in its manifest: `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/NetworkEntityTypes/ConstructionSiteNetworkEntity.cs:7`.
- `FinishedBuildingNetworkEntity` also includes it: `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/NetworkEntityTypes/FinishedBuildingNetworkEntity.cs:7`.
- Initial repair site setup writes the progression component onto the construction site: `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Server/ServerInitialConstructionSiteSpawnSystem.cs:47`.
- Completion copies progression from the site to the finished building, then despawns the site: `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Server/ServerCompleteConstructionSystem.cs:41` and `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Server/ServerCompleteConstructionSystem.cs:59`.
- Anchor lookup scans any entity with `Stage1SettlementProgression`: `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Stage1SettlementProgressionQuery.cs:20`.

This makes the authoritative identity unstable. The "camp" changes ECS entity during repair completion, exactly across spawn/despawn and initial-state replication boundaries.

### Target Model

Create a dedicated `SettlementAnchorNetworkEntity` or `Stage1CampAnchorNetworkEntity`.

It should own camp-global replicated components:

- `Stage1SettlementProgression`
- `Stage1ProgressionState`
- `SettlementWorkerSummary`
- `SettlementCampBuilderJobState`
- `ExpeditionAvailabilityState`
- `ActiveExpeditionState`
- `ThreatState`
- `RaidScheduleState`
- `BossEncounterState`
- `BossBuildPreparationState`
- `BossPreparedBuildSnapshot`
- a new anchor location component, for example `SettlementAnchorLocation`, if raid/encounter origin needs a stable camp position

Construction sites and finished buildings should not own Stage1 camp progression. They should carry a small reference such as `SettlementAnchorRef` plus construction/building state.

### Migration List

Move these off construction/building entities:

- `Stage1SettlementProgression`
- `Stage1ProgressionState`
- `SettlementWorkerSummary`
- `SettlementCampBuilderJobState`
- `ExpeditionAvailabilityState`
- `ActiveExpeditionState`
- `ThreatState`
- `RaidScheduleState`
- `BossEncounterState`
- `BossBuildPreparationState`
- `BossPreparedBuildSnapshot`

Keep these on construction/building entities:

- `ConstructionSiteState`
- `ConstructionTransform`
- `ConstructionResources`
- `ConstructionProgress`
- `ConstructionSiteTag`
- `FinishedBuildingTag`
- `BuildingFootprint`
- `SettlementAnchorRef`

## B. Spawn/Despawn Contract

### Status

- Done: removed the late anchor init systems called out in this section (`ServerFrontierAnchorInitSystem`, `ServerBossBuildPreparationAnchorInitSystem`, and `ServerStage1ProgressionAnchorInitSystem`).
- Done: `ServerStage1CampAnchorSpawnSystem` now creates the Stage1 camp anchor through `NetworkEntitySpawner.SpawnServerEntity(...)` and initializes the replicated camp state inside the spawn initializer before `SpawnBroadcaster.SendSpawn`.
- Done: `ConstructionSiteNetworkEntity` and `FinishedBuildingNetworkEntity` no longer declare `Stage1SettlementProgression` in their manifests, so camp-global progression is no longer part of the building spawn contract.
- Done: `NetworkEntitySpawner` now validates after `initialize` that every replicated component declared by `NetworkEntityManifest` is both registered and present before broadcast.
- Done: `ServerSettlementWorkerCampBuilderJobSystem` and `ServerSettlementWorkerTaskSyncSystem` now treat `SettlementCampBuilderJobState` and `SettlementWorkerSummary` as required anchor state and mutate/read them directly instead of late-attaching or guarding them as optional.

### Problem

The spawner itself supports correct initial state by invoking `initialize` before `SpawnBroadcaster.SendSpawn`: `Assets/Scripts/StaticMlp/Game/Replication/NetworkEntitySpawner.cs:35` and `Assets/Scripts/StaticMlp/Game/Replication/NetworkEntitySpawner.cs:38`.

The architecture still allows unsafe late attachment:

- `ServerFrontierAnchorInitSystem` adds critical replicated state lazily with `Has` checks: `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Logic/ServerFrontierAnchorInitSystem.cs:13`.
- `ServerBossBuildPreparationAnchorInitSystem` does the same: `Assets/Scripts/StaticMlp/Features/Build/Runtime/Logic/Systems/Server/ServerBossBuildPreparationAnchorInitSystem.cs:13`.
- `ServerStage1ProgressionAnchorInitSystem` adds `Stage1ProgressionState` after finding the pseudo-anchor: `Assets/Scripts/StaticMlp/Features/Progression/Runtime/Logic/ServerStage1ProgressionAnchorInitSystem.cs:12`.
- Worker job state/summary can be attached during ordinary gameplay: `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Systems/Server/ServerSettlementWorkerCampBuilderJobSystem.cs:217`.

Because `SpawnBroadcaster.CreateSpawn` collects current state once: `Assets/Scripts/StaticMlp/Game/Replication/SpawnBroadcaster.cs:47`, late-added critical components become deltas rather than spawn contract. That is fragile for ordering, late joins, and tests that do not run the real packet lifecycle.

### Contract

Critical replicated state is illegal to attach after `NetworkEntitySpawner.SpawnServerEntity(...)` for a newly created authoritative entity.

Allowed after spawn:

- ordinary mutable value changes through `ReplicationMut.Mut<T>()`;
- optional gameplay states whose absence is a valid game state;
- client-only view/presentation components.

Illegal after spawn:

- identity/state components required for lookup;
- camp-global flow/progression state;
- components required by registered systems to run without `Has<T>()` guards;
- components that the client UI assumes are part of the replicated read model.

### Guardrails

- Replace anchor init systems with one `Stage1CampAnchorSpawner` that builds the full initial replicated component set inside the spawn initializer.
- Remove `Stage1SettlementProgression` from building network entity manifests.
- Add a generated/runtime validation in `NetworkEntitySpawner`: after `initialize`, verify the spawned network entity has every component declared in its `NetworkEntityManifest`. This should fail fast before broadcast.
- Add a code review rule: if a server system calls `Set(...)` for a replicated component on a networked entity after spawn, it must be either optional-by-design or converted to spawn initialization.

## C. Stage1 Flow Ownership And Gating

### Status

- Done: introduced a dedicated Stage1 flow owner in `Features/Stage1/Runtime/Logic` as the only code path that mutates `Stage1SettlementProgression.Stage`.
- Done: `ServerCompleteConstructionSystem`, `SetSettlementWorkerAssignmentHandler`, and `ServerReceivePrepareBuildCommandSystem` now publish typed Stage1 flow facts instead of mutating progression directly.
- Done: `ServerFrontierExpeditionAvailabilitySystem` no longer advances Stage1 progression and now only computes frontier availability from replicated state.
- Done: added replicated `Stage1FlowViewState` on the camp anchor and switched HUD/build/context-panel gating to that owner-authored read model.
- Done: added flow-owner server tests for automatic repair stages, repair completion, worker assignment, build preparation, and the negative case that frontier availability no longer advances the stage.
- Not done: `PrepareBuildCommand` still has no explicit `AnchorId`; the build-prepared fact currently targets `SettlementAnchorCatalog.HomeCampId` and remains single-camp specific.
- Not done: `ClientStage1ContextPanelSessionSystem` still chooses repair/building focus from `Stage1SettlementProgression` plus site lookup instead of a dedicated repair-focus read model.

### Problem

The Stage1 sequence is encoded across unrelated systems:

- repair start/resources readiness: `ServerStage1SettlementProgressionSystem` in Settlement, `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/ServerStage1SettlementProgressionSystem.cs:17`;
- camp repair completion: Buildings, `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Server/ServerCompleteConstructionSystem.cs:46`;
- worker assignment and `WorkerAssigned`: Workers, `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Systems/Server/ServerSettlementWorkerCampBuilderJobSystem.cs:25`;
- `BuildPrepared`: Frontier, `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Logic/ServerFrontierExpeditionAvailabilitySystem.cs:34`;
- HUD objective/gates: Presentation, `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/ClientStage1HudStateSystem.cs:101`;
- Build screen availability opens at `CampRepaired`, not `WorkerAssigned`: `Assets/Scripts/StaticMlp/Features/Build/Runtime/Presentation/ClientBuildPreparationScreenStateSystem.cs:24`;
- Expedition start requires `BuildPrepared`: `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Logic/ServerFrontierStartExpeditionSystem.cs:47`.

This explains the contradictory player flow: UI can expose build preparation before the domain flow says worker assignment is complete.

### Gate Ownership Matrix

| User-visible step | Gameplay truth owner | Current conflicting owners | Target rule |
| --- | --- | --- | --- |
| Repair camp resources ready | Stage1 flow owner observes construction/storage state | Settlement | Flow owner advances from `RepairObjectiveActive` to `RepairResourcesReady`. |
| Camp repaired | Stage1 flow owner observes linked repair site completion event | Buildings mutates progression | Buildings emits construction completed event; flow owner advances. |
| Worker assigned | Stage1 flow owner observes worker assignment accepted event/state | Workers mutates progression | Workers owns worker assignment; flow owner owns progression transition. |
| Build prepared | Stage1 flow owner observes accepted build selection/preparation | Frontier advances progression | Build owns selection; flow owner advances. |
| Expedition available | Frontier read model derived from flow state and threat | Frontier plus HUD | Frontier computes availability, does not advance flow. |
| HUD objective | Presentation read model only | HUD has fallback flow logic | HUD displays projected Stage1 flow state, does not decide canonical gates. |

### Target Contract

Introduce a server-side `Stage1FlowSystem` or `Stage1FlowFeature` that is the only code allowed to mutate `Stage1SettlementProgression.Stage`.

Other features publish domain events/state:

- Buildings: `ConstructionCompletedEvent(anchorId, siteGid, buildingId)`.
- Workers: accepted assignment state/event.
- Build: accepted build prepared state/event.
- Frontier: expedition/boss/threat state, not Stage1 stage transitions.

Presentation reads a single projected flow read model: `Stage1FlowViewState` or the anchor components, without independently inventing gate precedence.

## D. Intent Normalization And Command Semantics

### Problem

The same player-facing construction action is represented differently depending on input surface:

- world hold input sends `_buildWorkPerSecond * Time.deltaTime`: `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Client/ClientConstructionInteractionSystem.cs:68`;
- context panel click sends a fixed `35f`: `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Stage1ContextPanelController.cs:13`;
- server handler clamps with its own default `35f`: `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Requests/BuildConstructionHandler.cs:15`;
- client projector repeats the same default: `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Requests/BuildConstructionProjector.cs:13`;
- worker AI uses `8f` per second: `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Actions/BuildConstruction/BuildConstructionExecutor.cs:14`.

These are not inherently all wrong, but they are not named as separate domain operations. The architecture cannot tell whether "Build" means one click, one hold tick, one worker tick, or one normalized build action.

### Solution

Create domain-owned command profiles in Buildings logic, for example `ConstructionActionProfile`:

- `PlayerBuildHold`: rate per second and max per request.
- `PlayerBuildClick`: explicit click quantum, if click is intended to be stronger than hold.
- `WorkerBuild`: worker rate per second.
- `ResourceDeposit`: request policy for "deposit all needed resources".

Controllers and input systems should not own gameplay numbers. They should request a named operation. The server handler should validate the operation profile, not trust arbitrary `WorkAmount` from UI code.

If the design wants identical user actions across world input and context panel, both should emit the same semantic command, e.g. `BuildConstructionIntent(site, PlayerBuildAction)`, and the request/projector should resolve amounts through the shared profile.

## E. Runtime/Test Parity

### Problem

Current tests frequently hand-build ECS entities and component sets:

- `CombatTestServerWorldScope.CreateNetworkedConstructionSite` constructs a networked construction site manually and sets `Stage1SettlementProgression` directly: `Assets/Tests/Editor/Combat/CombatTestServerWorldScope.cs:191`.
- `AiTestServerWorldScope.CreateSettlementAnchor` creates a fake anchor as a plain entity with only `Stage1SettlementProgression`: `Assets/Tests/Editor/Ai/AiTestServerWorldScope.cs:105`.
- `Stage1PresentationClientWorldScope` manually registers a subset of features/events instead of using runtime discovery/bootstrap: `Assets/Tests/Editor/Combat/Stage1PresentationClientWorldScope.cs:32`.
- Runtime uses `GameplayFeatureDiscovery`, generated component registration, prefab registration, spawn apply, despawn apply, and component deltas: `Assets/Scripts/StaticMlp/Composition/Runtime/Bootstrap/GameplayFeatureDiscovery.cs:38` and `Assets/Scripts/StaticMlp/Composition/Runtime/Bootstrap/MultiplayerSystemBootstrap.cs:55`.

This lets tests pass while bypassing the exact boundary that failed at runtime: server spawn -> spawn payload -> client apply -> post-spawn deltas.

### Target Fixture Strategy

Add shared test fixtures:

- `RuntimeServerWorldFixture`: uses `GameplayFeatureDiscovery.GetEcsTypeAssemblies`, `RegisterNetworkEvents`, `RegisterReplicationComponents`, `RegisterPrefabs`, and runtime server system registration.
- `RuntimeClientWorldFixture`: same for client core, including `ClientSpawnApplySystem`, `ClientComponentDeltaApplySystem`, projections, view sync, and `NetInbox`/`NetOutbox`.
- `ReplicationLoopFixture`: transfers encoded packets from server `NetOutbox` to client `NetInbox`, then runs the real client apply systems.
- feature test helpers may create domain state only through public spawners/builders used by runtime, not by manually setting critical replicated components.

Regression categories that must use real replication:

- networked spawn initial-state completeness;
- despawn/replacement flows such as construction site -> finished building;
- late join existing spawn payloads;
- anchor lookup after entity replacement/despawn;
- request projection vs authoritative reconciliation.

## F. Presentation Boundary

### Problem

Presentation contains too much domain interpretation:

- HUD resolves Stage1 objective precedence locally from many feature states: `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/ClientStage1HudStateSystem.cs:101`.
- Context panel focuses the repair target by first trying the anchor entity as a construction site, then falling back to anchor-linked site lookup: `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/ClientStage1ContextPanelSessionSystem.cs:60`.
- Expedition screen silently tolerates missing anchor/projected state and computes availability UI defaults: `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Presentation/ClientExpeditionSelectionScreenStateSystem.cs:21`.
- Combat visual spawning hardcodes ability-to-projectile/effect mapping in presentation: `Assets/Scripts/StaticMlp/Features/Effects/Runtime/Presentation/Systems/Client/ClientCombatVisualSpawnSystem.cs:42`.
- Several view parts create/destroy runtime Unity objects directly and some are not edit-mode safe, for example `PassiveAutoAttackViewPart` uses `Destroy` in `OnDestroy`: `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Presentation/Presentation/PassiveAutoAttackViewPart.cs:148`.

### Solution

Presentation should consume explicit view/read models:

- `Stage1FlowViewState` from the Stage1 flow owner for objective and button gates.
- `Stage1RepairFocusState` or `SettlementAnchorRef`-based lookup for context panel focus.
- `CombatVisualSpec` or a presentation catalog keyed by ability/effect id instead of switch statements embedded in presentation systems.
- a shared runtime visual cleanup helper for generated materials/game objects, with edit-mode behavior standardized.

Presentation may be permissive for optional visuals, but required gameplay read models should fail fast or be guaranteed by bootstrap. Missing mandatory anchor/read-model state should not silently degrade into default UI states.

## G. Spawn Factory API Boundary

### Status

- Done: `NetworkEntitySpawner` still initializes the entity before `SpawnBroadcaster.SendSpawn`, and typed `SpawnServerEntity<TNetworkEntityType>(...)` now fails fast when manifest-declared replicated components are missing after `initialize`.
- Done: Stage1 no longer uses building spawn callbacks to move `Stage1SettlementProgression` between transient building entities; camp-global progression stays on the dedicated camp anchor.
- Done: status spawning already exposes typed feature entrypoints (`SpawnPoison`, `SpawnBurning`, `SpawnOiled`) and keeps subtype-specific initialization private inside `StatusEntitySpawns`.
- Not done: `ServerBuildingSpawns.SpawnConstructionSite(...)` and `SpawnFinishedBuilding(...)` still expose `Action<SW.Entity> configure`, so callers can still inject arbitrary components into the spawn contract.
- Not done: initial construction-site spawn and construction completion still depend on that building `configure` callback for anchor-linking and seed-specific setup; these flows should move to explicit spawn specs or dedicated typed factory methods.
- Not done: `ServerStage1CampAnchorSpawnSystem` still assembles the full anchor entity inline and calls `NetworkEntitySpawner` directly instead of going through a dedicated `Stage1CampAnchorSpawner` with an explicit spawn spec.
- Not done: feature spawn APIs are still inconsistent about explicit spawn contracts; Buildings remains callback-driven, and Player/AI/Worker spawn paths still use ad-hoc method parameters instead of shared `*SpawnSpec` contracts that document required versus optional spawn-time state.

### Problem

`NetworkEntitySpawner.SpawnServerEntity(..., Action<SW.Entity> initialize)` has the correct timing: it invokes `initialize` before `SpawnBroadcaster.SendSpawn`: `Assets/Scripts/StaticMlp/Game/Replication/NetworkEntitySpawner.cs:31` and `Assets/Scripts/StaticMlp/Game/Replication/NetworkEntitySpawner.cs:35`.

The architectural problem is that the callback is exposed as a general feature API:

- `ServerBuildingSpawns.SpawnConstructionSite` and `SpawnFinishedBuilding` accept an extra `Action<SW.Entity> configure`: `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Server/ServerBuildingSpawns.cs:18` and `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Server/ServerBuildingSpawns.cs:40`.
- Initial camp repair site setup uses that callback to attach `Stage1SettlementProgression`: `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Server/ServerInitialConstructionSiteSpawnSystem.cs:32`.
- Construction completion uses the callback to copy progression to a different entity: `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Server/ServerCompleteConstructionSystem.cs:49`.
- Status spawning uses a private callback to vary status subtype initialization: `Assets/Scripts/StaticMlp/Features/Statuses/Runtime/Logic/Spawning/StatusEntitySpawns.cs:96`.
- Several feature spawners call `NetworkEntitySpawner` directly and define their own component sets: `Assets/Scripts/StaticMlp/Features/Player/Runtime/Systems/Server/PlayerSpawns.cs:17`, `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/Spawning/AiBotSpawns.cs:21`, and `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Systems/Server/ServerSettlementWorkerSpawnSystem.cs:33`.

This is fragile because the callback can add any component, overwrite required state, capture arbitrary external values, and split required spawn state between the entity type, archetype registry, spawner helper, and caller. Nothing in the API says which components are part of the spawn contract, which are optional, or which module owns them.

### Decision

Keep `SpawnServerEntity` only as an infrastructure primitive inside the replication/spawning layer. Feature code should call typed entity factories/spawners with explicit spawn specs, not pass arbitrary `Action<SW.Entity>` callbacks.

Target shape:

- `Stage1CampAnchorSpawner.Spawn(Stage1CampAnchorSpawnSpec spec)` creates the full camp anchor manifest.
- `BuildingEntitySpawner.SpawnConstructionSite(ConstructionSiteSpawnSpec spec)` creates construction-only state and an optional explicit `SettlementAnchorRef`, not arbitrary caller components.
- `BuildingEntitySpawner.SpawnFinishedBuilding(FinishedBuildingSpawnSpec spec)` creates finished-building state only.
- `StatusEntitySpawner.SpawnPoison/SpawnBurning/SpawnOiled(...)` may keep private subtype initialization internally, but callers should not provide arbitrary initialization callbacks.
- `PlayerSpawner`, `AiBotSpawner`, and `SettlementWorkerSpawner` should own their complete required component set through typed specs.

The feature-facing factory owns these rules:

- the network entity type;
- the network archetype id;
- required replicated components;
- required tags;
- allowed optional components;
- spawn-time values;
- validation at trust boundaries.

Callers provide data, not mutation code.

### Add/Set Rule

StaticEcs already documents that `Add<T>()` without a value is idempotent and silently returns the existing component, while `Set(value)` overwrites and calls delete/add hooks.

Do not introduce or rely on a non-throwing `Add<TComponent>` as a required-state pattern. For authoritative replicated state, duplicate initialization is an architecture bug and should fail loudly. Silent idempotence hides double factory calls, wrong spawn ordering, and accidental reuse of entities.

Recommended convention:

- Use `Set(value)` inside spawn factories when assigning deterministic initial values before broadcast.
- Use `Set<TTag>()` for tags, accepting the existing tag semantics.
- Use `Mut<T>()` through `ReplicationMut.Mut<T>()` for post-spawn replicated value changes.
- Avoid `Add<T>()` for required replicated component initialization unless the component is a container such as `Multi<T>` where "create if missing" is the explicit data-structure contract.
- If idempotent addition is genuinely needed, expose it with a narrow name such as `TryAddOptional...` in the owning module, not as a broad gameplay habit.

### Command Boundary

A universal command transport is acceptable at the infrastructure level: a shared envelope, request registration, delivery policy, and typed serialization pipeline.

Universal gameplay commands are not acceptable if they mean generic opcode/parameter bags. They erase domain contracts, weaken validation, make tests less specific, and move gameplay rules into dispatch code.

Target rule:

- client/server transport may be generic;
- gameplay commands remain typed events/requests;
- UI/world input normalizes to named domain intents before networking;
- the server validates typed command specs against domain-owned profiles.

For construction this means a command such as `BuildConstructionRequestEvent` should carry a semantic operation/profile id or validated domain intent, not arbitrary UI-owned work constants. For Stage1 flow this means feature events feed the flow owner, not a generic command mutating progression directly.

## H. Feature Boundary Contracts

### Status

- Done: Frontier no longer mutates `Stage1SettlementProgression` directly; Stage1 flow transitions now go through `Stage1BuildPreparedEvent` and `ServerStage1FlowSystem`.
- Done: Stage1, Buildings, and Settlement.Workers now have typed spawn entrypoints/specs for camp anchors, construction/finished buildings, and workers.
- Done: Buildings exposes typed request handlers for player construction work and resource deposit instead of letting UI mutate construction state directly.
- Done: Stage1 flow view state is owner-authored and consumed as a read model by presentation.
- Done: feature contract folders/asmdefs now exist for Stage1, Buildings, Settlement, Settlement.Workers, Build, Frontier, Progression, Combat, Effects, and Statuses.
- Done: first-pass public contract types were moved to contract assemblies: settlement anchor ids/refs, Stage1 flow events/read model, Buildings requests/results/spawn specs, Worker assignment requests/results, Build boss request, Frontier ids/start requests, Progression ids/events, Combat result/request events, Status marker tags, and Effects lifecycle tags.
- Done: Combat and Statuses now mark Effects lifecycle state through the Effects-owned `EffectLifecycle` contract instead of setting `EffectProcessedTag` directly.
- Not done: most feature contracts, owned components, systems, catalogs, and read models are still public from `Runtime/Logic`, so asmdefs do not enforce ownership boundaries.
- Not done: Settlement.Workers still mutates Buildings-owned construction state and Settlement-owned shared resources directly from worker executors.
- Not done: Progression still mutates Settlement-owned shared resources and Build-owned boss preparation state directly.
- Not done: Combat and Statuses still query Effects-owned effect entity state directly; only processed/rejected lifecycle writes are routed through the owner contract.
- Not done: there is no automated or review-time ownership map that defines which feature owns each component, tag, event, resource, factory, and read model.

### Problem

The codebase currently has no enforceable rule for inter-feature writes. Features import each other freely and mutate whatever state is convenient. That makes invariants local in code but global in runtime behavior.

Examples:

- Frontier mutates Settlement progression directly when a prepared build exists: `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Logic/ServerFrontierExpeditionAvailabilitySystem.cs:42`.
- Settlement.Workers applies build work by mutating construction site state and progress: `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Actions/BuildConstruction/BuildConstructionExecutor.cs:59`.
- Settlement.Workers delivery mutates both shared settlement storage and construction site resources in one executor: `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Actions/DeliveryBuildResources/DeliveryBuildResourcesExecutor.cs:71` and `Assets/Scripts/StaticMlp/Features/Settlement.Workers/Runtime/Logic/Actions/DeliveryBuildResources/DeliveryBuildResourcesExecutor.cs:75`.
- Progression applies rewards by mutating settlement resource storage: `Assets/Scripts/StaticMlp/Features/Progression/Runtime/Logic/ServerStage1RewardApplicationSystem.cs:37`.
- Progression mutates Build-owned boss preparation state and snapshot: `Assets/Scripts/StaticMlp/Features/Progression/Runtime/Logic/ServerStage1BossPreparationProgressionSystem.cs:46` and `Assets/Scripts/StaticMlp/Features/Progression/Runtime/Logic/ServerStage1BossPreparationProgressionSystem.cs:53`.
- Combat and Statuses process generic Effects entities by writing effect lifecycle tags: `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Logic/Systems/Server/ServerDamageApplySystem.cs:58` and `Assets/Scripts/StaticMlp/Features/Statuses/Runtime/Logic/Systems/Server/ServerApplyPoisonStatusSystem.cs:35`.

Some of these are legitimate integration points, but the project does not distinguish public command surfaces from private owned state. As a result, any feature can accidentally become a second owner of another feature's invariants.

### Decision

Adopt component ownership as an architecture rule:

- Every component, tag, event, resource, and entity factory has one owning feature or shared-kernel module.
- Only the owner may `Set`, `Mut`, `Delete`, spawn, despawn, or otherwise structurally manage its owned authoritative state.
- Other features may read public state and send typed commands/events to the owner.
- Cross-feature writes are forbidden by default.
- Any exception must be documented as a public write contract in the owning feature.

This should be stricter than the current style, but not simplified to "everything must go through `StaticEcs.Events`".

### Communication Patterns

Use `StaticEcs.Events` for transient commands and facts:

- `ConstructionCompletedEvent`
- `BuildPreparedEvent`
- `ExpeditionRewardGrantedEvent`
- `SettlementResourcesGrantRequestedEvent`
- `WorkerAssignmentAcceptedEvent`

Use typed request/response handlers when a caller needs an accepted/rejected result, client prediction, or validation path.

Use typed spawn factories for entity creation. A feature should not create another feature's entity by manually setting its components.

Use public read-model components for presentation and cross-feature queries. Read models can be read by other features, but their owner still controls writes.

Use contribution registries only for extension points whose ownership is explicit. For example, AI action packages may emit an `AiMoveRequest` only if `AiMoveRequest` is documented as a public command component owned by AiBots. They must not mutate `AiBrain`, `AiTaskState`, or `AiNetState` unless the AI owner explicitly exposes that as an extension contract.

### Why Not Events Only

A hard rule that all feature interaction must use only `StaticEcs.Events` is too narrow:

- events are transient and same-tick/order dependent;
- events are not persistent authoritative state;
- events do not solve spawn-time initialization;
- events are a poor fit for long-running tasks unless paired with owned state;
- multi-state transactions reveal ownership problems rather than being solved by event dispatch alone.

If a transaction must atomically update state owned by two different features, do not let a third feature mutate both directly. Either move the state under one aggregate owner, or introduce an explicit domain coordinator that owns the transaction contract.

The current worker resource delivery is the clearest example: it spends settlement storage and updates construction resources in one worker executor. That executor should not own both invariants. The architecture should decide whether construction economy belongs to Buildings, Settlement, or a dedicated coordinator, then expose a typed command such as `DepositConstructionResourcesRequested`.

### Enforcement Strategy

Target folder/API shape:

- `FeatureX/Runtime/Contracts`: public events, request structs, read models, ids, spawn specs, and explicitly writable command components.
- `FeatureX/Runtime/Logic`: owned components, authoritative systems, validators, rules, and private mutation code.
- `FeatureX/Runtime/Presentation`: client-only view state and controllers.

With asmdefs, keep implementation types `internal` where possible. Public types should mostly be contracts. A feature importing another feature's `Logic` namespace to call `ReplicationMut.Mut<T>` on its components should be treated as an architecture violation.

Code review rule:

- `Read<T>` on foreign public read-model/contract state is allowed.
- `Set<T>`, `Mut<T>`, `Delete<T>`, `ReplicationMut.Mut<T>`, `Spawn`, and `Despawn` on foreign owned state are not allowed.
- If a foreign write looks necessary, either the component ownership is wrong, or the owner is missing a command/event/factory contract.

## Prioritized Migration Plan

1. Add the dedicated camp anchor network entity and spawn it with the full initial component set.
2. Move all camp-global systems to query the camp anchor directly; remove progression from construction/building network manifests.
3. Restrict `SpawnServerEntity(..., Action<SW.Entity>)` to the replication layer and expose typed spawn factories with explicit spawn specs.
4. Define component ownership rules and contract folders for Stage1, Buildings, Settlement, Workers, Build, Frontier, Progression, Combat, Effects, and Statuses.
5. Replace lazy anchor init systems with spawn-time initialization.
6. Introduce `Stage1FlowSystem` as the only mutator of `Stage1SettlementProgression.Stage`.
7. Normalize construction intents through named domain profiles and typed network requests.
8. Replace direct cross-feature mutations with owner-handled events/requests/factories, starting with Workers -> Buildings/Settlement and Frontier -> Stage1 flow.
9. Add runtime parity fixtures and convert the repair completion regression to server spawn -> client spawn apply -> build completion -> despawn/spawn -> client apply.
10. Replace HUD/context/expedition local gate logic with projected flow/read models.

## Non-Goals

- Do not add defensive `Has<T>()` or silent fallback wrappers to mask missing anchor state.
- Do not add temporary services around the current pseudo-anchor model.
- Do not keep `Stage1SettlementProgression` on buildings as a compatibility bridge unless explicitly marked temporary with `[Obsolete("Temp")]`.
- Do not solve spawn fragility by making required component initialization silently idempotent.
- Do not replace direct writes with untyped generic event bags. Contracts must stay typed and owned by the receiving feature.
