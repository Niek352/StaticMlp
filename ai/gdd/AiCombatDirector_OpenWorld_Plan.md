# AiCombatDirector OpenWorld Plan

## Цель

Переделать `AiCombatDirector` из wave/orde-first режиссера в **OpenWorld Encounter Director**, более подходящий для игры в духе **Bellwright / Palworld**:

- игрок спокойно путешествует по миру;
- в обычном исследовании он чаще встречает одиночных AI или маленькие группы;
- большой наплыв врагов не появляется без причины;
- Director не должен превращать open world в постоянную арену;
- давление боя должно возникать из мира: лагеря, логова, патрули, тревога, охота, рейды, события, шум, ценная добыча;
- спавн должен быть server-authoritative и ECS-first;
- client-only остаются View/VFX/UI/предупреждения;
- `AiNavigation` отвечает за достижимость и маршруты, но не принимает дизайнерские решения о составе угрозы.

## Почему текущий CombatDirector нужно изменить

Текущий `ai/gdd/02-combat-director.md` описывает правильный технический фундамент:

- `CombatCell`;
- `ThreatBudget`;
- `DirectorPhase`;
- `SpawnSource`;
- `EnemyRole`;
- `SpawnRequest`;
- server systems pipeline;
- client telegraph pipeline;
- static placements -> dynamic actors.

Но текущий MVP в документе формулируется вокруг:

```text
pressure peak -> relief -> pressure peak
low budget: 6-10 swarmers
medium budget: 8-14 swarmers + 1 marker
high budget: 10-18 swarmers + 1 marker + 1 anchor
```

Для survival-builder open world это слишком агрессивная базовая модель. Такой подход больше подходит для:

- arena survival;
- extraction event;
- dungeon/combat room;
- base raid;
- boss phase;
- Left 4 Dead-like pressure pacing.

Для Bellwright/Palworld-like open world базовая модель должна быть другой:

```text
ambient exploration -> isolated encounter -> optional escalation -> local alarm -> pursuit/retreat -> calm recovery
```

То есть **волна — это не базовый режим**, а результат конкретной причины.

## Design lock

### Главный принцип

`AiCombatDirector` не должен “создавать бой ради боя”.  
Он должен **управлять уже существующим напряжением мира**.

Правильная логика:

```text
World placement / faction / biome / player action / noise / objective
    -> creates local attention
    -> local attention may become encounter
    -> encounter may escalate
    -> only escalated encounter may spawn pressure packs
```

Неправильная логика:

```text
player exists in cell
    -> budget grows forever
    -> wave spawns
    -> relief
    -> next wave
```

### Что игрок должен чувствовать

В обычном open world:

- “Я могу идти, смотреть мир, собирать ресурсы.”
- “Иногда встречаю одиночного волка / бандита / зверя / патруль.”
- “Если я шумлю, лезу в лагерь, таскаю ценный груз или вскрываю логово — мир реагирует.”
- “Большой бой был вызван моим действием, а не потому что система решила наспавнить волну.”

### Где уместны большие волны

Большие волны оставить только для специальных случаев:

- base raid;
- faction alarm;
- monster lair activation;
- dungeon/ruin event;
- caravan ambush;
- boss phase;
- extraction/resource overharvest event;
- night/horde biome;
- scripted world event.

## Новая модель Director

### 1. Ambient Ecology Layer

Это базовый слой open world.  
Он не должен быть “боевым режиссером волн”.

Задачи:

- поддерживать ощущение живого мира;
- активировать одиночных AI;
- включать малые группы;
- не перегружать игрока постоянным боем;
- уважать safe/travel pacing.

Примеры:

- один хищник у водопоя;
- один дикий Pal-like creature у ресурса;
- пара бандитов у дороги;
- 2-3 волка возле трупа;
- рабочий NPC в лагере;
- одиночный разведчик фракции.

Важно: такие сущности лучше брать из deterministic placements / world spawn facts, а не “создавать из воздуха”.

### 2. Encounter Layer

Это слой локального столкновения.  
Он включается, когда игрок реально вошел в контакт.

Примеры причин:

- игрок подошел к агрессивному AI;
- игрок атаковал NPC;
- игрок вошел в территорию лагеря;
- игрок начал добывать охраняемый ресурс;
- игрок несет ценный груз;
- игрок прошел рядом с логовом;
- AI услышал шум.

Encounter может остаться маленьким:

```text
1 boar
1 bandit
2 wolves
1 scout + 1 dog
```

И это нормальный результат. Не каждый encounter должен эскалировать.

### 3. Escalation Layer

Это слой “мир начал отвечать”.  
Он включается только при накоплении причин.

Причины:

- длительный бой;
- убийство охранника рядом с лагерем;
- шумные атаки;
- добыча редкого ресурса;
- игрок слишком долго находится в запретной зоне;
- игрок несет high-value loot;
- игрок разрушает faction property;
- AI успел подать тревогу.

Escalation может дать:

- прибытие 1-2 подкреплений;
- локальный патруль;
- охотника, который идет по следу;
- сигнал тревоги лагеря;
- временную блокировку быстрого выхода;
- рост faction attention.

Но даже escalation не обязана превращаться в орду.

### 4. Pressure Event Layer

Это уже старый wave-like CombatDirector, но теперь он не default.

Pressure event разрешен только когда:

- encounter escalated;
- есть valid spawn source;
- есть navigation reachability;
- игрок не в safe exploration state;
- alive cap не превышен;
- cooldown прошел;
- событие имеет понятную причину.

Примеры:

- “из логова вылезли еще 4 зверя”;
- “из лагеря прибежал patrol squad”;
- “rift открылся после добычи cursed ore”;
- “faction sent hunters after player”.

## Новые состояния Director

Старый `DirectorPhase` можно заменить или расширить.

```csharp
public enum OpenWorldDirectorPhase : byte
{
    Dormant = 0,        // Director ничего не делает, только наблюдает.
    Ambient = 1,        // Разрешены одиночные AI и малые встречи.
    Contact = 2,        // Игрок вошел в локальный encounter.
    Suspicion = 3,      // AI/лагерь слышит шум, но еще нет большой тревоги.
    Escalation = 4,     // Мир начал отвечать: подкрепление, охота, тревога.
    PressureEvent = 5,  // Ограниченное событие давления.
    Recovery = 6,       // После боя мир успокаивается.
    Cooldown = 7        // Запрет немедленного повторного давления.
}
```

### Правила переходов

```text
Dormant -> Ambient
    если игрок вошел в loaded/openworld region.

Ambient -> Contact
    если игрок вошел в aggro/contact range AI или в hostile territory.

Contact -> Suspicion
    если есть шум, смерть NPC, добыча guarded resource, trespass.

Suspicion -> Escalation
    если накоплен attention score или AI успел вызвать тревогу.

Escalation -> PressureEvent
    если есть valid reason + valid spawn source + budget + nav reachability.

PressureEvent -> Recovery
    если событие выполнено, враги убиты/отступили или игрок ушел.

Recovery -> Cooldown
    когда local danger снизился.

Cooldown -> Ambient
    после cooldown, если нет активных причин.
```

Главное: `Ambient` не должен автоматически вести в `PressureEvent`.

## Новые данные ECS

### OpenWorldCombatCell

`CombatCell` из текущего документа оставить, но расширить смысл.  
Cell не должна быть только ареной. Это runtime-зона внимания вокруг игрока/группы.

```csharp
public struct OpenWorldCombatCell : IComponent
{
    public int CellId;
    public float3 Center;
    public float Radius;
    public int ActivePlayerCount;
    public OpenWorldDirectorPhase Phase;
}
```

### CellAttention

Заменяет идею “threat grows forever”.

```csharp
public struct CellAttention : IComponent
{
    public float Current;
    public float Max;
    public float DecayPerSecond;
    public float Noise;
    public float Trespass;
    public float Combat;
    public float Loot;
    public float FactionAlarm;
}
```

Смысл:

- attention растет только от действий/причин;
- attention затухает со временем;
- passive exploration почти не растит attention;
- мир не “злится” просто потому что игрок гуляет.

### EncounterState

```csharp
public struct EncounterState : IComponent
{
    public int CellId;
    public int EncounterId;
    public EncounterKind Kind;
    public EncounterIntensity Intensity;
    public float TimeAlive;
    public int AliveEnemyCount;
    public bool EscalationAllowed;
}
```

```csharp
public enum EncounterKind : byte
{
    None = 0,
    AmbientSolo = 1,
    AmbientSmallPack = 2,
    CampContact = 3,
    LairContact = 4,
    PatrolContact = 5,
    ResourceGuard = 6,
    CaravanAmbush = 7,
    BaseRaid = 8,
    BossEvent = 9
}
```

```csharp
public enum EncounterIntensity : byte
{
    Passive = 0,
    Minor = 1,
    Moderate = 2,
    Dangerous = 3,
    Raid = 4,
    Boss = 5
}
```

### SpawnSource classification

Текущий `SpawnSource` расширить:

```csharp
public enum SpawnSourceKind : byte
{
    AmbientPoint = 1,       // одиночный AI / зверь / ресурсная точка
    PatrolRoute = 2,        // патруль, который приходит из мира
    CampGate = 3,           // лагерь / поселение врагов
    LairEntrance = 4,       // логово зверей
    Rift = 5,               // магический/аномальный источник
    RoadAmbush = 6,         // засада у дороги
    BaseRaidEntry = 7       // вход для рейда на базу
}
```

```csharp
public struct OpenWorldSpawnSource : IComponent
{
    public SpawnSourceKind Kind;
    public float3 Position;
    public float Radius;
    public int FactionId;
    public int BiomeId;
    public bool IsActive;
    public bool AllowsAmbient;
    public bool AllowsEscalation;
    public bool AllowsPressureEvent;
}
```

### AmbientSpawnMarker

Для одиночных AI и малых групп.

```csharp
public struct AmbientSpawnMarker : IComponent
{
    public int BiomeId;
    public int FactionId;
    public AmbientSpawnKind Kind;
    public float RespawnCooldown;
    public float TimeSinceDespawn;
}
```

```csharp
public enum AmbientSpawnKind : byte
{
    SoloAnimal = 1,
    SoloBandit = 2,
    SmallAnimalPack = 3,
    SmallFactionPatrol = 4,
    ResourceGuardian = 5,
    NeutralCreature = 6
}
```

### EscalationReason

Чтобы подкрепления появлялись не “по budget”, а по причине.

```csharp
public struct EscalationReason : IComponent
{
    public int CellId;
    public EscalationReasonKind Kind;
    public float Strength;
    public float TimeToLive;
}
```

```csharp
public enum EscalationReasonKind : byte
{
    Noise = 1,
    GuardKilled = 2,
    CampAlarm = 3,
    LairProvoked = 4,
    RareResourceHarvested = 5,
    HighValueLootCarried = 6,
    FactionHunt = 7,
    BaseRaidScheduled = 8
}
```

### Alive caps

Для open world критично ограничить число активных врагов.

```csharp
public struct CellAliveEnemyCaps : IComponent
{
    public int MaxAmbientEnemies;
    public int MaxEncounterEnemies;
    public int MaxEscalationEnemies;
    public int MaxPressureEventEnemies;
}
```

Пример для раннего open world:

```text
MaxAmbientEnemies = 1-4
MaxEncounterEnemies = 1-6
MaxEscalationEnemies = 2-8
MaxPressureEventEnemies = 6-16
```

Волны 10-18 swarmers должны быть не default, а high intensity event.

## Конфиг

```csharp
public sealed class OpenWorldCombatDirectorConfig
{
    public float CellRadius;
    public float AmbientScanInterval;
    public float AttentionDecayPerSecond;

    public float NoiseAttentionMultiplier;
    public float TrespassAttentionMultiplier;
    public float CombatAttentionMultiplier;
    public float LootAttentionMultiplier;
    public float FactionAlarmMultiplier;

    public float SuspicionThreshold;
    public float EscalationThreshold;
    public float PressureEventThreshold;

    public float MinTimeBetweenAmbientEncounters;
    public float MinTimeBetweenEscalations;
    public float MinTimeBetweenPressureEvents;

    public int MaxAmbientEnemiesPerCell;
    public int MaxEncounterEnemiesPerCell;
    public int MaxEscalationEnemiesPerCell;
    public int MaxPressureEventEnemiesPerCell;

    public bool AllowRandomAmbientSpawns;
    public bool AllowPressureEventsDuringExploration;
}
```

Рекомендуемые defaults:

```text
AllowRandomAmbientSpawns = true
AllowPressureEventsDuringExploration = false

Ambient: одиночные AI и small packs
PressureEvent: только от explicit reason
Attention decay: достаточно высокий, чтобы прогулка быстро возвращалась в calm
```

## Новый systems pipeline

### Server pipeline

```text
OpenWorldCombatCellTrackingSystem
→ AmbientWorldInterestScanSystem
→ PlayerAttentionInputSystem
→ CellAttentionDecaySystem
→ EncounterContactDetectionSystem
→ OpenWorldDirectorPhaseSystem
→ AmbientEncounterSpawnRequestSystem
→ EscalationReasonBuildSystem
→ EscalationSpawnRequestSystem
→ PressureEventEligibilitySystem
→ PressureEventSpawnRequestSystem
→ SpawnRequestReachabilitySystem
→ SpawnRequestValidationSystem
→ EnemySpawnApplySystem
→ EncounterLifetimeSystem
→ EncounterRecoverySystem
```

### Client pipeline

```text
EncounterTelegraphReceiveSystem
→ SpawnSourceVfxSystem
→ EnemyViewBindSystem
→ AmbientCreatureViewBindSystem
→ EnemySpawnAudioSystem
→ DangerUiSystem
```

## System details

### OpenWorldCombatCellTrackingSystem

Назначение:

- создать runtime cell вокруг игрока или группы игроков;
- если co-op игроки рядом — использовать один общий cell;
- если игроки далеко — разрешить несколько independent cells;
- не делать director глобальным на весь мир.

Правила:

```text
1 player:
    center = player position

2-4 players close:
    center = average position

players split far:
    create separate cells, but use stricter spawn caps
```

Важно для co-op:

- если игроки разделились, director не должен устраивать каждому full horde;
- pressure budget должен масштабироваться мягко;
- far cell может быть в Ambient/Contact, пока основной cell в PressureEvent.

### AmbientWorldInterestScanSystem

Назначение:

- найти nearby ambient placements;
- выбрать одиночных AI / малые группы;
- активировать их как dynamic actors только когда игрок близко.

Правило:

```text
static placement fact -> server promotes to dynamic actor when player approaches
```

Примеры:

- одиночный кабан;
- один bandit scout;
- 2 волка;
- neutral creature;
- resource guardian.

Не делать:

- не создавать волну;
- не поднимать global threat;
- не спавнить за спиной без причины.

### PlayerAttentionInputSystem

Собирает причины внимания:

```text
Noise:
    attacks, explosions, mining, tree cutting, building destruction

Trespass:
    hostile camp territory, sacred grove, lair radius

Combat:
    enemy damaged, guard killed, elite damaged

Loot:
    high-value resources, stolen faction goods, boss parts

FactionAlarm:
    alarm bell, survivor witness, scout escaped
```

Пассивное путешествие:

```text
attention += 0 или очень мало
```

Это ключевое отличие от старого `ThreatBudgetAccumulationSystem`.

### CellAttentionDecaySystem

Назначение:

- снижать attention, если игрок ушел/затих;
- позволить миру успокоиться;
- не хранить вечную обиду cell без системы faction memory.

Правило:

```text
Current -= DecayPerSecond * dt
Current = max(0, Current)
```

Faction memory можно делать отдельно позже, но не смешивать с moment-to-moment director.

### EncounterContactDetectionSystem

Создает `EncounterState`, когда есть настоящий контакт:

- player entered aggro range;
- player attacked;
- enemy spotted player;
- player entered hostile territory;
- player harvested guarded resource.

Encounter starts with intensity:

```text
AmbientSolo -> Minor
AmbientSmallPack -> Minor/Moderate
CampContact -> Moderate
LairContact -> Moderate
BossEvent -> Boss
```

### OpenWorldDirectorPhaseSystem

Новая phase machine должна быть conservative:

```text
Ambient stays Ambient unless there is contact/reason.
Contact does not auto-escalate.
Suspicion can decay back to Ambient.
Escalation requires reason.
PressureEvent requires explicit eligibility.
```

Пример:

```text
if phase == Ambient and contactDetected:
    phase = Contact

if phase == Contact and attention > SuspicionThreshold:
    phase = Suspicion

if phase == Suspicion and attention decays:
    phase = Recovery

if phase == Suspicion and escalationReason exists:
    phase = Escalation

if phase == Escalation and PressureEventEligibility == true:
    phase = PressureEvent

if phase == PressureEvent and event done:
    phase = Recovery

if phase == Recovery and timer ended:
    phase = Cooldown

if phase == Cooldown and timer ended:
    phase = Ambient
```

### AmbientEncounterSpawnRequestSystem

Создает только small-scale requests.

Примеры:

```text
SoloAnimal: count 1
SoloBandit: count 1
SmallAnimalPack: count 2-3
SmallFactionPatrol: count 2-4
ResourceGuardian: count 1-2
```

Запрещено:

- swarmers 10+;
- anchor elite без события;
- спавн рядом с игроком без source;
- постоянный respawn после убийства.

### EscalationReasonBuildSystem

Создает явные причины эскалации.

Примеры:

```text
GuardKilled:
    if guard died within camp territory

CampAlarm:
    if alarm entity activated or witness reached alarm point

LairProvoked:
    if player damaged lair guardian or stole egg/resource

RareResourceHarvested:
    if rare node harvested above threshold

HighValueLootCarried:
    if player carries valuable loot through dangerous region

FactionHunt:
    if faction memory says player is wanted
```

### EscalationSpawnRequestSystem

Создает небольшое подкрепление.

Примеры:

```text
CampAlarm:
    2-4 guards from CampGate / PatrolRoute

LairProvoked:
    2-5 creatures from LairEntrance

Noise:
    1-3 predators/investigators

FactionHunt:
    1 hunter + 1 dog/scout
```

### PressureEventEligibilitySystem

Разрешает wave-like director только если все условия выполнены:

```text
has explicit escalation reason
has valid source
source allows pressure events
cell is not safe zone
player is not in pure exploration mode
cooldown ended
alive cap allows it
AiNavigation says source can reach target area
```

Если условия не выполнены:

```text
PressureEvent is skipped.
Director may stay in Escalation or go Recovery.
```

### PressureEventSpawnRequestSystem

Использует старую логику волн, но только для специальных событий.

Примеры состава:

```text
LairPressureEvent:
    4-8 small creatures + optional 1 alpha

CampAlarmPressureEvent:
    3-6 guards + 1 marker/captain

RiftPressureEvent:
    6-10 swarmers + 1 marker

BaseRaid:
    separate raid config, not generic ambient config
```

### SpawnRequestReachabilitySystem

Должен использовать `AiNavigation`, но не владеть навигацией.

Контракт:

```text
CombatDirector asks:
    CanSpawnSourceReachTarget(source, targetArea, agentKind)?

AiNavigation answers:
    Reachable / NotReachable / Unknown / NeedsRebuild
```

Если `NotReachable`:

- не спавнить;
- выбрать другой source;
- либо поставить delayed request после rebuild.

Если `Unknown`:

- для ambient можно разрешить только near spawn;
- для pressure event лучше пропустить или телеграфировать delayed spawn.

### EnemySpawnApplySystem

Оставить server-authoritative:

- создает gameplay ECS entity на server;
- не создает View на server;
- присваивает GID/NetworkIdentity;
- добавляет `ServerOwned`;
- добавляет `CharacterNetState`;
- добавляет combat/AI компоненты;
- добавляет role tag;
- отправляет replicated state клиентам.

### EncounterLifetimeSystem

Следит:

- сколько врагов живо;
- сколько длится encounter;
- ушел ли игрок;
- есть ли путь преследования;
- нужно ли врагам отступить/вернуться;
- нужно ли despawn/deactivate far actors.

### EncounterRecoverySystem

После боя:

- снижает локальный attention;
- включает cooldown;
- запрещает немедленный новый pressure event;
- сохраняет долгосрочные последствия отдельно, если нужна faction memory.

## Разделение ответственности

### AiCombatDirector отвечает за

- cell state;
- attention;
- phase;
- encounter state;
- reason-based escalation;
- spawn request composition;
- alive caps;
- cooldown/recovery;
- debug state.

### AiNavigation отвечает за

- локальную навигацию AI;
- reachability source -> target;
- global routing для непрогруженных чанков;
- far/local handoff;
- NavMesh areas;
- runtime rebuild around combat/base zones.

### AiBots / AiActions отвечают за

- выполнение behavior/action;
- `AiMoveRequest`;
- attack/flee/follow;
- blackboard/task state;
- переходы задач.

### Networking отвечает за

- GID;
- replication;
- ownership;
- server authoritative state;
- client View binding.

## Пример gameplay сценариев

### Сценарий 1: спокойное путешествие

```text
Игрок идет через лес.
Cell phase = Ambient.
Director активирует 1 boar placement.
Игрок может обойти или убить.
Attention почти не растет.
После убийства нет волны.
Cell возвращается в Ambient.
```

Ожидаемый результат:

- игрок не устает от постоянного боя;
- мир ощущается живым;
- одиночные встречи имеют смысл.

### Сценарий 2: добыча охраняемого ресурса

```text
Игрок находит rare ore.
Рядом есть ResourceGuardian.
Игрок начинает добычу.
Attention получает RareResourceHarvested.
Guardian входит в Contact.
Если игрок быстро убил и ушел — Recovery.
Если шумит долго — Suspicion.
Если добыл слишком много — Escalation.
Из nearby LairEntrance приходят 2-3 существа.
```

Ожидаемый результат:

- escalation объяснима;
- игрок понимает причину;
- нет ощущения рандомной орды.

### Сценарий 3: лагерь бандитов

```text
Игрок входит в camp territory.
Phase = Contact.
Если игрок крадется/быстро убивает scout — encounter остается малым.
Если guard добежал до alarm point — EscalationReason: CampAlarm.
Director выбирает CampGate / PatrolRoute.
Приходят 3-4 guards.
Если лагерь большой, может быть PressureEvent.
```

Ожидаемый результат:

- Bellwright-like camp combat;
- тревога важна;
- игрок может контролировать эскалацию.

### Сценарий 4: логово зверей

```text
Игрок подходит к lair.
Ambient показывает 1-2 creature.
Игрок ворует яйцо / атакует nest.
EscalationReason: LairProvoked.
Director вызывает 2-5 creatures.
Если игрок продолжает бой рядом с lair — PressureEvent разрешен.
```

Ожидаемый результат:

- Palworld-like creature ecology;
- логово имеет понятное поведение;
- большая драка появляется от действия игрока.

### Сценарий 5: base raid

```text
Base wealth / faction hostility / story trigger creates BaseRaidScheduled.
Director не использует ambient caps.
BaseRaid uses separate config.
Spawn sources: BaseRaidEntry.
AiNavigation checks route to base attack area.
Raid has telegraph and preparation time.
```

Ожидаемый результат:

- рейды отделены от обычного исследования;
- open world не превращается в бесконечный raid.

## Implementation phases for Codex

### Phase 1 — Audit current CombatDirector design

Цель: понять, что уже реализовано/запланировано и что нужно заменить.

Tasks:

- Найти существующие файлы `CombatDirector`, `SpawnSource`, `ThreatBudget`, `DirectorPhase`, если они уже есть.
- Проверить `ai/gdd/02-combat-director.md`.
- Проверить `.planning/BRIEF.md` по `AiNavigation`.
- Проверить `AiMoveRequest` и `ServerAiNavigationSystem`.
- Зафиксировать, какие части уже есть в коде, а какие только в документах.
- Не писать код, если feature еще не создана. Сначала обновить план.

Deliverable:

- короткий audit note в `ai/gdd` или `.planning`, где указано:
  - existing files;
  - missing files;
  - conflicts with open world direction.

### Phase 2 — Replace ThreatBudget with CellAttention

Цель: убрать автоматическую “злость мира” от самого факта присутствия игрока.

Tasks:

- Добавить `CellAttention`.
- Добавить `CellAttentionDecaySystem`.
- Переделать threat input в reason-based attention input.
- Пассивное путешествие не должно копить dangerous pressure.
- Добавить тесты decay/threshold.

Acceptance:

- если игрок стоит/идет без шума и боя, pressure event не возникает;
- если игрок шумит/дерется/ворует, attention растет;
- attention убывает после прекращения причины.

### Phase 3 — Add EncounterState

Цель: отделить одиночные ambient encounters от pressure events.

Tasks:

- Добавить `EncounterState`.
- Добавить `EncounterKind`.
- Добавить `EncounterIntensity`.
- Добавить `EncounterContactDetectionSystem`.
- Добавить `EncounterLifetimeSystem`.
- Добавить `EncounterRecoverySystem`.

Acceptance:

- одиночный AI может создать encounter;
- encounter может завершиться без волны;
- alive count/cooldown работает.

### Phase 4 — Expand SpawnSource model

Цель: сделать спавн “из мира”, а не из абстрактной точки.

Tasks:

- Расширить `SpawnSource` до `OpenWorldSpawnSource`.
- Добавить `SpawnSourceKind`.
- Разделить flags:
  - `AllowsAmbient`;
  - `AllowsEscalation`;
  - `AllowsPressureEvent`.
- Добавить `AmbientSpawnMarker`.

Acceptance:

- ambient source не может создать pressure event;
- camp/lair/rift имеют разные правила;
- spawn request знает reason/source kind.

### Phase 5 — Add Ambient layer

Цель: спокойный open world с одиночными AI.

Tasks:

- Добавить `AmbientWorldInterestScanSystem`.
- Добавить `AmbientEncounterSpawnRequestSystem`.
- Настроить caps:
  - 1-4 ambient enemies;
  - no swarm by default.
- Добавить cooldown per marker.

Acceptance:

- игрок встречает одиночных/малые группы;
- после убийства одиночного AI не начинается автоматическая волна;
- repeated respawn не происходит сразу.

### Phase 6 — Add EscalationReason

Цель: все подкрепления должны иметь причину.

Tasks:

- Добавить `EscalationReason`.
- Добавить `EscalationReasonBuildSystem`.
- Поддержать причины:
  - Noise;
  - GuardKilled;
  - CampAlarm;
  - LairProvoked;
  - RareResourceHarvested;
  - HighValueLootCarried.
- Добавить TTL и strength.

Acceptance:

- подкрепления появляются только при reason;
- reason истекает;
- debug overlay показывает reason.

### Phase 7 — Gate PressureEvent

Цель: оставить старую wave-логику только для специальных случаев.

Tasks:

- Добавить `PressureEventEligibilitySystem`.
- Запретить pressure events в pure exploration.
- Проверять:
  - explicit reason;
  - source;
  - cooldown;
  - alive cap;
  - reachability;
  - safe zone.
- Существующий `SpawnRequestBuildSystem` использовать только после eligibility.

Acceptance:

- player walking in forest never triggers wave;
- camp alarm may trigger pressure event;
- lair provocation may trigger pressure event;
- base raid can use separate config.

### Phase 8 — AiNavigation contract

Цель: Director не должен знать NavMesh/ProjectDawn/UnityEngine.AI details.

Tasks:

- Добавить query interface в `AiNavigation`:
  - `CanReachSpawnSourceToArea`;
  - `FindBestReachableSpawnSource`;
  - `GetReachabilityStatus`.
- `CombatDirector` вызывает только контракт.
- Если reachability unknown, выбирать conservative fallback.

Acceptance:

- Director не содержит `NavMeshBuilder`;
- Director не мутирует navigation state;
- unreachable source не используется для pressure event.

### Phase 9 — Debug and tuning

Цель: сделать feature настраиваемой и понятной.

Debug overlay должен показывать:

- cell id;
- phase;
- attention current/max;
- attention breakdown;
- encounter kind/intensity;
- escalation reasons;
- chosen spawn source;
- alive enemies;
- caps;
- cooldown timers;
- reachability status.

Acceptance:

- можно быстро понять, почему враги появились;
- можно увидеть, почему pressure event не произошел;
- дизайнер может менять config без чтения кода.

## Recommended folders

Если feature еще не создана:

```text
Assets/Scripts/StaticMlp/Features/AiCombatDirector/
  Runtime/
    Components/
      OpenWorldCombatCell.cs
      CellAttention.cs
      EncounterState.cs
      OpenWorldSpawnSource.cs
      AmbientSpawnMarker.cs
      EscalationReason.cs
      CellAliveEnemyCaps.cs
    Configs/
      OpenWorldCombatDirectorConfig.cs
      EncounterSpawnCatalog.cs
    Systems/
      Server/
        OpenWorldCombatCellTrackingSystem.cs
        AmbientWorldInterestScanSystem.cs
        PlayerAttentionInputSystem.cs
        CellAttentionDecaySystem.cs
        EncounterContactDetectionSystem.cs
        OpenWorldDirectorPhaseSystem.cs
        AmbientEncounterSpawnRequestSystem.cs
        EscalationReasonBuildSystem.cs
        EscalationSpawnRequestSystem.cs
        PressureEventEligibilitySystem.cs
        PressureEventSpawnRequestSystem.cs
        SpawnRequestReachabilitySystem.cs
        SpawnRequestValidationSystem.cs
        EnemySpawnApplySystem.cs
        EncounterLifetimeSystem.cs
        EncounterRecoverySystem.cs
      Client/
        EncounterTelegraphReceiveSystem.cs
        SpawnSourceVfxSystem.cs
        EnemyViewBindSystem.cs
        DangerUiSystem.cs
    Debug/
      AiCombatDirectorDebugOverlay.cs
    Authoring/
      OpenWorldSpawnSourceAuthoring.cs
      AmbientSpawnMarkerAuthoring.cs
  Tests/
    Runtime/
      CellAttentionTests.cs
      DirectorPhaseTests.cs
      AmbientEncounterTests.cs
      EscalationReasonTests.cs
      PressureEventEligibilityTests.cs
```

Если в проекте уже есть другой feature layout — соблюдать существующие правила asmdef/namespace.

## Codex prompt

```text
We need to rework AiCombatDirector toward an OpenWorld Encounter Director suitable for a Bellwright/Palworld-like co-op survival-builder.

Important direction:
- The player should be able to calmly explore the open world.
- Default exploration must not create constant enemy waves.
- Most open world encounters should be solo AI or small groups from world placements.
- Big pressure waves are allowed only for explicit reasons: camp alarm, lair provocation, base raid, rare resource overharvest, high-value loot, boss/ruin/rift event.
- Implement ECS-first, server-authoritative gameplay state.
- Client-only systems are only for View/VFX/UI/telegraph.
- Do not put NavMeshBuilder, NavMeshData, or UnityEngine.AI details into AiCombatDirector.
- AiCombatDirector consumes AiNavigation reachability contracts.
- AiBots/AiActions still express movement through AiMoveRequest; ServerAiNavigationSystem remains the system that applies navigation output to CharacterNetState.

Please first audit the existing repo:
- ai/gdd/02-combat-director.md
- .planning/BRIEF.md
- AiMoveRequest
- ServerAiNavigationSystem
- any existing CombatDirector runtime files

Then implement or plan the following:
1. Replace always-growing ThreatBudget with reason-based CellAttention.
2. Add OpenWorldDirectorPhase with Ambient/Contact/Suspicion/Escalation/PressureEvent/Recovery/Cooldown.
3. Add EncounterState so solo/small encounters can exist without waves.
4. Expand SpawnSource into OpenWorldSpawnSource with source kinds and flags for Ambient/Escalation/PressureEvent.
5. Add AmbientSpawnMarker for solo/small open world AI.
6. Add EscalationReason so reinforcements always have a clear cause.
7. Gate PressureEvent behind explicit reason, cooldown, alive caps, source validity, safe zone check, and AiNavigation reachability.
8. Add debug overlay showing why enemies appeared or why pressure event was blocked.
9. Add tests for attention decay, phase transitions, ambient encounter without wave, escalation reason TTL, pressure event eligibility, and caps.

Do not implement temporary hacks silently.
If a temporary solution is necessary, mark it explicitly as [Obsolete("Temp")].
Architecture quality is more important than code quantity.
```

## Acceptance criteria

Feature считается правильной, если:

- прогулка по лесу не вызывает автоматическую волну;
- одиночный зверь/бот может быть убит без эскалации;
- лагерь может эскалировать бой, если поднята тревога;
- логово может вызвать подкрепление, если игрок его провоцирует;
- pressure event не запускается без explicit reason;
- все gameplay-spawn решения authoritative на server;
- client создает только View/VFX/UI;
- Director не содержит low-level NavMesh code;
- unreachable spawn source не используется для pressure event;
- debug показывает причины, phase, attention, caps и cooldown.

## Anti-goals

Не делать:

- endless random waves during exploration;
- global threat that grows only because player exists;
- spawn enemies directly near player without source/reason;
- use GameObject/MonoBehaviour as gameplay state;
- let CombatDirector own NavMesh building;
- make every encounter escalate;
- solve base raids inside ambient exploration config;
- overfit to swarmers as default enemy type.

## Summary

Новая версия `AiCombatDirector` должна быть не “Left 4 Dead Director в лесу”, а **OpenWorld attention/encounter system**:

```text
calm exploration
    -> small believable encounter
    -> optional suspicion
    -> reason-based escalation
    -> rare pressure event
    -> recovery
```

Такой дизайн лучше подходит для Bellwright/Palworld-like игры: мир остается исследуемым и спокойным по умолчанию, но становится опасным, когда игрок трогает важные точки, шумит, атакует лагерь, провоцирует логово или приносит ценный лут.
