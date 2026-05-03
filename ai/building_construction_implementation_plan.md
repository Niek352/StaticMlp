# Building Construction Implementation Plan

План реализации прототипа строительства зданий для текущей StaticEcs + Unity Transport архитектуры.

## Цель

Собрать минимальный игровой цикл:

```text
выбрал здание -> увидел ghost preview -> разместил чертёж -> внёс ресурсы -> построил -> получил готовое здание
```

На первом этапе система не включает:

- NPC-строителей;
- автоматическую доставку ресурсов;
- склады;
- приоритеты задач;
- поселения и аванпосты;
- производство ресурсов;
- апгрейды зданий;
- повреждение, ремонт, разборку и перенос.

## Архитектурные Правила

- StaticEcs хранит состояние строительства.
- Unity Transport только доставляет байты; gameplay systems не трогают `NetworkDriver`.
- Клиент может показывать ghost preview и предварительную валидность, но сервер финально решает, можно ли создать чертёж.
- Создание чертежа, внесение ресурсов, прогресс строительства и завершение здания выполняются на сервере.
- Клиентские действия отправляются через replicated events.
- Реплицируемое состояние меняется через `Mut<T>()`.
- Для долгоживущих ссылок между сущностями использовать `EntityGID`, не `Entity`.
- Prefab assets, Canvas hierarchy и inspector references собираются вручную в Unity Editor.

## Feature 1: BuildingCatalog

Назначение: хранить описание доступных типов зданий.

Содержит:

- `BuildingId`
- `BuildingDefinition`
- стабильные ids зданий и сетевых archetypes
- стоимость строительства
- footprint здания
- `buildWorkRequired`
- пути/ids для blueprint и finished building views

Минимум для прототипа:

```text
WoodenHut
- cost: wood 10, stone 4
- footprint: 4 x 5
- buildWorkRequired: 100
```

Предлагаемые данные:

```csharp
public readonly struct BuildingId {
    public readonly ushort Value;
}
```

```csharp
public readonly struct BuildingDefinition {
    public readonly BuildingId Id;
    public readonly string DisplayName;
    public readonly int FootprintWidth;
    public readonly int FootprintLength;
    public readonly float BuildWorkRequired;
    public readonly ushort BlueprintArchetypeId;
    public readonly ushort FinishedArchetypeId;
}
```

Результат готовности:

- есть registry доступных зданий;
- можно получить definition по `BuildingId`;
- ids стабильны и не зависят от Unity prefab instance.

## Feature 2: BuildingPlacement

Назначение: клиентский режим размещения здания.

Содержит:

- выбор здания;
- ghost preview;
- движение preview по поверхности;
- поворот;
- локальную проверку валидности;
- подтверждение или отмену placement mode;
- отправку запроса на сервер.

Клиентские компоненты:

```text
PlacementPreview
- BuildingId
- Position
- Rotation
- IsValid
- InvalidReason
```

Клиентский event на сервер:

```text
PlaceBuildingRequestEvent
- BuildingId
- Position
- Rotation
```

Управление MVP:

```text
ЛКМ / Confirm -> отправить запрос размещения
ПКМ / Cancel -> отменить placement mode
Mouse Aim -> двигать ghost preview
R -> повернуть здание
```

Результат готовности:

- игрок может выбрать здание;
- видит ghost preview;
- может двигать и поворачивать preview;
- preview подсвечивается зелёным или красным;
- при подтверждении отправляется request на сервер.

## Feature 3: BuildingPlacementValidation

Назначение: единая логика проверки размещения.

Проверки MVP:

- footprint не пересекает занятые зоны;
- точка стоит на земле;
- поверхность не круче лимита;
- место не входит в restricted zone.

Важно:

- клиент использует эту feature для UX-подсветки;
- сервер использует эту feature как окончательную проверку перед созданием construction site;
- клиентская проверка не считается авторитетной.

Предлагаемые структуры:

```text
BuildingFootprint
- Width
- Length
```

```text
RestrictedBuildZone
- Center
- Radius
```

```text
PlacementValidationResult
- IsValid
- Reason
```

Результат готовности:

- нельзя поставить здание в занятое место;
- нельзя поставить здание на слишком крутой склон;
- нельзя поставить здание в запрещённую зону;
- сервер отклоняет невалидный `PlaceBuildingRequestEvent`.

## Feature 4: ConstructionSites

Назначение: сетевой строительный чертёж в мире.

Construction site создаётся сервером после валидного placement request.

Реплицируемое состояние:

```text
ConstructionSiteState
- BuildingId
- State
```

```text
ConstructionTransform
- Position
- Rotation
```

```text
ConstructionResources
- Required resources
- Delivered resources
```

```text
ConstructionProgress
- BuildWorkRequired
- BuildWorkDone
```

Состояния:

```text
WaitingForResources
ReadyToBuild
BuildingInProgress
Completed
```

Сетевые правила:

- state/resources/progress доставлять через `ReliableSequenced`;
- construction site является server-owned;
- клиенты не мутируют construction state напрямую.

Результат готовности:

- сервер создаёт replicated construction site;
- все клиенты видят чертёж;
- чертёж хранит требуемые и внесённые ресурсы;
- состояние меняется с `WaitingForResources` на `ReadyToBuild`.

## Feature 5: ResourcesInventoryMinimal

Назначение: минимальный слой ресурсов, достаточный для строительства.

Если полноценного инвентаря ещё нет, нужен временный gameplay feature.

Содержит:

- `ResourceId`
- `Inventory`
- стартовые тестовые ресурсы игрока;
- операции проверки, списания и добавления ресурсов.

Минимальные ресурсы:

```text
Wood
Stone
```

Результат готовности:

- у игрока есть тестовые ресурсы;
- сервер может проверить наличие ресурсов;
- сервер может списать ресурсы при внесении в construction site.

## Feature 6: ConstructionInteraction

Назначение: взаимодействие игрока со строительным чертежом.

Replicated events:

```text
DepositConstructionResourcesRequestEvent
- ConstructionSiteGid
```

```text
BuildConstructionRequestEvent
- ConstructionSiteGid
- IsBuilding
```

Правила:

- клиент отправляет intention/action;
- сервер проверяет дистанцию, состояние site и ресурсы игрока;
- сервер переносит ресурсы из inventory в construction site;
- сервер увеличивает `BuildWorkDone`, пока игрок строит;
- когда ресурсов достаточно, site переходит в `ReadyToBuild`;
- когда progress достиг максимума, site завершается.

Алгоритм внесения ресурсов:

```text
for each required resource:
    missing = required - delivered
    playerAmount = inventory amount
    transfer = min(missing, playerAmount)
    inventory -= transfer
    delivered += transfer
```

Результат готовности:

- игрок может внести ресурсы в чертёж;
- UI видит актуальные delivered/required значения;
- игрок может удерживать build interaction;
- progress растёт на сервере.

## Feature 7: FinishedBuildings

Назначение: готовые построенные здания.

На первом этапе finished building может быть только визуальным объектом без собственной функции.

Завершение:

```text
ConstructionProgress >= BuildWorkRequired
-> spawn finished building на том же месте
-> despawn construction site
```

Сетевые правила:

- finished building создаёт сервер;
- spawn reliable;
- finished building server-owned;
- view применяется на клиентах через prefab registry / EcsViews.

Результат готовности:

- после завершения чертёж исчезает;
- готовое здание появляется на той же позиции и с тем же rotation;
- все клиенты видят одинаковый результат.

## Feature 8: BuildingPresentation

Назначение: client-only визуал строительства.

Содержит:

- ghost preview view;
- материалы `valid` / `invalid`;
- blueprint view;
- finished building view;
- progress bar;
- resource requirement UI;
- view-state sync для construction site.

Важно:

- presentation не должна жить на сервере;
- UI references должны быть явно проставлены через inspector;
- не использовать defensive `if (x != null)` для обязательных UI references;
- не искать UI объекты в hierarchy из кода.

Результат готовности:

- ghost preview понятно показывает валидность;
- construction site показывает ресурсы и progress;
- finished building имеет отдельный visual.

## Feature 9: BuildingMenu

Назначение: UI выбора здания и вход в placement mode.

Содержит:

- список доступных buildings из `BuildingCatalog`;
- кнопку выбора здания;
- выход из placement mode;
- отображение стоимости выбранного здания.

Результат готовности:

- игрок может открыть меню строительства;
- выбрать `WoodenHut`;
- перейти в placement mode.

## Рекомендуемый Порядок Реализации

1. Создать `BuildingCatalog` с одним зданием `WoodenHut`.
2. Создать минимальный `ResourcesInventoryMinimal`.
3. Создать `BuildingMenu` без финального дизайна.
4. Создать client-only `BuildingPlacement` и ghost preview.
5. Создать `BuildingPlacementValidation` с базовой проверкой земли и footprint.
6. Добавить `PlaceBuildingRequestEvent`.
7. На сервере валидировать request и создавать `ConstructionSite`.
8. Зарегистрировать blueprint prefab/view через feature registration.
9. Реплицировать state/resources/progress construction site.
10. Добавить `DepositConstructionResourcesRequestEvent`.
11. Реализовать серверное внесение ресурсов.
12. Добавить `BuildConstructionRequestEvent`.
13. Реализовать серверный progress строительства.
14. Реализовать завершение: despawn blueprint, spawn finished building.
15. Добавить UI ресурсов и progress bar.
16. Доработать validation: slope limit и restricted zones.

## Предлагаемая Структура Папок

```text
Assets/Scripts/StaticMlp/Features/BuildingCatalog
    StaticMlp.Features.BuildingCatalog.asmdef
    BuildingCatalogGameplayFeature.cs
    Components/
    Definitions/

Assets/Scripts/StaticMlp/Features/ResourcesInventoryMinimal
    StaticMlp.Features.ResourcesInventoryMinimal.asmdef
    ResourcesInventoryMinimalGameplayFeature.cs
    Components/
    Systems/

Assets/Scripts/StaticMlp/Features/Buildings
    StaticMlp.Features.Buildings.asmdef
    BuildingsGameplayFeature.cs
    Components/
    Events/
    Tags/
    Systems/Client/
    Systems/Server/
    Presentation/
```

Если фичи начинают расти, `Buildings` можно позже разделить на:

```text
BuildingPlacement
ConstructionSites
ConstructionInteraction
FinishedBuildings
BuildingPresentation
BuildingMenu
```

Для первого прототипа допустимо держать их в одном feature assembly `StaticMlp.Features.Buildings`, но внутри разделить папками и типами.

## Networked Contracts

Минимальные replicated events:

```text
PlaceBuildingRequestEvent
DepositConstructionResourcesRequestEvent
BuildConstructionRequestEvent
```

Минимальные replicated components:

```text
ConstructionSiteState
ConstructionTransform
ConstructionResources
ConstructionProgress
```

Минимальные tags:

```text
ConstructionSiteTag
FinishedBuildingTag
```

Минимальные client-only components:

```text
PlacementPreview
BuildingMenuState
ConstructionViewState
```

## Server Systems

```text
ServerPlaceBuildingRequestSystem
- читает PlaceBuildingRequestEvent
- валидирует placement
- создаёт ConstructionSite

ServerDepositConstructionResourcesSystem
- читает DepositConstructionResourcesRequestEvent
- проверяет дистанцию/валидность/inventory
- переносит ресурсы
- обновляет state

ServerBuildConstructionSystem
- читает BuildConstructionRequestEvent или client action state
- проверяет ReadyToBuild/BuildingInProgress
- увеличивает BuildWorkDone
- завершает стройку

ServerCompleteConstructionSystem
- создаёт FinishedBuilding
- удаляет ConstructionSite
```

## Client Systems

```text
ClientBuildingMenuSystem
- открывает/закрывает меню
- выбирает BuildingId

ClientPlacementInputSystem
- обновляет PlacementPreview
- поворачивает preview
- отправляет PlaceBuildingRequestEvent

ClientPlacementValidationPreviewSystem
- считает локальный canPlace
- обновляет цвет preview

ClientConstructionInteractionSystem
- отправляет DepositConstructionResourcesRequestEvent
- отправляет BuildConstructionRequestEvent

ClientConstructionViewStateSystem
- копирует replicated construction state в client-only view state
```

## Unity Работы Руками

AI agent не создаёт prefab assets. В Unity Editor вручную собрать:

- ghost preview prefab/view;
- blueprint construction site prefab/view;
- finished building prefab/view;
- UI меню строительства;
- UI panel construction site interaction;
- materials для valid/invalid preview;
- inspector references для всех обязательных UI fields.

## Критерии Готовности MVP

Прототип считается готовым, если игрок может:

1. Открыть меню строительства.
2. Выбрать `WoodenHut`.
3. Увидеть ghost preview в мире.
4. Повернуть preview.
5. Понять по цвету, можно ли строить.
6. Подтвердить размещение.
7. Получить replicated construction site.
8. Подойти к construction site.
9. Увидеть требуемые ресурсы.
10. Внести ресурсы из inventory.
11. Удерживать build interaction.
12. Довести progress до 100%.
13. Увидеть замену чертежа на готовое здание.
14. Проверить тот же результат на сервере и другом клиенте.

## Что Отложить

- NPC-строители;
- рабочие задачи;
- склады;
- доставка ресурсов;
- производство ресурсов;
- поселения;
- дороги;
- апгрейды;
- повреждение зданий;
- ремонт;
- перенос;
- разборка;
- возврат ресурсов.

