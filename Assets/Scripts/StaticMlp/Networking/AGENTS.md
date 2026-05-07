# Networking Layer Guide

Short operational rules for `StaticMlp.Networking.*`. Keep this file small; put protocol details and architecture notes in `ai/*.md`.

## Purpose

This layer owns transport, replication, ownership derivation, network requests, and the contracts that connect ECS state to packets.

Gameplay code should depend on its public contracts, not on its internal transport details.

## Submodules

- `Ownership`: derive local runtime tags from replicated identity and authority.
- `ReplicationContracts`: shared replicated component and event contracts.
- `Runtime/Replication`: apply incoming state and collect outgoing deltas.
- `Runtime/Transport`: Unity Transport startup, send, receive, scheduling, and completion.
- `Runtime/Requests`: explicit networking requests and orchestration inputs.
- `Runtime/Diagnostics`: tooling and debugging support.

## Boundaries

- Transport code may read and write raw bytes; gameplay code must not.
- Replication code converts packets to ECS changes and ECS changes to packets.
- Ownership code derives tags from `NetworkIdentity`; ownership tags themselves are not replicated.
- Feature and game systems should use typed replicated components and events, not raw inbox or outbox access.

## Invariants

- Keep frame flow aligned with the global order: complete jobs, drain inbox, apply state, run gameplay, collect dirty state, send, schedule, tick.
- Mutate replicated state through `Mut<T>()` in gameplay systems.
- Use reliable delivery for spawn, despawn, ownership, inventory, and important events.
- Use unreliable sequenced delivery for frequent state such as movement.
- Unexpected missing runtime resources or registrations are bugs; fail fast with explicit exceptions.

## Editing Rules

- Do not move gameplay rules into networking just because the change is multiplayer-related.
- Do not let transport code depend on feature-specific presentation or UI types.
- Keep packet shape and replicated contracts stable and intentional.
- When changing registrations, ensure all component, tag, event, and link types are registered before world initialization.

## Where To Start

- For ownership behavior, open `Ownership`.
- For replicated type flow, open `ReplicationContracts` and `Runtime/Replication`.
- For send and receive behavior, open `Runtime/Transport`.
- For feature-facing entry points, inspect request and registration code first.
