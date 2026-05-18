# Codex Research — Scope 03 — NPC Economy Prototype

> Назначение документа: research/implementation plan для Codex.
> Проект: co-op open world survival-builder на ECS.
> Важное ограничение: все runtime features должны проектироваться через ECS components/systems/resources/events, а не через MonoBehaviour-first архитектуру.
> Сетевой принцип: server-authoritative для боевых/экономических изменений; client-only только для View/VFX/UI/предпросмотра.


## 1. Цель scope

Сделать ECS-прототип базы как “машины дефицита”, где NPC не просто генерируют ресурсы, а закрывают производственные цепочки.

MVP scope:

- Board of priorities.
- 4 роли NPC: Gatherer, Hauler, Processor, Guard.
- 3 постоянные утечки: Food, Fuel, Durability/Wear.
- Минимум 1 progression recipe, который требует Specialist.
- Server-authoritative resource changes.
- ECS job/task pipeline для NPC.

## 2. Core idea

NPC economy должна отвечать на вопрос: “почему NPC нужны всегда?”

Ответ:

- Ресурсы не только добываются, но и тратятся.
- Производство упирается в роли, станции, расстояния и specialist knowledge.
- База не должна быстро становиться self-sufficient без player choices.
- Игрок управляет приоритетами, а не каждым шагом NPC.

## 3. Gameplay model

### 3.1 Board of priorities

Игрок задает веса направлений:

- Food.
- Fuel.
- Construction.
- Repair.
- Ammo.
- Research.
- Incubation.
- Defense.

NPC выбирают задачи по:

```text
score = priorityWeight * taskUrgency * npcAptitude * distanceFactor * stationAvailability
```

### 3.2 NPC roles MVP

1. **Gatherer** — добывает raw resources из resource nodes.
2. **Hauler** — переносит ресурсы между node → storage → station → construction.
3. **Processor** — работает на station, превращает raw в refined.
4. **Guard** — патрулирует, реагирует на raid/pressure.

### 3.3 Leaks MVP

1. **Food upkeep** — каждый NPC/companion расходует food per tick/day.
2. **Fuel upkeep** — stations расходуют fuel при processing.
3. **Durability/Wear** — stations/buildings/tools теряют durability при использовании.

## 4. ECS data model

```csharp
[Flags]
public enum NpcWorkRoleFlags : ushort
{
    None = 0,
    Gatherer = 1 << 0,
    Hauler = 1 << 1,
    Processor = 1 << 2,
    Guard = 1 << 3,
    Builder = 1 << 4,
    Researcher = 1 << 5,
}

public enum EconomyTaskType : byte
{
    Gather = 1,
    Haul = 2,
    Process = 3,
    Guard = 4,
    Repair = 5,
    Feed = 6,
    Refuel = 7,
}

public struct NpcWorker : IComponent
{
    public NpcWorkRoleFlags Roles;
    public float WorkSpeed;
}

public struct NpcNeeds : IComponent
{
    public float Food;
    public float Rest;
    public float Morale;
}

public struct CurrentEconomyTask : IComponent
{
    public Entity TaskEntity;
    public EconomyTaskType TaskType;
}

public struct EconomyTask : IComponent
{
    public EconomyTaskType Type;
    public float3 Position;
    public float Urgency;
    public int RequiredRoleMask;
}

public struct StorageInventory : IComponent
{
    public StorageId StorageId;
}

public struct ResourceStack : IComponent
{
    public ResourceId ResourceId;
    public int Amount;
}

public struct ProcessingStation : IComponent
{
    public StationId StationId;
    public RecipeId ActiveRecipe;
}

public struct Durability : IComponent
{
    public float Current;
    public float Max;
}

public struct RequiresSpecialist : IComponent
{
    public SpecialistKnowledgeId KnowledgeId;
}
```

## 5. Systems pipeline

```text
SettlementNeedScanSystem
→ UpkeepLeakSystem
→ StationFuelDemandSystem
→ DurabilityWearSystem
→ EconomyTaskGenerationSystem
→ EconomyTaskScoringSystem
→ NpcTaskAssignmentSystem
→ NpcTaskExecutionSystem
→ ResourceTransferApplySystem
→ RecipeUnlockApplySystem
→ EconomyReplicationSystem
```

## 6. System details

### SettlementNeedScanSystem

Считает агрегированное состояние базы:

- сколько food доступно.
- сколько fuel доступно.
- сколько repair demand.
- какие stations простаивают.
- какие recipes locked.

### UpkeepLeakSystem

Server-side tick:

- NPC consume food.
- Companions consume food/special feed.
- Если food не хватает: снижать work speed/morale или добавлять Hunger component.

### StationFuelDemandSystem

Если station имеет active recipe:

- проверить fuel.
- создать Refuel task, если fuel below threshold.
- processor не должен работать без fuel, если recipe требует fuel.

### DurabilityWearSystem

При работе station/building/tool:

- уменьшить durability.
- если ниже threshold — создать Repair task.
- если 0 — station disabled.

### EconomyTaskGenerationSystem

Создает task entities:

- Gather task для marked resource nodes.
- Haul task для ресурсов не в storage.
- Process task для recipe queue.
- Guard task для patrol/defense point.
- Repair task для low durability.
- Feed/Refuel task для shortages.

### NpcTaskAssignmentSystem

Назначает idle NPC на highest score task:

- Не назначать task двум NPC, если task не supports multi-worker.
- Учитывать role mask.
- Учитывать reserved resources.

### NpcTaskExecutionSystem

Не должен напрямую менять ресурсы без apply step.

```text
Task execution produces EconomyOperationRequest
EconomyOperationValidationSystem validates
ResourceTransferApplySystem applies authoritative changes
```

## 7. Commands / Events

```csharp
public struct SetEconomyPriorityCommand : IDomainCommand
{
    public SettlementId SettlementId;
    public EconomyPriorityKind Kind;
    public float Weight;
}

public struct QueueRecipeCommand : IDomainCommand
{
    public SettlementId SettlementId;
    public StationId StationId;
    public RecipeId RecipeId;
    public int Count;
}

public struct AssignNpcRoleCommand : IDomainCommand
{
    public Entity Npc;
    public NpcWorkRoleFlags Roles;
}

public struct EconomyOperationRequest : IComponent
{
    public EconomyOperationType Type;
    public Entity Source;
    public Entity Target;
    public ResourceId ResourceId;
    public int Amount;
}
```

## 8. Networking

Server authoritative:

- Inventory/resource changes.
- Task assignment.
- Upkeep consumption.
- Station processing.
- Recipe unlocks.
- Durability.

Replicated:

- NPC CurrentTask/LocomotionState/CombatState.
- Storage summary.
- Priority board.
- Station state.
- Shortage alerts.

Client-only:

- UI task board.
- NPC overhead icons.
- hauling preview lines.
- station progress bars.
- low-resource alerts.

## 9. Tests

- Food is consumed per tick.
- Fuel shortage creates refuel task.
- Low durability creates repair task.
- Gatherer does not process recipe.
- Processor does not work without fuel.
- Specialist unlocks recipe.
- Without specialist recipe stays locked.
- Hauler moves resource from node to storage.
- Server applies resource changes, client only observes.

## 10. Implementation order for Codex

1. Найти текущие AI task components.
2. Не ломать existing AI; добавить economy layer поверх task-oriented AI.
3. Добавить priority board resource.
4. Добавить role components.
5. Добавить resource inventory API через ECS-friendly operations.
6. Добавить upkeep leak.
7. Добавить task generation.
8. Добавить scoring/assignment.
9. Добавить simple task execution.
10. Добавить specialist recipe lock.
11. Добавить UI/debug read model.
12. Добавить tests.

## 11. Acceptance criteria

- Игрок может менять priority board.
- NPC сами выбирают задачи по ролям.
- База потребляет food/fuel.
- Stations изнашиваются.
- Hauler реально нужен для доставки.
- Есть рецепт, который не работает без specialist.
- Gameplay state не хранится в MonoBehaviour.
- Все ресурсные изменения идут через server-authoritative ECS apply step.
