# AiNavigation.md

## 1. Цель фичи

`AiNavigation` — отдельная ECS-first runtime-фича, отвечающая за навигацию AI в процедурном open world на чанках.

Фича не должна быть частью `CombatDirector`, `EnemyAI`, `BaseJobs` или View-системы.

`AiNavigation` отвечает за:

- создание и обновление `NavMesh` для активных `CombatCell`;
- создание и обновление `NavMesh` для базы игрока;
- регистрацию navigation geometry от загруженных чанков;
- глобальное прокладывание маршрутов между непрогруженными чанками;
- дешёвое перемещение AI вне зоны интереса игрока;
- перевод AI между режимами `GlobalRoute`, `ApproximateMove`, `WaitingForLocalNavMesh`, `LocalNavMesh`;
- выдачу сервисов для проверки достижимости spawn sources, целей, base jobs и nav points.

---

## 2. Главный принцип

```text
Chunk — это источник навигационной геометрии.
CombatCell/BaseArea — это nav interest area.
RuntimeNavMeshZone — это точная локальная навигация активной зоны.
GlobalNavGraph — это дешёвый граф для дальнего пути между чанками.
```

Нельзя строить `NavMesh` как независимый остров на каждый chunk и ожидать, что Unity автоматически создаст корректные связи между чанками.

Правильная модель:

```text
Chunk generates geometry
    ↓
ChunkNavSourceRegistry stores NavMeshBuildSource
    ↓
AiNavigation selects interest area
    ↓
RuntimeNavMeshZone collects multiple chunk sources + margin
    ↓
NavMeshBuilder.UpdateNavMeshDataAsync builds local nav
```

---

## 3. Runtime boundaries

### AiNavigation owns

- `RuntimeNavMeshZone`.
- `GlobalNavGraph`.
- `ChunkNavSourceRegistry`.
- `NavMeshRebuildQueue`.
- `SpawnSourceNavState`.
- `BaseNavPoint` reachability.
- `AiNavigationState`.
- `LocalNavMeshAttachRequest`.
- `GlobalRoute`.
- `ApproximateMove`.

### AiNavigation does not own

- Combat Director phases.
- Threat budget.
- Enemy spawn composition.
- Damage/combat state.
- NPC economy decisions.
- Base production jobs.
- Unity View/VFX/UI.
- GameObject gameplay state.

---

## 4. Навигационные уровни

`AiNavigation` работает на трех уровнях.

### 4.1 GlobalNavGraph

Используется для дальнего маршрута между чанками/регионами.

Работает даже если чанки не загружены.

Нужен для:

- дальних NPC;
- companions/specialists, возвращающихся на базу;
- enemy patrols;
- caravan/logistics;
- migration;
- route to objective;
- route to combat cell;
- route from spawn source to player area.

### 4.2 ApproximateMovement

Используется, если AI далеко от игрока и его chunk не загружен.

В этом режиме:

- AI не использует `NavMeshAgent`;
- AI не требует `NavMesh`;
- позиция является server-authoritative logical position;
- движение обновляется редко, например 1 раз в секунду;
- движение идет в направлении следующего global waypoint;
- высота берется приблизительно из `HeightProvider`, chunk metadata или сохраняется как logical height;
- collision/avoidance не симулируются точно.

### 4.3 LocalNavMesh

Используется, когда AI находится в активной зоне:

- рядом с игроком;
- в активной `CombatCell`;
- внутри базы игрока;
- в загруженном chunk, который видит игрок;
- в зоне важного objective.

В этом режиме:

- строится или используется `RuntimeNavMeshZone`;
- позиция AI семплится через `NavMesh.SamplePosition`;
- AI двигается через `NavMeshAgent`, `NavMeshQuery` или собственный ECS movement adapter;
- path updates throttled;
- дорогие path/reachability queries ограничены budget-ом.

---

## 5. Nav interest areas

`AiNavigation` не строит `NavMesh` просто потому, что chunk загружен.

`NavMesh` строится только для зон интереса:

- `CombatCellNavArea`;
- `BaseNavArea`;
- `ObjectiveNavArea`;
- optional `ImportantNpcNavArea`.

---

## 6. CombatCellNavArea

`CombatCell` создается Combat Director. `AiNavigation` читает `CombatCell` и создает nav interest area.

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

Правило:

```text
SourceCollectRadius >= NavBuildRadius >= CombatCell.Radius
```

Пример:

```text
CombatCell radius:       80m
NavBuildRadius:         128m
SourceCollectRadius:    160m
```

Это нужно, чтобы агенты не ломали путь на краях `NavMesh`.

---

## 7. BaseNavArea

База игрока — отдельная permanent/semi-permanent nav interest area.

```csharp
public struct BaseNavArea : IComponent
{
    public int BaseId;
    public float3 Center;
    public float Radius;
    public float NavBuildRadius;
    public float SourceCollectRadius;
    public int Priority;
    public int Version;
}
```

`BaseNavArea` нужна для:

- NPC workers;
- companions;
- specialists;
- station jobs;
- logistics;
- patrols;
- defense;
- rescue/extraction/incubation interactions;
- storage/workstation/bed/gate navigation.

База имеет более высокий приоритет, чем обычные distant chunks, потому что NPC должны стабильно выполнять работу.

---

## 8. RuntimeNavMeshZone

MVP:

- один `RuntimeNavMeshZone` для активной `CombatCell`;
- один `RuntimeNavMeshZone` для базы игрока.

Production:

- пул `RuntimeNavMeshZone`;
- priority-based rebuild queue;
- shared source registry;
- per-agent-type `NavMeshData`, если нужно;
- отдельные zones для важных objectives.

```csharp
public enum RuntimeNavMeshZoneKind : byte
{
    CombatCell = 1,
    PlayerBase = 2,
    Objective = 3,
    ImportantNpc = 4
}

public struct RuntimeNavMeshZone : IComponent
{
    public int ZoneId;
    public RuntimeNavMeshZoneKind Kind;
    public float3 Center;
    public float Radius;
    public int AgentTypeId;
    public int NavVersion;
    public bool IsReady;
    public bool IsBuilding;
}
```

---

## 9. ChunkNavSourceRegistry

Chunk не строит `NavMesh` сам.

Chunk только предоставляет `NavMeshBuildSource`.

```csharp
public struct ChunkNavSourceRef : IComponent
{
    public int SourceId;
    public int ChunkVersion;
}

public struct ChunkNavBounds : IComponent
{
    public int2 Coord;
    public Bounds Bounds;
}

public struct ChunkNavSourceReadyTag : IComponent
{
}
```

Сервис:

```csharp
public interface IChunkNavSourceRegistry
{
    void Register(int2 chunkCoord, NavMeshBuildSource source, Bounds bounds, int version);
    void Unregister(int2 chunkCoord);
    void CollectSources(Bounds navBounds, List<NavMeshBuildSource> results);
    int GetCombinedVersion(Bounds navBounds);
}
```

`CollectSources` должен возвращать sources в стабильном порядке:

1. sort by `chunkCoord.x`;
2. then by `chunkCoord.y/z`;
3. then by source local index.

Это важно для incremental rebuild.

---

## 10. Runtime NavMesh rebuild queue

`NavMesh` rebuild не должен запускаться напрямую из Combat Director, Enemy AI или Base Job.

Все запросы идут через `AiNavigationRebuildQueue`.

```csharp
public enum NavRebuildReason : byte
{
    CombatCellMoved = 1,
    BaseChanged = 2,
    ChunkLoaded = 3,
    ChunkUnloaded = 4,
    StructurePlaced = 5,
    StructureRemoved = 6,
    GateStateChanged = 7,
    ObjectiveActivated = 8
}

public struct NavMeshRebuildRequest : IComponent
{
    public int ZoneId;
    public NavRebuildReason Reason;
    public int Priority;
    public float RequestedAt;
}
```

Правила:

- Peak combat phase не должен запускать дорогой rebuild без крайней необходимости.
- Base rebuild должен debounce-иться, например 0.5–2 секунды после строительства.
- CombatCell rebuild запускается при переходе cell center в новый nav-sector.
- Одновременно активен максимум один build operation на zone.
- Rebuild requests должны объединяться, если несколько причин пришли подряд.

---

## 11. GlobalNavGraph

`GlobalNavGraph` — дешёвый граф мира поверх чанков/регионов.

Он нужен, когда:

- игрок далеко;
- chunk не загружен;
- точный `NavMesh` отсутствует;
- NPC должен продолжать логически двигаться;
- нужно рассчитать дальний маршрут к базе, цели, поселению, rift, resource zone.

```csharp
public struct GlobalNavNode
{
    public int NodeId;
    public int2 ChunkCoord;
    public float3 ApproxCenter;
    public GlobalNavAreaFlags AreaFlags;
    public float Cost;
}

public struct GlobalNavEdge
{
    public int FromNodeId;
    public int ToNodeId;
    public float Cost;
    public GlobalNavEdgeFlags Flags;
}
```

```csharp
[Flags]
public enum GlobalNavAreaFlags : ushort
{
    None = 0,
    Walkable = 1 << 0,
    Water = 1 << 1,
    Mountain = 1 << 2,
    EnemyControlled = 1 << 3,
    PlayerBase = 1 << 4,
    Blocked = 1 << 5,
    Dangerous = 1 << 6,
    Road = 1 << 7
}
```

Глобальный путь:

```csharp
public struct GlobalRoute : IComponent
{
    public int CurrentNodeIndex;
    public int TargetNodeId;
    public float RepathTimer;
}
```

---

## 12. Far AI movement

Если AI далеко от игрока и его chunk не загружен, `AiNavigation` не использует `NavMesh`.

```csharp
public enum AiNavigationMode : byte
{
    None = 0,
    GlobalRoute = 1,
    ApproximateMove = 2,
    WaitingForLocalNavMesh = 3,
    LocalNavMesh = 4,
    Stuck = 5
}

public struct AiNavigationState : IComponent
{
    public AiNavigationMode Mode;
    public float NextTickTime;
    public float3 DesiredPosition;
    public int CurrentGlobalNodeId;
    public int TargetGlobalNodeId;
}
```

Approximate movement tick rate:

```text
Far NPC:             1.0 sec
Important far NPC:   0.25–0.5 sec
Dormant NPC:         2–5 sec
```

Server-authoritative logical movement:

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

## 13. Переход в LocalNavMesh

Когда игрок входит в chunk или зона становится активной:

1. Chunk загружается.
2. Chunk регистрирует nav sources.
3. `AiNavigation` создает/обновляет `RuntimeNavMeshZone`.
4. AI entity получает `WaitingForLocalNavMesh`.
5. После готовности зоны вызывается `SamplePosition`.
6. Если sample успешен — AI переводится в `LocalNavMesh`.
7. Если sample неуспешен — AI остается в `ApproximateMove` или получает `Stuck` fallback.

```csharp
public struct LocalNavMeshAttachRequest : IComponent
{
    public int ZoneId;
    public float3 ApproxPosition;
    public float MaxSampleDistance;
}

public struct LocalNavMeshPosition : IComponent
{
    public float3 Position;
    public int ZoneId;
    public int NavVersion;
}
```

---

## 14. Base NPC navigation

Base-specific features:

- `BaseNavAreaSourceCollectSystem`;
- `BaseNavMeshRebuildSystem`;
- `BaseJobReachabilitySystem`;
- `BaseStationNavPointSystem`;
- `BaseDefenseNavPointSystem`.

```csharp
public struct BaseNavPoint : IComponent
{
    public int BaseId;
    public BaseNavPointKind Kind;
    public float3 Position;
    public bool IsReachable;
}

public enum BaseNavPointKind : byte
{
    Storage = 1,
    Workstation = 2,
    Bed = 3,
    DefensePoint = 4,
    Gate = 5,
    ResourceDropoff = 6,
    Incubator = 7,
    ExtractionStation = 8
}
```

Base NPC navigation rules:

- Workers use `BaseNavArea`.
- Base structures can invalidate `BaseNavArea`.
- Building placement/removal should request rebuild with debounce.
- Gate open/closed state can request partial reachability refresh.
- Station jobs should check `BaseNavPoint.IsReachable`.
- If point is unreachable, job should not silently fail; it should produce explicit unreachable state.

---

## 15. SpawnSource reachability

Combat Director owns `SpawnSource`.

`AiNavigation` owns `SpawnSourceNavState`.

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

Reachability checks must be:

- budgeted;
- batched;
- cached;
- invalidated by `NavVersion`;
- not run unbounded during Peak.

---

## 16. ECS systems

Server systems:

```text
ChunkNavSourceRegisterSystem
ChunkNavSourceUnregisterSystem
NavInterestAreaBuildSystem
RuntimeNavMeshRebuildQueueSystem
RuntimeNavMeshBuildSystem
GlobalNavGraphBuildSystem
GlobalRouteRequestSystem
GlobalRouteFollowSystem
ApproximateAiMoveSystem
LocalNavMeshAttachSystem
LocalNavMeshMoveSystem
BaseNavPointReachabilitySystem
SpawnSourceReachabilitySystem
AiNavigationDebugOverlaySystem
```

Client systems:

```text
ClientNavDebugReceiveSystem
AiNavigationGizmosSystem
```

---

## 17. Performance rules

- No unbounded path queries.
- No NavMesh rebuild inside enemy AI.
- No NavMesh rebuild inside Combat Director.
- No per-frame path recalculation for far AI.
- Far AI uses approximate movement.
- Near AI uses local `NavMesh`.
- Base NPCs use `BaseNavArea`.
- Combat NPCs use `CombatCellNavArea`.
- Reachability checks are cached.
- Rebuild requests are debounced and prioritized.
- Peak phase avoids expensive navigation operations.

---

## 18. Networking

Server authoritative:

- AI logical position;
- AI navigation mode;
- global route state;
- local nav attach;
- base reachability;
- spawn source reachability if needed for debug;
- enemy movement/combat state.

Client-only:

- nav debug gizmos;
- View binding;
- animation;
- VFX;
- UI;
- local visual smoothing.

---

## 19. Acceptance criteria

- Chunk does not build NavMesh directly.
- CombatCell creates nav interest area.
- BaseArea creates nav interest area.
- Runtime NavMesh is built only through AiNavigation.
- Sources are collected from multiple chunks, not from one chunk.
- Sources have stable order.
- Far AI can move through `GlobalNavGraph` without loaded chunk.
- Far AI updates at low frequency.
- When chunk loads, AI can switch from `ApproximateMove` to `LocalNavMesh`.
- Base NPCs have stable navigation inside base.
- Combat Director does not call `NavMeshBuilder`.
- Enemy AI does not start NavMesh rebuild.
- Server remains authoritative.
- Client only creates View/VFX/UI/debug visuals.
