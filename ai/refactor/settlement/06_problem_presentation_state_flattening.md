# Проблема: Flattening presentation state и cross-feature чтения (найдена при аудите)

> **СТАТУС РЕФАКТОРА (2025-05-22): частично выполнен**
>
> ## Сделано
> - Монолитный `Stage1HudState` удалён. Созданы owner-scoped ресурсы:
>   `SettlementHudState`, `ExpeditionHudState`, `ThreatHudState`, `RaidHudState`, `BossHudState`, `LoadoutHudState`, `ProgressionHudState`.
> - Смешанный `Stage1ContextPanelState` удалён. Созданы `BuildingContextPanelState` + `WorkerContextPanelState`.
> - Удалены старые builder-системы: `ClientStage1HudStateSystem`, `ClientStage1ContextPanelStateSystem`.
> - Созданы composite bridge-системы: `Stage1HudCompositeBridgeSystem`, `Stage1ContextPanelCompositeBridgeSystem`.
> - Создан `PresentationDependencyScopeTests` — архитектурный guardrail, предотвращающий новые cross-feature чтения в `Settlement.Presentation`.
>
> ## Осталось
> 1. **Writer-системы (`ISystem`) не созданы**. Вместо `ClientSettlementHudStateSystem`, `ClientBuildingContextPanelStateSystem` и т.д. используются static классы (`SettlementHudPresentation.Build()`, `BuildingContextPanelPresentation.Build()`), вызываемые из bridge каждый кадр. Это работает, но не соответствует изначальному плану (Task 1–3 в `06.02_problem_fixing.md`).
> 2. **Cross-feature reads в presentation остаются** (например, `ExpeditionHudPresentation` читает `ExpeditionAvailabilityState`, `LoadoutHudPresentation` читает `PreparedLoadoutSnapshot`). Это задокументировано как baseline debt, но требует дальнейшего выноса read model в owner features.
> 3. **Остаточный долг**: `Settlement.Presentation` всё ещё читает `ConstructionSiteState` / `ConstructionProgress` (Buildings feature) в `BuildingContextPanelPresentation`. Это отражено в baseline `PresentationDependencyScopeTests`.

---

# Проблема: Flattening presentation state и cross-feature чтения (найдена при аудите)

## Краткое описание

`Stage1HudState` — это монолитный `IResource` с 30+ полей, собирающий данные из 6 разных фич: Settlement, Loadout, Frontier, Progression, Threat, Raid, Boss. `ClientStage1HudStateSystem` читает компоненты, которые ей не принадлежат, нарушая архитектурные границы.

---

## Конкретные места

### 1. Stage1HudState — монолитный resource
**Файл:** `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Stage1HudState.cs`

```csharp
public struct Stage1HudState : IResource
{
    public SettlementAnchorId AnchorId;
    public Stage1ObjectiveKind Objective;
    public string ObjectiveHint;
    public Stage1SettlementProgressStage SettlementStage;
    public int Wood;                          // ← Settlement
    public int Stone;                         // ← Settlement
    public ushort TotalWorkers;               // ← Settlement.Workers
    public ushort AssignedWorkers;            // ← Settlement.Workers
    public SettlementWorkerBlockingReason WorkerBlockingReason; // ← Settlement.Workers
    public LoadoutModuleId PreparedPrimaryModuleId; // ← Loadout
    public bool HasPreparedBuild;             // ← Loadout
    public ExpeditionAvailabilityStatus ExpeditionAvailability; // ← Frontier
    public ExpeditionActivityStatus ExpeditionActivity;         // ← Frontier
    public ThreatPhase ThreatPhase;           // ← Threat
    public RaidScheduleStatus RaidScheduleStatus; // ← Raid
    public uint RaidActivateAtTick;           // ← Raid
    public bool HasRecoveredWarCache;         // ← Progression
    public bool HasCounterattackDefended;     // ← Progression
    public bool HasBossUnlocked;              // ← Progression
    public byte BossPreparationTokens;        // ← Progression
    public BossEncounterStatus BossEncounterStatus; // ← Boss
    public bool CanOpenLoadoutPreparation;    // ← derived
    public bool CanOpenExpeditionSelection;   // ← derived
}
```

### 2. ClientStage1HudStateSystem читает чужие компоненты
**Файл:** `ClientStage1HudStateSystem.Update()`

```csharp
// Settlement (свои)
ref readonly var flow = ref ClientProjection.Read<Stage1FlowViewState>(anchor);
Wood = SettlementSharedResourcesAccess.GetProjectedAmount(resources, ResourceCatalog.WoodId);

// Progression (чужая фича)
ref readonly var progression = ref ClientProjection.Read<Stage1ProgressionState>(anchor);
next.HasRecoveredWarCache = progression.HasFlag(ProgressFlagCatalog.RecoveredWarCacheAppliedId);
next.HasBossUnlocked = progression.HasFlag(ProgressFlagCatalog.BossUnlockedId);

// Frontier (чужая фича)
ref readonly var expedition = ref ClientProjection.Read<ExpeditionAvailabilityState>(anchor);
next.ExpeditionAvailability = expedition.Status;

// Threat (чужая фича)
ref readonly var threat = ref ClientProjection.Read<ThreatState>(anchor);
next.ThreatPhase = threat.Phase;

// Raid (чужая фича)
ref readonly var raid = ref ClientProjection.Read<RaidScheduleState>(anchor);
next.RaidScheduleStatus = raid.Status;

// Boss (чужая фича)
ref readonly var boss = ref ClientProjection.Read<BossEncounterState>(anchor);
next.BossEncounterStatus = boss.Status;

// Loadout (чужая фича)
foreach (var player in CW.Query<All<PreparedLoadoutSnapshot>>().Entities())
    next.PreparedPrimaryModuleId = player.Read<PreparedLoadoutSnapshot>().PrimaryModuleId;
```

### 3. Settlement presentation знает о внутренностях других фич
- `ProgressFlagCatalog.RecoveredWarCacheAppliedId` — internal detail Progression feature.
- `ExpeditionAvailabilityState`, `ActiveExpeditionState` — internal Frontier feature.
- `ThreatState`, `RaidScheduleState`, `BossEncounterState` — internal Threat/Boss features.
- `PreparedLoadoutSnapshot` — internal Loadout feature.

По архитектуре (AGENTS.md): **"A feature must not write another feature's component or tag state through Mut<T>()"**. Хотя здесь `Read<T>()` в presentation layer, это всё равно создаёт жёсткую coupling: изменение структуры `ThreatState` сломает Settlement presentation system.

---

## Проблемы

1. **Coupling:** Settlement presentation зависит от внутренних replicated компонентов 5 других фич.
2. **Merge conflicts:** несколько фич правят `Stage1HudState` и `ClientStage1HudStateSystem` одновременно.
3. **Тестирование:** чтобы протестировать Settlement HUD, нужно мокать компоненты Loadout, Frontier, Boss.
4. **Reusability:** `Stage1HudState` нельзя переиспользовать в другом режиме игры (например, Stage2), потому что он заточен под конкретный набор cross-feature полей.

---

## План рефактора

### Шаг 1. Разбить Stage1HudState на специализированные resources
Каждая фича поставляет свой собственный `IResource`:

```csharp
// Settlement feature
public struct SettlementHudState : IResource
{
    public SettlementAnchorId AnchorId;
    public string Objective;
    public string Hint;
    public int Wood;
    public int Stone;
    public ushort TotalWorkers;
    public ushort AssignedWorkers;
    public SettlementWorkerBlockingReason WorkerBlockingReason;
}

// Frontier feature
public struct ExpeditionHudState : IResource
{
    public ExpeditionAvailabilityStatus Availability;
    public ExpeditionActivityStatus Activity;
    public bool CanOpenExpeditionSelection;
}

// Loadout feature
public struct LoadoutHudState : IResource
{
    public LoadoutModuleId PreparedPrimaryModuleId;
    public bool HasPreparedBuild;
    public bool CanOpenLoadoutPreparation;
}

// Threat feature
public struct ThreatHudState : IResource
{
    public ThreatPhase ThreatPhase;
}

// Raid feature
public struct RaidHudState : IResource
{
    public RaidScheduleStatus Status;
    public uint ActivateAtTick;
}

// Boss feature
public struct BossHudState : IResource
{
    public bool HasBossUnlocked;
    public byte BossPreparationTokens;
    public BossEncounterStatus EncounterStatus;
}

// Progression feature
public struct ProgressionHudState : IResource
{
    public bool HasRecoveredWarCache;
    public bool HasCounterattackDefended;
}
```

### Шаг 2. Каждая фича — своя presentation system
- `ClientSettlementHudStateSystem` — в `Settlement` feature, читает только Settlement компоненты.
- `ClientExpeditionHudStateSystem` — в `Frontier` feature, читает `ExpeditionAvailabilityState` / `ActiveExpeditionState`.
- `ClientLoadoutHudStateSystem` — в `Loadout` feature.
- `ClientThreatHudStateSystem` — в `Threat` feature.
- `ClientRaidHudStateSystem` — в `Raid` feature.
- `ClientBossHudStateSystem` — в `Boss` feature.
- `ClientProgressionHudStateSystem` — в `Progression` feature.

Каждая система пишет в свой `IResource`. Никаких cross-feature reads.

### Шаг 3. Composite HUD Controller
`Stage1HudController` становится агрегирующим контейнером:

```csharp
public class Stage1HudController : ControllerBase<Stage1HudView>
{
    private readonly SettlementHudController _settlement;
    private readonly ExpeditionHudController _expedition;
    private readonly LoadoutHudController _loadout;
    // ...

    public void Apply(
        in SettlementHudState settlement,
        in ExpeditionHudState expedition,
        in LoadoutHudState loadout,
        ...)
    {
        _settlement.Apply(in settlement);
        _expedition.Apply(in expedition);
        _loadout.Apply(in loadout);
        // ...
    }
}
```

Каждый sub-controller отвечает за свой участок UI (отдельные rect-ы внутри общего HUD canvas).

### Шаг 4. Bridge System для композиции
В `Settlement` feature (или в Game.Core) ввести `Stage1HudCompositeSystem`:

```csharp
public sealed class Stage1HudCompositeSystem : ISystem
{
    public void Update()
    {
        var settlement = CW.GetResource<SettlementHudState>();
        var expedition = CW.GetResource<ExpeditionHudState>();
        var loadout = CW.GetResource<LoadoutHudState>();
        // ...

        // Обновить composite controller
        // (или каждый sub-controller подписан на свой bridge)
    }
}
```

Альтернатива: каждый sub-controller имеет свой `ControllerResourceBridgeSystem`, binding к своему `IResource`. Unity View просто имеет несколько child panels, каждый управляется своим контроллером.

---

## Что сломается, если не рефакторить

1. Изменение `ThreatState` (добавление поля) потребует перекомпиляции Settlement asmdef.
2. Две команды (Settlement + Frontier) не могут параллельно работать над HUD — merge conflicts в одном файле.
3. Settlement presentation system знает о существовании `BossEncounterState` — нарушение feature boundaries.
4. Невозможно отключить Expedition/Boss UI для прототипа без комментирования кода в Settlement.
