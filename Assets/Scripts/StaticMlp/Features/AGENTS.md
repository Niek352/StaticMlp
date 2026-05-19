# Feature Layer Guide

Rules for `StaticMlp.Features.*` modules. This file inherits the root `AGENTS.md`; do not duplicate project-wide ECS, networking, ownership, presentation, or fail-fast rules here.

## Scope

- Use this layer for ordinary gameplay features and feature-local presentation glue.
- Put reusable shared contracts and bootstrap-facing contracts in `Game.Core`, not in an ordinary feature.
- Keep transport, packet serialization, raw inbox/outbox access, and replication infrastructure outside feature gameplay systems.
- Each feature should be understandable as an isolated module with explicit public contracts and runtime boundaries.

## Feature Shape

- Prefer one feature per top-level folder and one asmdef per runtime module.
- Follow `.agents/skills/staticmlp-code-writing/SKILL.md` for the canonical feature folder layout.
- Place new or touched files in established role buckets such as `Components`, `Events`, `Systems`, `Factories`, `WorldResources`, `Ids`, `EntityTypes`, `NetworkEntityTypes`, `Domain`, `Catalogs`, `Definitions`, `Validation`, `Queries`, `Seeds`, `Input`, `Controllers`, `Views`, `ViewParts`, or `Actions`.
- Do not invent arbitrary first-level folders. If no existing role bucket fits, stop and propose either a new feature boundary or a new folder-layout rule.
- Use `WorldResources` for StaticEcs `IResource` scripts; do not create script folders named `Resources`.
- Use `Systems/Client` and `Systems/Server` only when runtime behavior truly differs by context.
- When a feature grows feature-specific invariants, add a local `AGENTS.md` inside that feature folder instead of expanding this file.

## Local Boundaries

- Split `Runtime/Logic` and `Runtime/Presentation` when a feature has both gameplay state and Unity-facing views.
- Keep feature `MonoBehaviour` classes passive: inspector references, Unity callbacks, rendering, and forwarding UI intent only.
- Read another feature's public contracts only when the dependency direction is valid.
- Cross-feature state changes go through typed events or commands owned by the target feature.
- Queries scoped to a concrete entity or network archetype must include `EntityIs<T>` or `EntityIsAny<...>`, unless the system is intentionally generic across entity types.

## Where To Start

- Read the feature's `GameplayFeature` entry point first.
- Then inspect `Components`, `Events`, and public contracts.
- Then inspect `Systems/Client`, `Systems/Server`, and `Runtime/Presentation` if they exist.
- Open feature-specific `AGENTS.md` files before changing a feature that has one.

## Feature Guides

- `AiBots/AGENTS.md`: generic active AI-agent behavior, navigation requests, bot spawning, and thin replicated AI presentation summaries.
- `AiTaskExecution/AGENTS.md`: server-side execution of already selected bot tasks.
- `CombatDirector/AGENTS.md`: encounter pressure, combat cells, threat budget, phase transitions, spawn sources, and enemy spawn requests.
- `Npc/AGENTS.md`: product/domain NPC identity, class, acquisition path, roles, roster, and future NPC economy contracts.
- `OpenWorldGeneration/AGENTS.md`: deterministic chunk generation, LayerProcLite scheduling, spatial cluster streaming, server snapshots, and client terrain presentation.
- `OpenWorldResources/AGENTS.md`: placement indexing, chunk overlays, resource proxy views, legacy replicated resource nodes, and open-world host-spike profiling notes.
