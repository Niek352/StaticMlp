# Replication CodeGen Plan

## Цель

Свести добавление синхронизируемого состояния к локальному контракту компонента:

```csharp
[ReplicatedComponent(
    authority: ReplicationAuthority.Owner,
    delivery: NetDelivery.UnreliableSequenced,
    sendRate: 20
)]
public struct CharacterNetState : IComponent {
    [ReplicatedField(Quantize = 0.01f)]
    public Vector3 Position;

    [ReplicatedField(Quantize = 0.01f)]
    public Vector3 Velocity;

    [ReplicatedField(Compress = true)]
    public Quaternion Rotation;
}
```

После этого генератор должен создать типовые части репликации: type id, сериализацию, apply, collect dirty, initial spawn snapshot и регистрацию.

## Текущая архитектура

Проект уже разделен на несколько явных слоев:

```text
StaticEcs worlds
    ServerWT      - серверное состояние и серверная симуляция
    ClientCoreWT  - клиентское реплицируемое состояние
    ClientUxWT    - клиентский UX/input/camera слой

Gameplay systems
    Client local gameplay mutates LocalOwned state
    Server gameplay mutates ServerOwned state
    Remote presentation reads RemoteOwned state

Replication
    NetInbox / NetOutbox
    PacketCodec
    Spawn / Despawn / OwnershipChanged / ComponentBatch / NetworkEvent messages
    ReplicationRegistry

Transport
    UtpTransportContext
    client/server complete, drain, send, schedule systems
    Unity Transport access is isolated here
```

### Frame flow

Серверный порядок:

```text
Complete transport jobs
Drain raw inbox
Handle connection lifecycle
Receive client-owned state
Run server gameplay
Collect server-owned dirty state
Relay client-owned dirty state
Send outbox packets
Schedule transport jobs
SW.Tick()
```

Клиентский порядок:

```text
Complete transport jobs
Drain raw inbox
Apply spawn/despawn/ownership/component deltas
Run local gameplay
Run remote smoothing/presentation
Collect local-owned dirty state
Send outbox packets
Schedule transport jobs
CW.Tick()
```

### Ownership model

Реплицируется только `NetworkIdentity`:

```csharp
public struct NetworkIdentity : IComponent {
    public NetworkPeerId Owner;
    public NetworkAuthority Authority;
    public ushort PrefabId;
}
```

Runtime-теги выводятся локально:

```text
Client:
    Authority.Owner && Owner == LocalPeerId -> LocalOwned
    otherwise                               -> RemoteOwned

Server:
    Authority.Server -> ServerOwned
    Authority.Owner  -> ClientOwned
```

### Current replication path

Сейчас автоматизации еще нет: `ReplicationRegistry` вручную знает про `CharacterNetState` и `NetworkIdentity`.

```text
CollectDirty(entity)
    if Has<CharacterNetState>() && HasChanged<CharacterNetState>()
        serialize CharacterNetState manually
        enqueue ComponentBatch

ApplyDelta(entity, delta)
    switch delta.ComponentTypeId
        CharacterNetState -> deserialize manually, Set()
        NetworkIdentity   -> deserialize manually, Set()
```

Spawn snapshot тоже собирается вручную: `SpawnBroadcaster` добавляет `NetworkIdentity`, затем отдельно проверяет `CharacterNetState`.

## Что уже хорошо

- Есть правильное разделение на gameplay, replication, ownership и transport.
- Gameplay-системы используют ownership-теги и `Mut<T>()`.
- Unity Transport почти весь изолирован в transport systems и `UtpTransportContext`.
- Уже есть контрактные атрибуты: `ReplicatedComponentAttribute`, `ReplicatedFieldAttribute`, `ReplicatedEventAttribute`.
- Компоненты имеют стабильные StaticEcs GUID и trackable-интерфейсы.
- Есть `ComponentDelta` как универсальный контейнер для дельт.
- Есть `ComponentTypeIds`, значит уже выбран компактный `ushort` id для wire protocol.

## Что нужно улучшить

1. Убрать ручную привязку компонентов из `ReplicationRegistry`.
   Сейчас каждый новый компонент требует править switch, serialize, deserialize, collect и spawn code.

2. Убрать ручной список ids из `ComponentTypeIds`.
   Генератор должен стабильно назначать ids или требовать явный id/GUID в атрибуте.

3. Разделить runtime API и generated registry.
   Runtime должен знать только интерфейсы и таблицы обработчиков, а не конкретные game-компоненты.

4. Добавить generated metadata по компоненту.
   Для каждого `[ReplicatedComponent]` нужны:
   - type id;
   - authority;
   - delivery;
   - send rate;
   - field layout/version;
   - serializer;
   - deserializer/apply handler;
   - dirty collector.

5. Добавить validation generator pass.
   Ошибки должны находиться при генерации:
   - компонент не `struct`;
   - нет `IComponent`;
   - нет track changed для replicated state;
   - поле неподдерживаемого типа;
   - `Quantize` стоит не на float/vector-compatible поле;
   - нет стабильного GUID/id;
   - duplicate type id.

6. Определить стратегию совместимости протокола.
   Нужна версия payload layout на компонент или строгий запрет менять layout без миграции.

7. Добавить поддержку initial state.
   Один и тот же generated serializer должен использоваться для spawn snapshot и обычных deltas.

8. Добавить event replication отдельно от component replication.
   `[ReplicatedEvent]` должен генерировать event type id, serializer, decoder и dispatch/apply API.

9. Добавить batching.
   `NetOutbox.EnqueueComponentDelta` сейчас создает отдельный `ComponentBatch` на каждую дельту. Лучше собирать дельты по peer + delivery + frame.

10. Добавить send rate/throttle.
    `sendRate` в атрибуте сейчас описан, но не применяется.

11. Добавить interest filtering.
    Сервер сейчас рассылает server-owned состояние всем peers. Нужна точка расширения под observers.

12. Уточнить источник `LocalPeerId`.
    Сейчас это global static `NetworkRuntime.LocalPeerId`; для нескольких локальных клиентов в одном процессе лучше хранить peer id в ресурсе client world.

13. Сделать generated files deterministic.
    Порядок компонентов, ids и текст файлов должны быть стабильными, чтобы git diff был маленьким.

14. Добавить тестовый/diagnostic слой.
    Нужны проверки encode/decode roundtrip, registry coverage и duplicate ids.

## Целевая структура CodeGen

```text
Assets/Scripts/StaticMlp/Networking/ReplicationContracts
    ReplicatedComponentAttribute.cs
    ReplicatedFieldAttribute.cs
    ReplicatedEventAttribute.cs

Assets/Scripts/StaticMlp/Networking/ReplicationRuntime
    IReplicatedComponentCodec.cs
    ReplicatedComponentDescriptor.cs
    ReplicationRuntimeRegistry.cs
    ReplicationWriter.cs / ReplicationReader.cs

Assets/Scripts/StaticMlp/Networking/ReplicationGenerated
    ReplicatedComponentIds.Generated.cs
    ReplicatedComponentRegistry.Generated.cs
    CharacterNetState.Replication.Generated.cs
    ...

Assets/Editor/StaticMlp/ReplicationCodeGen
    ReplicationCodeGenerator.cs
```

## Runtime API после рефакторинга

Runtime registry должен выглядеть примерно так:

```csharp
public readonly struct ReplicatedComponentDescriptor {
    public readonly ushort TypeId;
    public readonly ReplicationAuthority Authority;
    public readonly NetDelivery Delivery;
    public readonly ushort SendRate;
}

public interface IReplicatedComponentCodec<TComponent>
    where TComponent : struct, IComponent {
    ComponentDelta CreateDelta<TWorld>(World<TWorld>.Entity e)
        where TWorld : struct, IWorldType;

    void Apply<TWorld>(World<TWorld>.Entity e, byte[] payload)
        where TWorld : struct, IWorldType;
}
```

Практически для StaticEcs удобнее сгенерировать static methods, а registry сделать таблицей делегатов:

```csharp
public readonly struct ReplicatedComponentHandler {
    public readonly ushort TypeId;
    public readonly ReplicationAuthority Authority;
    public readonly NetDelivery Delivery;
    public readonly ushort SendRate;
    public readonly Func<CW.Entity, bool> CollectClient;
    public readonly Func<SW.Entity, bool> CollectServer;
    public readonly Action<CW.Entity, ComponentDelta> ApplyClient;
    public readonly Action<SW.Entity, ComponentDelta> ApplyServer;
}
```

## Генерируемые файлы

### 1. ReplicatedComponentIds.Generated.cs

```csharp
// <auto-generated/>
namespace StaticMlp.Networking.Replication {
    public static class ReplicatedComponentIds {
        public const ushort CharacterNetState = 1;
        public const ushort NetworkIdentity = 2;
    }
}
```

### 2. CharacterNetState.Replication.Generated.cs

```csharp
// <auto-generated/>
namespace StaticMlp.Networking.Replication.Generated {
    public static class CharacterNetStateReplication {
        public const ushort TypeId = ReplicatedComponentIds.CharacterNetState;

        public static ComponentDelta CreateDelta(EntityGID gid, in CharacterNetState state) {
            var writer = BinaryPackWriter.CreateFromPool(64);
            writer.WriteFloat(
                Quantize001(state.Position.x),
                Quantize001(state.Position.y),
                Quantize001(state.Position.z)
            );
            writer.WriteFloat(
                Quantize001(state.Velocity.x),
                Quantize001(state.Velocity.y),
                Quantize001(state.Velocity.z)
            );
            writer.WriteFloat(state.Rotation.x, state.Rotation.y, state.Rotation.z, state.Rotation.w);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return new ComponentDelta(gid, TypeId, bytes);
        }

        public static CharacterNetState Read(byte[] payload) {
            var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
            return new CharacterNetState {
                Position = new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()),
                Velocity = new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()),
                Rotation = new Quaternion(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat())
            };
        }

        private static float Quantize001(float value) {
            return (float)Math.Round(value / 0.01f) * 0.01f;
        }
    }
}
```

### 3. ReplicatedComponentRegistry.Generated.cs

```csharp
// <auto-generated/>
namespace StaticMlp.Networking.Replication {
    public static partial class ReplicationRegistry {
        public static void ApplyDelta(CW.Entity e, ComponentDelta delta) {
            switch (delta.ComponentTypeId) {
                case ReplicatedComponentIds.CharacterNetState:
                    e.Set(CharacterNetStateReplication.Read(delta.Payload));
                    break;
            }
        }

        public static void CollectDirty(CW.Entity e, NetOutbox outbox, NetworkPeerId peer) {
            if (e.Has<CharacterNetState>() && e.HasChanged<CharacterNetState>())
                outbox.EnqueueComponentDelta(
                    peer,
                    CharacterNetStateReplication.CreateDelta(e.GID, e.Read<CharacterNetState>()),
                    NetDelivery.UnreliableSequenced
                );
        }
    }
}
```

## План внедрения

### Этап 1. Подготовить runtime к генерации

1. Сделать `ReplicationRegistry` partial.
2. Вынести ручную логику `CharacterNetState` в отдельный generated-like файл без генератора.
3. Переименовать `ComponentTypeIds` в `ReplicatedComponentIds` или оставить compatibility wrapper.
4. Вынести `NetworkIdentity` в отдельный special-case path: это системный replicated metadata component, не обычный gameplay component.
5. Добавить единый API:
   - `CreateDelta(entity, component)`;
   - `ApplyDelta(entity, delta)`;
   - `CollectDirty(entity, outbox, peer)`;
   - `CollectInitialState(entity, list)`.

### Этап 2. Сделать минимальный generator

1. Добавить editor assembly/folder для генератора.
2. Создать класс:

```csharp
[Generator]
public sealed class ReplicationCodeGenerator : ICodeGenerator {
    public void Execute(GeneratorContext context) {
        context.OverrideFolderPath("Assets/Scripts/StaticMlp/Networking/ReplicationGenerated");
        // scan assemblies, collect [ReplicatedComponent], emit files
    }
}
```

3. Использовать `TypeCache.GetTypesWithAttribute<ReplicatedComponentAttribute>()`.
4. Для первого прохода поддержать только:
   - `bool`;
   - integer primitives;
   - `float`;
   - `Vector2`;
   - `Vector3`;
   - `Quaternion`;
   - enum byte/ushort/int.
5. Сгенерировать ids, serializers, apply switch и collect dirty.

### Этап 3. Подключить к текущему коду

1. Заменить ручной `ReplicationRegistry` generated registry.
2. Заменить ручной `SpawnBroadcaster` на `ReplicationRegistry.CollectInitialState`.
3. Убрать ручные `ComponentTypeIds`.
4. Проверить, что добавление нового `[ReplicatedComponent]` не требует правок registry/spawn/apply.

### Этап 4. Добавить проверки и диагностику

1. Генерировать `StaticMlp.Replication.CodeGenDiagnostics.Generated.cs` с понятными compile errors через `#error`, если контракт нарушен.
2. Проверять duplicate ids/GUIDs.
3. Проверять unsupported fields.
4. Проверять, что replicated component имеет tracking.
5. Добавить editor menu command `StaticMlp/Replication/Generate`.

### Этап 5. Расширить protocol features

1. Batching по peer/delivery.
2. Send rate scheduler.
3. Interest filtering hook.
4. Event replication generator.
5. Payload versioning.
6. Optional full snapshot/resync path.

## Первый критерий готовности

Минимальная цель считается достигнутой, когда можно:

1. Добавить новый компонент с `[ReplicatedComponent]`.
2. Запустить генерацию.
3. Получить generated serializer/type id/apply/collect.
4. Добавить компонент на server-owned или owner-owned entity.
5. Увидеть, что delta отправляется и применяется без ручной правки `ReplicationRegistry`, `ComponentTypeIds` и `SpawnBroadcaster`.

