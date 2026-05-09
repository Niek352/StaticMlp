# AI + Combat Research

Date: 2026-05-09

Scope:
- `Assets/Scripts/StaticMlp/Features/AiBots`
- `Assets/Scripts/StaticMlp/Features/AiActions`
- `Assets/Scripts/StaticMlp/Features/AiTaskExecution`
- `Assets/Scripts/StaticMlp/Features/Combat`
- `Assets/Scripts/StaticMlp/Composition/Runtime/Bootstrap`
- `docs/GameAiBots/AiBots_Action_Extensibility_Guideline.md`
- `Assets/Tests/Editor/Ai/*`
- `Assets/Tests/Editor/Combat/*`

## Stage 1. Architectural Research

### Current runtime shape

AI is currently split across three runtime modules:
- `AiBots`: server-side sensing, blackboard state, behavior selection, navigation coordination, thin replicated state.
- `AiTaskExecution`: one facade system that keeps the active executor in sync with the selected task.
- `AiActions`: concrete task packages, utility bindings, manual command binders, and executors.

Combat is currently split across two mostly independent flows:
- Client flow: target acquisition -> local fire intent -> send typed network event -> local presentation.
- Server flow: receive attack request -> create effect entity -> apply damage -> mark death -> cleanup effect.

The intended high-level order is clean:
- AI server order is `Needs -> Perception -> ActionVariables -> UtilityDecision -> TaskExecution -> Navigation -> NetState`.
- Combat server order is `Request -> Damage -> DeathMark -> Cleanup`.

The main problems are not frame order. The problems are hidden contracts, incomplete semantic wiring, and several state models that only partially connect to real gameplay truth.

### What is architecturally weak

#### 1. AI composition is highly implicit

Two reflection-based discovery layers assemble the feature at runtime:
- `GameplayFeatureDiscovery` reflects every `IGameplayFeature` in loaded assemblies.
- `AiActionCatalog.Discover()` reflects every `IAiActionPackage` in loaded assemblies.

Implications:
- The real AI behavior catalog does not exist in one explicit file anymore. It is synthesized from package contributions.
- `AiActionsGameplayFeature` is empty, but still important because it helps load the assembly that contains action packages.
- Behavior availability depends on assembly loading and reflection order instead of explicit compile-time composition.

Files:
- `Assets/Scripts/StaticMlp/Composition/Runtime/Bootstrap/GameplayFeatureDiscovery.cs`
- `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/Actions/AiActionCatalog.cs`

Why this is weak:
- The runtime graph is hard to read from code navigation.
- Missing actions fail late.
- There is no single place where a reviewer can answer "what behaviors exist for this feature build?"

#### 2. The AI blackboard is a weakly typed schema with no central semantic registry

The core blackboard is a dynamic `Multi<AiBlackboardEntry>` with:
- `ushort VariableId`
- runtime `Kind`
- three payload slots: `FloatValue`, `EntityValue`, `VectorValue`

This is flexible, but the semantic schema is now scattered across:
- `AiCoreVariableIds`
- action-local constants like `1101`, `1201`, `1301`, `1401`, `1501`, `2101`, `2201`
- editor-only reflection name lookup in `AiEditorUtilityBindingNameCatalog`

Implications:
- The real blackboard contract is not self-documenting.
- Utility inputs and execution-only inputs are mixed into one bag.
- Renaming, removing, or reusing ids is risky because there is no central ownership map.

Files:
- `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/Blackboard/AiBlackboardEntry.cs`
- `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/Blackboard/AiBlackboardAccess.cs`
- `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/Blackboard/AiCoreVariableIds.cs`
- `Assets/Scripts/StaticMlp/Editor/Ai/AiEditorUtilityBindingNameCatalog.cs`

#### 3. AI combat intent is not integrated with Combat

`AttackEnemyExecutor` writes `AiAttackRequest`, but nothing in `Combat` consumes it.

Today `AiAttackRequest` is only used for:
- being set by `AttackEnemyExecutor`
- being deleted on executor exit
- feeding `AiNetState.CombatState`

That means:
- Bots can "choose attack" and "show combat state"
- but they do not actually cause damage through the combat pipeline

Files:
- `Assets/Scripts/StaticMlp/Features/AiActions/Runtime/Actions/AttackEnemy/AttackEnemyExecutor.cs`
- `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/Systems/Server/ServerAiNetStateSystem.cs`
- there is no server combat system that reads `AiAttackRequest`

This is the largest semantic hole between the new AI and Combat features.

#### 4. Builder AI crosses module boundaries by mutating building state directly

`BuildConstructionExecutor` and `DeliveryBuildResourcesExecutor` reach directly into `Buildings` replicated state:
- they resolve construction sites themselves
- they mutate `ConstructionSiteState`, `ConstructionProgress`, and `ConstructionResources` directly
- `DeliveryBuildResourcesExecutor` deposits hardcoded `100, 100` resources from nowhere

Implications:
- AI actions know too much about building internals.
- There is no explicit economy/inventory contract.
- The AI "delivery" action mints resources instead of using an actual inventory source.

Files:
- `Assets/Scripts/StaticMlp/Features/AiActions/Runtime/Actions/BuildConstruction/BuildConstructionExecutor.cs`
- `Assets/Scripts/StaticMlp/Features/AiActions/Runtime/Actions/DeliveryBuildResources/DeliveryBuildResourcesExecutor.cs`

#### 5. Several AI decision variables drift away from real gameplay truth

`Health01` and `WoodStorage01` are decision signals, but they are not sourced from authoritative gameplay state every frame.

Current behavior:
- `AiBotSpawns` initializes `Health01` from spawn health and sets `WoodStorage01 = 1f`
- `ServerAiNeedsSystem` only clamps those values, but does not derive them from `Health` or inventory/resource components

Implications:
- Combat damage does not necessarily affect AI utility selection.
- Resource delivery/build utility depends on a placeholder value, not on actual inventory state.

Files:
- `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/Spawning/AiBotSpawns.cs`
- `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/Systems/Server/ServerAiNeedsSystem.cs`

This is not just cleanup debt. It changes behavior correctness.

#### 6. Combat auto-attack is client-driven and weakly validated on the server

The server validates:
- the source peer resolves to a player
- the target is a monster with `Health`
- the target is in range

The server does not validate:
- fire cadence
- duplicate shot sequence reuse
- replayed requests
- any server-side auto-attack state

`ShotSequence` is forwarded into `EffectRequestId`, but that id is not consumed later.

Implications:
- the server trusts client timing
- `PassiveAutoAttackState.NextFireAt` is only a client local convention
- `ShotSequence` is diagnostic metadata, not an authority mechanism

Files:
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Systems/Server/ServerPassiveAutoAttackRequestSystem.cs`
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Commands/EffectCommands.cs`
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Components/EffectRequestId.cs`

#### 7. The effect pipeline is only half semantic, half placeholder

The effect model is promising:
- request becomes effect entity
- effect entity is processed
- effect entity is cleaned up

But several parts do not participate in behavior:
- `EffectCreatedTick` is always `0`
- `EffectRequestId` is written but never interpreted
- `EffectKind` currently has one real branch only
- debug log access is duplicated in both `EffectCommands` and `ServerDamageApplySystem`

Implications:
- the model looks extensible, but the current semantics are only partially alive
- the code suggests planned features that never entered the pipeline

Files:
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Commands/EffectCommands.cs`
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Systems/Server/ServerDamageApplySystem.cs`
- `Assets/Scripts/StaticMlp/Features/Combat/Runtime/Components/EffectCreatedTick.cs`

#### 8. Documentation is behind the code

AI has docs, but the main extension guideline is stale:
- it references `AiBehaviorCatalogDefaults.cs`, which no longer exists
- it references `AiBlackboard.cs`, which no longer exists
- it says executors live under `AiTaskExecution/Runtime/Execution/Executors/*`, but concrete executors are now in `AiActions`
- it shows an old executor API with `ref AiBlackboard blackboard`
- it says executors are registered in `ServerAiTaskExecutionSystem`, but registration is now package discovery

Combat has no feature-level doc at all. Its semantics currently live only in source and tests.

Files:
- `docs/GameAiBots/AiBots_Action_Extensibility_Guideline.md`
- no dedicated `Combat` design doc was found under `ai/` or `docs/`

### Undocumented semantic entities

#### AI semantics currently underdocumented or undocumented

Runtime orchestration:
- `AiActionCatalog`: runtime-assembled registry of executors, binders, collectors, utility bindings, and synthesized behavior definitions.
- `IAiActionPackage`: the true extension point for new AI actions.
- `IAiTaskExecutor`: the task lifecycle contract.
- `IAiActionVariableCollector`: per-action sensor/enrichment hook.
- `IAiActionCommandTargetBinder`: per-action manual command target translator.
- `AiTaskExecutionTransitions`: shared transition contract that mutates `AiBrain`, `AiTaskState`, and movement state.

Decision model:
- `AiBehaviorDefinition`: one behavior id mapped to a list of utility tasks.
- `AiBehaviorTaskContribution`: package-local contribution into a behavior.
- `UtilityTaskDefinition`: one candidate task plus its considerations.
- `UtilityConsideration`: variable id + curve + weight triple.
- `UtilityCurveType`: utility shaping semantics.
- `AiBehaviorIds`: the current behavior taxonomy (`Monster`, `PeacefulBuilder`).

Blackboard model:
- `AiBlackboardEntry`: low-level storage record with runtime kind switching.
- `AiBlackboardValueKind`: payload kind discriminator.
- `AiBlackboardAccess`: the real API for blackboard mutation and reads.
- `AiCoreVariableIds`: canonical ids for hunger, health, fear, enemy, enemy distance, last known enemy position, leader, wood storage.
- Action-local variable ids:
  - `FleeVariableBindings`: `1101..1104`
  - `AttackEnemyVariableBindings`: `1201..1204`
  - `FollowLeaderVariableBindings`: `1301..1303`
  - `BuildConstructionVariableBindings`: `1401..1403`
  - `DeliveryBuildResourcesVariableBindings`: `1501`
  - `BuildConstructionCollectVariables.BuildTargetSite`: `2101`
  - `DeliveryBuildResourcesCollectVariables.TargetSite`: `2201`

Task and replicated state:
- `AiBrain`: long-lived selected behavior and decision cooldown.
- `AiTaskState`: split between `Task` and `ActiveTask`, with `HasActiveTask`, `Step`, and `Timer`.
- `AiMoveRequest`: navigation intent contract.
- `AiAttackRequest`: AI combat intent placeholder, currently not connected to real combat processing.
- `AiNetState`: thin replicated presentation summary with three compressed fields.
- `AiNetState.LocomotionState`: byte flag, semantic values are implicit.
- `AiNetState.CombatState`: byte flag, semantic values are implicit.

Spawn/control/navigation:
- `CommandBotEvent`: manual player-issued bot command payload.
- `InitialBotSpawnDefinition.LeaderIndex`: boot-time relative leader reference semantic.
- `AiNavigationRuntime`: AI navigation facade resource.
- `IAiNavigationBackend`: backend abstraction boundary.
- `AiNavigationRuntime.AgentInput`: per-frame movement command payload to backend.
- `AiNavigationRuntime.AgentResult`: backend result contract.
- `AiNavigationRuntime.DebugState`: editor/debug visualization contract.

Concrete task semantics that are not written down anywhere:
- `AttackEnemy`: chase enemy and mark attack intent, but currently does not damage through combat.
- `Flee`: move away from `LastKnownEnemyPosition`.
- `FollowLeader`: move to leader with `StopDistance = 3`.
- `BuildConstruction`: move to nearest buildable site and directly apply build work.
- `DeliveryResourceToBuilding`: move to nearest deposit site and directly inject resources.

#### Combat semantics currently undocumented

Server effect model:
- `EffectCommands`: effect construction entry point.
- `EffectEntityType`: ephemeral ECS type for effect entities.
- `EffectTag`: "this is an effect entity".
- `DamageEffectTag`: specialization for damage processing.
- `EffectKind`: semantic category field; currently only `Damage`.
- `EffectValue`: generic payload amount.
- `EffectSource`: source entity reference.
- `EffectTarget`: target entity reference.
- `EffectRequestId`: optional caller request metadata.
- `EffectCreatedTick`: intended creation tick metadata.
- `EffectProcessedTag`: lifecycle marker after server processing.
- `DamageData`: damage-specific payload extension.
- `DamageType`: current taxonomy (`Physical`, `Fire`, `Poison`).

Client/server auto-attack semantics:
- `PassiveAutoAttackState`: local targeting/fire cadence state.
- `PassiveAutoAttackIntent`: one-shot local firing intent with `LocalFireTime`.
- `PassiveAutoAttackRequestEvent`: network event from client to server.
- `CombatAutoAttackConfig`: local tuning resource for radius, cadence, tracer lifetime, highlight fade, damage flash lifetime.

Presentation-only semantics:
- `PassiveAutoAttackViewState`: tracer playback state on shooter.
- `PassiveAutoAttackTargetViewState`: target highlight state on victim/current target.
- `HealthPresentationState`: previous-health cache used to derive damage flashes.
- `DamageFeedbackViewState`: flash/intensity/lifetime state driven by health deltas.
- `CombatDebugLogBuffer`: runtime combat trace buffer used by tests and debug inspection.

### Recommended architectural cleanup order

1. Fix semantic correctness first.
- Make AI combat actually flow into Combat, or remove `AiAttackRequest` until that link exists.
- Recompute `Health01` from `Health` every frame.
- Replace placeholder `WoodStorage01` with real inventory/resource ownership data.
- Make server auto-attack cadence authoritative or at least validate `ShotSequence`.

2. Replace hidden contracts with explicit ones.
- Add a feature-level `Combat` doc.
- Rewrite the AI action extension guideline to describe packages, reflection discovery, and the real executor API.
- Add a central semantic registry doc for AI variable ids and task semantics.

3. Only then simplify code shape.
- Once semantics are explicit, the current small systems become easier to merge or factor safely.

## Stage 2. System-by-system simplification review

### AI systems

#### `ServerAiRuntimeInitSystem`

What it does:
- Creates `AiActionCatalog` if missing.
- Validates NavMesh.
- Creates `AiNavigationRuntime` if missing.

What is clumsy:
- Resource acquisition and environment validation are mixed.
- The same "resource exists but is null" boilerplate is repeated across the feature set.

How to simplify:
- Introduce a small explicit runtime services helper such as `AiRuntimeResources.RequireCatalog()` and `AiRuntimeResources.RequireNavigation()`.
- Keep this system as a pure one-time initializer and move NavMesh validation into `AiNavigationRuntime.CreateDefault()`.

#### `ServerAiBotSeedSystem`

What it does:
- Spawns initial bots exactly once.

What is clumsy:
- `_spawned || HasAnyBot()` is a slightly ambiguous lifecycle rule.
- Leader resolution is index-based and only valid inside one spawn batch.

How to simplify:
- Replace `_spawned` with an explicit seed-complete resource or consume-once bootstrap resource.
- Document `LeaderIndex` as a boot-only relative reference or replace it with a more explicit temporary spawn id.

#### `ServerCommandBotRequestSystem`

What it does:
- Validates player-issued bot command.
- Uses binder to populate blackboard.
- Overwrites current task.

What is clumsy:
- It manually resets `AiBrain` and `AiTaskState` instead of using one shared transition entrypoint.
- Validation, authority checks, binder invocation, and task switching all live in one method.

How to simplify:
- Add `AiTaskExecutionTransitions.ForceTask(...)` or similar and reuse it here.
- Extract `TryResolveCommandableBot(...)`.
- Move task switching responsibility into `AiActionCatalog.TryApplyManualCommand(...)` so the catalog owns the package-level contract end-to-end.

#### `ServerBotAiDeathSystem`

What it does:
- Collects dead bots and despawns them.

What is clumsy:
- It is structurally fine, but it is another "collect gid -> replay structural change" pattern repeated elsewhere.

How to simplify:
- If more features get this pattern, introduce an explicit reusable "despawn all entities with tag X" utility system with a domain-specific name.
- If not, leave it as-is. This system is already small and readable.

#### `ServerAiNeedsSystem`

What it does:
- Evolves hunger/fear and clamps normalized values.

What is clumsy:
- `Health01` and `WoodStorage01` are treated as local cached floats instead of derived truth.

How to simplify:
- Split into:
  - derive normalized state from real components/resources
  - apply passive need drift
- Read `Health` directly here and compute `Health01 = Current / Max`.
- Replace `WoodStorage01` with a real inventory read or remove it from utility until inventory exists.

#### `ServerAiPerceptionSystem`

What it does:
- Finds nearest player for each bot.
- Writes enemy, distance, last known position, and fear boost.

What is clumsy:
- Nested query is fine for the current scale, but all writeback semantics are inline.
- Detection radius is a hardcoded constant.

How to simplify:
- Move the radius into an AI config resource.
- Extract `WriteEnemyContact(...)` and `ClearEnemyContact(...)`.
- If bot/player counts grow, pre-collect players once per frame into a temporary list of snapshots.

#### `ServerAiActionVariablesCollectSystem`

What it does:
- Runs all registered action collectors for every bot.

What is clumsy:
- Another repeated catalog lookup/null guard.
- Empty collectors still participate in the loop.

How to simplify:
- Let packages with no collector return `null`.
- Cache only non-null collectors, which the catalog already almost does.
- Centralize catalog resolution into a shared `RequireCatalog()` helper.

#### `ServerAiUtilityDecisionSystem`

What it does:
- Decrements decision cooldown.
- Picks best task from behavior catalog.

What is clumsy:
- Task switching logic duplicates transition resets that already exist elsewhere.
- There is no explicit distinction between autonomous task selection and temporary forced/manual task ownership.

How to simplify:
- Route task switch through `AiTaskExecutionTransitions.SwitchTo(...)`.
- If manual commands should persist longer than one utility tick, add explicit "task source / forced-until" state instead of only `DecisionCooldown`.

#### `ServerAiTaskExecutionSystem`

What it does:
- Keeps `AiTaskState.ActiveTask` synchronized with `AiTaskState.Task`.
- Runs executor lifecycle.

What is clumsy:
- `Task` vs `ActiveTask` is correct, but undernamed.
- `EnsureActiveExecutor` and `EnsurePostExecuteTransition` split one concept into two very similar steps.

How to simplify:
- Rename semantics in a future cleanup:
  - `Task` -> `SelectedTask`
  - `ActiveTask` -> `ExecutorTask`
- Collapse transition handling into a single method:
  - ensure current executor
  - execute
  - if selected task changed during execute, transition once

#### `ServerAiNavigationSystem`

What it does:
- Sends current movement state into navigation runtime.
- Applies resulting `CharacterNetState`.

What is clumsy:
- It mixes request extraction, backend sync, result application, and move-request cleanup.
- Epsilon constants live locally and are not shared with the backend.

How to simplify:
- Extract:
  - `BuildAgentInput(...)`
  - `ApplyNavigationResult(...)`
  - `TryCompleteMoveRequest(...)`
- Keep the current system boundary, but make the update loop read like orchestration instead of mixed low-level logic.

#### `ServerAiNetStateSystem`

What it does:
- Builds client-facing AI presentation summary.

What is clumsy:
- `LocomotionState` and `CombatState` are raw bytes with implicit meaning.
- `CombatState` is currently inferred from `AiAttackRequest`, which is not real combat state.

How to simplify:
- Replace raw bytes with explicit small enums.
- Feed combat presentation from real combat events/state, not from a placeholder AI request component.

### AI action packages

#### `Idle`, `FollowLeader`, `Flee`, `AttackEnemy`

What is clumsy:
- `IdleCollectVariables`, `FollowLeaderCollectVariables`, `FleeCollectVariables`, `AttackEnemyCollectVariables` are empty classes.
- `AttackEnemyCommandTargetBinder` and `FleeCommandTargetBinder` are near-duplicates.
- `FollowLeaderExecutor`, `FleeExecutor`, and `AttackEnemyExecutor` all inline "resolve target -> validate -> fallback to idle".

How to simplify:
- Allow `VariableCollector => null` for no-op actions.
- Add an explicit binder helper for "bind entity target + update enemy context".
- Add a small explicit target-resolution helper, for example `AiTargetContext.TryGetEnemy(...)`, instead of repeating the same unpack logic.

#### `BuildConstruction` and `DeliveryBuildResources`

What is clumsy:
- Both collectors do "find nearest site of specific kind".
- Both executors do "walk into interaction range, then mutate site state".
- Both actions directly know building internals.

How to simplify:
- Create an explicit building-domain AI bridge with a clear name, for example:
  - `ConstructionSiteSelection`
  - `ConstructionSiteInteractionRules`
- Better option: stop mutating building state directly from AI executors and express those actions as typed building commands/events.

### Combat systems

#### `ServerPassiveAutoAttackRequestSystem`

What it does:
- Accepts client auto-attack requests and turns them into damage effects.

What is clumsy:
- Important gameplay rules are hardcoded here: range and damage.
- No server-side cadence or dedupe state exists.

How to simplify:
- Introduce a server combat config/resource shared with client tuning where appropriate.
- Add authoritative per-player or per-entity firing state on the server.
- Validate `ShotSequence` and/or `NextFireAt` server-side before effect creation.

#### `ServerDamageApplySystem`

What it does:
- Applies pending damage effects to `Health`.

What is clumsy:
- Debug log access is duplicated here and in `EffectCommands`.
- The effect-processing loop is still generic-looking, but only one effect type exists.

How to simplify:
- Move debug buffer access into one shared helper.
- Keep this system specialized for damage until more effect kinds exist.
- When more effect kinds appear, route by `EffectKind` in a dedicated dispatcher instead of cloning full effect loops.

#### `ServerHealthDeathMarkSystem`

What it does:
- Converts `Health <= 0` into `IsDiedTag`.

What is clumsy:
- Very little. This is already the right size.

How to simplify:
- Leave it alone unless death semantics become richer.

#### `ServerEffectCleanupSystem`

What it does:
- Destroys processed effect entities.

What is clumsy:
- Also already small and clear.

How to simplify:
- Leave it alone unless effect lifetime semantics become more complex.

#### `ClientPassiveAutoAttackInitSystem`

What it does:
- Lazily creates `CombatAutoAttackConfig`.

What is clumsy:
- It exists mostly because config bootstrap was not given a better home.

How to simplify:
- Create this resource in client bootstrap/composition and remove the dedicated init system.

#### `ClientPassiveAutoAttackTargetingSystem`

What it does:
- Finds nearest target and keeps local targeting state.

What is clumsy:
- It collects players into a list, then re-resolves them.
- It is still readable, but shares traversal patterns with later client combat systems.

How to simplify:
- If the three client auto-attack systems stay separate, create one reusable local-player iteration helper.
- If not, merge targeting + intent into one system, because both operate on the same local cadence state.

#### `ClientPassiveAutoAttackIntentSystem`

What it does:
- Turns target selection plus fire timer into `PassiveAutoAttackIntent`.

What is clumsy:
- `PassiveAutoAttackIntent` is effectively a transient message but is kept as normal persistent component state.

How to simplify:
- Either:
  - merge intent generation and sending into one system
  - or redefine intent as a one-frame event-like component and delete it after send

#### `ClientPassiveAutoAttackSendSystem`

What it does:
- Sends the latest local attack intent to the server.

What is clumsy:
- Very small, but it only exists because `PassiveAutoAttackIntent` is split from generation.

How to simplify:
- Merge with `ClientPassiveAutoAttackIntentSystem` unless you intentionally need a visible intermediate debug state.

#### `ClientPassiveAutoAttackPresentationSystem`

What it does:
- Decays tracers.
- Decays highlights.
- Builds new shot snapshots from local intent.
- Applies new shot visuals.
- Applies persistent current-target highlights.

What is clumsy:
- This is the most overloaded combat system.
- It owns three different presentation concerns with separate lifecycles.
- It maintains four temporary collections and one internal snapshot type.

How to simplify:
- Split into two explicit presentation systems:
  - `ClientPassiveAutoAttackTracerPresentationSystem`
  - `ClientPassiveAutoAttackTargetHighlightPresentationSystem`
- Or keep one system but extract reducer-style functions into a clearly named local presentation module.

#### `ClientDamageFeedbackPresentationSystem`

What it does:
- Decays flash feedback.
- Detects health drops and emits a new `DamageFeedbackViewState`.

What is clumsy:
- It combines previous-health caching and presentation lifetime logic in one place.

How to simplify:
- Keep current shape if the feature stays small.
- If combat presentation grows, split:
  - health delta tracking
  - view-state lifetime update

## Practical refactor suggestions

High-priority fixes:
1. Connect `AiAttackRequest` to the combat pipeline or remove it and rename the current behavior to reflect that it only chases.
2. Recompute `Health01` from `Health` and stop treating it as spawn-time cached data.
3. Replace fake builder economy semantics with explicit inventory/resource ownership.
4. Make server auto-attack timing authoritative.

High-value simplifications:
1. Replace reflection-only semantics with one small explicit AI semantics doc and one combat semantics doc.
2. Remove no-op collectors and duplicate target binders in AI actions.
3. Split `ClientPassiveAutoAttackPresentationSystem`.
4. Centralize repeated "require resource / get debug log" helpers.

Low-priority cleanup:
1. Rename `AiTaskState.Task` and `AiTaskState.ActiveTask` to clearer names.
2. Replace byte presentation flags in `AiNetState` with explicit enums.
3. Move hardcoded combat constants into shared config/state.
