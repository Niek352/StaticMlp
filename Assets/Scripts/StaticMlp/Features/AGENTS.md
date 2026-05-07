# Feature Layer Guide

Short operational rules for `StaticMlp.Features.*` modules. Keep this file small; put deeper explanations in `ai/*.md`.

## Purpose

This layer contains ordinary gameplay features such as player, buildings, inventory, input, and view-facing feature glue.

Each feature should be understandable as an isolated module with explicit runtime boundaries.

## Feature Shape

- Prefer one feature per top-level folder and one asmdef per feature runtime module.
- Typical runtime split is `Components`, `Events`, `Requests`, `Systems`, `Presentation`, and optional `Domain`, `Input`, or `NetworkEntityTypes`.
- Use `Systems/Client` and `Systems/Server` only when the runtime behavior truly differs by context.
- Register feature systems through `GameplayFeature`, not by editing global bootstrap classes directly.
- Register typed network commands through `GameplayFeature.RegisterNetworkEvents`.

## Boundaries

- Put reusable shared contracts in `Game.Core`, not in an ordinary feature.
- Keep gameplay state and rules inside ECS data and systems.
- Keep presentation code passive and feature-local.
- Do not put packet serialization or Unity Transport calls inside feature gameplay systems.
- Do not let feature `MonoBehaviour` classes become ECS hosts, MVC hosts, or composition roots.

## Data Flow

- Client gameplay reads and writes `LocalOwned` entities.
- Remote presentation reads `RemoteOwned` entities and applies visuals without simulating gameplay.
- Server gameplay reads `ServerOwned` or validated `ClientOwned` entities.
- Interactions without direct ownership should go through typed replicated events, not hidden cross-feature calls.

## Editing Rules

- Before adding code, decide whether it belongs in feature gameplay, feature presentation, shared `Game`, or `Networking`.
- Prefer explicit names such as `Rules`, `Spawner`, `Request`, or `System`; avoid `Helper` and `Utility`.
- When a feature grows multiple subsystems or invariants, add a local `AGENTS.md` inside that specific feature folder.
- Fail fast on required runtime state; do not add silent early returns for missing bindings or references.

## Where To Start

- Read the feature's `GameplayFeature` entry point first.
- Then inspect `Components` and `Events`.
- Then inspect `Systems/Client` and `Systems/Server`.
- Open feature-specific `AGENTS.md` files when they exist.
