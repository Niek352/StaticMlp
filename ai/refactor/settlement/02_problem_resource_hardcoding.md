# Проблема: Хардкод ресурсов в компонентах и view state

## Краткое описание

Во всей Settlement-фиче ресурсы захардкожены по полям и по if/else цепочкам. Добавление 10-го ресурса потребует правки 8+ файлов, включая replicated component `WorkbenchOperationState`. Это нарушает принцип data-driven design и делает каталоги бессмысленными.

---

## Конкретные места

### 1. WorkbenchOperationState — explicit fields per resource
**Файл:** `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Components/WorkbenchOperationState.cs`

```csharp
public struct WorkbenchOperationState : IComponent
{
    public int InputWood;
    public int InputStone;
    public int InputPlanks;
    public int InputSimpleParts;
    public int InputFuel;

    public int OutputPlanks;
    public int OutputSimpleParts;
    public int OutputRepairKits;
    public int OutputMedicine;
}
```

**Почему это плохо:**
- `WorkbenchOperationState` — gameplay component (возможно replicated, если позже добавят атрибут). Любое изменение struct требует обновления generated replication.
- Новый ресурс = новое поле + новый `if` в `WorkbenchResourceAccess`.

### 2. WorkbenchResourceAccess — if/else mapping
**Файл:** `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Domain/WorkbenchResourceAccess.cs`

```csharp
public static int GetInput(in WorkbenchOperationState state, ResourceId resourceId)
{
    if (resourceId == ResourceCatalog.WoodId) return state.InputWood;
    if (resourceId == ResourceCatalog.StoneId) return state.InputStone;
    if (resourceId == ResourceCatalog.PlanksId) return state.InputPlanks;
    if (resourceId == ResourceCatalog.SimplePartsId) return state.InputSimpleParts;
    if (resourceId == ResourceCatalog.FuelId) return state.InputFuel;
    return 0;
}
```

То же самое для `GetOutput`.

### 3. ConstructionViewState — только Wood и Stone
**Файл:** `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Components/ConstructionViewState.cs`

```csharp
public struct ConstructionViewState : IViewComponent
{
    public int WoodRequired;
    public int StoneRequired;
    public int WoodDelivered;
    public int StoneDelivered;
    public float Progress01;
}
```

**Почему это плохо:**
- `BuildingDefinition.ConstructionCost` поддерживает `ResourceAmount[]` (любые ресурсы).
- `ConstructionResources` (multi-component `ConstructionResourceEntry`) хранит `Required`/`Delivered` для любого ресурса.
- Но view state обрезает всё до Wood/Stone. Если добавить здание, стоящее Planks — UI не покажет прогресс по Planks.

### 4. ClientConstructionViewStateSystem — явное чтение WoodId/StoneId
**Файл:** `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Client/ClientConstructionViewStateSystem.cs`

```csharp
WoodRequired = ConstructionResourcesAccess.GetProjectedRequired(e, ResourceCatalog.WoodId),
StoneRequired = ConstructionResourcesAccess.GetProjectedRequired(e, ResourceCatalog.StoneId),
WoodDelivered = ConstructionResourcesAccess.GetProjectedDelivered(e, ResourceCatalog.WoodId),
StoneDelivered = ConstructionResourcesAccess.GetProjectedDelivered(e, ResourceCatalog.StoneId),
```

### 5. Stage1ContextPanelState — Wood/Stone fields
**Файл:** `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Stage1ContextPanelState.cs`

```csharp
public int WoodRequired;
public int StoneRequired;
public int WoodDelivered;
public int StoneDelivered;
```

### 6. Stage1HudState — Wood/Stone fields
**Файл:** `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Stage1HudState.cs`

```csharp
public int Wood;
public int Stone;
```

### 7. ClientStage1HudStateSystem — явное чтение WoodId/StoneId
**Файл:** `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/ClientStage1HudStateSystem.cs`

```csharp
Wood = SettlementSharedResourcesAccess.GetProjectedAmount(resources, ResourceCatalog.WoodId),
Stone = SettlementSharedResourcesAccess.GetProjectedAmount(resources, ResourceCatalog.StoneId),
```

### 8. ExtractionRules — hardcoded building → resource
**Файл:** `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Domain/ExtractionRules.cs`

```csharp
public static bool TryGetOutputResource(BuildingId buildingId, out ResourceId resourceId)
{
    if (buildingId == BuildingCatalogData.LumberCampId) { resourceId = ResourceCatalog.WoodId; return true; }
    if (buildingId == BuildingCatalogData.StoneMineId) { resourceId = ResourceCatalog.StoneId; return true; }
    resourceId = default; return false;
}
```

**Почему это плохо:**
- Новое extraction здание (например, `IronMine`) требует правки `ExtractionRules`.
- Здание само по себе в `BuildingDefinition` уже знает, что оно `ExtractsFromNode`, но не декларирует `OutputResourceId`.

### 9. ResourceLabel дублирование
**Файлы:**
- `ClientStage1ContextPanelStateSystem.ResolveResourceLabel()`
- `BuildingMenuPresentation.ResourceLabel()`

Оба содержат идентичные if/else цепочки на 9 ресурсов. Причина: `ResourceDefinition` не содержит `DisplayName`.

---

## План рефактора

### Шаг 1. Добавить DisplayName в каталоги
- `ResourceDefinition` → добавить `string DisplayName`.
- `BuildingDefinition` → убедиться, что `DisplayName` есть (уже есть, но используется ли для category? Нет, category тоже нужен label).
- Удалить `ResolveResourceLabel()` и `ResourceLabel()` из presentation систем; читать из `ResourceCatalog.Get(id).DisplayName`.

### Шаг 2. WorkbenchOperationState → multi-component
Ввести `WorkbenchResourceEntry`:
```csharp
public struct WorkbenchResourceEntry : IMultiComponent
{
    public ResourceId Id;
    public int Amount;
    public WorkbenchResourceRole Role; // Input / Output
}
```

Или два отдельных multi-component: `WorkbenchInputResource` / `WorkbenchOutputResource`.

Удалить explicit fields из `WorkbenchOperationState`. Оставить только:
```csharp
public struct WorkbenchOperationState : IComponent
{
    public ushort ActiveRecipeId;
    public bool Enabled;
    public byte WorkerSlotCount;
    public float WorkDone;
}
```

`WorkbenchResourceAccess.GetInput/Output(entity, resourceId)` — ищет в multi rows по `ResourceId`.

**Риск:** если `WorkbenchOperationState` уже replicated, нужно убедиться, что codegen поддерживает multi-component replication. Если нет — альтернатива: сгенерировать поля автоматически из `ResourceCatalog` через Source Generator (см. Option C в общем плане).

### Шаг 3. ConstructionViewState → generic array
```csharp
public struct ConstructionResourceViewEntry
{
    public ResourceId Id;
    public int Required;
    public int Delivered;
}

public struct ConstructionViewState : IViewComponent
{
    public ConstructionPhase Phase;
    public ConstructionResourceViewEntry[] Resources; // или FixedList64 / span
    public float Progress01;
}
```

`ClientConstructionViewStateSystem` — итерировать `ConstructionResources` multi rows и копировать в массив.

### Шаг 4. Stage1ContextPanelState / Stage1HudState → generic arrays
Аналогично шагу 3:
- `Stage1ContextPanelState` — заменить `WoodRequired/StoneRequired/WoodDelivered/StoneDelivered` на `ConstructionResourceViewEntry[] ConstructionResources`.
- `Stage1HudState` — заменить `Wood/Stone` на `SettlementResourceViewEntry[] Resources`.

### Шаг 5. ExtractionRules → data-driven
Добавить в `BuildingOperationDefinition`:
```csharp
public ResourceId OutputResourceId; // валидно только если ProducesResources + ExtractsFromNode
```

Удалить if/else из `ExtractionRules.TryGetOutputResource`. Сделать:
```csharp
public static bool TryGetOutputResource(BuildingId buildingId, out ResourceId resourceId)
{
    var def = BuildingCatalogData.Get(buildingId);
    resourceId = def.Operation.OutputResourceId;
    return resourceId.Value != 0;
}
```

Обновить `BuildingCatalogValidator` — проверять, что extraction building имеет `OutputResourceId != 0` и что ресурс имеет флаг `ProductionOutput`.

---

## Что сломается, если не рефакторить

1. Добавление нового ресурса (например, `IronOre`) потребует изменений минимум в 8 файлах.
2. Добавление нового extraction здания (`IronMine`) потребует правки `ExtractionRules`.
3. Добавление здания со стоимостью в `Planks` приведёт к тому, что Construction UI будет показывать только Wood/Stone, скрывая недостающие Planks.
4. Невозможно автоматически генерировать UI для новых ресурсов — каждый раз нужен программист.
