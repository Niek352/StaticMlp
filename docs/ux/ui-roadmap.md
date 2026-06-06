# UI Roadmap

Сводный документ по gameplay UI: что уже есть в коде, что сейчас считается актуальным UX-контрактом, и что нужно сверстать или разнести по owner-фичам дальше.

## Архитектурные правила

- UI остается client-only presentation. Он читает feature-owned presentation state/read model и отправляет intent/request events, но не мутирует gameplay truth напрямую.
- Композитный HUD может собирать секции из Settlement, Loadout, Progression, Frontier, Combat и Inventory, но каждая секция должна читать состояние владельца фичи, а не лезть в чужие `*.Logic` internals.
- Новые окна и HUD-секции должны идти через `Aspid.StaticEcs.Windows`, `EcsLinkRegistry` и Aspid.MVVM ViewModel.
- Unity `View`/`ViewPart` остаются пассивными: inspector references, binders, rendering, forwarding UI intent.
- Префабы, Canvas hierarchy, inspector wiring и `.meta` файлы не создаются и не правятся агентом.
- `Stage1ContextPanelView` считается legacy artifact. Его не нужно развивать как актуальный UX-контракт; следующий cleanup должен удалить его или перенести нужные worker/building части в отдельные interaction-first панели.

## Легенда

| Статус | Значение |
|---|---|
| `Есть, MVP` | Кодовый путь и View/ViewModel уже есть, но верстка в основном summary-based или требует prefab polish. |
| `Есть частично` | Есть state, системный поток или world-space part, но нет полноценного player-facing окна/секции. |
| `Нужно сверстать` | Нужен новый или расширенный View/prefab layout поверх уже намеченного UX. |
| `Нужно спроектировать` | Нужна отдельная архитектурная развязка owner state/read model перед версткой. |
| `Legacy` | Остаток Stage1/старого UX, не считать целевым контрактом. |

## Что есть сейчас

| Область | Сейчас |
|---|---|
| Settlement HUD | `SettlementHudView` зарегистрирован как persistent window, но текущая верстка сведена к `summary` и кнопкам `Prepare Build`, `Open Expedition`, `Close`. В summary уже агрегируются objective, hint, ресурсы поселения, workers, loadout, expedition, threat, raid и boss flags. |
| Resources inventory HUD | `ResourcesInventoryHudView` зарегистрирован как persistent window. Есть `IsReady` и `summary` со слотами/total/capacity, но нет отдельной компактной сетки слотов. |
| Loadout HUD section | Есть `LoadoutHudState` и `LoadoutHudPresentation`; отображается внутри composite summary Settlement HUD. Отдельной HUD-секции/верстки нет. |
| Progression HUD section | Есть `ProgressionHudState` и `ProgressionHudPresentation`; отображается внутри composite summary Settlement HUD. Отдельной HUD-секции/верстки нет. |
| Frontier HUD sections | Есть `ExpeditionHudState`, `ThreatHudState`, `RaidHudState`, `BossHudState` и presentation builders; отображение сейчас в основном через Settlement HUD summary и `ThreatBannerView`. |
| Interaction prompt | `InteractionPromptView` есть как persistent window. Prompt показывается только при focus на interactable и скрывается без focus. |
| Settlement context panel | `SettlementContextPanelView` есть, но его нужно вести осторожно: он должен оставаться компактной interaction-first панелью, а building/worker-specific UX лучше выносить в owner-секции. |
| Building menu | `BuildingMenuView` есть, карточки зданий и выбор переводят в placement mode. Текущая View имеет `summary`, selected building и команды выбора/close; нужна более явная карточная верстка. |
| Placement | Есть `PlacementPreviewViewPart`, preview state и valid/invalid material. Bottom overlay с подсказками rotate/confirm/cancel и текстовым invalid reason пока не оформлен как полноценный HUD. |
| Building management panel | `BuildingManagementPanelView` есть. Сейчас это summary плюс primary/secondary actions/close; typed intents уже проходят через panel action pipeline. |
| Building panel subsections | В state уже выделены `ConstructionPanelState`, `StockpilePanelState`, `WorkbenchPanelState`, `ExtractionPanelState`, `ShelterPanelState`, `CampCorePanelState`. Визуально они еще не оформлены отдельными reusable sections. |
| Loadout preparation | `LoadoutPreparationView` есть: summary, selected module, states для Poison Arrow/Fire Flask, confirm, close. |
| Expedition selection | `ExpeditionSelectionView` есть: summary, start, close. Карточная верстка экспедиции и requirements/readiness еще нужна. |
| Reward popup | `RewardResultPopupView` есть: summary и close. Нужна richer reward list layout. |
| Threat banner | `ThreatBannerView` есть: visibility и summary. Подходит как базовый warning/banner, но требует visual polish. |
| Combat world-space | `CombatHealthViewPart`, `StatusAuraViewPart`, `PassiveAutoAttackViewPart` существуют как entity view parts. Полноценный `PlayerCombatHud` отдельно еще не сделан. |
| Resource world-space | `ResourcePickupViewPart` и `OpenWorldResourceNodeViewPart` существуют. Есть состояния pickup/node, highlight/depleted/ready нужно проверить в Unity. |
| Building world-space | `ConstructionSiteViewPart`, `WoodenHutViewPart`, `PlacementPreviewViewPart` существуют. Есть world-state presentation, но UX маркеры и прогресс требуют polish. |
| NPC, item inventory, settings, debug | Отдельные player-facing панели пока не являются текущим UI-контрактом. |

## Что нужно сделать

### Основной HUD

1. `SettlementHudView`
   - Разбить текущий `summary` на явные блоки: objective/hint, ресурсы поселения, workers assigned/total.
   - Оставить команды `Prepare Build`, `Open Expedition`, `Close`.
   - Не превращать Settlement HUD в владельца чужой gameplay state; Loadout/Progression/Frontier должны отдавать свои секции через read model/presentation contracts.

2. `ResourcesInventoryHudView`
   - Сверстать компактный инвентарь переносимых ресурсов.
   - Показать ready state, total/used/capacity summary и слоты ресурсов отдельными элементами, а не только текстом.

3. `LoadoutHud` section
   - Сделать отдельную секцию текущего подготовленного билда.
   - Показать primary module и статус `build prepared / not prepared`.

4. `ProgressionHud` section
   - Сделать отдельную секцию ключевых этапов.
   - Показать boss unlock и boss prep tokens.

5. `FrontierHud` sections
   - Разнести expedition status, threat phase, raid warning/timer и boss encounter status по отдельным HUD-секциям.
   - `ThreatBannerView` оставить для коротких событий поверх HUD, а не как замену постоянному Frontier HUD.

6. `PlayerCombatHud`
   - Спроектировать отдельный HUD для локального игрока: HP, статусы/ауры, активные эффекты, cooldown/auto-attack state.
   - Не использовать entity view parts как замену player HUD; world-space части остаются маркерами над сущностями.

### Взаимодействие

7. `InteractionPromptView`
   - Сверстать нижний/центральный prompt.
   - Показать keycap `E` и текст вида `Press E to Open <Building>`.
   - Скрывать без focus и при открытой панели, как сейчас задано state-логикой.

8. `SettlementContextPanelView`
   - Держать панель компактной: summary, primary action, secondary action.
   - Не раздувать ее в универсальный building manager. Building-specific и worker-specific UX должны постепенно уходить в owner-секции.

### Строительство

9. `BuildingMenuView`
   - Сверстать категории зданий, карточки зданий, cost line, locked reason, selected building summary и close.
   - Сделать UX выбора явным: клик по карточке переводит в placement mode.

10. `PlacementOverlay` / `PlacementPreviewViewPart`
    - Оставить world ghost здания и valid/invalid material в `PlacementPreviewViewPart`.
    - Добавить overlay hints: rotate, confirm, cancel.
    - Показать invalid reason текстом, а не только material state.

11. `BuildingManagementPanelView`
    - Сверстать большую панель выбранной стройки/здания: title/summary, primary action, secondary action, close, disabled reason, transient feedback.
    - Сохранить текущую fail-fast модель: disabled action не должен молча выполняться.

12. Reusable sections для `BuildingManagementPanelView`
    - `ConstructionPanel`: фаза стройки, список требуемых ресурсов, progress bar, deposit/build.
    - `StockpilePanel`: storage used/capacity, carried raw, expected transfer, `Store Items`.
    - `WorkbenchPanel`: active recipe, recipe choices, inputs, outputs, claimable output, storage capacity, `Claim Output`, смена рецепта.
    - `ExtractionPanel`: output buffer, collect action, статус добычи.
    - `ShelterPanel`: beds/slots/status.
    - `CampCorePanel`: core status, settlement-level summary.
    - `WorkerSlotsPanel`: worker slots, assigned workers, blocking reason.

### Прогрессия и loop

13. `LoadoutPreparationView`
    - Текущий View уже покрывает Poison Arrow / Fire Flask, selected module, confirm и close.
    - Следующий шаг: сверстать список модулей и выбранный модуль как нормальную карточную/списочную структуру, а не только summary.

14. `ExpeditionSelectionView`
    - Сверстать карточку экспедиции.
    - Показать требования, готовность, start и close.

15. `RewardResultPopupView`
    - Сверстать reward summary и список наград.
    - Оставить close как единственную обязательную команду popup.

16. `ThreatBannerView`
    - Сверстать короткий warning/banner поверх HUD для threat/raid/boss события.
    - Не делать из него тяжелую blocking-панель.

### World-space presentation

17. `CombatHealthViewPart`
    - Сверстать health bar над entity или привязанный к entity marker.

18. `StatusAuraViewPart`
    - Сверстать иконки/кольца статусов над entity.

19. `ResourcePickupViewPart` / `OpenWorldResourceNodeViewPart`
    - Сверстать highlight, depleted/ready state, pickup/resource marker.

20. `ConstructionSiteViewPart` / `WoodenHutViewPart`
    - Сверстать состояние здания в мире: under construction, finished, progress, interactable marker.

### Будущие панели

21. `NpcInteractionPanel`
    - Спроектировать после появления стабильного NPC owner read model.
    - Верстать NPC name, role/job, recruit/rescue/assign actions, blocking reason.

22. `NpcRosterPanel`
    - Спроектировать отдельно от building management.
    - Верстать список NPC, роли, traits, текущие задания, назначение к зданиям.

23. `ItemInventoryPanel`
    - Вести отдельно от raw resources inventory.
    - Верстать предметы/loadout inventory после стабилизации item/loadout contracts.

24. `Settings/Pause/Menu`
    - Делать позже отдельным shell/window stack, не смешивать с gameplay HUD.

25. `DebugHud`
    - Только dev UI: network/session/world/debug stats.
    - Не смешивать с player-facing HUD.

## Рекомендуемый порядок работ

1. Довести текущие MVP Views до читаемой верстки без изменения gameplay contracts: `SettlementHudView`, `ResourcesInventoryHudView`, `InteractionPromptView`, `BuildingMenuView`, `BuildingManagementPanelView`.
2. Разбить summary-based HUD на owner-секции: Loadout, Progression, Frontier.
3. Оформить reusable sections внутри `BuildingManagementPanelView`.
4. Добавить `PlacementOverlay` с hints и invalid reason.
5. Сделать отдельный `PlayerCombatHud`.
6. Провести cleanup `Stage1ContextPanelView` и связанных legacy типов.
7. После стабилизации NPC/item contracts добавить NPC и item inventory панели.

## Unity checklist

- [ ] HUD показывает objective/hint, settlement resources и workers не только одной summary-строкой.
- [ ] Inventory HUD показывает слоты ресурсов отдельными элементами.
- [ ] Building menu явно показывает locked reason и переводит выбранную карточку в placement mode.
- [ ] Placement mode показывает rotate/confirm/cancel hints и invalid reason.
- [ ] Building management panel показывает disabled reason и transient feedback в понятном месте.
- [ ] Workbench recipe selection не выглядит как cycling hidden action, если доступно несколько recipes.
- [ ] Interaction prompt скрывается без focus и не конфликтует с открытыми панелями.
- [ ] `Stage1ContextPanelView` не используется как актуальный UX-контракт.
