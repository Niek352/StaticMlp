# Проблема: Дублирование label-мапперов (найдена при аудите)

## Краткое описание

`ResourceDefinition`, `BuildingCategory`, `BuildingInteractionKind` не содержат display names. В результате во всех presentation-системах появляются идентичные `if/else` и `switch` цепочки, маппящие ID/enum → строку. Это чистое нарушение DRY: изменение названия ресурса требует правки 3+ файлов.

---

## Конкретные места

### 1. ResourceLabel — идентичные if/else цепочки
**Файл A:** `ClientStage1ContextPanelStateSystem.ResolveResourceLabel()` (строки 348–371)

```csharp
private static string ResolveResourceLabel(ResourceId id)
{
    if (id == ResourceCatalog.WoodId) return "Wood";
    if (id == ResourceCatalog.StoneId) return "Stone";
    if (id == ResourceCatalog.PlanksId) return "Planks";
    if (id == ResourceCatalog.SimplePartsId) return "Simple Parts";
    if (id == ResourceCatalog.RepairKitsId) return "Repair Kits";
    if (id == ResourceCatalog.FoodId) return "Food";
    if (id == ResourceCatalog.FuelId) return "Fuel";
    if (id == ResourceCatalog.ResearchDataId) return "Research Data";
    if (id == ResourceCatalog.MedicineId) return "Medicine";

    ResourceCatalog.Get(id);
    return $"Resource {id.Value}";
}
```

**Файл B:** `BuildingMenuPresentation.ResourceLabel()` (строки 182–205)

```csharp
private static string ResourceLabel(ResourceId id)
{
    if (id == ResourceCatalog.WoodId) return "Wood";
    if (id == ResourceCatalog.StoneId) return "Stone";
    if (id == ResourceCatalog.PlanksId) return "Planks";
    if (id == ResourceCatalog.SimplePartsId) return "Parts";
    if (id == ResourceCatalog.RepairKitsId) return "Repair Kits";
    if (id == ResourceCatalog.FoodId) return "Food";
    if (id == ResourceCatalog.FuelId) return "Fuel";
    if (id == ResourceCatalog.ResearchDataId) return "Research";
    if (id == ResourceCatalog.MedicineId) return "Medicine";

    ResourceCatalog.Get(id);
    return $"Resource {id.Value}";
}
```

**Внимание:** эти два метода почти идентичны, но с расхождениями:
- `Simple Parts` vs `Parts`
- `Research Data` vs `Research`

Это означает, что **один и тот же ресурс может отображаться по-разному** в меню зданий и в контекстной панели. Это баг UX.

### 2. CategoryLabel — идентичные switch
**Файл A:** `ClientStage1ContextPanelStateSystem.ResolveCategoryLabel()` (строки 325–346)

```csharp
private static string ResolveCategoryLabel(BuildingCategory category)
{
    switch (category)
    {
        case BuildingCategory.Housing: return "Housing";
        case BuildingCategory.Logistics: return "Logistics";
        case BuildingCategory.Extraction: return "Extraction";
        case BuildingCategory.Production: return "Production";
        case BuildingCategory.Service: return "Service";
        case BuildingCategory.Defense: return "Defense";
        case BuildingCategory.Research: return "Research";
        default: return "Uncategorized";
    }
}
```

**Файл B:** `BuildingMenuPresentation.LabelForCategory()` (строки 159–180)

Идентичный switch. Любое изменение категории требует синхронизации в двух файлах.

### 3. InteractionLabel — switch в state system
**Файл:** `ClientStage1ContextPanelStateSystem.ResolveLabel()` (строки 188–223)

```csharp
private static string ResolveLabel(BuildingInteractionKind kind)
{
    switch (kind)
    {
        case BuildingInteractionKind.OpenDetails: return "Open";
        case BuildingInteractionKind.DepositConstructionResources: return "Deposit";
        case BuildingInteractionKind.ContributeBuildWork: return "Build";
        // ... 11 case
    }
}
```

Этот mapping тоже должен жить в одном месте.

---

## Корневая причина

`ResourceDefinition` (строки 1–25) содержит только:
```csharp
public readonly ResourceId Id;
public readonly ResourceFamily Family;
public readonly ResourceUsageFlags Usage;
public readonly bool IsSettlementStored;
public readonly int StartingSettlementAmount;
```

Нет `DisplayName`.

`BuildingCategory` — чистый enum.
`BuildingInteractionKind` — чистый enum.

В результате presentation layer вынуждена "додумывать" строки.

---

## План рефактора

### Шаг 1. Добавить DisplayName в ResourceDefinition
```csharp
public readonly string DisplayName;
```

Обновить `ResourceCatalog` — передать DisplayName для каждого ресурса.

### Шаг 2. Extension method для BuildingCategory
```csharp
public static string GetDisplayName(this BuildingCategory category)
{
    return category switch
    {
        BuildingCategory.Housing => "Housing",
        BuildingCategory.Logistics => "Logistics",
        BuildingCategory.Extraction => "Extraction",
        BuildingCategory.Production => "Production",
        BuildingCategory.Service => "Service",
        BuildingCategory.Defense => "Defense",
        BuildingCategory.Research => "Research",
        _ => "Uncategorized"
    };
}
```

Поместить в `BuildingCatalog` или `BuildingCatalogData`.

### Шаг 3. Extension method для BuildingInteractionKind
Аналогично шагу 2.

### Шаг 4. Удалить дублирующие методы
- Удалить `ResolveResourceLabel()` из `ClientStage1ContextPanelStateSystem`.
- Удалить `ResourceLabel()` из `BuildingMenuPresentation`.
- Удалить `ResolveCategoryLabel()` из `ClientStage1ContextPanelStateSystem`.
- Удалить `LabelForCategory()` из `BuildingMenuPresentation`.
- Удалить `ResolveLabel()` из `ClientStage1ContextPanelStateSystem`.

Везде заменить на вызовы каталога / extension methods.

### Шаг 5. Унифицировать имена
Исправить расхождение `Simple Parts` vs `Parts`, `Research Data` vs `Research`. Теперь единый источник правды — `ResourceCatalog`.

---

## Что сломается, если не рефакторить

1. Новый ресурс требует правки 2+ файлов ради одной строки.
2. Расхождения в naming (`Parts` vs `Simple Parts`) создают путаницу у игрока.
3. Локализация невозможна — строки размазаны по private методам.
4. Добавление новой категории требует правки двух presentation классов.
