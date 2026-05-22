# Settlement UX Status

Canonical UX status для текущего Settlement vertical slice. Research overview остается в `ai/refactor/settlement/01_settlement_ux_overview.md`; этот документ отвечает на практический вопрос: что игрок сейчас видит, что можно нажать, что работает, что скрыто, и что нужно проверить в Unity.

## Легенда статусов

| Статус | Значение |
|---|---|
| `Реализовано, нужна Unity-проверка` | Code path существует; поведение нужно проверить в Unity Editor на реальной view/prefab сборке. |
| `Проверено в Unity` | Ручная проверка в Unity Editor выполнена. |
| `UX gap` | Поведение технически существует, но player-facing affordance или объяснение слабые. |
| `Hidden input` | Input action работает через keyboard/mouse/controller state, но не объяснен в UI. |
| `Legacy Stage1` | Поведение осталось от Stage1 сценария и не должно быть долгосрочной UX-опорой. |

## Interaction Prompt

**View path:** `Resources/Views/Settlement/InteractionPromptView`  
**Controller:** `InteractionPromptController`  
**State:** `InteractionPromptState`  
**Focus source:** `InteractionFocus`, построенный через `InteractableFocusPoint`.

| Условие focus | Prompt text | Result |
|---|---|---|
| Construction site, ресурсы еще не доставлены | `Press E to Open Construction` | `InteractPressedEvent` открывает `BuildingManagementPanelSession`. |
| Construction site, ресурсы доставлены | `Press E to Manage Construction` | Открывается панель строительства с build/details actions. |
| Finished building | `Press E to Open <Building Name>` | Открывается панель управления выбранным зданием. |

Статус: `Реализовано, нужна Unity-проверка`.

Изменение поведения: prompt появляется только при focus на интерактивный объект. Если focus нет или building panel уже открыта, prompt скрыт.

## Building Management Panel

**View path:** `Resources/Views/Settlement/BuildingManagementPanelView`  
**Controller:** `BuildingManagementPanelController`  
**Session:** `BuildingManagementPanelSession`  
**Presentation state:** `BuildingManagementPanelState`

| Control | Что делает | Доступность |
|---|---|---|
| Primary action | `Deposit`, `Build` или основная operation для completed building. | `BuildingAvailableActionPresentation.Enabled`; disabled reason показывается в summary. |
| Secondary action | `Open` details/summary. | Доступна для выбранного building/site. |
| `Close` | Закрывает `BuildingManagementPanelSession`. | Всегда доступна в открытой панели. |
| `Cancel` input | Закрывает открытую панель. | Активен только когда `BuildingManagementPanelSession.IsOpen`. |

Статус: `Реализовано, нужна Unity-проверка`.

Важно: `Interact` больше не делает deposit напрямую. `Interact` только открывает панель. Deposit/build/operation clicks остаются typed UI intents и уходят через request/event pipeline.

## Interaction Focus

**Feature:** `StaticMlp.Features.Interaction`  
**Focus contract:** `InteractableFocusPoint`  
**Focus state:** `InteractionFocus`

| Поведение | Code path | Статус |
|---|---|---|
| Aim/raycast-first focus | `ClientInteractionFocusSystem` выбирает объект под aim ray, если он попадает в focus radius. | Реализовано, нужна Unity-проверка. |
| Nearest fallback | Если aim ray не попал ни в один interactable, выбирается ближайший объект в 4m. | Реализовано, нужна Unity-проверка. |
| Building focus data | `ClientConstructionInteractableFocusPointSystem` копирует `ConstructionTransform.Position` в `InteractableFocusPoint.Position`. | Реализовано, нужна Unity-проверка. |

Known gap: focus работает через ECS focus volumes, а не через физические collider hit objects. Это лучше текущего nearest-only поведения, но визуальная точность зависит от `InteractableFocusPoint.Radius`.

## Stage1HudView

**View path:** `Resources/Views/Stage1/Stage1HudView`  
**Controller:** `Stage1HudController`  
**Presentation state:** `SettlementHudState` плюс owner HUD states из Frontier, Loadout и Progression.

| Control | Что делает | Когда доступен | Статус |
|---|---|---|---|
| `Prepare Build` | Открывает `LoadoutPreparationController`. | `SettlementHudState.LoadoutPreparationAction.Enabled`. | Реализовано, нужна Unity-проверка. |
| `Open Expedition` | Открывает `ExpeditionSelectionController`. | `SettlementHudState.ExpeditionSelectionAction.Enabled`. | Реализовано, нужна Unity-проверка. |
| `X` / close | Прячет HUD через `Stage1HudSession.IsVisible = false`. | Пока HUD видим. | Реализовано, нужна Unity-проверка. |

Stage1 HUD пока остается `Legacy Stage1`: он все еще читает часть Stage1/Frontier/Loadout flow state. Цель текущего прохода - отвязать building interaction UX, не удалять Stage1 полностью.

## BuildingMenu

**View path:** `Resources/Views/Buildings/BuildingMenu`  
**Controller:** `BuildingMenuController`  
**State:** `BuildingMenuState`

| Control | Что делает | Code path | Статус |
|---|---|---|---|
| Building cards: `CampCore`, `Stockpile`, `BedrollShelter`, `LumberCamp`, `StoneMine`, `Workbench` | Выбирает здание и переводит игрока в placement. | `BuildingMenuView -> BuildingMenuController.SelectBuilding -> BuildingMenuState.Select` | Реализовано, нужна Unity-проверка. |
| `Close` | Закрывает меню и очищает selection. | `BuildingMenuController.CloseMenu` | Реализовано, нужна Unity-проверка. |
| Build menu toggle input | Открывает/закрывает меню. | `ClientBuildingMenuSystem`, `BuildingsInputActions.BuildMenuToggle` | Hidden input, UX gap. |

Known gap: меню показывает cards и costs, но UX все еще должен явно объяснять, что выбор card переводит игрока в placement mode.

## Placement

**Systems:** `ClientPlacementInputSystem`, `ClientPlacementValidationPreviewSystem`, `ClientPlacementConfirmSystem`  
**State:** `PlacementPreview`, `PlacementPreviewViewState`

| Действие игрока | Input source | Code path | Статус |
|---|---|---|---|
| Move ghost preview | Aim ray или camera fallback. | `ClientPlacementInputSystem.ReadPlacementPosition` | Реализовано, нужна Unity-проверка. |
| Rotate preview | `BuildingsInputActions.PlacementRotate` | `ClientPlacementInputSystem` | Hidden input, UX gap. |
| Confirm placement | `CoreInputActions.Primary` | `ClientPlacementConfirmSystem -> PlaceBuildingRequestEvent` | Hidden input, UX gap. |
| Cancel placement | `CoreInputActions.Cancel` или `CoreInputActions.Secondary` | `ClientPlacementInputSystem.ClearSelection` | Hidden input, UX gap. |

Known gap: invalid preview меняет material через `PlacementPreviewViewPart`, но конкретный `PlacementInvalidReason` не показан игроку текстом.

## Legacy Stage1 Context Panel

Старая постоянная `Stage1ContextPanel` больше не регистрируется в `SettlementPresentationFeature`.

Оставшиеся legacy-типы (`Stage1ContextPanelSession`, `ClientStage1ContextPanelSessionSystem`, `Stage1ContextPanelController`, `Stage1ContextPanelView`) пока остаются в коде как неиспользуемый Stage1 artifact. Их нельзя считать текущим UX контрактом. Следующий cleanup-проход должен либо удалить их, либо перенести оставшуюся worker-management часть в отдельный interaction-first worker/building workflow.

## Ручной Unity Checklist

- [ ] Без focus на interactable prompt не виден.
- [ ] При наведении на construction site появляется `Press E to Open Construction` или `Press E to Manage Construction`.
- [ ] При наведении на finished building появляется `Press E to Open <Building Name>`.
- [ ] `Interact` открывает `BuildingManagementPanelView` именно для выбранного building/site.
- [ ] Смена focus не меняет уже открытую building panel.
- [ ] `Close` и `Cancel` закрывают building panel.
- [ ] Primary/secondary actions показывают label, expected effect и disabled reason.
- [ ] `Interact` больше не делает deposit напрямую; deposit происходит через primary action в панели.
- [ ] Старая постоянная `Stage1ContextPanelView` больше не появляется сама.
- [ ] Stage1 HUD/objective, если виден, воспринимается как `Legacy Stage1`, а не как building interaction UX.
