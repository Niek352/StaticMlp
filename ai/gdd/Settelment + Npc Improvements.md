# Settlement и Stage1 для NPC Economy

Этот текст уже оформлен как единый markdown-ready feature spec: его удобно сохранять как основной `settlement_feature_spec_ru.md`, а последний раздел с таблицами — как отдельный `settlement_resources_buildings_loadout_ru.md`.

## Аудит текущего прототипа

В репозитории направление уже задано правильно: settlement и NPC economy проектируются как ECS-first и server-authoritative слои, а client-only часть ограничивается UI, preview и presentation. Это не просто намерение в документах: текущий building prototype уже опирается на ECS-компоненты `ConstructionSiteState`, `ConstructionResources`, `ConstructionProgress`, request handlers и server systems, а не на gameplay-state внутри `MonoBehaviour`. fileciteturn87file0L3-L3 fileciteturn34file0L3-L3 fileciteturn37file0L3-L3 fileciteturn19file0L3-L3

Но фактический контент settlement пока очень узкий. Каталог зданий содержит только один `WoodenHut`: стоимость 10 wood и 4 stone, footprint `4x5`, требуемая работа `100f`. Presentation catalog для него тоже один: `Ghost`, `Blueprint` и `Finished` view заведены только для этой постройки. Это означает, что сейчас у проекта есть рабочий каркас строительства, но ещё нет feature-based settlement catalog, который масштабируется на жильё, добычу, сервис и производство. fileciteturn23file0L3-L3 fileciteturn25file0L3-L3 fileciteturn61file0L3-L3

Stage1 в коде тоже уже существует, но он пока привязан к repair-сценарию. Seed manifest спавнит home-camp construction site на якоре лагеря, задаёт стартовые ресурсы `50 wood / 25 stone` и один стартовый worker role `CampBuilder`; progression stage затем проходит путь от `DamagedCampStart` до `LoadoutPrepared`. По факту, текущий сценарий — это “почини лагерь, назначь одного строителя, выйди в loadout”, а не поселение как полноценная экономическая система. fileciteturn12file0L3-L3 fileciteturn15file0L3-L3 fileciteturn90file0L3-L3 fileciteturn91file0L3-L3

Текущий UX подтверждает, что это именно прототип. Build menu рендерит одну карточку и одну кнопку `woodenHutButton`, а controller умеет только выбрать `WoodenHut` или закрыть меню. Placement уже собран технически грамотно — с preview entity, поворотом, raycast-позицией, client validation и server authoritative placement check, — но user-facing слой ещё debug-like: мало контекста, мало объяснений и почти нет management UX. fileciteturn93file0L3-L3 fileciteturn45file0L3-L3 fileciteturn49file0L3-L3 fileciteturn50file0L3-L3 fileciteturn51file0L3-L3 fileciteturn52file0L3-L3

## Где прототип ограничивает развитие

Главный архитектурный bottleneck сейчас не в том, что строительство “вообще не работает”, а в том, что settlement пока описан слишком бедно на уровне данных. Shared storage хранит только `Wood` и `Stone`, а worker role catalog содержит только один role id — `CampBuilder`, причём набор разрешённых job flags тоже минимален: только доставка ресурсов на стройку и build work. Для repair prototype этого достаточно, но для кроватей, шахт, переработки, upkeep, fuel loop и logistics этого явно мало. fileciteturn21file0L3-L3 fileciteturn13file0L3-L3 fileciteturn15file0L3-L3 fileciteturn16file0L3-L3 fileciteturn18file0L3-L3

Второй bottleneck — interaction UX. Context panel умеет только два режима: `Building` и `Worker`. Для здания она показывает `Deposit` или `Build`, а для worker — `Assign Worker`/`Unassign Worker`. Причём фокус панели сейчас выбирается не через понятный interact-under-crosshair flow, а через repair-target/nearest-site logic в радиусе четырёх метров. Для раннего vertical slice это терпимо, но как только появятся кровати, станки, склады, шахты и guard posts, такой интерфейс станет не просто неудобным, а концептуально неверным: он не различает construction interaction и operational interaction. fileciteturn42file0L3-L3 fileciteturn43file0L3-L3 fileciteturn72file0L3-L3 fileciteturn73file0L3-L3 fileciteturn94file0L3-L3

Третий bottleneck — presentation pipeline для зданий. С одной стороны, проект уже аккуратно разделяет replicated state и view state: `ViewPath` — client-only, а `ApplyComponentToViewSystem<T>` и `IEntityViewPart<TViewState>` делят runtime logic и визуализацию. С другой — текущая загрузка view идёт через `Resources.Load` по `ViewPath`, а реальные wooden hut visuals генерируются прямо внутри `WoodenHutViewPart` через примитивы `GameObject.CreatePrimitive`. Это очень полезно для прототипа, но не масштабируется на production catalog зданий и богатую модульную визуалку. Сам Unity прямо предупреждает, что большие наборы ассетов в `Resources` усложняют memory management, замедляют startup и сборки, а для сложных наборов контента лучше использовать AssetBundles/Addressables. fileciteturn99file0L3-L3 fileciteturn79file0L3-L3 fileciteturn80file0L3-L3 citeturn4view3turn4view2

Именно поэтому следующий шаг должен быть не “добавить ещё пару hardcoded построек”, а перевести settlement в feature-based модель, где новые здания добавляются через definition assets, runtime operation profiles и generic interaction entries, а не через новые ветки `if (buildingId == ...)` в controller или view. Такой подход хорошо совпадает с уже зафиксированным в репозитории boundary rule: чужое состояние нельзя менять напрямую, публичный write-port фичи должен идти через event/request path. fileciteturn100file0L3-L3

## Целевая архитектура Settlement

Я бы разбил Settlement на шесть внутренних слоёв: `Settlement.Core`, `Settlement.Build`, `Settlement.Operations`, `Settlement.Storage`, `Settlement.NpcJobs` и `Settlement.Presentation`. Это продолжает уже существующую структуру проекта, где build lifecycle, projection и client view sync вынесены по разным системам, и одновременно снимает будущий риск “монолитной settlement-системы”, в которой всё знает обо всём. fileciteturn65file0L3-L3 fileciteturn98file0L3-L3

Ключевой принцип здесь такой: у каждого здания должны быть три уровня описания. Во-первых, **definition** — что это за здание, сколько стоит, какой у него footprint, какие interaction actions оно поддерживает, какие NPC roles к нему привязаны, какие service points и operation components ему нужны. Во-вторых, **instance state** — текущая стадия, occupancy, durability, queue, buffers, assignments, reservations. В-третьих, **presentation** — какой prefab показывать в ghost/blueprint/completed mode, какие иконки и какой details panel нужен. Unity как раз описывает `ScriptableObject` как data-store asset, независимый от `GameObject`, useful for shared data across many objects, а Addressables позволяют держать логический адрес ассета отдельно от его физического расположения и использовать `AssetReference` fields внутри `ScriptableObject` или `MonoBehaviour`. citeturn3view0turn4view2

Практически это выглядит так:

```csharp
public enum BuildingCategory : byte
{
    Housing = 1,
    Logistics = 2,
    Extraction = 3,
    Production = 4,
    Service = 5,
    Defense = 6,
    Research = 7
}

[Flags]
public enum BuildingCapabilityFlags : ushort
{
    None = 0,
    ProvidesHousing = 1 << 0,
    ProvidesStorage = 1 << 1,
    ProvidesWorkplace = 1 << 2,
    ProducesResources = 1 << 3,
    ConsumesResources = 1 << 4,
    ExtractsFromNode = 1 << 5,
    SupportsNpcInteraction = 1 << 6,
    SupportsPlayerInteraction = 1 << 7,
    RequiresMaintenance = 1 << 8,
    RequiresFuel = 1 << 9,
    ProvidesRest = 1 << 10,
    BlocksPathing = 1 << 11,
    OpensQueue = 1 << 12,
}

public sealed class BuildingDefinitionAsset : ScriptableObject
{
    public ushort BuildingId;
    public string Code;
    public string DisplayName;
    public string Description;
    public BuildingCategory Category;
    public BuildingCapabilityFlags Capabilities;
    public Vector2Int Footprint;
    public float BuildWorkRequired;
    public ResourceCostEntry[] ConstructionCost;
    public BuildingInteractionDefinition[] Interactions;
    public BuildingNpcProfileDefinition NpcProfile;
    public BuildingOperationDefinition Operation;
    public AssetReferenceGameObject GhostView;
    public AssetReferenceGameObject BlueprintView;
    public AssetReferenceGameObject CompletedView;
    public AssetReferenceSprite Icon;
}
```

Для interaction layer я рекомендую не множить отдельные специальные controller-ветки, а обобщить уже существующий request pattern (`BuildConstructionRequestEvent`, `DepositConstructionResourcesRequestEvent`) в generic `ExecuteBuildingActionRequest`. Это особенно важно, потому что в проекте уже закреплён boundary rule: owner feature должен сам валидировать и применять свои мутации через event path. Значит, правильный settlement router — это не новый god-system, а thin routing layer между UI/NPC и owner systems здания, склада, кровати, станции или ремонта. fileciteturn95file0L3-L3 fileciteturn96file0L3-L3 fileciteturn100file0L3-L3

```csharp
public enum BuildingInteractionKind : byte
{
    OpenDetails = 1,
    DepositConstructionResources = 2,
    ContributeBuildWork = 3,
    AssignWorker = 4,
    OpenProductionQueue = 5,
    SetRecipe = 6,
    ClaimOutput = 7,
    AssignBed = 8,
    ToggleEnabled = 9,
    TriggerRepair = 10,
    Extract = 11,
    Rest = 12,
    StoreItems = 13,
    WithdrawItems = 14
}

public struct BuildingAvailableAction : IComponent
{
    public EntityGID Building;
    public ushort ActionId;
    public bool Enabled;
    public byte Priority;
    public FixedString64Bytes Label;
    public FixedString64Bytes DisabledReason;
}
```

По хранению моделей лучший следующий шаг — гибридная схема. Оставить `Resources` только для минимального bootstrap и fallback view, а весь catalog settlement views вынести в Addressables: ghost, blueprint, completed, icons, large UI atlases. В проекте уже есть `ViewPath`/`EntityView` separation, но Unity отдельно предупреждает, что большие наборы в `Resources` ухудшают startup и усложняют memory management; Addressables, наоборот, дают address-based lookup, dependency management и явное load/release поведение. fileciteturn99file0L3-L3 citeturn4view3turn4view2

И, наконец, settlement надо сразу связать с nav. В репозитории уже есть отдельный `AiNavigation` design, где `BaseNavArea` описан как постоянная зона для workers, companions, logistics, storage/workstation/bed/gate navigation, а `StructurePlaced` прямо указан как причина nav rebuild. Unity для runtime перестроения navmesh предоставляет `NavMeshBuilder.UpdateNavMeshDataAsync`, который работает от `sources` и `localBounds`, что хорошо ложится на settlement build events и base-area rebuild queue. Поэтому beds, mines, stockpiles и workshop надо проектировать сразу как nav-affecting объекты, а не “добавим потом”. fileciteturn86file0L3-L3 citeturn3view1

## UX и процесс строительства

Игроку нужен не debug menu, а понятный управленческий цикл. Сейчас flow технически есть: открыть build menu, выбрать деревянный дом, получить preview, вращать его, подтвердить placement, потом отдельно вложить ресурсы и строить. Но menu знает только одну кнопку, а context focus выбирается через “repair target или ближайший site в радиусе”, что ощущается скорее как тестовый harness, чем как settlement UX. fileciteturn93file0L3-L3 fileciteturn49file0L3-L3 fileciteturn50file0L3-L3 fileciteturn94file0L3-L3

Правильный UX для Settlement я бы строил в три слоя. Первый — **Build Overlay**: категории слева, карточки по центру, подробная карточка справа, hotkeys снизу. Второй — **Placement Mode**: footprint, snap/grid, rotate, high-visibility invalid reason прямо над курсором, а не только цвет preview. Третий — **Building Details Panel**: после постройки у игрока должен открываться не “Deposit / Build”, а нормальная operational panel с вкладками `Обзор`, `NPC`, `Ресурсы`, `Очередь`, `Обслуживание`, `Назначения`.

Последовательность состояния лучше сделать такой:

```text
Open Build Overlay
-> Select Category
-> Select Building Card
-> Preview Placement
-> Server Validate Placement
-> Spawn Construction Site
-> Deliver Resources
-> Build Work
-> Complete Construction
-> Operational Bootstrap
-> Open Building Details
```

Очень важный UX-штрих: игроку не нужно видеть две “равноправные” primary actions вроде `Deposit` и `Build`, как в текущем Stage1 context panel. Ему нужна одна контекстная CTA-кнопка, которая меняет смысл по стадии объекта. Если стройке не хватает ресурсов — кнопка становится `Пополнить стройку`; если ресурсы уже доставлены — `Строить`; если стройка завершена — `Открыть`; если здание сломано — `Починить`; если это кровать — `Назначить кровать`; если это workbench — `Открыть очередь`. Нынешняя split-логика полезна как внутренний техтест, но в production UX будет путать и дробить mental model. fileciteturn72file0L3-L3 fileciteturn73file0L3-L3

Отдельно стоит изменить focus acquisition. Вместо ближайшей стройки в радиусе четырёх метров нужен `raycast-first` подход: сначала объект под прицелом/курсором, затем fallback на близость, затем — только если ничего нет — no focus. Иначе с ростом числа объектов игрок будет взаимодействовать “не с тем” зданием. Сейчас код прямо показывает, что nearest-site logic встроен в context panel session system; именно это место я бы заменил в первую очередь. fileciteturn94file0L3-L3

## NPC-взаимодействия и первые здания

Базовый NPC-economy scope в репозитории уже фиксирует правильное направление: в MVP нужны как минимум `Gatherer`, `Hauler`, `Processor`, `Guard`, а экономика должна иметь утечки `Food`, `Fuel` и `Durability/Wear`, чтобы база не становилась самоподдерживающейся без решений игрока. Design-lock документ, в свою очередь, закрепляет families `Raw`, `Flow`, `Refined`, `Progression`, `Stability`, а также общий принцип, что specialists должны открывать recipe/progression, а не быть просто “ещё одним worker”. Мой набор ранних построек ниже опирается именно на эту рамку и одновременно продолжает уже существующий builder/repair prototype. fileciteturn87file0L3-L3 fileciteturn88file0L3-L3 fileciteturn15file0L3-L3

| Постройка | Что даёт игроку | Как взаимодействуют NPC | Почему нужна уже в Stage1 |
|---|---|---|---|
| **Camp Core** | Якорь поселения, objective root, unlock node | Все NPC используют как home anchor и fallback point | Это уже есть в текущем repair flow и должно остаться центром settlement |
| **Stockpile** | Вместимость, settlement totals, точка логистики | Hauler приносит и забирает ресурсы; Builder тянет стройматериалы; Processor тянет input | Без нормального склада любая NPC-economy быстро превращается в hidden singleton |
| **Bedroll Shelter** | Жильё и восстановление отдыха | NPC резервируют bed slots, спят, снимают `Rest` дефицит | Это первый “живой” NPC service building |
| **Lumber Camp** | Стабильный приток дерева | Gatherer добывает, Hauler вывозит output | Дерево должно идти не только из стартового seed |
| **Stone Mine** | Стабильный приток камня | Gatherer добывает, Hauler вывозит output | Позволяет закрыть стройку и ремонт без ручного читерского потока |
| **Workbench** | Planks, simple parts, repair kits | Processor крафтит, Hauler кормит input/output | Это первый bridge между raw и refined economy |
| **Field Kitchen** | Улучшенная food loop | Processor готовит, Hauler подвозит input | Иначе `Food` leak из NPC-economy scope остаётся абстракцией |
| **Research Tent** | Ранний specialist gate и research unlock | Researcher/Specialist занимает station slot | Позволяет ввести specialist не как декор, а как progression key |
| **Repair Post** | Repair queue и maintenance hub | Builder/Maintenance закрывает износ | Делает `Durability/Wear` реальной, а не только текстом в дизайне |

Самые критичные NPC-интеграции я бы продумал так. **Кровати** — это не простое число “housing +4”, а набор `BedSlot` с состояниями `Free`, `Reserved`, `Occupied`, `Blocked`. NPC не должен мгновенно “получать отдых”, если shelter существует где-то в мире; отдых должен идти через bed service point, который достижим по навигации и не отключён. Это прямо согласуется с тем, что base navigation в проекте нужна для bed/storage/workstation navigation, а не только для общего перемещения по базе. fileciteturn86file0L3-L3

**Шахты и добывающие постройки** нельзя делать “магическими генераторами ресурса”. Правильнее думать о них как о связке `Workplace + Extraction Zone + Output Buffer + Haul Ports`. Gatherer работает на linked node cluster, добыча складывается во внутренний output buffer, Hauler забирает из него ресурс в `Stockpile`; если буфер full или storage недоступен, extraction должна паузиться. Такой подход естественно вырастает из уже описанного в NPC-economy scope task pipeline `Gather -> Haul -> Process -> Apply`, а не противоречит ему. fileciteturn87file0L3-L3

**Workbench/production stations** в Stage1 должны сразу стать образцом operational interaction. Здание публикует input demand, output availability, worker slots, active recipe и maintenance state. Игрок меняет recipe и priority; Hauler возит input/output; Processor выполняет queue; Maintenance/Builder чинит station; UI показывает не сырые компоненты, а human-readable details panel. Это создаст шаблон, который дальше можно копировать на kitchen, smelter, lab и другие станции без нового hardcode. fileciteturn87file0L3-L3 fileciteturn100file0L3-L3

## Ресурсы, loadout и рамки Stage1

Для ресурсов я бы не изобретал новую taxonomy: в design lock уже закреплены пять семейств — `Raw`, `Flow`, `Refined`, `Progression`, `Stability`. Поэтому settlement table ниже лучше строить именно на них. Это важно ещё и потому, что иначе нынешний singleton-style `Wood / Stone` зацементирует слишком узкую модель хранения, а потом придётся болезненно мигрировать на food, fuel, repair kits, med items и progression drops. fileciteturn88file0L3-L3 fileciteturn21file0L3-L3

| Ресурс | Семейство | Где появляется | Где хранится | Кто потребляет |
|---|---|---|---|---|
| Wood | Raw | Lumber Camp, field gather | Stockpile | стройка, Workbench |
| Stone | Raw | Stone Mine, field gather | Stockpile | стройка, ремонт, Workbench |
| Planks | Refined | Workbench | Stockpile | mid-tier стройка, улучшения |
| Simple Parts | Refined | Workbench | Stockpile | repair, station upgrade |
| Repair Kits | Stability | Workbench / Repair Post | Stockpile / Repair locker | damaged buildings |
| Food | Flow | Kitchen / field loot | Stockpile / Kitchen buffer | NPC upkeep |
| Fuel | Flow | expedition reward / later refinery | Stockpile / station input | processing stations |
| Research Data | Progression | Research Tent / expeds | Research storage | unlocks и recipes |
| Medicine | Stability | later workshop | Stockpile / med locker | NPC recovery |

Отдельно про loadout. В текущем коде уже существуют два combat modules — `PoisonArrow` и `FireFlask`; design lock при этом закрепляет рамку vertical slice по слотам: `3 Combat`, `2 Utility`, `2 Base Signal`, `4 Base Infrastructure`. Значит, Stage1 settlement spec стоит строить так: подтверждённые на сегодня боевые модули использовать как существующий combat baseline, а внутри settlement параллельно спроектировать оболочку под `Base Signal` и `Base Infrastructure`, даже если весь контент там заполнится позднее. Иначе settlement не будет нормально стыковаться с более широким progression/loadout layer. fileciteturn83file0L3-L3 fileciteturn88file0L3-L3

Внутри этого Stage1 я бы рекомендовал такой settlement-aware preset мышления о loadout: combat slots остаются за уже имеющимися боевыми модулями, utility даёт ускорение строительства/логистики, base signals — вызов builder/hauler priority, а base infrastructure — пассивные modifiers типа `+storage capacity`, `+bed efficiency`, `+repair efficiency`, `+extraction yield`. Это позволит игроку воспринимать settlement не отдельно от своего билда, а как часть общей подготовки к вылазкам и обороне базы. fileciteturn83file0L3-L3 fileciteturn88file0L3-L3

Наконец, сам Stage1 лучше немного расширить относительно текущего flow. Сейчас progress stages по коду идут так: `DamagedCampStart -> RepairObjectiveActive -> RepairResourcesReady -> CampRepaired -> WorkerAssigned -> LoadoutPrepared`, а worker assignment handler ещё и не даёт назначить больше одного camp builder на anchor. Для раннего settlement slice это слишком узко. Я бы оставил repair как старт, но между `CampRepaired` и `LoadoutPrepared` вставил бы ещё четыре реальные settlement цели: `StockpilePlaced`, `ShelterPlaced`, `ExtractionOnline`, `WorkbenchOnline`. Тогда Stage1 перестанет быть просто “технической разблокировкой следующего флоу” и станет полноценным первым экономическим контуром. fileciteturn90file0L3-L3 fileciteturn91file0L3-L3 fileciteturn92file0L3-L3

В сжатом виде я бы зафиксировал acceptance criteria так: Settlement должен позволять добавлять новые здания без gameplay-state в `MonoBehaviour`; каждое здание должно иметь definition, operation profile, NPC profile и три visual states; игрок должен проходить цикл *выбор → размещение → стройка → эксплуатация* с человеческим UX; beds, mines, workstations и stockpiles должны сразу публиковать service points и workplace slots для NPC; а Stage1 должен завершаться не в момент “лагерь починен”, а в момент, когда запущен хотя бы один рабочий resource chain и settlement реально начал жить как NPC economy layer. fileciteturn87file0L3-L3 fileciteturn100file0L3-L3