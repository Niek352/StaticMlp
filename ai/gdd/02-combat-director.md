# Codex Research — Scope 02 — Combat Director & Enemy Spawn

> Назначение документа: research/implementation plan для Codex.
> Проект: co-op open world survival-builder на ECS.
> Важное ограничение: все runtime features должны проектироваться через ECS components/systems/resources/events, а не через MonoBehaviour-first архитектуру.
> Сетевой принцип: server-authoritative для боевых/экономических изменений; client-only только для View/VFX/UI/предпросмотра.


## 1. Цель scope

Реализовать co-op encounter director для open world, который управляет давлением боя через combat cells, threat budget и честные spawn sources.

MVP scope:

- 1 combat cell вокруг группы игроков.
- Threat budget.
- 3 archetype врагов: Swarmer, Marker, Anchor Elite.
- 2 spawn source: Burrow и Rift.
- Ритм: pressure peak → relief → pressure peak.
- ECS-first implementation.
- Server-authoritative spawn и combat state.

## 2. Gameplay rules

### 2.1 Combat cell

Combat cell — runtime зона вокруг player group или активной цели.

Cell хранит:

- center.
- radius.
- active players.
- local noise.
- loot/value carried by players.
- proximity to enemy placements.
- current threat budget.
- current phase.

### 2.2 Director phases

```csharp
public enum DirectorPhase : byte
{
    Calm = 0,
    BuildUp = 1,
    Peak = 2,
    Relief = 3,
    Cooldown = 4
}
```

Правила:

- Calm: минимальные фоновые угрозы.
- BuildUp: director копит budget и выбирает spawn source.
- Peak: spawn pressure pack.
- Relief: временное снижение угрозы, игроки могут собирать ресурсы/лут.
- Cooldown: запрет немедленного повторного пика.

### 2.3 Enemy roles

1. **Swarmer** — много слабых врагов, низкий budget cost.
2. **Marker** — телеграфы/опасные зоны, средний budget cost.
3. **Anchor Elite** — держит фазу, баффает пачку, высокий budget cost.

## 3. ECS data model

```csharp
public struct CombatCell : IComponent
{
    public int CellId;
    public float3 Center;
    public float Radius;
}

public struct ThreatBudget : IComponent
{
    public float Current;
    public float Max;
    public float AccumulationPerSecond;
}

public struct DirectorState : IComponent
{
    public DirectorPhase Phase;
    public float PhaseTimer;
    public float TimeSinceLastPeak;
}

public struct PlayerNoise : IComponent
{
    public float Value;
}

public struct CarriedLootValue : IComponent
{
    public float Value;
}

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

public enum EnemyRole : byte
{
    Swarmer = 1,
    Marker = 2,
    AnchorElite = 3
}

public struct EnemyArchetype : IComponent
{
    public EnemyRole Role;
}

public struct SpawnRequest : IComponent
{
    public int CellId;
    public EnemyRole Role;
    public int Count;
    public float3 SpawnPosition;
}
```

## 4. Resources

```csharp
public sealed class EncounterDirectorConfig
{
    public float CellRadius;
    public float BaseThreatPerSecond;
    public float NoiseThreatMultiplier;
    public float LootThreatMultiplier;
    public float MinReliefSeconds;
    public float MinCooldownSeconds;
    public int MaxAliveEnemiesPerCell;
}

public sealed class EnemySpawnCatalog
{
    public EnemySpawnDefinition Get(EnemyRole role);
}
```

## 5. Systems pipeline

### Server systems

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

### Client systems

```text
DirectorTelegraphReceiveSystem
→ SpawnSourceVfxSystem
→ EnemyViewBindSystem
→ EnemySpawnAudioSystem
```

## 6. System details

### CombatCellTrackingSystem

- Если игрок один — cell center = player position.
- Если игроков несколько рядом — center = average position.
- Для vertical slice начать с одной cell.

### PlayerThreatInputSystem

Считает входы угрозы:

- шум от атак.
- mining/harvesting actions.
- carried loot value.
- нахождение рядом с enemy placements.
- время в cell.

Не спавнит врагов напрямую.

### ThreatBudgetAccumulationSystem

```text
threatDelta =
    baseThreatPerSecond
  + noise * noiseMultiplier
  + carriedLootValue * lootMultiplier
  + activeObjectiveBonus
```

Budget clamp до Max.

### DirectorPhaseSystem

```text
Calm -> BuildUp, если threat > buildUpThreshold
BuildUp -> Peak, если threat > peakThreshold и есть valid spawn source
Peak -> Relief, если wave spawned или alive enemies below threshold after peak
Relief -> Cooldown, если relief timer ended
Cooldown -> Calm, если cooldown timer ended
```

### SpawnSourceSelectionSystem

Выбирает источник, который:

- вне прямой видимости игрока, если есть visibility service.
- достаточно близко, чтобы враги быстро пришли.
- не внутри базы/запрещенной safe zone.
- активен в текущем biome/cell.

MVP без дорогого visibility:

- выбрать source в диапазоне `minDistance <= distance <= maxDistance`.
- prefer behind/side relative to player forward.
- не спавнить ближе X метров.

### SpawnRequestBuildSystem

MVP composition:

- low budget: 6–10 swarmers.
- medium budget: 8–14 swarmers + 1 marker.
- high budget: 10–18 swarmers + 1 marker + 1 anchor.

### EnemySpawnApplySystem

Создает authoritative ECS enemy entities:

- Не создавать View на server.
- Не использовать GameObject как gameplay state.
- Присвоить NetworkIdentity/GID, если проект использует cross-client id.
- Добавить Health/CombatState/AI state.
- Добавить role tag.

## 7. Static placements → dynamic actors

1. Client может видеть static proxy: нора/разлом.
2. Server имеет placement fact.
3. Director выбирает source.
4. Server promotes wave into dynamic ECS actors.
5. Clients получают replicated spawn/enemy state.
6. Client creates views.

## 8. Networking

Server authoritative:

- Budget.
- Phase.
- Spawn request.
- Enemy creation.
- Health.
- Damage.
- AI combat state.

Client-only:

- spawn warning VFX.
- sound stingers.
- dust/portal animation.
- UI danger meter.

## 9. Tests

- Threat budget accumulates correctly.
- Phase transitions are deterministic.
- Budget composition creates expected role counts.
- Spawn cap prevents over-spawn.
- Invalid catalog id fails.
- Player noise triggers BuildUp.
- Peak creates enemies.
- Relief prevents immediate second wave.
- Server creates gameplay entity, client creates View only.

## 10. Implementation order for Codex

1. Найти существующую ECS/world/server/client separation.
2. Добавить configs/catalogs.
3. Добавить components.
4. Реализовать combat cell tracking.
5. Реализовать budget accumulation.
6. Реализовать phase machine.
7. Реализовать spawn source selection.
8. Реализовать spawn requests.
9. Реализовать apply spawn server-side.
10. Добавить минимальный client telegraph/view binding.
11. Добавить tests.
12. Добавить debug overlay: cell, budget, phase, alive enemies.

## 11. Acceptance criteria

- При фарме/шуме threat растет.
- Director создает волну только через Burrow/Rift.
- Есть фаза передышки.
- Враги не создаются бесконечно без cap.
- Все боевые сущности authoritative на server.
- Client View создается только на client.
- Можно расширить EnemyRole без переписывания director.
