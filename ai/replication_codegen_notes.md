# Replication CodeGen Notes

Current note for generated component replication.

## Current State

The project already has an editor generator in:

```text
Assets/Editor/StaticMlp/ReplicationCodeGen/ReplicationCodeGenerator.cs
```

It scans Unity `TypeCache` for `[ReplicatedComponent]` structs and emits generated code into:

```text
Assets/Scripts/StaticMlp/Game/ReplicationGenerated
```

Generated output currently includes:

- `ReplicatedComponentIds.Generated.cs`
- one serializer per replicated component, for example `CharacterNetState.Replication.Generated.cs`
- `ReplicatedComponentRegistry.Generated.cs`
- `StaticMlp.Replication.CodeGenDiagnostics.Generated.cs`

The runtime path uses `ReplicationRegistry` for:

- applying component deltas on client/server;
- collecting local-owned dirty state on clients;
- collecting server-owned dirty state on servers;
- collecting initial state for spawn snapshots.

`NetworkIdentity` remains a special built-in replicated metadata component.

## Component Contract

Replicated gameplay state should look like this:

```csharp
[ReplicatedComponent(
    authority: ReplicationAuthority.Owner,
    delivery: NetDelivery.UnreliableSequenced,
    sendRate: 20
)]
public struct CharacterNetState :
    IComponent,
    IComponentConfig<CharacterNetState>,
    ITrackableAdded,
    ITrackableChanged,
    ITrackableDeleted {

    [ReplicatedField(Quantize = 0.01f, Interpolation = ReplicatedFieldInterpolation.Auto)]
    public Vector3 Position;

    public ComponentTypeConfig<CharacterNetState> Config() => new(
        guid: new Guid("PUT-STABLE-GUID-HERE")
    );
}
```

Rules:

- The type must be a `struct`.
- It must implement `IComponent`.
- It must expose a stable StaticEcs GUID through `IComponentConfig<T>`.
- It must implement `ITrackableChanged` if deltas should be collected.
- Replicated fields must use supported primitive, enum, `Vector2`, `Vector3`, or `Quaternion` types.
- Gameplay systems must mutate replicated state through `Mut<T>()`.

## Interpolated Replicated Fields

Use `Interpolation = ReplicatedFieldInterpolation.Auto` on replicated fields that should be smoothed on remote clients:

```csharp
[ReplicatedField(Quantize = 0.01f, Interpolation = ReplicatedFieldInterpolation.Auto)]
public Vector3 Position;

[ReplicatedField(Compress = true, Interpolation = ReplicatedFieldInterpolation.Auto)]
public Quaternion Rotation;
```

The generated client path keeps the replicated component as the network truth and writes presentation state into:

- `Interpolated<T>`: current blended value for rendering.
- `InterpolatedPrevious<T>`: previous received network value.
- `InterpolatedClock<T>`: interpolation start time and duration.

Generated code registers the closed generic client-core components, applies deltas through `ApplyClientDelta`, initializes interpolated state after snapshots, and adds `ReplicatedInterpolationSystem<T>`.

Rules:

- `Auto` supports only `float`, `Vector2`, `Vector3`, and `Quaternion`.
- Unsupported interpolated field types produce codegen diagnostics.
- `float` and vectors use lerp; `Quaternion` uses slerp.
- Non-interpolated fields in the same component are copied from the current replicated value into `Interpolated<T>`.
- Presentation systems for remote entities should read `Interpolated<T>`, not mutate the replicated component.
- Local-owned presentation should continue to read the replicated component directly.

## Extension Note

`Game.Core` now discovers gameplay feature assemblies through `GameplayFeatureDiscovery`, so feature types can be registered into StaticEcs without editing the central bootstrap.

The generated registry is still emitted as one generated gameplay registry. If a feature adds new replicated components, run `StaticMlp/Replication/Generate` and let Unity recompile. The next improvement should be making generated replication registration fully feature-local so a new `StaticMlp.Features.FeatureA` asmdef can own both its components and generated serializers without central generated references.

## Remaining Improvements

1. Generate feature-local replication modules, one module per asmdef or source assembly.
2. Register generated modules through `GameplayFeature` or a dedicated replication discovery hook.
3. Add replicated event generation for `[ReplicatedEvent]`.
4. Enforce deterministic ids and duplicate diagnostics.
5. Add send-rate throttling; `sendRate` is currently metadata for generated serializers.
6. Add batching by peer + delivery + frame.
7. Add interest filtering hooks for server-owned state.
8. Add encode/decode roundtrip tests for generated serializers.
