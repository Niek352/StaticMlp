# Gameplay Systems Memo

Short human checklist for writing gameplay systems in this project.

## First Decision

Before writing a system, answer:

1. Which world owns this logic: `SW` or `CW`?
2. Which ownership tag should the query use: `ServerOwned`, `ClientOwned`, `LocalOwned`, or `RemoteOwned`?
3. Is this replicated state, a replicated event, or presentation-only state?
4. Does it belong in `Game.Core` or in a feature asmdef such as `StaticMlp.Features.FeatureA`?

Default choice: put gameplay in a feature asmdef. Put code in `Game.Core` only when multiple features should share it.

## Where Systems Go

```text
Server simulation:
    StaticMlp.Features.FeatureA/Systems/Server
    query ServerOwned or validated ClientOwned

Client local gameplay:
    StaticMlp.Features.FeatureA/Systems/Client
    query LocalOwned

Client remote presentation:
    StaticMlp.Features.FeatureA/Systems/Client or Presentation
    query RemoteOwned, read replicated state, smooth/render locally
    write IViewComponent state for EntityView parts

Client UX/input/camera:
    CW presentation systems or MonoBehaviour bridge
    do not replicate UI-only state
```

## Feature Registration

Each feature has one entry point:

```csharp
using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.FeatureA {
    public sealed class FeatureAGameplayFeature : GameplayFeature {
        public override void RegisterNetworkEvents() {
            // Register typed client-to-server commands here.
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems) {
            systems.Add(new FeatureAServerSystem(), GameplaySystemOrder.Gameplay);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems) {
            systems.Add(new FeatureAClientSystem(), GameplaySystemOrder.Gameplay);
        }

        public override void RegisterClientViewSync(ViewSyncBuilder views) {
            views.Register<FeatureAViewState>();
        }
    }
}
```

The bootstrap discovers this class automatically. Do not edit `MultiplayerSystemBootstrap` for normal feature systems.

## System Rules

- Gameplay systems never call Unity Transport APIs.
- Gameplay systems never serialize packets.
- Gameplay systems use `NetworkEvents` for replicated commands, not raw `NetInbox` or `NetOutbox`.
- Server-owned spawns go through `NetworkEntitySpawner`, not handwritten `NetworkIdentity` setup.
- Domain rules do not know about network archetype ids, view paths, raw inbox/outbox, or concrete UI/input state.
- Put network and presentation metadata in adapter catalogs keyed by domain ids.
- Local client gameplay queries `LocalOwned`.
- Remote client systems query `RemoteOwned` and smooth or render; they do not simulate gameplay.
- Client view state implements `IViewComponent`, is changed through `Mut<T>()`, and is not replicated.
- `ViewPath` and `View` are client-only. Never add them in server prefab recipes.
- Server gameplay queries `ServerOwned`; it accepts `ClientOwned` only through explicit validation.
- Mutate replicated components through `ReplicationMut.Mut<T>()`.
- Read through `Read<T>()` when no mutation is intended.
- Do not store `Entity` across frames. Store `EntityGID`.
- Do not modify filtered component/tag types on other entities while iterating a strict query.
- During `ForParallel`, modify only the current entity and avoid structural changes.

## Good Client System

```csharp
public sealed class FeatureAClientMoveSystem : ISystem {
    public void Update() {
        foreach (var e in CW.Query<All<LocalOwned, FeatureAActorTag, FeatureAState>>().Entities()) {
            ref var state = ref ReplicationMut.Mut<FeatureAState>(e);
            state.Position += ReadMove() * UnityEngine.Time.deltaTime;
        }
    }
}
```

## Good Remote Presentation System

```csharp
public sealed class FeatureARemoteViewSystem : ISystem {
    public void Update() {
        foreach (var e in CW.Query<All<RemoteOwned, FeatureAState, FeatureAViewState>>().Entities()) {
            ref readonly var state = ref e.Read<FeatureAState>();
            ref var view = ref e.Mut<FeatureAViewState>();
            view.RenderPosition = UnityEngine.Vector3.Lerp(view.RenderPosition, state.Position, 0.2f);
        }
    }
}
```

## Good Server System

```csharp
public sealed class FeatureAServerSystem : ISystem {
    public void Update() {
        foreach (var e in SW.Query<All<ServerOwned, FeatureAState>>().Entities()) {
            ref var state = ref ReplicationMut.Mut<FeatureAState>(e);
            state.ServerTimer += UnityEngine.Time.deltaTime;
        }
    }
}
```

## Red Flags

- A gameplay system imports `Unity.Networking.Transport`.
- A feature gameplay system reads `NetInbox`, writes `NetOutbox`, or hardcodes `new NetworkPeerId(0)`.
- A feature creates `NetworkIdentity`, applies `OwnershipTags`, or calls spawn/despawn broadcasters directly.
- A gameplay system checks network authority manually instead of querying ownership tags.
- A gameplay system changes replicated state with direct `entity.Mut<T>()` instead of `ReplicationMut.Mut<T>()`.
- A replicated component is changed via `Ref<T>()`.
- A remote client system writes authoritative gameplay state.
- A replicated component implements `IViewComponent`.
- A server system registers `BindEntityViewSystem` or `ApplyComponentToViewSystem<T>`.
- A feature requires editing central bootstrap code just to add ordinary systems.
- A system keeps `Entity` in a field.

## Minimal New Feature Checklist

1. Create `StaticMlp.Features.FeatureA.asmdef`.
2. Reference `Game.Core`, `Ecs.Networking`, `FFS.StaticEcs`, `FFS.StaticPack`, and `FFS.StaticEcs.Unity`.
3. Add tags/components/events in the feature assembly.
4. Add systems in `Systems/Server` and/or `Systems/Client`.
5. Add `FeatureAGameplayFeature : GameplayFeature`.
6. Register typed network commands in `RegisterNetworkEvents` when the feature has client-to-server actions.
7. Register systems with the correct `GameplaySystemOrder`.
8. Add client `ViewPath` and view-state components only to client network archetype recipes.
9. Register view-state apply systems in `RegisterClientViewSync`.
10. Register network archetype recipes only if the feature spawns networked entities.
11. Run Unity/codegen checks after adding replicated components.
