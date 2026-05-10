# Game Layer Guide

Short operational rules for `StaticMlp.Game.*`. Keep this file small; put deeper rationale in `ai/*.md`.

## Purpose

This layer contains shared gameplay and bootstrap-facing contracts that are common across features.

Use it for stable core data shapes and reusable gameplay abstractions, not as a dumping ground for unrelated code.

## What Belongs Here

- Core gameplay components shared by multiple features.
- Shared requests, events, and network entity type definitions.
- Reusable bootstrap-facing feature registration contracts.
- Shared presentation contracts that are genuinely cross-feature.
- Shared entity-reference contracts should use `EntityGID` directly; raw ids are allowed only inside explicit packet/codegen serialization boundaries.

## What Does Not Belong Here

- Ordinary feature-specific gameplay that can live in `StaticMlp.Features.*`.
- Networking internals, packet logic, or Unity Transport code.
- Composition-only scene wiring or UI lifecycle ownership.
- Concrete prefab paths, scene object lookups, or other Unity authoring details in domain rules.

## Boundaries

- `Game` defines common contracts.
- `Features` implement ordinary gameplay modules on top of those contracts.
- `Networking` delivers and replicates the state.
- `Composition` wires worlds, services, and Unity-facing runtime setup.
- Feature-local gameplay and replication belong in `Features/*/Runtime/Logic`.
- Feature-local client visuals and view-state assembly belong in `Features/*/Runtime/Presentation`.
- `Game.Core` may be referenced by both layers, but it must not collapse their responsibilities back together.

## Editing Rules

- If a type is only used by one feature, keep it in that feature until reuse is real.
- Keep shared rules generic and independent from transport, presentation runtime objects, and editor wiring.
- Prefer clear module names and explicit responsibilities over generic shared helpers.
- When adding shared gameplay systems, confirm they are truly cross-feature and not feature glue in disguise.

## Where To Start

- Read `Features` when you need the registration and composition surface.
- Read `Components`, `Requests`, and `Replication` for shared gameplay contracts.
- Read `Systems/Client` and `Systems/Server` only for shared cross-feature gameplay behavior.
- Open feature modules when the change is not genuinely shared.
