# Combat Feature Contracts

Date: 2026-05-09

## Core rule

```text
Ability does not mutate Health.
Hit does not mutate Health.
Status does not mutate Health directly.
Only apply systems mutate replicated state.
```

## Runtime flow

Player flow:

```text
Client local target/prediction
-> UseAbilityCommand
-> ServerReceiveCombatCommandsSystem
-> ServerValidateCombatCommandsSystem
-> ServerAbilityCastSystem
-> ServerHitToEffectSystem
-> ServerEffectPreprocessSystem
-> ServerSynergyTriggerSystem
-> ServerAddStatusApplySystem / ServerStatusTickSystem / ServerAreaEffectTickSystem
-> ServerDamageApplySystem
-> replication + DamageNumberEvent / DeathEvent
```

AI flow:

```text
AiAttackRequest
-> ServerAiAttackRequestSystem
-> shared CombatAbilityRequest ingress
-> same server combat pipeline as players
```

## Effect lifecycle

- `EffectCommands` creates short-lived effect entities with `EffectTag`.
- `EffectChainData` carries root id and current chain depth.
- `ServerEffectPreprocessSystem` rejects invalid targets and over-depth chains.
- `ServerDamageApplySystem` is the only place where `DamageEffect` changes `Health`.
- `ServerAddStatusApplySystem` is the only place where `AddStatusEffect` changes status components.
- `ServerEffectCleanupSystem` destroys processed or rejected effects at the end of the frame.

## Status model

- MVP statuses use dedicated replicated components: `PoisonStatus`, `BurningStatus`, `OiledStatus`.
- Tick-based statuses create follow-up `DamageEffect` entities instead of mutating `Health` inline.
- Fire + Oil synergy is handled by `ServerSynergyTriggerSystem`, which creates `BurningStatus` plus a burning-pool `AreaEffectState`.

## Prediction and feedback

- Client auto-attack still predicts locally through existing tracer/presentation systems.
- `UseAbilityCommand.ClientCommandId` is the reconcile key.
- `DamageNumberEvent` and `DeathEvent` confirm server-side results back to the local attacker.
- `LocalCombatPredictionState` stores last predicted and confirmed command ids.

## Presentation

- Internal effect entities are not replicated.
- Status feedback is derived on the client through `CombatViewState`.
- `CombatCharacterViewPart` renders lightweight runtime-only aura feedback for poison, burning, and oiled states.
