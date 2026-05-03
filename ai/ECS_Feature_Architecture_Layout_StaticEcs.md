# ECS Architecture: Abstraction Layers & Feature Layout for StaticEcs

Краткая архитектурная памятка по ECS на основе идеи Seba's Lab: **abstraction layers and modules encapsulation**, адаптированная под **StaticEcs**.

Фокус документа:

- только архитектура;
- только layout конкретной фичи;
- без лишних слоев абстракций;
- с кодовым скелетом под StaticEcs.

---

## 1. Главная идея

Feature в ECS — это **модуль поведения**.

Feature предоставляет:

```text
Components + Events + Systems + Compose()
```

Сущность получает поведение не через наследование и не через вызов сервиса, а через набор компонентов.

```text
Entity + ComponentA + ComponentB
  -> будет обработана SystemA
```

---

## 2. Минимальная архитектура

```text
App / Game Composition Root
  -> Common Features
  -> Domain Features
```

Где:

- `CompositionRoot` создает мир, регистрирует типы и подключает фичи;
- `Common Features` дают переиспользуемое поведение;
- `Domain Features` описывают конкретные правила приложения или игры.

---

## 3. StaticEcs world setup

В StaticEcs мир обычно задается через тип мира и статический alias.

```csharp
using FFS.Libraries.StaticEcs;

public struct GameWorldType : IWorldType { }

public abstract class W : World<GameWorldType> { }
```

Для систем удобно завести отдельный тип систем:

```csharp
public struct GameSystemsType : ISystemsType { }

public abstract class GameSystems : W.Systems<GameSystemsType> { }
```

---

## 4. Composition Root

`CompositionRoot` — единственное место, где приложение собирается.

Он делает:

1. `W.Create(...)`
2. регистрацию типов;
3. регистрацию систем;
4. `W.Initialize(...)`
5. `GameSystems.Initialize()`
6. игровой update loop.

Пример:

```csharp
using FFS.Libraries.StaticEcs;

public static class GameCompositionRoot
{
    public static void Create()
    {
        W.Create(WorldConfig.Default());
        GameSystems.Create();

        RegisterTypes();
        ComposeFeatures();

        W.Initialize(baseEntitiesCapacity: 4096);
        GameSystems.Initialize();
    }

    public static void Update()
    {
        GameSystems.Update();

        // В StaticEcs Tick продвигает change tracking.
        // Обычно вызывается один раз после update систем.
        W.Tick();
    }

    public static void Destroy()
    {
        GameSystems.Destroy();
        W.Destroy();
    }

    private static void RegisterTypes()
    {
        TransformFeature.RegisterTypes();
        ProjectileFeature.RegisterTypes();
        WeaponFeature.RegisterTypes();
    }

    private static void ComposeFeatures()
    {
        TransformFeature.Compose();
        ProjectileFeature.Compose();
        WeaponFeature.Compose();
    }
}
```

`CompositionRoot` не должен:

- содержать domain-логику;
- обрабатывать entity;
- вызывать системы вручную;
- знать внутренние детали фич.

---

## 5. Правило зависимостей

Зависимости идут только от более специализированного к более общему.

```text
WeaponFeature
  -> ProjectileFeature
      -> TransformFeature
```

Разрешено:

- `WeaponFeature` создает entity с компонентами `ProjectileFeature`;
- `ProjectileFeature` использует `Position` из `TransformFeature`;
- верхняя фича знает публичные компоненты нижней.

Запрещено:

- `ProjectileFeature` знает про `WeaponFeature`;
- нижний модуль вызывает верхний;
- системы разных фич вызывают друг друга напрямую;
- появляются циклические зависимости.

Короткое правило:

> Нижний слой не знает о верхнем.

---

## 6. Роль компонентов как абстракций

В ECS роль интерфейсов выполняют компоненты.

Фича говорит:

```text
Если entity имеет такие компоненты,
мои systems применят к ней такое поведение.
```

Пример:

```text
ProjectileFeature предоставляет:
  Projectile
  ProjectileLifetime
  Velocity

ProjectileMoveSystem обрабатывает:
  Position + Velocity + Projectile
```

Любая entity с этими компонентами становится projectile-сущностью для этой фичи.

---

# 7. Конкретный layout фичи

Базовый layout:

```text
FeatureA/
  FeatureAFeature.cs

  Components/
    FeatureAState.cs
    FeatureAConfig.cs

  Events/
    FeatureARequested.cs
    FeatureACompleted.cs

  Systems/
    StartFeatureASystem.cs
    ProcessFeatureASystem.cs
    CompleteFeatureASystem.cs
    CleanupFeatureAEventsSystem.cs
```

Это основной шаблон. Новые слои добавлять не нужно.

---

## 8. `FeatureAFeature.cs`

`FeatureAFeature.cs` — публичная точка входа фичи.

Она отвечает только за:

- регистрацию типов StaticEcs;
- регистрацию систем;
- порядок pipeline.

```csharp
using FFS.Libraries.StaticEcs;

public static class FeatureAFeature
{
    public static void RegisterTypes()
    {
        W.RegisterComponentType<FeatureAState>();
        W.RegisterComponentType<FeatureAConfig>();

        W.Events.RegisterEventType<FeatureARequested>();
        W.Events.RegisterEventType<FeatureACompleted>();
    }

    public static void Compose()
    {
        GameSystems
            .Add(new StartFeatureASystem(), order: 0)
            .Add(new ProcessFeatureASystem(), order: 10)
            .Add(new CompleteFeatureASystem(), order: 20)
            .Add(new CleanupFeatureAEventsSystem(), order: 100);
    }
}
```

`FeatureAFeature.cs` не должен:

- считать логику;
- менять компоненты;
- хранить runtime-state;
- создавать domain-сущности без необходимости.

---

## 9. `Components/`

Компоненты — данные и публичный контракт фичи.

```csharp
using FFS.Libraries.StaticEcs;

public struct FeatureAState : IComponent
{
    public float Value;
    public bool IsCompleted;
}

public struct FeatureAConfig : IComponent
{
    public float Speed;
    public float Duration;
}
```

Правила:

- компонент — `struct`;
- компонент реализует `IComponent`;
- компонент хранит данные;
- компонент не вызывает системы;
- компонент не содержит gameplay pipeline.

Допустимо:

```csharp
public struct Health : IComponent
{
    public int Current;
    public int Max;

    public bool IsDead => Current <= 0;
}
```

Недопустимо:

```csharp
public struct Health : IComponent
{
    public int Current;

    public void ApplyDamage(int value)
    {
        // Плохо: domain-логика уехала в компонент.
    }
}
```

---

## 10. `Events/`

Events — одноразовая связь между системами и фичами.

```csharp
using FFS.Libraries.StaticEcs;

public struct FeatureARequested : IEvent
{
    public EntityGID Target;
    public float Value;
}

public struct FeatureACompleted : IEvent
{
    public EntityGID Target;
}
```

Использовать events для:

- намерений;
- команд;
- фактов;
- переходов между этапами pipeline.

Примеры имен:

```text
FireRequested
ShotFired
DamageRequested
DamageApplied
DeathRequested
DeathCompleted
```

---

## 11. `Systems/`

Система — единственное место, где живет логика.

Пример системы, которая читает events и меняет entity:

```csharp
using FFS.Libraries.StaticEcs;

internal sealed class StartFeatureASystem : ISystem
{
    public void Update()
    {
        var receiver = W.Events.GetReceiver<FeatureARequested>();

        receiver.ReadAll(static (W.Event<FeatureARequested> evt) =>
        {
            ref readonly var request = ref evt.Value;

            if (!request.Target.TryUnpack<GameWorldType>(out var target))
            {
                return;
            }

            if (!target.Has<FeatureAState>())
            {
                target.Add(new FeatureAState());
            }

            ref var state = ref target.Ref<FeatureAState>();
            state.Value = request.Value;
            state.IsCompleted = false;
        });
    }
}
```

Если в проекте используется хранение receiver как resource, допустим такой вариант:

```csharp
// registration/bootstrap
var receiver = W.Events.RegisterEventReceiver<FeatureARequested>();
W.SetResource(receiver);

// system
internal sealed class StartFeatureASystem : ISystem
{
    public void Update()
    {
        ref var receiver = ref W.GetResource<EventReceiver<GameWorldType, FeatureARequested>>();

        receiver.ReadAll(static (W.Event<FeatureARequested> evt) =>
        {
            // process event
        });
    }
}
```

Выберите один стиль на проект и используйте его последовательно.

---

## 12. Query system example

Обычная система StaticEcs через `W.Query()`:

```csharp
using FFS.Libraries.StaticEcs;

internal sealed class ProcessFeatureASystem : ISystem
{
    public void Update()
    {
        W.Query().For(
            static (ref FeatureAState state, in FeatureAConfig config) =>
            {
                if (state.IsCompleted)
                {
                    return;
                }

                state.Value += config.Speed;

                if (state.Value >= config.Duration)
                {
                    state.IsCompleted = true;
                }
            }
        );
    }
}
```

Правила query:

- `ref` — если компонент изменяется;
- `in` — если компонент только читается;
- static lambda — чтобы не делать capture;
- не хранить ссылки на компоненты между кадрами.

---

## 13. Emit event from system

Система может отправить event как результат обработки.

```csharp
using FFS.Libraries.StaticEcs;

internal sealed class CompleteFeatureASystem : ISystem
{
    public void Update()
    {
        W.Query().For(
            static (W.Entity entity, ref FeatureAState state) =>
            {
                if (!state.IsCompleted)
                {
                    return;
                }

                W.SendEvent(new FeatureACompleted
                {
                    Target = entity.GID
                });
            }
        );
    }
}
```

---

## 14. Cleanup events

Если используются event components как компоненты на entity, их нужно чистить в конце pipeline.

```csharp
internal sealed class CleanupFeatureAEventsSystem : ISystem
{
    public void Update()
    {
        W.Query<All<FeatureARequestedComponent>>()
            .BatchDelete<FeatureARequestedComponent>();
    }
}
```

Если используются `IEvent` + event receiver, отдельная cleanup-система обычно не нужна: события читаются через receiver.

Рекомендация:

> Для межсистемных одноразовых сообщений в StaticEcs чаще использовать `IEvent`.  
> Для состояния entity использовать обычные `IComponent`.

---

# 15. Полный пример: `ProjectileFeature`

## Layout

```text
Projectile/
  ProjectileFeature.cs

  Components/
    Projectile.cs
    ProjectileLifetime.cs
    Velocity.cs

  Events/
    SpawnProjectileRequested.cs
    ProjectileExpired.cs

  Systems/
    SpawnProjectileSystem.cs
    MoveProjectileSystem.cs
    ExpireProjectileSystem.cs
    DestroyExpiredProjectileSystem.cs
```

---

## Components

```csharp
using System.Numerics;
using FFS.Libraries.StaticEcs;

public struct Projectile : IComponent
{
    public int Damage;
}

public struct ProjectileLifetime : IComponent
{
    public float Remaining;
}

public struct Position : IComponent
{
    public Vector3 Value;
}

public struct Velocity : IComponent
{
    public Vector3 Value;
}

public struct ProjectileEntityType : IEntityType { }
```

---

## Events

```csharp
using System.Numerics;
using FFS.Libraries.StaticEcs;

public struct SpawnProjectileRequested : IEvent
{
    public Vector3 Position;
    public Vector3 Velocity;
    public int Damage;
    public float Lifetime;
}

public struct ProjectileExpired : IEvent
{
    public EntityGID Projectile;
}
```

---

## Feature entry point

```csharp
using FFS.Libraries.StaticEcs;

public static class ProjectileFeature
{
    public static void RegisterTypes()
    {
        W.RegisterComponentType<Projectile>();
        W.RegisterComponentType<ProjectileLifetime>();
        W.RegisterComponentType<Position>();
        W.RegisterComponentType<Velocity>();

        W.Events.RegisterEventType<SpawnProjectileRequested>();
        W.Events.RegisterEventType<ProjectileExpired>();
    }

    public static void Compose()
    {
        GameSystems
            .Add(new SpawnProjectileSystem(), order: 0)
            .Add(new MoveProjectileSystem(), order: 10)
            .Add(new ExpireProjectileSystem(), order: 20)
            .Add(new DestroyExpiredProjectileSystem(), order: 30);
    }
}
```

---

## Spawn system

```csharp
using FFS.Libraries.StaticEcs;

internal sealed class SpawnProjectileSystem : ISystem
{
    public void Update()
    {
        var receiver = W.Events.GetReceiver<SpawnProjectileRequested>();

        receiver.ReadAll(static (W.Event<SpawnProjectileRequested> evt) =>
        {
            ref readonly var request = ref evt.Value;

            W.NewEntity<ProjectileEntityType>(
                new Projectile
                {
                    Damage = request.Damage
                },
                new ProjectileLifetime
                {
                    Remaining = request.Lifetime
                },
                new Position
                {
                    Value = request.Position
                },
                new Velocity
                {
                    Value = request.Velocity
                }
            );
        });
    }
}
```

---

## Move system

```csharp
using FFS.Libraries.StaticEcs;

internal sealed class MoveProjectileSystem : ISystem
{
    private readonly float _deltaTime;

    public MoveProjectileSystem(float deltaTime = 1f / 60f)
    {
        _deltaTime = deltaTime;
    }

    public void Update()
    {
        W.Query().For(
            _deltaTime,
            static (ref float dt, ref Position position, in Velocity velocity, in Projectile projectile) =>
            {
                position.Value += velocity.Value * dt;
            }
        );
    }
}
```

---

## Expire system

```csharp
using FFS.Libraries.StaticEcs;

internal sealed class ExpireProjectileSystem : ISystem
{
    private readonly float _deltaTime;

    public ExpireProjectileSystem(float deltaTime = 1f / 60f)
    {
        _deltaTime = deltaTime;
    }

    public void Update()
    {
        W.Query().For(
            _deltaTime,
            static (ref float dt, W.Entity entity, ref ProjectileLifetime lifetime, in Projectile projectile) =>
            {
                lifetime.Remaining -= dt;

                if (lifetime.Remaining <= 0f)
                {
                    W.SendEvent(new ProjectileExpired
                    {
                        Projectile = entity.GID
                    });
                }
            }
        );
    }
}
```

---

## Destroy expired system

```csharp
using FFS.Libraries.StaticEcs;

internal sealed class DestroyExpiredProjectileSystem : ISystem
{
    public void Update()
    {
        var receiver = W.Events.GetReceiver<ProjectileExpired>();

        receiver.ReadAll(static (W.Event<ProjectileExpired> evt) =>
        {
            ref readonly var expired = ref evt.Value;

            if (expired.Projectile.TryUnpack<GameWorldType>(out var projectile))
            {
                projectile.Destroy();
            }
        });
    }
}
```

---

# 16. Пример: `WeaponFeature` использует `ProjectileFeature`

`WeaponFeature` может зависеть от `ProjectileFeature`, потому что оружие более специализировано.

```text
Weapon/
  WeaponFeature.cs

  Components/
    Weapon.cs
    WeaponCooldown.cs

  Events/
    FireRequested.cs
    ShotFired.cs

  Systems/
    FireWeaponSystem.cs
    TickWeaponCooldownSystem.cs
```

---

## Components

```csharp
using FFS.Libraries.StaticEcs;

public struct Weapon : IComponent
{
    public int Damage;
    public float ProjectileSpeed;
    public float ProjectileLifetime;
}

public struct WeaponCooldown : IComponent
{
    public float Current;
    public float Duration;
}
```

---

## Events

```csharp
using System.Numerics;
using FFS.Libraries.StaticEcs;

public struct FireRequested : IEvent
{
    public EntityGID Shooter;
    public Vector3 Position;
    public Vector3 Direction;
}

public struct ShotFired : IEvent
{
    public EntityGID Shooter;
}
```

---

## Feature entry point

```csharp
using FFS.Libraries.StaticEcs;

public static class WeaponFeature
{
    public static void RegisterTypes()
    {
        W.RegisterComponentType<Weapon>();
        W.RegisterComponentType<WeaponCooldown>();

        W.Events.RegisterEventType<FireRequested>();
        W.Events.RegisterEventType<ShotFired>();
    }

    public static void Compose()
    {
        GameSystems
            .Add(new TickWeaponCooldownSystem(), order: 0)
            .Add(new FireWeaponSystem(), order: 10);
    }
}
```

---

## Fire system

```csharp
using FFS.Libraries.StaticEcs;

internal sealed class FireWeaponSystem : ISystem
{
    public void Update()
    {
        var receiver = W.Events.GetReceiver<FireRequested>();

        receiver.ReadAll(static (W.Event<FireRequested> evt) =>
        {
            ref readonly var request = ref evt.Value;

            if (!request.Shooter.TryUnpack<GameWorldType>(out var shooter))
            {
                return;
            }

            if (!shooter.Has<Weapon>() || !shooter.Has<WeaponCooldown>())
            {
                return;
            }

            ref readonly var weapon = ref shooter.Read<Weapon>();
            ref var cooldown = ref shooter.Ref<WeaponCooldown>();

            if (cooldown.Current > 0f)
            {
                return;
            }

            cooldown.Current = cooldown.Duration;

            W.SendEvent(new SpawnProjectileRequested
            {
                Position = request.Position,
                Velocity = request.Direction * weapon.ProjectileSpeed,
                Damage = weapon.Damage,
                Lifetime = weapon.ProjectileLifetime
            });

            W.SendEvent(new ShotFired
            {
                Shooter = request.Shooter
            });
        });
    }
}
```

Важно:

- `WeaponFeature` знает про `SpawnProjectileRequested`;
- `ProjectileFeature` не знает про `WeaponFeature`;
- связь идет через public event contract.

---

## Cooldown system

```csharp
using FFS.Libraries.StaticEcs;

internal sealed class TickWeaponCooldownSystem : ISystem
{
    private readonly float _deltaTime;

    public TickWeaponCooldownSystem(float deltaTime = 1f / 60f)
    {
        _deltaTime = deltaTime;
    }

    public void Update()
    {
        W.Query().For(
            _deltaTime,
            static (ref float dt, ref WeaponCooldown cooldown) =>
            {
                if (cooldown.Current <= 0f)
                {
                    return;
                }

                cooldown.Current -= dt;

                if (cooldown.Current < 0f)
                {
                    cooldown.Current = 0f;
                }
            }
        );
    }
}
```

---

# 17. Минимальный layout для маленькой фичи

Если фича маленькая, можно не создавать папки:

```text
FeatureA/
  FeatureAFeature.cs
  FeatureAComponents.cs
  FeatureAEvents.cs
  FeatureASystems.cs
```

Пример:

```csharp
public static class FeatureAFeature
{
    public static void RegisterTypes()
    {
        W.RegisterComponentType<FeatureAState>();
        W.Events.RegisterEventType<FeatureARequested>();
    }

    public static void Compose()
    {
        GameSystems.Add(new FeatureASystem(), order: 0);
    }
}
```

Полный layout нужен, когда:

- файлов стало больше 5-7;
- появились отдельные этапы pipeline;
- фича начала использоваться несколькими другими фичами.

---

# 18. Когда делать подфичи

Подфичи нужны только если фича реально выросла.

```text
Combat/
  CombatFeature.cs

  Damage/
    DamageFeature.cs
    Components/
    Events/
    Systems/

  Health/
    HealthFeature.cs
    Components/
    Events/
    Systems/
```

`CombatFeature` только собирает подфичи:

```csharp
public static class CombatFeature
{
    public static void RegisterTypes()
    {
        DamageFeature.RegisterTypes();
        HealthFeature.RegisterTypes();
    }

    public static void Compose()
    {
        DamageFeature.Compose();
        HealthFeature.Compose();
    }
}
```

Не создавать подфичи заранее.

---

# 19. StaticEcs-specific правила

## Регистрация типов

Все component/event/tag types регистрируются до `W.Initialize()`.

```csharp
W.RegisterComponentType<Position>();
W.RegisterComponentType<Velocity>();
W.Events.RegisterEventType<SpawnProjectileRequested>();
```

## Query

Читать через `in`, менять через `ref`.

```csharp
W.Query().For(
    static (ref Position position, in Velocity velocity) =>
    {
        position.Value += velocity.Value;
    }
);
```

## Static lambda

В hot path использовать `static` lambda и передавать данные явно.

```csharp
W.Query().For(
    deltaTime,
    static (ref float dt, ref Position position, in Velocity velocity) =>
    {
        position.Value += velocity.Value * dt;
    }
);
```

## EntityType

Для массовых однородных сущностей использовать `IEntityType`.

```csharp
public struct ProjectileEntityType : IEntityType { }

var projectile = W.NewEntity<ProjectileEntityType>();
```

## Parallel

В `ForParallel` менять только текущую entity. Если нужно сообщить результат наружу — отправить event и обработать его позже на main-thread.

```csharp
W.Query().ForParallel(
    static (ref Position position, in Velocity velocity) =>
    {
        position.Value += velocity.Value;
    },
    minEntitiesPerThread: 50000
);
```

## Tick

После `GameSystems.Update()` вызывать:

```csharp
W.Tick();
```

Это важно для change tracking.

---

# 20. Что держать public / internal

Public:

```text
FeatureAFeature
Components
Events
Tags
EntityTypes
```

Internal:

```text
Systems
Helpers
Factories
Implementation details
```

Идея:

> Снаружи фича видна как контракт компонентов и событий.  
> Внутри фича скрывает способ обработки.

---

# 21. Чеклист фичи

Перед добавлением `FeatureA`:

- У фичи есть одна ответственность?
- Есть `FeatureAFeature.RegisterTypes()`?
- Есть `FeatureAFeature.Compose()`?
- Все StaticEcs-типы зарегистрированы до `W.Initialize()`?
- Порядок систем виден в `Compose()`?
- Системы не вызывают друг друга напрямую?
- Связь с другими фичами идет через public components/events?
- Нижние фичи не знают про верхние?
- Systems можно оставить `internal`?
- Нет лишних слоев вроде `Manager`, `Service`, `Controller`, если они не нужны?

---

# 22. Итог

Минимальная формула:

```text
Feature = Components + Events + Systems + RegisterTypes + Compose
```

Правильная зависимость:

```text
Specialized Feature -> Generic Feature
```

Неправильная зависимость:

```text
Generic Feature -> Specialized Feature
```

Главная цель:

> Сделать поведение black-box модулем:  
> фича открывает компоненты и события, но скрывает свои системы и pipeline.
