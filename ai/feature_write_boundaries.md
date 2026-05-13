# Feature Write Boundaries

Feature state is owned by the feature that defines the component or tag. Other features may read valid public contracts, but they must not mutate that state directly.

## Rule

Do not write another feature's components or tags with:

- `Mut<T>()`
- `ReplicationMut.Mut<T>()`
- `ClientProjection.Mut<T>()`

The public write port of a feature is a StaticEcs `IEvent`.

## Migration Recipe

1. Replace the foreign mutation with a request or command event.

```csharp
SW.SendEvent(new ConstructionWorkRequested(siteGid, actorGid, workAmount));
```

2. Add an owner feature system that receives the event, resolves the target entity, validates the feature invariant, and applies `Mut<T>()` to owned components.

```csharp
foreach (var evt in _requests)
{
    var site = ConstructionSiteQuery.GetServerSite(evt.Value.Site);
    ref var state = ref ReplicationMut.Mut<ConstructionSiteState>(site);
    ref var progress = ref ReplicationMut.Mut<ConstructionProgress>(site);

    ConstructionRules.ApplyBuildWork(ref state, ref progress, ...);
}
```

3. Emit an `Applied`, `Rejected`, or `Completed` fact event when another feature needs the result.

```csharp
SW.SendEvent(new ConstructionWorkApplied(siteGid, acceptedWork));
```

## Notes

- Network requests should adapt into the same local event path instead of mutating feature state directly.
- If one gameplay operation must atomically mutate state owned by two features, reconsider the feature boundary before adding orchestration.
- Keep validation fail-fast inside the owner feature except at trust boundaries such as client network input.
