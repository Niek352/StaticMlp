# Проблема: Хардкод Stage1Flow objective/hint (найдена при аудите)

> **СТАТУС РЕФАКТОРА (2025-05-22): НЕ выполнен — практически не начат**
>
> ## Сделано
> - Нет значимых изменений по этой проблеме.
>
> ## Осталось (весь план рефактора)
> 1. **`ServerStage1FlowViewStateSystem.ResolveObjective()`** — всё ещё содержит длинную if/else цепочку, маппящую `Stage1SettlementProgressStage` + world state → `Stage1FlowObjective`.
> 2. **`ServerStage1FlowViewStateSystem.ResolveHint()`** — всё ещё содержит if/else цепочку stage → `Stage1FlowHint`.
> 3. **`SettlementHudPresentation.ToPresentationObjective()`** — switch `Stage1FlowObjective` → `Stage1ObjectiveKind` (лишний enum-уровень).
> 4. **`SettlementHudPresentation.ToPresentationHint()`** — switch `Stage1FlowHint` → string.
> 5. **`Stage1HudController.DescribeObjective()`** — switch `Stage1ObjectiveKind` → display string.
> 6. **`Stage1FlowCatalog` не создан**. Нужен статический каталог `Stage1FlowStageDefinition`, декларирующий objective/hint строки и флаг `AutoAdvance`.
> 7. **`IStage1ObjectiveOverride` не создан**. Boss/Threat/Expedition overrides всё ещё захардкожены в `ResolveObjective()`.
> 8. **Избыточные enum-ы** (`Stage1FlowObjective`, `Stage1FlowHint`, `Stage1ObjectiveKind`) всё ещё существуют вместо прямых строк в `Stage1FlowViewState` / `SettlementHudState`.

---

# Проблема: Хардкод Stage1Flow objective/hint (найдена при аудите)

## Краткое описание

Stage1 progression использует линейную цепочку стадий. Каждая стадия должна показывать игроку objective (что делать) и hint (подсказка). Вместо того чтобы каждая стадия декларировать свои строки в данных, маппинг размазан по 4+ файлам через if/else и switch.

---

## Конкретные места

### 1. Server — вычисление objective
**Файл:** `ServerStage1FlowViewStateSystem.ResolveObjective()`

Длинная if/else цепочка, маппящая `Stage1SettlementProgressStage` + world state → `Stage1FlowObjective`:
```csharp
if (stage == Stage1SettlementProgressStage.RepairObjectiveActive) return Stage1FlowObjective.RepairCamp;
if (stage == Stage1SettlementProgressStage.WorkerAssigned) return Stage1FlowObjective.AssignWorker;
// ... + overrides для boss/threat/expedition
```

### 2. Server — вычисление hint
**Файл:** `ServerStage1FlowViewStateSystem.ResolveHint()`

Аналогичная if/else цепочка stage → `Stage1FlowHint`.

### 3. Client — маппинг objective → presentation kind
**Файл:** `ClientStage1HudStateSystem.ToPresentationObjective()`

```csharp
private static Stage1ObjectiveKind ToPresentationObjective(Stage1FlowObjective objective)
{
    switch (objective)
    {
        case Stage1FlowObjective.RepairCamp: return Stage1ObjectiveKind.RepairCamp;
        case Stage1FlowObjective.AssignWorker: return Stage1ObjectiveKind.AssignWorker;
        // ... 12 case
    }
}
```

### 4. Client — маппинг hint → string
**Файл:** `ClientStage1HudStateSystem.ToPresentationHint()`

```csharp
private static string ToPresentationHint(Stage1FlowHint hint)
{
    switch (hint)
    {
        case Stage1FlowHint.GatherRepairResources: return "Gather the camp resources...";
        case Stage1FlowHint.ContinueRepairBuild: return "Resources delivered...";
        // ... 7 case
    }
}
```

### 5. Controller — маппинг kind → display string
**Файл:** `Stage1HudController.DescribeObjective()`

```csharp
private string DescribeObjective(Stage1ObjectiveKind objective)
{
    return objective switch
    {
        Stage1ObjectiveKind.RepairCamp => "Repair the Camp",
        Stage1ObjectiveKind.AssignWorker => "Assign a Worker",
        // ...
    };
}
```

---

## Проблемы

1. **Добавление новой стадии** требует правки 4+ файлов.
2. **Изменение текста hint** требует поиска по проекту (строка может быть в `ToPresentationHint` или `DescribeObjective`).
3. **Локализация невозможна** — строки разбросаны по private методам в разных слоях (Server → Client → Controller).
4. **Избыточные enum-ы:** `Stage1FlowObjective` → `Stage1ObjectiveKind` → `string`. Два лишних уровня абстракции, не несущих gameplay-логики.

---

## План рефактора

### Шаг 1. Обогатить SettlementAnchorCatalog (или создать Stage1FlowCatalog)
Ввести `Stage1FlowStageDefinition`:

```csharp
public readonly struct Stage1FlowStageDefinition
{
    public readonly Stage1SettlementProgressStage Stage;
    public readonly string ObjectiveDisplayName;   // "Repair the Camp"
    public readonly string HintDisplayName;        // "Gather the camp resources..."
    public readonly bool AutoAdvance;              // true для DamagedCampStart → RepairObjectiveActive
}
```

Создать статический каталог:
```csharp
public static class Stage1FlowCatalog
{
    private static readonly Stage1FlowStageDefinition[] Definitions = { ... };
}
```

### Шаг 2. Удалить if/else из ServerStage1FlowViewStateSystem
`ResolveObjective` и `ResolveHint` заменить на lookup:
```csharp
var def = Stage1FlowCatalog.Get(currentStage);
next.ObjectiveDisplayName = def.ObjectiveDisplayName;
next.HintDisplayName = def.HintDisplayName;
```

Убрать `Stage1FlowObjective` и `Stage1FlowHint` enum-ы из `Stage1FlowViewState`.

### Шаг 3. Упростить ClientStage1HudStateSystem
`Stage1HudState` получает готовые строки:
```csharp
public struct Stage1HudState : IResource
{
    public string ObjectiveDisplayName;
    public string HintDisplayName;
    // ...
}
```

`ClientStage1HudStateSystem` просто копирует строки из `Stage1FlowViewState`, без switch.

### Шаг 4. Упростить Stage1HudController
`DescribeObjective` удалить. Controller использует `state.ObjectiveDisplayName` напрямую.

### Шаг 5. Boss/Threat/Expedition overrides
Вместо if/else в `ResolveObjective` ввести `IStage1ObjectiveOverride` interface:
```csharp
public interface IStage1ObjectiveOverride
{
    bool TryOverride(in Stage1FlowContext context, out string objective, out string hint);
}
```

Каждая фича (Threat, Boss, Expedition) регистрирует свой override provider. `ServerStage1FlowViewStateSystem` запрашивает override-ы до fallback на каталог.

---

## Что сломается, если не рефакторить

1. Дизайнер не может менять тексты без программиста.
2. Добавление под-стадии (например, `RepairHalfDone`) требует создания новых enum значений в 3 enum-ах + правки 4 файлов.
3. Локализация потребует grep по всем .cs файлам.
4. Разные слои (Server, Client, Controller) дублируют одни и те же концепции разными enum-ами.
