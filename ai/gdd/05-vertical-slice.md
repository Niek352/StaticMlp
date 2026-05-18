# Codex Research — Scope 05 — Biome-Complete Vertical Slice

> Назначение документа: research/implementation plan для Codex.
> Проект: co-op open world survival-builder на ECS.
> Важное ограничение: все runtime features должны проектироваться через ECS components/systems/resources/events, а не через MonoBehaviour-first архитектуру.
> Сетевой принцип: server-authoritative для боевых/экономических изменений; client-only только для View/VFX/UI/предпросмотра.


## 1. Цель scope

Собрать одну законченную игровую петлю, а не весь мир.

Vertical Slice должен доказать, что core loop работает:

```text
spawn → добыча → первый NPC → новый станок → mini-boss → boss → новый biome resource
```

MVP content:

- 1 biome.
- 1 mini-boss.
- 1 boss.
- 2 NPC families.
- 8–12 Build/Equipment items.
- 6–8 buildings.
- Combat Director.
- NPC Economy.
- Server-authoritative ECS gameplay.

## 2. Рекомендуемая тема первого биома

**Forest Frontier / Лесной рубеж**.

Причины:

- Простой readable environment.
- Легко объяснить wood/fiber/food.
- Можно встроить burrow/rift spawn sources.
- Биомный налог не слишком сложный: fuel + resin + repair pressure.
- Хорошо подходит для первого companion family.

## 3. Biome resources

### Raw

- Wood.
- Stone.
- Fiber.
- Meat.
- Wild Resin.

### Flow

- Food.
- Fuel.

### Refined

- Planks.
- Rope.
- Resin Glue.
- Iron Scrap / Simple Ingots.

### Progression

- Forest Sigil.
- Root Heart.
- Mini-boss Resin Core.
- Boss Verdant Core.

### Stability

- Repair Kit.
- Torch Fuel.
- Basic Medicine.

## 4. NPC families

### Companion family — Rootlings

Функции:

- Gatherer: собирает wood/fiber/resin.
- Hauler variant: переносит мелкие грузы.
- Combat support: slow/root small enemies.

Acquisition:

- Extraction from wild unstable root creature.
- Incubation from Root Heart later.

### Specialist family — Stranded Craftsmen

Функции:

- Unlocks Sawmill recipe.
- Unlocks Resin Glue.
- Improves repair efficiency.
- Может быть rescued from bandit/parasite camp.

Acquisition:

- Rescue from camp/cocoon.
- Later recruit chain.

## 5. Buildings MVP

1. Campfire / Hearth.
2. Storage Crate.
3. Workbench.
4. Sawmill — wood → planks, requires rescued specialist unlock.
5. Kiln / Resin Cooker.
6. Companion Stable / Binding Post.
7. Guard Post.
8. Incubator.

## 6. Enemies MVP

1. Swarmer: Thorn Mite.
2. Marker: Spore Lobber.
3. Anchor Elite: Barkbound Brute.
4. Mini-boss: Resin Matriarch.
5. Boss: Heartwood Warden.

## 7. Build/Equipment items MVP

Минимум 8:

1. Brimcore Projector.
2. Halo Compass.
3. Microwave Kiln.
4. Mine Garden.
5. Magic Hat Annex.
6. Boombox Beacon.
7. Resin Shield.
8. Root Snare Totem.

Опционально до 12:

9. Spark Saw.
10. Splinter Nova.
11. Pack Mule Charm.
12. Emergency Repair Drone/Spirit.

## 8. ECS feature integration

Vertical Slice не должен быть отдельной monolithic feature. Он должен собрать уже существующие scopes:

```text
DesignLock
+ CombatDirector
+ NpcEconomy
+ BuildEquipment
+ OpenWorldPlacements
+ NetworkReplication
+ ClientViews
```

## 9. Main loop details

### Step 1 — Spawn

- Player spawns near starter camp.
- Server creates player entity.
- Client creates view.
- Starter base has Hearth + Storage.

### Step 2 — Basic harvesting/combat

- Player attacks resource nodes.
- Harvest creates server-side resource operation.
- Noise increases threat budget.
- Director may spawn small pressure pack from Burrow.

### Step 3 — First NPC

- Player finds unstable Rootling.
- Reduces it to ExtractableState.
- Extraction command creates companion entity/record.
- Rootling can be assigned as Gatherer.

### Step 4 — New station

- Player rescues Craftsman.
- Specialist unlocks Sawmill.
- Player builds Sawmill.
- Processor/worker converts wood → planks.

### Step 5 — Mini-boss

- Player activates Resin Nest.
- Director switches to event mode.
- Resin Matriarch fight.
- Drops Resin Core.
- Unlocks Resin Cooker or Incubator upgrade.

### Step 6 — Boss

- Player crafts Forest Sigil.
- Activates Heartwood Gate.
- Boss fight with waves.
- Drops Verdant Core.
- Unlocks next biome resource.

## 10. Vertical Slice systems checklist

### Must exist

- Player spawn system.
- Resource node damage/harvest system.
- Inventory/storage apply system.
- Combat effect apply system.
- Director budget/spawn system.
- NPC extraction system.
- Rescue/recruit system.
- NPC economy tasks.
- Station processing.
- Build/equipment active slots.
- Boss event state machine.
- Replication.
- Client view binding.

### Should exist

- Debug panel.
- Simple save/load for slice state.
- Basic balancing config.
- Test scene/bootstrap.

## 11. Boss event ECS model

```csharp
public enum BossEventPhase : byte
{
    Locked = 0,
    Available = 1,
    Starting = 2,
    Fight = 3,
    Reward = 4,
    Completed = 5
}

public struct BossEvent : IComponent
{
    public BossId BossId;
    public BossEventPhase Phase;
    public float PhaseTimer;
}

public struct BossGateRequirement : IComponent
{
    public ResourceId RequiredResource;
    public int Amount;
}
```

Pipeline:

```text
BossGateInteractionSystem
→ BossRequirementValidationSystem
→ BossEventStartSystem
→ BossSpawnSystem
→ BossPhaseSystem
→ BossRewardSystem
→ BiomeUnlockSystem
```

## 12. Networking

Server authoritative:

- spawn.
- combat.
- harvesting.
- extraction.
- rescue.
- station processing.
- boss event phase.
- rewards.
- unlocks.

Client-only:

- all views.
- VFX.
- UI.
- local previews.

## 13. Implementation order for Codex

1. Создать vertical slice scene/bootstrap.
2. Добавить biome config.
3. Добавить resource configs.
4. Добавить 6–8 buildings.
5. Подключить harvesting.
6. Подключить CombatDirector.
7. Подключить Rootling extraction.
8. Подключить Craftsman rescue.
9. Подключить Sawmill/Specialist unlock.
10. Подключить 8 modules.
11. Подключить mini-boss.
12. Подключить boss.
13. Добавить debug tools.
14. Добавить slice acceptance test.

## 14. Acceptance criteria

- Можно пройти loop от старта до boss reward.
- Все progression gates работают.
- Первый NPC реально меняет экономику.
- Specialist нужен для хотя бы одного рецепта.
- Combat Director создает давление, но не бесконечный хаос.
- Build/Equipment slots влияют на стиль боя.
- Server authoritative state не зависит от client View.
- Вертикальный срез можно повторно запускать в test scene.
