# StaticEcs Client View Feature

The view feature is client-only presentation infrastructure for binding ECS entities to Unity view prefabs.

```text
NetworkArchetypeId
    -> client NetArchetypeRegistry recipe
    -> ViewPath + view-state components
    -> BindEntityViewSystem
    -> EntityView
    -> ApplyComponentToViewSystem<TViewState>
    -> IEntityViewPart<TViewState>
```

## Assembly

Runtime code lives in:

```text
Assets/Scripts/StaticMlp/Game/Features/EcsViews/Runtime
```

The asmdef is `Game.Ecs.Views`. It references StaticEcs and `Ecs.Networking`, but it does not reference `Game.Core`. `Game.Core` references it for bootstrap composition and for built-in presentation state.

## Client-Only Components

- `ViewPath`: Resources path for the view prefab. Add it only from client prefab recipes or local client-only entities.
- `View`: runtime reference to the bound `IEntityView`. Never replicate it. Added tracking is enabled so generic apply systems can push already-existing view state immediately after binding.
- `DestroyViewRequest`: marker for explicit view cleanup before an entity is destroyed.
- `IViewComponent`: marker for client-only view state. It requires added and changed tracking.

Do not mark view components with `[ReplicatedComponent]`.

Replicated state and view state must stay separate:

```text
CharacterNetState / PhysicsCubeNetState
    -> client presentation sync system
    -> ViewTransform
    -> TransformViewComponent
```

Use `Mut<T>()` when changing view state so `ApplyComponentToViewSystem<T>` can observe `AllChanged<T>`.

## Unity Contracts

- `EntityView : MonoBehaviour` owns a client `CW.Entity` binding and forwards state to parts.
- `IEntityViewPart<TViewState>` is implemented by MonoBehaviours on the prefab.
- `ResourcesEntityViewFactory` loads `EntityView` prefabs through `Resources.Load<EntityView>(ViewPath.Value)`.
- `ViewRootProvider` optionally supplies a parent transform for spawned views.

Prefabs are still assembled by humans in the Unity Editor. Agents must not generate `.prefab` assets.

## System Registration

Client core registration currently adds:

```text
ViewSystemOrder.BindViews              BindEntityViewSystem
ViewSystemOrder.ApplyPresentationState ApplyComponentToViewSystem<T>
ViewSystemOrder.DestroyViews           DestroyEntityViewSystem
```

Features opt in to generic view-state apply systems through:

```csharp
public override void RegisterClientViewSync(ViewSyncBuilder views)
{
    views.Register<MyViewState>();
}
```

`RegisterClientViewSync` is for client view apply systems only. Server systems must never register view systems.

`BindEntityViewSystem` runs after normal presentation state sync. This avoids creating a view prefab while its first `ViewTransform` still contains default values.

`ApplyComponentToViewSystem<T>` also applies when `View` is newly added. This covers the common first-frame case where a view-state component already exists before the Unity view is bound.

## Networked Prefab Rules

Server recipes set replicated gameplay state only:

```csharp
NetArchetypeRegistry.RegisterServer(MyPrefabIds.Actor, e =>
{
    e.Set<ActorTag>();
    e.Set(new ActorNetState());
});
```

Client recipes may add view paths and view state:

```csharp
NetArchetypeRegistry.RegisterClient(MyPrefabIds.Actor, e =>
{
    e.Set<ActorTag>();
    e.Set(new ViewPath("Views/Actors/ActorView"));
    e.Set(new ActorViewState());
});
```

`ViewPath` must not be replicated. `NetworkArchetypeId` is the protocol id; the client maps it to presentation.

## Despawn

Network despawn currently destroys the `EntityView` immediately before destroying the client ECS entity. `DestroyViewRequest` remains available for explicit cleanup when the entity is kept alive for another frame.
