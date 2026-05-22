# Проблема: Не-системный подход к Interaction

## Краткое описание

Presentation layer (Context Panel) решает, какие действия доступны для здания, какой из них primary, какой label, какой summary — всё через `switch` и `if/else` цепочки, захардкоженные в `ClientStage1ContextPanelStateSystem` и `Stage1ContextPanelController`. Добавление нового `BuildingInteractionKind` требует правки минимум 4 методов.

---

## Конкретные места

### 1. Priority ladder для primary action (completed building)
**Файл:** `ClientStage1ContextPanelStateSystem.SelectCompletedPrimaryAction()`

```csharp
private static BuildingInteractionKind SelectCompletedPrimaryAction(in BuildingDefinition definition)
{
    if (HasInteraction(in definition, BuildingInteractionKind.OpenProductionQueue))
        return BuildingInteractionKind.OpenProductionQueue;
    if (HasInteraction(in definition, BuildingInteractionKind.StoreItems))
        return BuildingInteractionKind.StoreItems;
    if (HasInteraction(in definition, BuildingInteractionKind.AssignBed))
        return BuildingInteractionKind.AssignBed;
    if (HasInteraction(in definition, BuildingInteractionKind.Extract))
        return BuildingInteractionKind.Extract;
    if (HasInteraction(in definition, BuildingInteractionKind.Rest))
        return BuildingInteractionKind.Rest;
    if (HasInteraction(in definition, BuildingInteractionKind.AssignWorker))
        return BuildingInteractionKind.AssignWorker;
    if (HasInteraction(in definition, BuildingInteractionKind.OpenDetails))
        return BuildingInteractionKind.OpenDetails;

    throw new InvalidOperationException("...");
}
```

**Проблема:** приоритет определяется порядком строк кода. Чтобы изменить приоритет (например, сделать `Extract` важнее `StoreItems`), нужно менять код.

### 2. Label mapping через switch
**Файл:** `ClientStage1ContextPanelStateSystem.ResolveLabel()`

```csharp
private static string ResolveLabel(BuildingInteractionKind kind)
{
    switch (kind)
    {
        case BuildingInteractionKind.OpenDetails: return "Open";
        case BuildingInteractionKind.DepositConstructionResources: return "Deposit";
        case BuildingInteractionKind.ContributeBuildWork: return "Build";
        // ... ещё 11 case
        default: return string.Empty;
    }
}
```

### 3. Summary builder через switch
**Файл:** `ClientStage1ContextPanelStateSystem.ResolveOpenedBuildingActionSummary()`

```csharp
private static string ResolveOpenedBuildingActionSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
{
    switch (kind)
    {
        case BuildingInteractionKind.OpenDetails:
            return BuildDetailsSummary(in definition);
        case BuildingInteractionKind.AssignWorker:
            return $"Worker slots: {definition.NpcProfile.WorkerSlots}";
        case BuildingInteractionKind.OpenProductionQueue:
        case BuildingInteractionKind.SetRecipe:
        case BuildingInteractionKind.ClaimOutput:
            return BuildProductionSummary(in definition);
        case BuildingInteractionKind.AssignBed:
        case BuildingInteractionKind.Rest:
            return $"Bed slots: {definition.Operation.WorkerSlots}";
        case BuildingInteractionKind.Extract:
            return BuildExtractionSummary(in definition);
        case BuildingInteractionKind.StoreItems:
        case BuildingInteractionKind.WithdrawItems:
            return $"Storage capacity: {definition.Operation.StorageCapacity}";
        // ...
    }
}
```

**Проблема:** summary логика знает о внутреннем устройстве зданий (worker slots, bed slots, storage capacity, recipe list). Это domain knowledge, смешанный с presentation.

### 4. Controller execution switch
**Файл:** `Stage1ContextPanelController.HandleBuildingAction()`

```csharp
switch (action.Kind)
{
    case BuildingInteractionKind.DepositConstructionResources:
        SendDeposit(action.Target); return;
    case BuildingInteractionKind.ContributeBuildWork:
        SendBuild(action.Target); return;
    case BuildingInteractionKind.OpenDetails:
    case BuildingInteractionKind.AssignWorker:
    case BuildingInteractionKind.OpenProductionQueue:
    // ... 10 case-ов группируются в один bucket:
        OpenOperationIntent(action.Target, action.Kind); return;
    default:
        throw new InvalidOperationException("...");
}
```

**Проблема:** почти все interactions обрабатываются одинаково (пишут `Stage1BuildingOperationOpenIntent`), но каждый новый kind требует явного добавления в `switch`. Controller "знает" о каждом виде interaction.

---

## План рефактора

### Шаг 1. Data-driven priority
Добавить в `BuildingInteractionDefinition`:
```csharp
public byte Priority; // чем выше, тем выше в списке / primary
```

`SelectCompletedPrimaryAction` заменить на:
```csharp
var interactions = definition.Interactions
    .Where(i => i.RequiresCompletedBuilding == isCompleted)
    .OrderByDescending(i => i.Priority)
    .ToArray();

return interactions.FirstOrDefault().Kind;
```

### Шаг 2. Interaction Registry
Ввести `IBuildingInteractionHandler`:

```csharp
public interface IBuildingInteractionHandler
{
    BuildingInteractionKind Kind { get; }
    string GetLabel(); // или читать из каталога
    string GetSummary(in BuildingDefinition definition);
    bool CanHandle(BuildingInteractionKind kind);
    void Execute(EntityGID target); // для controller
}
```

Реализации:
- `DepositConstructionResourcesHandler`
- `ContributeBuildWorkHandler`
- `OpenOperationIntentHandler` — generic для всех "открыть окно" interactions.

Регистрация в `BuildingsGameplayFeature` или `SettlementGameplayFeature`:
```csharp
registry.Register(new DepositConstructionResourcesHandler());
registry.Register(new ContributeBuildWorkHandler());
registry.Register(new OpenOperationIntentHandler(
    BuildingInteractionKind.OpenDetails,
    BuildingInteractionKind.AssignWorker,
    BuildingInteractionKind.OpenProductionQueue,
    // ...));
```

### Шаг 3. Переписать StateSystem и Controller
- `ClientStage1ContextPanelStateSystem` — строит `AvailableActions` массив, итерируя `definition.Interactions` и спрашивая у registry label/summary для каждого.
- `Stage1ContextPanelController` — делает `registry.GetHandler(kind).Execute(target)` вместо switch.

### Шаг 4. BuildingSummaryProvider
Для `ResolveOpenedBuildingActionSummary` ввести `IBuildingSummaryProvider`, зарегистрированный по `BuildingCapabilityFlags`:

```csharp
public interface IBuildingSummaryProvider
{
    BuildingCapabilityFlags Capabilities { get; }
    string BuildSummary(in BuildingDefinition definition);
}
```

Реализации:
- `ProductionSummaryProvider` (для `ProducesResources` + `ConsumesResources`)
- `ExtractionSummaryProvider` (для `ExtractsFromNode`)
- `HousingSummaryProvider` (для `ProvidesHousing`)
- `StorageSummaryProvider` (для `ProvidesStorage`)

Это избавит от switch, связанного с `BuildingInteractionKind`, и перенесёт знание о зданиях в domain layer.

---

## Что сломается, если не рефакторить

1. Добавление `BuildingInteractionKind.TogglePower` потребует правки `SelectCompletedPrimaryAction`, `ResolveLabel`, `ResolveOpenedBuildingActionSummary`, `HandleBuildingAction`.
2. Изменение приоритета primary action требует перестановки строк кода.
3. Controller содержит список "всех возможных interactions" — это каталог знаний, который должен жить в данных, а не в коде.
4. Summary logic дублирует domain-знание о зданиях (worker slots, storage capacity), что нарушает separation of concerns между `Runtime/Logic` и `Runtime/Presentation`.
