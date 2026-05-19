# Codex Research — Scope 04 — Loadout/Equipment Modules

> Назначение документа: research/implementation plan для Codex.
> Проект: co-op open world survival-builder на ECS.
> Важное ограничение: все runtime features должны проектироваться через ECS components/systems/resources/events, а не через MonoBehaviour-first архитектуру.
> Сетевой принцип: server-authoritative для боевых/экономических изменений; client-only только для View/VFX/UI/предпросмотра.


## 1. Цель scope

Создать ECS-систему активируемых модулей билда, где игрок хранит библиотеку предметов, но в экспедицию/бой активирует только ограниченное число слотов.

MVP:

- Module library.
- Slot-limited active loadout.
- 3 Combat modules.
- 2 Utility modules.
- 2 BaseSignal modules.
- 4 BaseInfrastructure modules.
- Server-side validation.
- ECS effect application.
- Client UI только для выбора/preview.

## 2. Module categories

### Combat module

Влияет на бой:

- projectile behavior.
- beam attack.
- AoE.
- status proc.
- summon/mines.

### Utility module

Влияет на mobility/survival/harvest convenience:

- dash.
- auto aim assist.
- resource magnet.
- shield pulse.
- extraction speed.

### BaseSignal module

Связывает бой/экспедицию с базой:

- beacon for pressure packs.
- temporary turret marker.
- extraction marker.
- caravan call.
- mining overcharge.

### BaseInfrastructure module

Активируется на базе/settlement:

- station aura.
- work speed boost.
- storage routing.
- morale/fuel/repair modifier.

## 3. ECS data model

```csharp
public struct ModuleDefinitionRef : IComponent
{
    public ModuleId ModuleId;
}

public struct PlayerModuleLibrary : IComponent
{
    public PlayerId PlayerId;
}

public struct ModuleOwned : IComponent
{
    public PlayerId Owner;
    public ModuleId ModuleId;
}

public struct ActiveLoadoutSlot : IComponent
{
    public PlayerId PlayerId;
    public EquipmentSlotKind Kind;
    public byte Index;
    public ModuleId ModuleId;
}

public struct ModuleRuntimeState : IComponent
{
    public float CooldownRemaining;
    public int StackCount;
    public float DurationRemaining;
}

public struct ModuleEffectRequest : IComponent
{
    public PlayerId SourcePlayer;
    public ModuleId ModuleId;
    public Entity Source;
    public Entity Target;
    public float3 Position;
}

public struct BaseSignal : IComponent
{
    public BaseSignalType Type;
    public PlayerId Owner;
    public float3 Position;
    public float Duration;
}
```

## 4. Module examples for vertical slice

### Brimcore Projector

- Slot: Combat.
- Effect: charged frontal beam.
- Secondary: drills resource nodes in a line.
- ECS: `ChargedBeamAttack`, `BeamDamageEffect`, `BeamHarvestEffect`.
- Server validates hit/damage/harvest.
- Client predicts charge VFX only.

### Halo Compass

- Slot: Utility.
- Effect: soft target steering for auto-attacks/resource aim.
- ECS: `AimAssistModifier`.
- Client can use for input assistance.
- Server must not trust impossible hits from client.

### Microwave Kiln

- Slot: Combat or BaseInfrastructure variant.
- Combat effect: heat aura tick.
- Base effect: station heat/fuel efficiency.
- ECS: `AuraEffectEmitter`, `HeatDamageEffect`, `StationFuelModifier`.

### Mine Garden

- Slot: Combat/BaseSignal.
- Effect: places explosive mines in chokepoints/resource nodes.
- ECS: `MinePlacementRequest`, `ArmedMine`, `ExplosionEffect`, `NodeCrackHarvestEffect`.

### Magic Hat Annex

- Slot: Utility.
- Effect: reroll/rarity modifier for upgrade offers.
- ECS: `OfferRarityModifier`, `RerollCharges`.

### Boombox Beacon

- Slot: BaseSignal.
- Effect:
  - combat: attracts pressure packs to killzone.
  - economy: boosts work rhythm in radius.
- ECS: `AggroBeacon`, `WorkSpeedAura`, `NoiseEmitter`.

## 5. Systems pipeline

### Loadout

```text
ModuleUnlockSystem
→ LoadoutCommandValidationSystem
→ ActiveLoadoutApplySystem
→ LoadoutReplicationSystem
```

### Combat effects

```text
PlayerInputAbilitySystem
→ ModuleTriggerSystem
→ ModuleCooldownValidationSystem
→ ModuleEffectRequestSystem
→ ModuleEffectApplySystem
→ CombatEffectPipeline
```

### Base effects

```text
BaseInfrastructureModuleScanSystem
→ SettlementModifierBuildSystem
→ EconomySystem modifiers
```

## 6. Commands

```csharp
public struct EquipModuleCommand : IDomainCommand
{
    public PlayerId PlayerId;
    public ModuleId ModuleId;
    public EquipmentSlotKind SlotKind;
    public byte SlotIndex;
}

public struct UnequipModuleCommand : IDomainCommand
{
    public PlayerId PlayerId;
    public EquipmentSlotKind SlotKind;
    public byte SlotIndex;
}

public struct TriggerModuleCommand : IDomainCommand
{
    public PlayerId PlayerId;
    public ModuleId ModuleId;
    public Entity Source;
    public float3 AimPoint;
}
```

## 7. Validation rules

Equip:

- Player owns module.
- Module is unlocked.
- Module supports slot kind.
- Slot index exists.
- No conflict with exclusive group.
- BaseInfrastructure cannot be equipped into Combat.

Trigger:

- Module is active.
- Cooldown ready.
- Source entity owned/controlled by player.
- Target/position is valid.
- Server recalculates authoritative effect.

## 8. Effect architecture

Modules should not directly mutate health/resources. They should create effect requests.

```text
Brimcore Projector
→ ModuleEffectRequest
→ BeamQuerySystem
→ DamageEffect entities
→ DamageApplySystem
→ Health replication
```

For resource harvesting:

```text
BeamHarvestEffect
→ HarvestEffectRequest
→ ResourceNodeDamageApplySystem
→ ResourceDrop/Inventory operation
```

## 9. Networking

Server authoritative:

- Equipped loadout.
- Cooldowns.
- Damage.
- Harvest.
- spawned mines/beacons.
- offer/reroll outcomes.

Client-only:

- selection UI.
- cooldown visuals.
- charge VFX.
- aim assist visuals.
- predicted placement ghost.

## 10. Tests

- Cannot equip module without ownership.
- Cannot equip wrong slot kind.
- Cannot exceed slot count.
- Module trigger respects cooldown.
- Brimcore creates damage effect but does not directly mutate health.
- BaseInfrastructure modifier affects economy only when active.
- Client cannot trigger inactive module.

## 11. Implementation order for Codex

1. Найти existing effect/combat pipeline.
2. Добавить ModuleDefinitionCatalog.
3. Добавить owned/unlocked module state.
4. Добавить active loadout ECS state.
5. Добавить equip/unequip commands.
6. Добавить validation.
7. Добавить ModuleEffectRequest pipeline.
8. Реализовать 2 MVP modules first:
   - Halo Compass as simple modifier.
   - Boombox Beacon as signal/noise/work aura.
9. Затем Brimcore/Mine Garden/Microwave.
10. Добавить debug UI/read model.
11. Добавить tests.

## 12. Acceptance criteria

- Игрок имеет библиотеку модулей.
- Активны только слоты по лимиту.
- Нельзя сломать лимит через command.
- Модуль не меняет combat/resource state напрямую.
- Все gameplay effects идут через ECS effect pipeline.
- Client UI не является authoritative.
