# Compile Error Notes

## 2026-05-27: Repeated CS0246 in Unity log

Unity repeated the same compile error several times for `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Domain/UnlockEvaluation.cs`:

```text
CS0246: The type or namespace name 'ISettlementUnlockReadModel' could not be found
```

Cause: the `StaticMlp.Features.BuildingCatalog` asmdef already referenced `StaticMlp.Features.Settlement.Contracts`, but the source file did not import the `StaticMlp.Features.Settlement` namespace.

Fix pattern: before changing asmdef references, first check whether the missing type is already available through an existing assembly reference and only needs the correct `using`.

## 2026-05-27: Repeated FFSECS0010 for Multi row field access

Unity repeated `FFSECS0010` for `ResourcesInventoryAccess.cs` when code accessed `rows[i].Id` and `rows[i].Amount` from `World<TWorld>.Multi<T>` read paths.

Cause: `Multi.this[]` returns by reference, and the analyzer rejects consuming that ref-return by value through a field/property chain.

Fix pattern: in read-only loops, use `rows.Get(i)` for an explicit copy or bind the row first with `ref readonly var row = ref rows[i]` before reading fields. In mutation paths, bind `ref var row = ref rows[i]` and mutate through that local.

## 2026-05-27: Removed RegisterClientResources hook

Unity repeated `CS0115` for `SettlementSharedResourcesGameplayFeature.RegisterClientResources()`.

Cause: `GameplayFeature` no longer exposes a client-resource registration hook. Client-side world resources are initialized by client systems during `Init()`.

Fix pattern: do not re-add `RegisterClientResources()` to the base feature API for one feature. Move the client resource setup into an explicit client-core bootstrap system and register it through `RegisterClientCoreSystems`.

## 2026-05-27: ECS query generic type must be a component or tag

Unity reported `CS0315` when a query used `All<FinishedBuildingTag, BuildingNetworkDefinition>`.

Cause: `BuildingNetworkDefinition` is catalog data, not an ECS component/tag. Query filters must be StaticEcs component or tag types.

Fix pattern: query the actual ECS state that carries the needed data. For settlement buildings, use `All<FinishedBuildingTag, ConstructionSiteState>` and read `ConstructionSiteState.BuildingId`.
