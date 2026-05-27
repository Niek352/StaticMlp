# AiBots Feature Guide

Short operational rules for `StaticMlp.Features.AiBots`. Keep this file small; put deeper rationale in `ai/*.md` if this feature grows more subsystems.

## Purpose

- `AiBots` owns server-authoritative generic AI-agent behavior, navigation coordination, spawn setup, and the thin replicated state used by client presentation.
- `AiBots` does not own product-level NPC logic. NPC class, acquisition path, definition identity, roster records, incubation, rescue, extraction, specialist unlocks, and NPC economy contracts belong to `StaticMlp.Features.Npc` or the feature that owns the concrete gameplay state.
- Bots are gameplay actors first. Client visuals should mirror replicated state, not simulate decisions locally.

## Read First

- `Runtime/AiBotsGameplayFeature.cs`
- `Runtime/Systems/Server/*` except task execution
- `Runtime/Navigation/AiNavigationRuntime.cs`
- `Runtime/Navigation/AiNavigation.md`
- `Runtime/Actions/AiActionCatalog.cs`
- `../AiTaskExecution/CLAUDE.md`
- `../AiActions/Runtime/Actions/*`

## Runtime Shape

- Server simulation currently runs in this order: `Needs -> Perception -> ActionVariables -> UtilityDecision -> TaskExecution -> Navigation -> NetState`.
- Preserve that order when adding systems. Later stages depend on blackboard/task data produced by earlier stages.
- The `TaskExecution` stage is owned by the separate `StaticMlp.Features.AiTaskExecution` feature and still sits between utility selection and navigation.
- Client-side `AiBots` runtime is intentionally thin: prefab registration, replicated `AiNetState`, and passive view parts only.
- Navigation backend details stay behind `IAiNavigationBackend`; gameplay systems should express movement through `AiMoveRequest`, not backend-specific APIs.

## Core Data Roles

- `AiBrain` stores long-lived behavior selection state such as `BehaviorId`, current task, and decision cooldown.
- `Multi<AiBlackboardEntry>` plus `AiBlackboardAccess` stores sensed or derived context that may be rewritten every frame.
- `AiBotSpawnSpec.InitialEnemy` is an explicit spawn-time target. `ServerAiPerceptionSystem` must continue tracking an assigned enemy even outside the normal acquisition radius, unless the target is no longer available.
- `AiTaskState` stores execution progress for the current task, but the execution logic itself lives in `StaticMlp.Features.AiTaskExecution`.
- `AiNetState` is a replicated presentation summary for clients. Do not drive server AI decisions from it.

## Editing Rules

- Add new bot actions end-to-end: update `AiTaskType`, add a localized package under `../AiActions/Runtime/Actions/<ActionName>`, and update `AiNetState` only if clients need to visualize the new state.
- Route player-issued bot control through `CommandBotEvent` and `ServerCommandBotRequestSystem`; keep validation on the server.
- Spawn bots through `FrontierSeed` and `AiBotSpawns`. Do not hand-assemble bot entities in unrelated systems.
- If movement behavior changes, keep decision systems writing `AiMoveRequest` or `AiAttackRequest`, and let `ServerAiNavigationSystem` own `CharacterNetState` replication mutations.
- `AiNavigationRuntime` requires a baked NavMesh and creates its backend by reflected type name. If backend assembly or type names change, update the lookup strings together with the asmdef wiring.
- Keep `Presentation` view-only. Do not move bootstrap, AI decisions, or scene search logic into `MonoBehaviour` classes here.

## Boundaries

- Do not add NPC roster, acquisition, incubation, specialist, companion, or NPC economy source-of-truth state to `AiBots`.
- Do not treat `AiAgentTag` as proof that an entity is a product-level NPC. Use `NpcTag` or `NpcIdentity` from `StaticMlp.Features.Npc` when a system needs NPC identity.
- Do not make `AiBots` depend on `StaticMlp.Features.Npc`; `AiBots` remains the lower-level generic behavior layer.
- Do not put Unity Transport calls, packet serialization, or raw network inbox logic into this feature's gameplay systems.
- Do not store `Entity` across frames; use `EntityGID` in blackboard data, events, spawn relationships, and navigation lookups.
- Fail fast when required resources such as `AiActionCatalog`, `AiNavigationRuntime`, or `FrontierSeed` are missing or null.
- Keep behavior tuning inside `Domain` and ECS systems, not inside presentation or navigation backend implementation details.
- `AiBots` owns decision input and downstream AI state contracts: `AiBrain`, `AiBlackboardEntry`, `AiBlackboardAccess`, `AiActionCatalog`, `AiTaskState`, `AiTaskType`, `AiMoveRequest`, `AiAttackRequest`, and `AiNetState`.
- `AiBots` does not own task execution implementation anymore. Do not reintroduce task-specific execution systems into `AiBots/Runtime/Systems/Server`.

## Where To Start

- For behavior tuning, read `Domain`, `Runtime/Actions`, and `ServerAiUtilityDecisionSystem`.
- For task behavior, read `../AiTaskExecution/Runtime/Systems/Server/ServerAiTaskExecutionSystem.cs` and `../AiActions/Runtime/Actions/*`.
- For pathing and movement application, read `Navigation`, `NavigationBackend`, and `ServerAiNavigationSystem`.
- For client-issued commands, read `Events` and `ServerCommandBotRequestSystem`.
- For bootstrap and spawn flow, read `AiBotsGameplayFeature`, `AiBotSpawns`, and `FrontierSeed`.
- If task changes require new blackboard inputs or new command targets, add them through action-local collectors/binders first, then verify the execution facade still resolves the package correctly.
