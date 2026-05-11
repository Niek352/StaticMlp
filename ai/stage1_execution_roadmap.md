# Stage 1 Execution RoadMap

Дата оценки: `2026-05-11`

Источник оценки:

- `ai/MVP_plan.md`
- `ai/stage1_vertical_slice_loop_spec.md`
- `ai/stage1_stage2_ready_backlog.md`
- текущая структура `Assets/Scripts/StaticMlp/Features`
- тесты `Assets/Tests/Editor/Combat/FrontierServerSystemsTests.cs`
- тесты `Assets/Tests/Editor/Ai/SettlementWorkersSystemsTests.cs`

## Текущая точка

Проект уже вышел за пределы чистой подготовки архитектуры:

- `Settlement` введён и владеет camp anchor, shared resources и Stage 1 progression до `BuildPrepared`.
- `Settlement.Workers` введён и уже двигает этап `CampRepaired -> WorkerAssigned`.
- `Build` введён и формирует `PreparedBuildSnapshot`.
- `Frontier` фактически играет роль `World` для Stage 1: expedition availability, start, resolution, threat, raid activation, raid resolution уже есть.

Главный незакрытый архитектурный разрыв:

- `Progression` больше не ограничен только ids/catalogs/seeds: введён runtime owner/state и application layer для expedition rewards.
- `Frontier` больше не планирует raid напрямую из expedition resolution; handoff уже переведён в цепочку `expedition reward result -> progression write -> threat escalation`.
- незакрытым остаётся post-raid progression outcome, то есть `CampDefended` и дальнейший boss gate.

Это значит, что проект сейчас логичнее считать на этапе:

`между backlog item 10 и item 11/12`

То есть settlement/build/world foundation уже есть, но связка `expedition -> reward -> progression -> boss gate` ещё не доведена до архитектурно правильного состояния.

## RoadMap

### 1. Архитектурный фундамент

- [x] Зафиксировать typed ids и каталоги для settlement/build/world/progression доменов.
- [x] Вынести Stage 1 seeds из bootstrap в отдельные manifest/seed ресурсы.
- [x] Перенести основные construction contracts под `Settlement`, чтобы `Game.Core` не остался владельцем settlement gameplay.
- [x] Ввести `Settlement` как владельца camp anchor и базовой camp state chain.
- [x] Ввести shared settlement resources как новый authoritative resource source.
- [x] Довести progression chain лагеря до этапа `BuildPrepared`.

### 2. Workers и Build

- [x] Ввести `Settlement.Workers` с worker identity, assignment и summary.
- [x] Привязать worker job state к реальному construction loop.
- [x] Дочистить границу ownership между `Settlement.Workers` и общими `AiActions`/`AiTaskExecution`, чтобы worker domain не продолжал расползаться по mixed AI surface.
- [x] Ввести `Build` feature с `BuildModuleId`, `BuildArchetypeId` и `PreparedBuildSnapshot`.
- [x] Ограничить combat commands через prepared build snapshot.

### 3. Frontier / World slice

- [x] Ввести `Frontier` как текущий Stage 1 owner для expedition/threat/raid flow.
- [x] Открывать expedition только после `BuildPrepared`.
- [x] Запускать expedition через отдельный world-owned request flow.
- [x] Разрешать expedition completion и переводить threat в `RaidPending`.
- [x] Активировать raid по `ServerTick`.
- [x] Завершать raid и очищать active threat state.
- [x] Зафиксировать `Frontier` как постоянное runtime-имя для Stage 1 `World` role: expedition/threat/raid flow остаётся в `Frontier`, без отдельного rename debt.

### 4. Progression и loop completion

- [x] Ввести реальные progression state/contracts, а не только ids/catalogs.
- [x] Реализовать reward application после expedition completion.
- [x] Сделать `Progression` владельцем unlock flags для `RecoveredWarCacheApplied`, `CounterattackDefended`, `BossUnlocked`.
- [x] Перевести expedition result из прямого `raid scheduling` в цепочку `reward return -> progression write -> threat escalation`.
- [x] Зафиксировать `CampDefended` как явный progression outcome, а не только сброс threat state.

### 5. Boss ветка

- [ ] Добавить boss preparation spend/gate поверх `Build + Progression`.
- [ ] Добавить boss unlock contract после корректной expedition+raid progression chain.
- [ ] Добавить boss encounter wrapper на существующем combat/AI фундаменте.
- [ ] Добавить `VerticalSliceComplete` contract.

### 6. Presentation / MVC

- [ ] Ввести Stage 1 read models для `Settlement`, `Workers`, `Build`, `Frontier/World`, `Progression`.
- [ ] Собрать отдельные MVC surfaces для base HUD, build prep, expedition selection, reward result, threat state.
- [ ] Убрать зависимость gameplay path от prototype/debug UI маршрутов.

### 7. Cleanup перед Stage 2

- [ ] Убрать `ResourcesInventoryMinimal` из основной gameplay truth, оставив только legacy/compatibility слой если он ещё нужен.
- [ ] Убрать оставшиеся bootstrap shortcuts и прямые content literals из gameplay path.
- [ ] Проверить, что build selection больше не живёт как ad hoc client-side shortcut.
- [ ] Проверить, что progression/boss flow не шьётся напрямую в `Frontier` без отдельного доменного владельца.

## Что уже можно считать выполненным по Stage 1

- [x] Лагерь как progression anchor уже появился.
- [x] Repair/construction loop уже участвует в основном gameplay chain.
- [x] Worker assignment уже не декоративный и влияет на тот же camp loop.
- [x] Build prep уже меняет combat behavior через `PreparedBuildSnapshot`.
- [x] Expedition и counterattack уже существуют как playable wrappers вокруг существующего combat runtime.

## Что ещё мешает считать Stage 1 закрытым

- [x] Reward return уже записывается в authoritative `Progression`, а post-raid outcome теперь фиксируется в той же цепочке.
- [x] После raid есть корректный persistent progression outcome: `CampDefended` -> `CounterattackDefended`.
- [ ] Boss path отсутствует.
- [ ] Нет отдельного Stage 1 presentation layer поверх новых owners.
- [ ] Vertical slice формально не закрывается состоянием `BossDefeated`.

## Следующая рабочая очередь

Ниже порядок, который сейчас выглядит самым безопасным архитектурно.

1. [ ] Ввести runtime progression state и systems в `Features/Progression/Runtime/Logic`.
2. [ ] Перевести `ServerFrontierExpeditionResolutionSystem` с прямого raid scheduling на выдачу expedition reward result.
3. [ ] Добавить application layer: `reward result -> progression flags/resources -> threat escalation`.
4. [x] Зафиксировать отдельный `CampDefended` progression result после raid resolution.
5. [ ] Только после этого вводить boss prep, boss unlock и boss encounter.
6. [ ] После стабилизации owners строить Stage 1 MVC/read models.

Актуализация после выполнения:

1. [x] Ввести runtime progression state и systems в `Features/Progression/Runtime/Logic`.
2. [x] Перевести `ServerFrontierExpeditionResolutionSystem` с прямого raid scheduling на выдачу expedition reward result.
3. [x] Добавить application layer: `reward result -> progression flags/resources -> threat escalation`.
4. [x] Зафиксировать отдельный `CampDefended` progression result после raid resolution.
5. [ ] Только после этого вводить boss prep, boss unlock и boss encounter.
6. [ ] После стабилизации owners строить Stage 1 MVC/read models.

Изменение плана:

- Прямой переход `Frontier expedition resolution -> RaidPending` признан архитектурно неверным и заменён на handoff через `Progression`.
- `Frontier` сейчас остаётся постоянным runtime-владельцем `World` role для threat/raid runtime, но уже не владельцем reward application и progression flags.

## Текущее место для отметки прогресса

Текущий фокус:

- [x] `Progression runtime ownership`

Следующий после него:

- [x] `Reward return -> threat escalation through Progression`

После этого:

- [ ] `Boss gate and encounter wrapper`
