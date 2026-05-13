# Feature Layer Guide

Short operational rules for `StaticMlp.Features.*` modules. Keep this file small; put deeper explanations in `ai/*.md`.

## Purpose

This layer contains ordinary gameplay features such as player, buildings, inventory, input, and view-facing feature glue.

Each feature should be understandable as an isolated module with explicit runtime boundaries.

## Feature Shape

- Prefer one feature per top-level folder and one asmdef per feature runtime module.
- Follow `.agents/skills/staticmlp-code-writing/SKILL.md` for the canonical feature folder layout.
- New files, and existing files touched during a change, must use an approved type/role bucket such as `Components`, `Events`, `Systems`, `Factories`, `WorldResources`, `Ids`, `EntityTypes`, `NetworkEntityTypes`, `Domain`, `Catalogs`, `Definitions`, `Validation`, `Queries`, `Seeds`, `Input`, `Controllers`, `Views`, `ViewParts`, or `Actions`.
- Do not create arbitrary first-level folders inside a feature. If no approved bucket fits, stop and propose either a new feature boundary or a new folder-layout rule.
- Use `WorldResources` for StaticEcs `IResource` scripts; do not create script folders named `Resources`.
- Use `Systems/Client` and `Systems/Server` only when the runtime behavior truly differs by context.
- Register feature systems through `GameplayFeature`, not by editing global bootstrap classes directly.
- Register typed network commands through `GameplayFeature.RegisterNetworkEvents`.

## Boundaries

- Put reusable shared contracts in `Game.Core`, not in an ordinary feature.
- Keep gameplay state and rules inside ECS data and systems.
- Keep presentation code passive and feature-local.
- Do not put packet serialization or Unity Transport calls inside feature gameplay systems.
- Do not let feature `MonoBehaviour` classes become ECS hosts, MVC hosts, or composition roots.
- Reading another feature's public contracts is allowed when the dependency direction is valid.
- Writing another feature's components or tags through `Mut<T>()`, `ReplicationMut.Mut<T>()`, or `ClientProjection.Mut<T>()` is forbidden. Send an `IEvent`; the owner feature handles the event and mutates its own state.

## Data Flow

- Client gameplay reads and writes `LocalOwned` entities.
- Remote presentation reads `RemoteOwned` entities and applies visuals without simulating gameplay.
- Server gameplay reads `ServerOwned` or validated `ClientOwned` entities.
- Interactions without direct ownership should go through typed replicated events, not hidden cross-feature calls.
- Interactions without feature state ownership should also go through StaticEcs events. The requesting feature writes the event; the owning feature reads it, validates invariants, applies `Mut<T>()`, then emits a result or fact event when needed.

## Editing Rules

- Before adding code, decide whether it belongs in feature gameplay, feature presentation, shared `Game`, or `Networking`.
- Prefer explicit names such as `Rules`, `Spawner`, `Request`, or `System`; avoid `Helper` and `Utility`.
- Keep feature behavior in systems, not in free-floating static classes.
- Keep systems focused on one responsibility; split long multi-purpose systems into ordered systems connected by components/events.
- When a feature grows multiple subsystems or invariants, add a local `AGENTS.md` inside that specific feature folder.
- Fail fast on required runtime state; do not add silent early returns for missing bindings or references.

## Where To Start

- Read the feature's `GameplayFeature` entry point first.
- Then inspect `Components` and `Events`.
- Then inspect `Systems/Client` and `Systems/Server`.
- Open feature-specific `AGENTS.md` files when they exist.
