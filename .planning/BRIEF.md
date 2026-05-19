# AiNavigation Feature Brief

## Objective

Build `AiNavigation` as the shared server-authoritative navigation infrastructure for combat encounters, base NPC movement, spawn-source reachability, runtime nav areas, and far/local AI movement handoff.

The feature must preserve the existing local bot movement boundary:

- gameplay/action executors express movement through `AiMoveRequest`;
- `ServerAiNavigationSystem` remains the only system that applies local navigation output into `CharacterNetState`;
- ProjectDawn/Unity.Entities details stay behind `AiNavigationRuntime`, `IAiNavigationBackend`, and backend implementation code.

## Source Context

- `Assets/Scripts/StaticMlp/Features/AiBots/Runtime/Navigation/AiNavigation.md`
- `ai/CombatDirector_AiNavigation_Contract.md`
- `Assets/Scripts/StaticMlp/Features/AiBots/AGENTS.md`
- `ai/ECS_Feature_Architecture_Layout_StaticEcs.md`

## Architecture Decision

Do not expand `StaticMlp.Features.AiBots` into a broad navigation platform. Existing `AiBots` owns generic bot behavior and local bot movement coordination, but the contract also covers combat cells, spawn source reachability, base NPC navigation, runtime nav mesh areas, global routing, and far AI. Those responsibilities should live in a standalone `StaticMlp.Features.AiNavigation` feature with contracts that other gameplay features can consume.

`AiBots` should remain a consumer for local movement, through `AiMoveRequest` and the current `AiNavigationRuntime` backend path.

## Non-Goals

- Do not put `NavMeshBuilder`, `NavMeshData`, or direct `UnityEngine.AI` usage into `CombatDirector`.
- Do not let `AiNavigation` mutate `DirectorPhase`, `ThreatBudget`, enemy composition, rewards, loot, or progression.
- Do not create prefab assets, scene hierarchies, or inspector wiring from agent code.
- Do not run `dotnet build`; use Unity/MCP script validation where available and ask the user to run Unity compile/play checks when full verification is needed.
- Do not manually edit generated replication files.

