# Interaction Feature — Plan

## Background

Сейчас «взаимодействие» с объектами мира разбросано по нескольким несвязанных местах:

- `ClientConstructionInteractionSystem` — proximity-scan строек + нажатие `E` (Interact) или Hold `BuildConstruction` без какого-либо явного «focus» объекта.
- `ClientStage1ContextPanelSessionSystem` — ещё один proximity-scan, выбирает focused site для UI; дублирует логику нахождения ближайшей строики.
- `ClientStage1ContextPanelStateSystem` — hardcoded priority-ladder для выбора primary action, label/summary через switch.
- `Stage1ContextPanelController` — switch на kind при нажатии кнопки в UI.

**Цель:** выделить `StaticMlp.Features.Interaction` — отдельную фичу, которая:
1. Сканирует мир и публикует **текущий Focused объект** как world resource `InteractionFocus`.
2. Обрабатывает нажатие `E` (Interact) → пишет событие `InteractPressedEvent`.
3. Остальные фичи (Settlement, Buildings) реагируют на `InteractionFocus` чтобы показать HUD, и на `InteractPressedEvent` чтобы выполнить действие.
4. Не заменяет Settlement-логику целиком, но убирает дублирование proximity-scan и вводит явный контракт «что сейчас в фокусе».

---

## Open Questions

> [!IMPORTANT]
> **Q1: Scope первой итерации.** Делаем только «Interaction фокусирует объект + E-press event» или сразу рефакторим весь priority-ladder / handler registry (Шаги 1–4 из 03_problem_interaction_nonsystemic.md)?
>
> **Предложение:** Сначала вводим `InteractionFocus` и `InteractPressedEvent`, убираем дублирование proximity-scan. Рефактор handler-registry — отдельный следующий шаг.

> [!IMPORTANT]
> **Q2: Что может быть в фокусе?** Сейчас только `ConstructionSiteState` (строящееся/готовое здание). Планируется ли фокус на других объектах: ресурсные узлы, NPC, лут?
>
> **Предложение:** `InteractionFocus` хранит `EntityGID` + `InteractableKind` (enum: None, ConstructionSite). Легко расширяется.

> [!IMPORTANT]
> **Q3: Дальность фокуса.** Сейчас `ClientConstructionInteractionSystem` и `ClientStage1ContextPanelSessionSystem` оба используют `4f`. Нужна ли одна глобальная константа или per-kind настройка?
>
> **Предложение:** Один `InteractionSettings` resource с `DefaultFocusRange = 4f`, переопределяемый per-interactable-kind через данные.

---

## Proposed Changes

### 1. Новая фича `StaticMlp.Features.Interaction`

```
Assets/Scripts/StaticMlp/Features/Interaction/
  Runtime/
    InteractionGameplayFeature.cs          — регистрация систем и ресурсов
    StaticMlp.Features.Interaction.asmdef
    Components/
      InteractableTag.cs                  — ITag; ставится на сущности, с которыми можно взаимодействовать
    Events/
      InteractPressedEvent.cs             — IEvent; пишется когда игрок нажал E при наличии фокуса
    WorldResources/
      InteractionFocus.cs                 — IResource; текущий сфокусированный объект + kind
      InteractionSettings.cs              — IResource; радиус и параметры фокуса
    Systems/
      ClientInteractionFocusSystem.cs     — proximity-scan → пишет InteractionFocus
      ClientInteractPressSystem.cs        — читает InputState.WasPressed(Interact) + InteractionFocus → пишет InteractPressedEvent
```

**`InteractionFocus`** (IResource):
```csharp
public struct InteractionFocus : IResource
{
    public EntityGID Target;
    public InteractableKind Kind;
    public bool HasFocus => Kind != InteractableKind.None;
}
```

**`InteractableKind`** (enum):
```csharp
public enum InteractableKind : byte
{
    None = 0,
    ConstructionSite = 1,
    // будущее: ResourceNode, NPC, Loot...
}
```

**`InteractPressedEvent`** (IEvent):
```csharp
public struct InteractPressedEvent : IEvent
{
    public EntityGID Target;
    public InteractableKind Kind;
}
```

**`ClientInteractionFocusSystem`**:
- Запрашивает `ClientLocalPlayer.TryGetPosition`.
- Итерирует сущности с `InteractableTag` + transform-компонентом (ConstructionTransform для строек).
- Находит ближайшую в радиусе `InteractionSettings.FocusRange`.
- Пишет `InteractionFocus` в world resource каждый кадр.

**`ClientInteractPressSystem`**:
- Читает `ClientInputState.WasPressed(CoreInputActions.Interact)`.
- Если `InteractionFocus.HasFocus` → создаёт `InteractPressedEvent` через `W.Event<InteractPressedEvent>().Set(...)`.

> [!NOTE]
> `InteractableTag` — маркер. Его нужно установить на сущности при спавне. Для construction sites это делается в `BuildingsGameplayFeature.RegisterNetworkEvents` (клиентская архетипная инициализация) или в `ClientConstructionInteractionSystem` (если он остаётся). После рефактора construction-side взаимодействие будет реагировать на `InteractPressedEvent`.

---

### 2. Изменения в `Buildings` — реакция на `InteractPressedEvent`

#### [MODIFY] [ClientConstructionInteractionSystem.cs](file:///c:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Client/ClientConstructionInteractionSystem.cs)

Убрать proximity-scan и чтение `InputState.WasPressed(Interact)`. Система становится **реактором на события** вместо инициатора:

```csharp
public sealed class ClientConstructionInteractionSystem : ISystem
{
    public void Update()
    {
        // Реагируем на InteractPressedEvent от Interaction фичи
        foreach (var evt in CW.EventEntities<InteractPressedEvent>())
        {
            ref readonly var press = ref evt.Read<InteractPressedEvent>();
            if (press.Kind != InteractableKind.ConstructionSite) continue;
            if (!press.Target.TryUnpack<ClientCoreWT>(out var site)) continue;
            SendDeposit(site);
        }

        // Hold-build остаётся proximity-based (удерживание не требует фокуса через событие)
        // либо тоже переносится в отдельную систему
        HandleHoldBuild();
    }
    // ...
}
```

> [!NOTE]
> Hold-build (`BuildingsInputActions.BuildConstruction`) — держание кнопки, не единичное нажатие. Его можно оставить в системе как прямое чтение input + `InteractionFocus`, либо выделить в `ClientConstructionHoldBuildSystem`. Решаем по ситуации при реализации.

---

### 3. Изменения в `Settlement` — убрать дублирование proximity-scan

#### [MODIFY] [ClientStage1ContextPanelSessionSystem.cs](file:///c:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/ClientStage1ContextPanelSessionSystem.cs)

Метод `TryFindNearestSite` заменяется чтением `InteractionFocus`:

```csharp
// Было:
if (TryFindNearestSite(playerPosition, includeCompleted: true, out var site))
{
    session.Mode = Stage1ContextPanelMode.Building;
    session.FocusedSite = site.GID;
    return;
}

// Станет:
ref readonly var focus = ref CW.GetResource<InteractionFocus>();
if (focus.HasFocus && focus.Kind == InteractableKind.ConstructionSite)
{
    session.Mode = Stage1ContextPanelMode.Building;
    session.FocusedSite = focus.Target;
    return;
}
```

`TryFindNearestSite` удаляется из класса (дублирование с Interaction фичей).

> [!NOTE]
> `TryFindRepairFocusSite` (для фазы до `CampRepaired`) и `TryFindExplicitFocusSite` (из `Stage1ContextFocusTarget`) пока остаются — они не дублируют proximity-scan, а реализуют специфичную прогрессионную логику Stage 1.

---

### 4. `InteractableTag` — установка на строительные площадки

#### [MODIFY] [BuildingsGameplayFeature.cs](file:///c:/Users/pavel/_UnityProjects/StaticMlp/Assets/Scripts/StaticMlp/Features/Buildings/Runtime/BuildingsGameplayFeature.cs)

В клиентских архетипах construction-site добавить `InteractableTag` и `InteractableKind`:
```csharp
NetArchetypeRegistry.RegisterClient(network.BlueprintArchetypeId, e =>
{
    e.Set<ConstructionSiteTag>();
    e.Set<InteractableTag>();  // ← NEW
    // ...остальное
});
NetArchetypeRegistry.RegisterClient(network.FinishedArchetypeId, e =>
{
    e.Set<FinishedBuildingTag>();
    e.Set<InteractableTag>();  // ← NEW (для взаимодействия с готовыми зданиями)
    // ...
});
```

> [!NOTE]
> `InteractableTag` — в `StaticMlp.Features.Interaction`, поэтому Buildings asmdef должен будет ссылаться на Interaction. Альтернатива: `InteractableKind` как отдельный компонент в Contracts asmdef, но tag удобнее для query.

---

### 5. Новый asmdef и зависимости

**`StaticMlp.Features.Interaction.asmdef`** зависит от:
- `FFS.Libraries.StaticEcs`
- `StaticMlp.Networking` (для `EntityGID`, `ClientCoreWT`)
- `StaticMlp.Features.Input` (для `ClientInputState`, `CoreInputActions`)
- `StaticMlp.Features.Player` (для `ClientLocalPlayer`)

**`StaticMlp.Features.Buildings.asmdef`** добавляет зависимость на:
- `StaticMlp.Features.Interaction`

**`StaticMlp.Features.Settlement.Presentation.asmdef`** добавляет зависимость на:
- `StaticMlp.Features.Interaction`

---

## Scope этого плана (границы)

**Входит:**
- Новая фича `Interaction` с `InteractionFocus`, `InteractPressedEvent`, `InteractableTag`.
- `ClientInteractionFocusSystem` — единый proximity-scan.
- `ClientInteractPressSystem` — E-press → event.
- Удаление дублирующего proximity-scan из `ClientStage1ContextPanelSessionSystem`.
- `ClientConstructionInteractionSystem` реагирует на `InteractPressedEvent` вместо прямого чтения input.
- Добавление `InteractableTag` на construction site/finished building архетипы.

**Не входит (следующий шаг):**
- Рефактор handler-registry (`IBuildingInteractionHandler`, priority data-driven) из `03_problem_interaction_nonsystemic.md`.
- Focused HUD для зданий (например, всплывающий tooltip «Press E to Deposit») — визуальная часть, делается после того как `InteractionFocus` устойчиво работает.
- Расширение `InteractableKind` на ResourceNode, NPC, Loot.

---

## Verification Plan

### Build Check
- Попросить пользователя скомпилировать в Unity — убедиться, что нет ошибок сборки.

### Manual Gameplay Check
- Подойти к строящемуся зданию → `InteractionFocus` должен установиться → Context Panel должна показать строительную информацию.
- Нажать `E` → `DepositConstructionResourcesRequestEvent` должен уйти на сервер.
- Отойти от здания → `InteractionFocus` сброшен → Context Panel переключается в Worker mode.
- Hold-build (`B`) рядом со стройкой → работает как раньше.
