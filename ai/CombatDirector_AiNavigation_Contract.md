# CombatDirector_AiNavigation_Contract.md

## 1. Цель документа

Документ фиксирует контракт между `CombatDirector` и `AiNavigation`.

Главная цель — не смешать боевую дирекцию, spawn logic, pathfinding, NavMesh rebuild и AI movement в одну монолитную систему.

---

## 2. Ответственность Combat Director

`CombatDirector` отвечает за:

- combat pressure;
- director phase;
- threat budget;
- rhythm: `Calm → BuildUp → Peak → Relief → Cooldown`;
- выбор типа волны;
- выбор spawn source из уже валидированных кандидатов;
- server-authoritative spawn request;
- alive cap;
- fairness rules;
- telegraph intent.

`CombatDirector` не отвечает за:

- `NavMeshBuilder`;
- создание `NavMeshData`;
- pathfinding;
- chunk streaming;
- global route;
- локальное движение врага;
- base NPC navigation;
- View/VFX/UI.

---

## 3. Ответственность AiNavigation

`AiNavigation` отвечает за:

- наличие `RuntimeNavMesh` в зоне боя;
- наличие `RuntimeNavMesh` в зоне базы;
- `GlobalNavGraph`;
- far AI approximate movement;
- переход AI в `LocalNavMesh`;
- проверку достижимости spawn sources;
- path/reachability cache;
- nav rebuild queue;
- nav source registry.

`AiNavigation` не отвечает за:

- phase combat director;
- threat budget;
- enemy composition;
- создание enemy entities;
- damage;
- loot;
- progression;
- combat rewards.

---

## 4. Запрещенные зависимости

`CombatDirector` НЕ должен:

- вызывать `NavMeshBuilder`;
- создавать `NavMeshData`;
- напрямую строить `NavMesh`;
- каждый tick вызывать `NavMesh.CalculatePath` по всем sources;
- напрямую двигать AI;
- знать детали chunk streaming;
- создавать GameObject View;
- зависеть от MonoBehaviour `NavMeshSurface`.

`AiNavigation` НЕ должен:

- принимать решение о фазе боя;
- менять `ThreatBudget`;
- выбирать драматургию волны;
- создавать enemy entities;
- решать, когда `Peak`, `Relief`, `Cooldown`;
- начислять loot/progression.

---

## 5. Общие данные

`CombatDirector` создает или обновляет:

```csharp
public struct CombatCell : IComponent
{
    public int CellId;
    public float3 Center;
    public float Radius;
}
```

`AiNavigation` читает `CombatCell` и создает:

```csharp
public struct CombatCellNavArea : IComponent
{
    public int CellId;
    public float3 Center;
    public float Radius;
    public float NavBuildRadius;
    public float SourceCollectRadius;
    public int Priority;
}
```

`CombatDirector` owns:

```csharp
public enum SpawnSourceType : byte
{
    Burrow = 1,
    Rift = 2
}

public struct SpawnSource : IComponent
{
    public SpawnSourceType Type;
    public float3 Position;
    public float Radius;
    public bool IsActive;
}
```

`AiNavigation` adds runtime navigation state:

```csharp
public enum SpawnSourceNavStatus : byte
{
    Unknown = 0,
    WaitingForNavMesh = 1,
    Reachable = 2,
    Unreachable = 3,
    TemporarilyBlocked = 4
}

public struct SpawnSourceNavState : IComponent
{
    public SpawnSourceNavStatus Status;
    public float LastCheckedTime;
    public float NextCheckTime;
    public float ApproxPathCost;
    public int NavVersion;
}
```

---

## 6. Правило выбора spawn source

`CombatDirector` может выбирать только source, который:

- `SpawnSource.IsActive == true`;
- `SpawnSourceNavState.Status == Reachable`;
- source не находится в safe zone;
- source не ближе `MinSpawnDistance`;
- source не дальше `MaxSpawnDistance`;
- source не превышает `RecentlyUsedPenalty`;
- source принадлежит активной `CombatCell/NavArea`;
- source не заблокирован performance budget-ом.

---

## 7. Director phases и дорогие nav-операции

### Calm

Можно:

- делать `NavMesh` rebuild;
- проверять reachability;
- обновлять candidates.

### BuildUp

Можно:

- доготовить reachable sources;
- запросить prewarm nav area;
- выбрать будущий spawn source;
- показать telegraph.

### Peak

Нельзя:

- запускать массовый rebuild;
- делать массовые path checks;
- пересчитывать все spawn source candidates.

Нужно:

- использовать cached reachable sources;
- поддерживать уже созданных enemies;
- ограничивать path requests.

### Relief

Можно:

- обновлять `NavMesh` после разрушений/строительства;
- пересчитать reachability;
- снять часть pressure.

### Cooldown

Можно:

- подготовить candidates для следующего цикла;
- обновить source penalties;
- выполнить deferred rebuild.

---

## 8. Pipeline

Base Combat Director pipeline:

```text
CombatCellTrackingSystem
→ PlayerThreatInputSystem
→ ThreatBudgetAccumulationSystem
→ DirectorPhaseSystem
→ SpawnSourceSelectionSystem
→ SpawnRequestBuildSystem
→ SpawnRequestValidationSystem
→ EnemySpawnApplySystem
→ EnemyCombatReplicationSystem
```

Pipeline with AiNavigation:

```text
CombatCellTrackingSystem
→ PlayerThreatInputSystem
→ ThreatBudgetAccumulationSystem

→ CombatCellNavAreaSyncSystem
→ RuntimeNavMeshRebuildQueueSystem
→ RuntimeNavMeshBuildSystem
→ SpawnSourceReachabilitySystem

→ DirectorPhaseSystem
→ SpawnSourceScoringSystem
→ SpawnRequestBuildSystem
→ SpawnRequestValidationSystem
→ EnemySpawnApplySystem

→ EnemyAiNavigationModeSystem
→ EnemyCombatReplicationSystem
```

---

## 9. Spawn request contract

`CombatDirector` creates:

```csharp
public struct SpawnRequest : IComponent
{
    public int CellId;
    public EnemyRole Role;
    public int Count;
    public float3 SpawnPosition;
}
```

`AiNavigation` may provide resolved spawn point:

```csharp
public struct ResolvedSpawnPoint : IComponent
{
    public int SourceEntityId;
    public float3 Position;
    public int ZoneId;
    public int NavVersion;
}
```

Rule:

```text
SpawnRequestBuildSystem does not search a point on NavMesh directly.
It uses cached SpawnSourceNavState or prepared ResolvedSpawnPoint.
```

---

## 10. Performance budget contract

`ThreatBudget` is dramaturgy.

`PerformanceBudget` is technical limit.

```csharp
public struct CombatCellPerformanceBudget : IComponent
{
    public int MaxAliveEnemies;
    public int MaxNewSpawnsPerPeak;
    public int MaxPathRequestsPerTick;
    public int MaxReachabilityChecksPerTick;
    public int MaxAiDecisionsPerTick;
}
```

Director flow:

```text
ThreatBudget → desired pressure
PerformanceBudget → allowed pressure
FinalWave → compressed wave
```

Example:

```text
Threat wants:
- 20 swarmers
- 2 markers
- 1 anchor

Performance allows:
- 8 new enemies
- 1 anchor
- 1 marker

Final:
- 6 swarmers
- 1 marker
- 1 anchor
- stronger telegraph
- delayed trickle spawn
```

High threat should not always mean more entities.

High threat can mean:

- stronger telegraph;
- marker zone;
- anchor aura;
- delayed second wave;
- rift pulse;
- objective pressure;
- better enemy composition.

---

## 11. Far AI contract

If enemy/NPC is far from player:

- `CombatDirector` does not need to keep it in active combat simulation;
- `AiNavigation` may switch it to `ApproximateMove`;
- AI updates rarely;
- position remains server-authoritative logical position;
- client View may not exist;
- when player enters chunk, AI attaches to `LocalNavMesh`.

```csharp
public struct AiFarSimulationState : IComponent
{
    public float3 LogicalPosition;
    public float3 TargetPosition;
    public float NextSimulationTime;
    public int GlobalRouteNodeId;
}
```

---

## 12. Base NPC contract

Base NPC navigation is not owned by Combat Director.

Base systems may request:

- route to workstation;
- route to storage;
- route to bed;
- route to defense point;
- reachability check;
- local nav attach.

But base systems must not:

- build NavMesh directly;
- run unbounded path queries;
- depend on Combat Director;
- create gameplay state in MonoBehaviour.

---

## 13. Networking

Server authoritative:

- `CombatCell`;
- `ThreatBudget`;
- `DirectorState`;
- `SpawnRequest`;
- `SpawnSourceNavState`, if needed for debug;
- enemy creation;
- AI logical position;
- AI navigation mode;
- health/damage/combat state.

Client-only:

- View;
- VFX;
- spawn warning;
- danger meter;
- nav debug gizmos;
- local visual smoothing.

---

## 14. Debug and tooling

Required debug overlay:

- active `CombatCell`;
- active `CombatCellNavArea`;
- active `BaseNavArea`;
- `RuntimeNavMeshZone` state;
- nav build state;
- nav version;
- reachable/unreachable spawn sources;
- selected spawn source;
- current director phase;
- threat budget;
- performance budget;
- alive enemies;
- path requests this tick;
- reachability checks this tick.

---

## 15. Acceptance criteria

- `CombatDirector` does not depend on `UnityEngine.AI`.
- `CombatDirector` does not call `NavMeshBuilder`.
- `AiNavigation` does not change `DirectorPhase`.
- `AiNavigation` does not change `ThreatBudget`.
- `SpawnSourceSelection` uses only cached reachable sources.
- `Peak` does not start mass `NavMesh` rebuild.
- Base NPC navigation does not depend on `CombatDirector`.
- Far AI can move without loaded chunk.
- When chunk loads, AI switches from `ApproximateMove` to `LocalNavMesh`.
- Server remains authoritative.
- Client creates only View/VFX/UI/debug visuals.
