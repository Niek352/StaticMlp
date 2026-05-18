# Codex Research — Scope 06 — Production-Ready Direction

> Назначение документа: research/implementation plan для Codex.
> Проект: co-op open world survival-builder на ECS.
> Важное ограничение: все runtime features должны проектироваться через ECS components/systems/resources/events, а не через MonoBehaviour-first архитектуру.
> Сетевой принцип: server-authoritative для боевых/экономических изменений; client-only только для View/VFX/UI/предпросмотра.


## 1. Цель scope

Подготовить план расширения после vertical slice.

Этот scope не должен реализовываться до того, как один biome-complete loop доказан. Его задача — зафиксировать направление production-ready систем, чтобы текущие архитектурные решения не заблокировали будущий рост.

Основные направления:

- Biome taxes.
- Differentiated scarcity chains.
- Base raids.
- Caravans/escort.
- Outpost network.
- Scaling co-op open world через ECS simulation layers.

## 2. Biome taxes

Каждый биом должен добавлять не только новые ресурсы, но и новый постоянный налог.

Примеры:

### Forest Frontier

- Tax: fuel + repair.
- Pressure: resin shortage.
- Countermeasure: better sawmill/resin cooker.

### Ash Quarry

- Tax: water + filters + cooling.
- Pressure: equipment wear.
- Countermeasure: filtration station.

### Rot Bog

- Tax: cleanliness + antidote.
- Pressure: disease/contamination.
- Countermeasure: apothecary/sanitation.

### Glass Tundra

- Tax: heat + food.
- Pressure: freezing logistics.
- Countermeasure: insulation/heating grid.

## 3. ECS model for biome taxes

```csharp
public struct BiomeTaxProfile : IComponent
{
    public BiomeId BiomeId;
    public ResourceId PrimaryTaxResource;
    public float TaxPerSecond;
}

public struct BiomeExposure : IComponent
{
    public BiomeId BiomeId;
    public float ExposureValue;
}

public struct CountermeasureCoverage : IComponent
{
    public CountermeasureId CountermeasureId;
    public float Coverage;
}
```

Systems:

```text
BiomeExposureScanSystem
→ BiomeTaxDemandSystem
→ CountermeasureApplySystem
→ SettlementPressureApplySystem
```

## 4. Base raids

Raids должны быть extension CombatDirector, а не отдельный hack.

### Raid inputs

- settlement wealth.
- stored progression resources.
- noise history.
- biome hostility.
- enemy faction proximity.
- boss/mini-boss progress.

### Raid pipeline

```text
RaidThreatAccumulationSystem
→ RaidScheduleSystem
→ RaidTelegraphSystem
→ RaidSpawnSourceSelectionSystem
→ RaidWaveSpawnSystem
→ RaidResolutionSystem
```

### Components

```csharp
public struct RaidThreat : IComponent
{
    public float Value;
    public float Threshold;
}

public enum RaidPhase : byte
{
    None = 0,
    Warning = 1,
    Approaching = 2,
    Attack = 3,
    Retreat = 4,
    Resolved = 5
}

public struct RaidEvent : IComponent
{
    public RaidId RaidId;
    public RaidPhase Phase;
    public float Timer;
}
```

## 5. Caravans / Escort

Caravans нужны, когда появляется сеть outposts.

### Purpose

- Move resources between bases.
- Create field events.
- Force players to protect economy, not only static base.
- Give Guards/Rangers long-term role.

### ECS model

```csharp
public struct Caravan : IComponent
{
    public SettlementId Source;
    public SettlementId Target;
    public CaravanState State;
}

public struct CaravanCargo : IComponent
{
    public ResourceId ResourceId;
    public int Amount;
}

public struct EscortRequest : IComponent
{
    public CaravanId CaravanId;
    public float ThreatExpected;
}
```

Systems:

```text
CaravanPlanningSystem
→ CargoReservationSystem
→ CaravanSpawnSystem
→ CaravanRouteFollowSystem
→ CaravanThreatDirectorSystem
→ CargoDeliveryApplySystem
```

## 6. Outpost network

Late game should grow through a network, not only one mega-base.

### Outpost roles

- Mining outpost.
- Farming outpost.
- Military outpost.
- Research outpost.
- Biome countermeasure outpost.
- Caravan hub.

### ECS model

```csharp
public enum OutpostRole : byte
{
    Mining = 1,
    Farming = 2,
    Military = 3,
    Research = 4,
    Countermeasure = 5,
    CaravanHub = 6
}

public struct Outpost : IComponent
{
    public OutpostId OutpostId;
    public SettlementId ParentSettlement;
    public OutpostRole Role;
}
```

## 7. Simulation layers

Чтобы мир не стал слишком дорогим:

### Near layer

- Full ECS actors.
- Real combat.
- Real navigation.
- Replication.

### Mid layer

- Simplified groups.
- Abstract pressure packs.
- Limited collision/navigation.

### Far layer

- Facts and counters.
- Threat state.
- Production summaries.
- No full actors.

Systems:

```text
SimulationInterestSystem
→ ActorPromotionSystem
→ ActorDemotionSystem
→ FarStateTickSystem
```

## 8. Architecture rules for scaling

- Static world remains deterministic placements + sparse overlay.
- Dynamic actors exist only when needed.
- Promotion to full entity must be server-authoritative.
- Client proxies/views are never gameplay truth.
- Economy can be summarized for far outposts.
- Combat only full-simulates near active players or active raid/caravan events.

## 9. Implementation order after vertical slice

1. Add biome tax profiles.
2. Add countermeasure station/effects.
3. Add raid threat from settlement wealth.
4. Add raid warning/attack/resolution.
5. Add first outpost type.
6. Add caravan resource transfer.
7. Add escort threat event.
8. Add simulation interest/promote/demote.
9. Add save/load coverage.
10. Add balancing/debug dashboards.

## 10. Acceptance criteria

- Каждый новый биом создает новый тип дефицита.
- Raid связан с wealth/threat, а не случайный таймер.
- Caravan реально переносит ресурсы и может быть атакован.
- Outpost не требует full simulation всегда.
- Actor promotion/demotion не ломает network identity.
- Client never owns authoritative economy/combat state.
