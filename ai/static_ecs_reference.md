# StaticEcs Quick Reference

This project uses [StaticEcs](https://github.com/Felid-Force-Studios/StaticEcs), namespace `FFS.Libraries.StaticEcs`.

## Setup Pattern

```csharp
public struct WT : IWorldType { }
public abstract class W : World<WT> { }
public struct GameSystems : ISystemsType { }
public abstract class GameSys : W.Systems<GameSystems> { }
```

## World Lifecycle

Strict order:

1. `W.Create(WorldConfig.Default())`
2. `W.Types().RegisterAll()` or manual registration with `.Component<T>().Tag<T>().Event<T>()`
3. `W.Initialize()`
4. Create entities, run systems, iterate queries
5. `W.Destroy()`

`RegisterAll()` without arguments scans `typeof(TWorld).Assembly`, which is safe on IL2CPP, WebGL, and NativeAOT. For types split across assemblies use:

```csharp
W.Types().RegisterAll(typeof(TWorld).Assembly, typeof(OtherAssemblyMarker).Assembly);
```

In this project, Unity startup gets extra gameplay assemblies from:

```csharp
GameplayFeatureDiscovery.GetEcsTypeAssemblies()
```

Any loaded assembly with a concrete parameterless `GameplayFeature` is passed to `RegisterAll(...)` automatically.

## Critical Rules

- Register all component/tag/event/link types between `Create()` and `Initialize()`.
- `Entity` is a 4-byte handle, not a persistent reference. Use `EntityGID` across frames.
- `Add<T>()` without value is idempotent.
- `Set(value)` overwrites with an OnDelete/OnAdd hook cycle.
- `Ref<T>()` assumes the component exists; check `Has<T>()` if uncertain.
- Use `Read<T>()` and `in` for read-only access.
- Query filters are `All<>`, `None<>`, and `Any<>`; combine with `And<>` or `Or<>`.
- Default query mode is Strict; do not modify filtered component/tag types on other entities during iteration.
- Use `EntitiesFlexible()` only when that flexibility is actually needed.
- During `ForParallel`, only modify the current entity and do not perform structural changes.
- Systems implement `ISystem` with optional `Init()`, `Update()`, `UpdateIsActive()`, and `Destroy()`.

## Common Patterns

Create entity with components:

```csharp
var entity = W.NewEntity<Default>().Set(new Position { Value = v }, new Velocity { Value = 1f });
```

Query iteration:

```csharp
foreach (var e in W.Query<All<Position, Velocity>>().Entities()) {
    ref var pos = ref e.Ref<Position>();
    ref readonly var vel = ref e.Read<Velocity>();
    pos.Value += vel.Value;
}
```

Delegate query:

```csharp
W.Query().For(static (ref Position p, in Velocity v) => {
    p.Value += v.Value;
});
```

Persistent reference:

```csharp
EntityGID gid = entity.GID;
if (gid.TryUnpack<WT>(out var resolved)) {
    // resolved is alive
}
```

Tags:

```csharp
entity.Set<IsPlayer>();
if (entity.Has<IsPlayer>()) {
    // ...
}
```

Multi-components:

```csharp
ref var items = ref entity.Add<W.Multi<Item>>();
items.Add(new Item { Id = 1 });
items.Add(new Item { Id = 2 });
foreach (ref var item in items) {
    item.Weight *= 2f;
}
```

Relations:

```csharp
entity.Set(new W.Link<Parent>(parentEntity));
ref var children = ref entity.Add<W.Links<Children>>();
children.TryAdd(childEntity.AsLink<Children>());
```

Systems:

```csharp
public struct MoveSystem : ISystem {
    public void Init() { }

    public void Update() {
        W.Query().For(static (ref Position p, in Velocity v) => {
            p.Value += v.Value;
        });
    }

    public void Destroy() { }
}

GameSys.Create();
GameSys.Add(new MoveSystem(), order: 0);
GameSys.Initialize();
GameSys.Update();
```

Resources:

```csharp
W.SetResource(new GameConfig { });
ref var config = ref W.GetResource<GameConfig>();
```

## Full Documentation

- Concise AI reference: https://felid-force-studios.github.io/StaticEcs/llms.txt
- Full documentation: https://felid-force-studios.github.io/StaticEcs/en/features.html
