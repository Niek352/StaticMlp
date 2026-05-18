# Codex Research — Scope 01 — Design Lock

> Назначение документа: research/implementation plan для Codex.
> Проект: co-op open world survival-builder на ECS.
> Важное ограничение: все runtime features должны проектироваться через ECS components/systems/resources/events, а не через MonoBehaviour-first архитектуру.
> Сетевой принцип: server-authoritative для боевых/экономических изменений; client-only только для View/VFX/UI/предпросмотра.


## 1. Цель scope

Зафиксировать продуктовые и архитектурные правила, без которых нельзя стабильно реализовывать отдельные фичи.

Design Lock не должен реализовывать полноценный gameplay. Его задача — создать набор констант, enum, config assets, ECS components, feature contracts и документацию, от которой потом будут зависеть:

- NPC Economy.
- Combat Director.
- Build/Equipment system.
- Vertical Slice.
- Production-ready expansion.

Главный результат scope: проект получает единый “source of truth” по NPC-классам, ресурсным семействам, способам получения NPC, слотам Build/Equipment и правилам server-authoritative ECS.

## 2. Макро-решения, которые нужно зафиксировать

### 2.1 NPC classes

В игре должны быть два базовых класса NPC:

1. **Companion**
   - Добывается в поле через extraction/incubation.
   - Хорош для сырьевых задач, логистики, боевой поддержки, автоматических простых работ.
   - Может быть “нечеловеческим”: зверь, дух, биомеханика, аномалия.
   - Обычно имеет work aptitude и combat utility.

2. **Specialist**
   - Получается через rescue/recruit/incubation rare chain.
   - Открывает рецепты, управляет производственными цепочками, повышает эффективность, дает знания.
   - Не должен быть просто “+1 worker”.
   - Должен быть ключом к progression recipe или station upgrade.

### 2.2 NPC acquisition paths

Зафиксировать три пути получения NPC:

1. **Extraction**
   - Существо ослабляется в бою.
   - Переводится в `ExtractableState`.
   - Игрок применяет extraction action.
   - Сервер валидирует состояние и создает captured companion fact/entity.

2. **Rescue**
   - NPC найден в лагере, клетке, капсуле, коконе, руинах.
   - Игрок освобождает его через interaction/combat event.
   - Сервер создает specialist/settler entity или pending recruit record.

3. **Incubation**
   - На базе используется egg/core/biome heart/fragment.
   - Через station job появляется new companion/specialist.
   - Время, ресурсы и station tier должны быть частью ECS job pipeline.

### 2.3 Resource families

Нужно зафиксировать минимум 5 resource families:

1. **Raw** — Wood, stone, ore, fiber, meat, resin.
2. **Flow** — Food, water, fuel, heat, electricity, cleanliness.
3. **Refined** — Planks, ingots, oil, cloth, chemicals, parts.
4. **Progression** — Boss cores, biome sigils, blueprint plates, memory shards.
5. **Stability / Maintenance** — Repair kits, medicine, filters, antidotes, insulation, ammunition.

### 2.4 Build/Equipment slots-only rule

Игрок может хранить библиотеку найденных предметов, но активировать только лимитированную конфигурацию.

Базовое правило для vertical slice:

- 3 Combat modules.
- 2 Utility modules.
- 2 Build signals.
- 4 Base infrastructure effects.

Никакой бесконечной суммы пассивок в MVP. Все эффекты должны проходить через slot activation.

## 3. Архитектурные границы

### 3.1 Что должно жить в Domain/ECS

- Типы NPC.
- Роли NPC.
- Resource family/type.
- Slot definitions.
- Item/module definitions.
- Правила доступности рецептов.
- Authoritative state: captured, recruited, incubating, equipped, active modules.
- Server-side validation.

### 3.2 Что не должно жить в Domain

- Unity GameObject View.
- UI panels.
- VFX/SFX.
- Client prediction visuals.
- Inspector-only helper MonoBehaviour, кроме composition/bootstrap/config authoring.

## 4. ECS data model

```csharp
public enum NpcClass : byte
{
    Companion = 1,
    Specialist = 2
}

public enum NpcAcquisitionPath : byte
{
    Extraction = 1,
    Rescue = 2,
    Incubation = 3
}

public enum ResourceFamily : byte
{
    Raw = 1,
    Flow = 2,
    Refined = 3,
    Progression = 4,
    Stability = 5
}

public enum EquipmentSlotKind : byte
{
    Combat = 1,
    Utility = 2,
    BuildSignal = 3,
    BaseInfrastructure = 4
}

public struct NpcClassComponent : IComponent
{
    public NpcClass Value;
}

public struct NpcRoleMask : IComponent
{
    public NpcRoleFlags Value;
}

public struct ResourceTypeComponent : IComponent
{
    public ResourceId ResourceId;
    public ResourceFamily Family;
}

public struct SlotDefinition : IComponent
{
    public EquipmentSlotKind Kind;
    public byte Index;
}

public struct ActiveModule : IComponent
{
    public ModuleId ModuleId;
    public EquipmentSlotKind SlotKind;
    public byte SlotIndex;
}
```

## 5. Resources / catalogs

```csharp
public sealed class NpcDefinitionCatalog
{
    public NpcDefinition Get(NpcDefinitionId id);
}

public sealed class ResourceDefinitionCatalog
{
    public ResourceDefinition Get(ResourceId id);
}

public sealed class ModuleDefinitionCatalog
{
    public ModuleDefinition Get(ModuleId id);
}

public sealed class SlotRuleCatalog
{
    public SlotRules Get(PlayerProgressionTier tier);
}
```

## 6. Systems

### 6.1 ConfigValidationSystem

Fail-fast проверяет catalogs на старте gameplay world:

- Все ResourceId имеют family.
- Все NPC definitions имеют class.
- Все module definitions имеют allowed slot kind.
- Нет дублирующихся ids.
- Нет ссылок на missing recipe/station/resource.

Validation может падать exception. Не превращать это в silent return.

### 6.2 DesignLockBootstrapSystem

Создает singleton/resources для feature scope:

- catalogs.
- slot rules.
- default resource families.
- ECS singleton entities, если в проекте используется такой паттерн.

### 6.3 SlotRuleValidationSystem

Server-side проверка активной сборки игрока:

- Не больше N модулей каждого slot kind.
- Каждый module подходит к slot kind.
- Module unlocked.
- Module не конфликтует с mutually-exclusive group.
- Base infrastructure module не активируется в player combat slot.

## 8. Networking

Authoritative:

- Module activation.
- NPC acquisition.
- Resource unlocks.
- Recipe unlocks.
- Base infrastructure effects.

Client-only:

- UI preview of module activation.
- VFX for extraction/rescue/incubation.
- Ghost preview for slot loadout.

Replicated:

- Player active module list.
- Player unlocked modules.
- Settlement unlocked recipes.
- Captured/recruited NPC records.

## 9. Implementation order for Codex

1. Найти существующие namespace/assembly conventions.
2. Создать feature asmdef, если в проекте принято разделять features.
3. Добавить enums/value ids.
4. Добавить catalogs и authoring/config loading adapter.
5. Добавить validation systems.
6. Добавить slot rules.
7. Добавить activate/deactivate commands.
8. Добавить unit tests на invalid configs.
9. Добавить минимальную документацию `Docs/DesignLock.md`.

## 10. Acceptance criteria

- Проект компилируется.
- Все IDs/families/classes/slots валидируются на старте.
- Нельзя активировать module в неправильный slot.
- Нельзя активировать больше лимита.
- Можно получить список активных modules через ECS state.
- Нет MonoBehaviour gameplay state.
- Ошибки design config падают fail-fast, а не скрываются.
