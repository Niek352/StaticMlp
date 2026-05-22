# Settlement — текущий UX и архитектура

## Общая модель

Settlement в Stage1 — это «база» игрока, привязанная к `SettlementAnchorId` (`HomeCampId`). Все состояние settlement хранится в ECS (`StaticEcs`) и реплицируется через Networking.Replication.

Ключевые сущности:
- **Глобальный stockpile** — одна сущность с `SettlementSharedResources` + `SettlementStoredResource` (multi-component rows). Хранит все ресурсы поселения.
- **Construction Sites** — сущности с `ConstructionSiteState`, `ConstructionResources`, `ConstructionProgress`. Каждый site = стройплощадка конкретного здания.
- **Finished Buildings** — сущности с `FinishedBuildingTag`, привязанные к `BuildingDefinition` из `BuildingCatalogData`.
- **Workers** — сущности с `SettlementWorkerTag`, назначаемые на здания через `SettlementWorkerAssignment`.

---

## Что может делать игрок

### 1. Placement — размещение зданий
**Файлы:** `ClientBuildingMenuSystem`, `ClientPlacementInputSystem`, `ValidationPreviewSystem`, `ConfirmSystem`

1. Игрок открывает меню зданий (клавиша / UI кнопка).
2. `BuildingMenuPresentation` строит список из 6 зданий (`CampCore`, `Stockpile`, `BedrollShelter`, `LumberCamp`, `StoneMine`, `Workbench`), группируя по `BuildingCategory`.
3. Игрок выбирает здание → клиент показывает ghost preview (`PlacementPreviewViewState`).
4. При подтверждении отправляется `PlaceBuildingRequestEvent`.
5. Сервер (`ServerPlaceBuildingRequestSystem`) валидирует позицию через `ConstructionPlacementValidator` и спавнит **construction site** сущность с:
   - `ConstructionSiteState` (BuildingId, Phase = `WaitingForResources`)
   - `ConstructionResources` (multi rows по `BuildingDefinition.ConstructionCost`)
   - `ConstructionProgress` (BuildWorkRequired / BuildWorkDone)
   - `ConstructionTransform`

---

### 2. Construction — строительство
**Файлы:** `ClientConstructionInteractionSystem`, `ServerDepositConstructionResourcesSystem`, `ServerApplyConstructionBuildWorkSystem`, `ServerCompleteConstructionSystem`

Construction site проходит фазы:
```
WaitingForResources → ReadyToBuild → BuildingInProgress → Completed
```

**Действия игрока:**
- **Press Interact** (`CoreInputActions.Interact`) рядом с site:
  - `ClientConstructionInteractionSystem` находит ближайший site в радиусе 4m.
  - Если остались недоставленные ресурсы → отправляет `DepositConstructionResourcesRequestEvent` с *всеми* оставшимися ресурсами (`ConstructionResourcesAccess.GetProjectedRemainingResources`).
  - Сервер списывает ресурсы из `SettlementSharedResources` и добавляет в `ConstructionResources`.
- **Hold Build** (`BuildingsInputActions.BuildConstruction`):
  - Если site в фазе `ReadyToBuild` или `BuildingInProgress` → отправляет `BuildConstructionRequestEvent`.
  - Сервер применяет build work через `SettlementConstructionRules.ApplyBuildWork`.
- Когда `BuildWorkDone >= BuildWorkRequired` → `ServerCompleteConstructionSystem`:
  - Уничтожает site entity.
  - Спавнит finished building entity через `FinishedBuildingFactory`.
  - Если здание = `CampCore` → рассылает `Stage1RepairCompletedEvent`.

**Worker automation:**
- `SettlementWorkerDemandQuery` + `BuildConstructionExecutor` (AI) могут автоматически нести ресурсы и строить.

---

### 3. Worker management — управление рабочими
**Файлы:** `ClientStage1ContextPanelSessionSystem`, `Stage1ContextPanelController`, `SetSettlementWorkerAssignmentRequestEvent`

- Context panel переключается между **Building mode** и **Worker mode**.
- В Worker mode показывается назначенный на текущий anchor worker (или отсутствие).
- Игрок нажимает primary action → `ToggleWorkerAssignment` → отправляет `SetSettlementWorkerAssignmentRequestEvent`.
- Сервер обновляет `SettlementWorkerAssignment.IsAssigned`.

---

### 4. Building interaction — взаимодействие с построенными зданиями
**Файлы:** `ClientStage1ContextPanelStateSystem`, `Stage1ContextPanelController`

Доступные interaction kinds (`BuildingInteractionKind`):
- `OpenDetails` — открыть детали здания.
- `AssignWorker` — назначить рабочего (Workbench, LumberCamp, StoneMine).
- `OpenProductionQueue` / `SetRecipe` / `ClaimOutput` — верстак (Workbench).
- `AssignBed` / `Rest` — жилище (BedrollShelter).
- `Extract` — забрать ресурс из буфера (LumberCamp, StoneMine).
- `StoreItems` / `WithdrawItems` — склад (Stockpile).

Primary action для построенного здания выбирается через hardcoded priority ladder в `SelectCompletedPrimaryAction()`:
```csharp
OpenProductionQueue → StoreItems → AssignBed → Extract → Rest → AssignWorker → OpenDetails
```

Secondary action всегда `OpenDetails`.

При нажатии на primary/secondary action:
- `DepositConstructionResources` / `ContributeBuildWork` → отправляют сетевые запросы.
- Все остальные → пишут `Stage1BuildingOperationOpenIntent` resource, который открывает под-панель с summary.

---

### 5. Stage1 progression — линейное прохождение
**Файлы:** `ServerStage1FlowSystem`, `ServerStage1FlowViewStateSystem`

Стадии (строгий порядок):
```
DamagedCampStart
  ↓ (auto)
RepairObjectiveActive
  ↓ (auto, когда в stockpile достаточно Wood+Stone)
RepairResourcesReady
  ↓ (после завершения строительства CampCore)
CampRepaired
  ↓ (после назначения worker)
WorkerAssigned
  ↓ (после размещения Stockpile)
StockpilePlaced
  ↓ (после размещения Shelter)
ShelterPlaced
  ↓ (после запуска Extraction)
ExtractionOnline
  ↓ (после запуска Workbench)
WorkbenchOnline
  ↓ (после подготовки loadout)
LoadoutPrepared
```

- `ServerStage1FlowSystem` слушает события (`Stage1RepairCompletedEvent`, `Stage1WorkerAssignmentAcceptedEvent` и др.) и продвигает стадию.
- `ServerStage1FlowViewStateSystem` вычисляет `Stage1FlowObjective` и `Stage1FlowHint` на основе текущей стадии и world state.
- `ClientStage1HudStateSystem` превращает objective/hint в presentation strings.

---

### 6. Expedition / Boss / Threat — через HUD
**Файлы:** `ClientStage1HudStateSystem`, `Stage1HudController`

HUD отображает:
- Текущий objective и hint.
- Количество Wood и Stone (только эти два ресурса).
- Количество workers (total / assigned) и blocking reason.
- Статус expedition (доступна / активна).
- Статус threat / raid / boss.
- Готовность loadout (prepared primary module).
- Флаги progression (`HasRecoveredWarCache`, `HasBossUnlocked`, `BossPreparationTokens`).

По мере прохождения Stage1 unlocking-и открывают кнопки:
- `CanOpenLoadoutPreparation` → экран сборки loadout.
- `CanOpenExpeditionSelection` → экран выбора экспедиции.

---

## Ключевые asmdef и папки

```
Assets/Scripts/StaticMlp/Features/Settlement/
  Runtime/Logic/          — серверные системы, domain rules, components
  Runtime/Presentation/   — клиентские presentation systems, controllers, views
  Runtime/Contracts/      — ResourceCatalog, BuildingCatalogData, IDs

Assets/Scripts/StaticMlp/Features/Buildings/
  Runtime/Systems/Client/ — ClientConstructionViewStateSystem, ClientConstructionInteractionSystem
  Runtime/Components/     — ConstructionViewState

Assets/Scripts/StaticMlp/Features/BuildingCatalog/
  Runtime/Catalogs/       — BuildingCatalogData, BuildingPresentationCatalog
```
