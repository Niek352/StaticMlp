# Stage 1 Data/Config Architecture

This document recommends an MVP-ready data/config model for Stage 1 that matches the current project shape and keeps Stage 2-5 implementation fast.

## Current baseline

The current codebase already shows the right core pattern in one place:

- `Features/BuildingCatalog/Runtime/BuildingCatalogData.cs` holds small code-first domain definitions.
- `Features/BuildingCatalog/Runtime/Networking/BuildingNetworkCatalog.cs` keeps network archetype mapping separate.
- `Features/BuildingCatalog/Runtime/Presentation/BuildingPresentationCatalog.cs` keeps view paths separate.
- `Features/Combat/Runtime/Logic/Resources/CombatConfig.cs` and `Features/Statuses/Runtime/Logic/Resources/StatusesConfig.cs` use `IResource` for runtime tuning.

The main current gaps are:

- bootstrap seed data is still hardcoded in `StaticMlpMultiplayerBootstrap.cs`
- several gameplay contracts still carry raw `ushort` content ids
- combat/build availability is still switch-driven instead of catalog-driven
- `ResourcesInventoryMinimal` is still a prototype owner-only inventory, not a settlement economy model

## Recommended config model

## Core recommendation

Use a **code-first catalog model** for MVP instead of ScriptableObjects or a generic data DSL.

For Stage 1 through early Stage 3, the fastest structure is:

1. Small immutable domain definition structs/classes in `Runtime/Logic/Data`
2. One static catalog per domain with `All`, `Get(id)`, `TryGet(id)`
3. Separate network adapter catalogs in `Runtime/Networking`
4. Separate presentation adapter catalogs in `Runtime/Presentation`
5. Small runtime tuning resources only for simulation constants that are not content entries
6. Separate seed/manifests for initial slice setup

Recommended feature-level ownership:

- `Settlement`: buildings, resources, recipes, worker roles
- `Build`: build modules and loadout availability
- `World`: regions, expeditions, raids
- `Progression`: rewards, unlock flags, reward application

## Shared design rules

- Every catalog entry uses a typed stable id.
- Domain definitions contain only gameplay-relevant data.
- Domain definitions do not contain prefab paths, `NetworkArchetypeId`, UI labels, icons, or view paths.
- Adapter catalogs map domain ids into network/view data.
- Bootstrap loads a stage manifest; it does not own content constants.
- Do not introduce a generic effects language for Stage 1. Explicit fields and explicit structs are faster and safer here.

## Recommended domain definitions

### Buildings

Recommended type: `BuildingDefinition`

Minimal fields:

- `BuildingId Id`
- `BuildingKindId Kind`
- `Int2 Footprint`
- `ResourceAmount[] ConstructionCost`
- `float BuildWorkRequired`
- `BuildingCapabilityFlags Capabilities`
- `RecipeId[] EnablesRecipes`
- `BuildModuleId[] UnlocksModules`
- `int WorkerCapacityGranted`
- `int StorageCapacityGranted`
- `int RaidDefenseValue`

Notes:

- `Capabilities` should stay small and explicit for MVP: `CampCore`, `Storage`, `Production`, `Defense`.
- `DisplayName` should move out of the pure domain definition and into presentation data.
- The existing `BuildingCatalogData` shape is the right starting point.

### Resources

Recommended type: `ResourceDefinition`

Minimal fields:

- `ResourceId Id`
- `ResourceKindId Kind`
- `ResourceUsageFlags Usage`
- `bool IsSettlementStored`
- `int StartingSettlementAmount`

Notes:

- Stage 1 only needs `Wood` and `Stone`.
- Keep resources as ids plus gameplay usage rules, not UI text or icon references.
- Move away from the current hardcoded `ResourcesInventory` fields over time and let resource ids drive storage.

### Recipes / production

Recommended type: `RecipeDefinition`

Minimal fields:

- `RecipeId Id`
- `BuildingId ProducedAtBuilding`
- `WorkerRoleId RequiredWorkerRole`
- `ResourceAmount[] Inputs`
- `ResourceAmount[] Outputs`
- `float WorkSeconds`
- `byte QueueLimit`

Notes:

- This is enough for Stage 3 worker economy without building a factory game framework.
- Use recipes for both production and crafted build-module prerequisites where needed.

### Worker roles

Recommended type: `WorkerRoleDefinition`

Minimal fields:

- `WorkerRoleId Id`
- `WorkerJobFlags AllowedJobs`
- `float BuildSpeedMultiplier`
- `float ProductionSpeedMultiplier`
- `int HousingCost`
- `int RaidCombatValue`

Notes:

- Stage 1 only needs one real role: `CampBuilder`.
- Do not let worker role definitions directly hold prefab or behavior-view references.
- If worker AI needs a behavior mapping, keep that in a settlement runtime adapter catalog or a worker runtime profile map.

### Build modules

Recommended type: `BuildModuleDefinition`

Minimal fields:

- `BuildModuleId Id`
- `BuildModuleSlotType SlotType`
- `CombatAbilityId GrantedAbility`
- `BuildingId[] RequiredBuildings`
- `RecipeId[] RequiredRecipes`
- `ProgressFlagId[] RequiredProgress`
- `BuildArchetypeId Archetype`

Notes:

- For Stage 1, this can stay simple: one module grants one combat ability.
- This replaces hardcoded ability cycling as the source of available player combat options.
- Keep module unlock rules in domain data, not in UI or input code.

### Expeditions / regions

Use two definitions instead of one mixed blob.

`RegionDefinition` minimal fields:

- `RegionId Id`
- `RegionKindId Kind`
- `ProgressFlagId[] RequiredProgress`
- `ExpeditionId[] Expeditions`
- `RaidId[] PossibleRaids`

`ExpeditionDefinition` minimal fields:

- `ExpeditionId Id`
- `RegionId RegionId`
- `EncounterProfileId Encounter`
- `RewardPackageId SuccessReward`
- `ProgressFlagId[] RequiredProgress`
- `int ThreatTier`

Notes:

- Region owns world progression grouping.
- Expedition owns one playable outing and its reward link.
- Concrete spawn positions and visual map markers should not live here.

### Rewards

Recommended type: `RewardPackageDefinition`

Minimal fields:

- `RewardPackageId Id`
- `ResourceAmount[] ResourceGrants`
- `BuildModuleId[] ModuleUnlocks`
- `BuildingId[] BuildingUnlocks`
- `ProgressFlagId[] ProgressFlagsGranted`
- `RaidId[] RaidUnlocks`

Notes:

- Stage 1 should use one real reward package: `RecoveredWarCache`.
- Keep reward application explicit. Avoid an overly generic modifier system this early.

### Raids

Recommended type: `RaidDefinition`

Minimal fields:

- `RaidId Id`
- `RegionId SourceRegion`
- `EncounterProfileId Encounter`
- `ProgressFlagId[] TriggerProgress`
- `int ThreatValue`
- `RewardPackageId SuccessReward`
- `RewardPackageId FailurePenalty`

Notes:

- A raid should be world/progression data that instantiates combat, not a separate special-case mode.
- Settlement defense values and worker combat contribution should feed into raid resolution, but the raid definition itself should stay small.

## Recommended supporting types

These ids/contracts should exist from the start:

- `BuildingId`
- `ResourceId`
- `RecipeId`
- `WorkerRoleId`
- `BuildModuleId`
- `BuildArchetypeId`
- `RegionId`
- `ExpeditionId`
- `RewardPackageId`
- `RaidId`
- `ProgressFlagId`
- `EncounterProfileId`

Useful shared value types:

- `ResourceAmount { ResourceId Id; int Amount; }`
- `Int2`
- small flag enums such as `BuildingCapabilityFlags`, `WorkerJobFlags`, `ResourceUsageFlags`

## Seed / manifest model

Keep initial slice setup in dedicated manifests, not in bootstrap literals.

Recommended Stage 1 manifests:

- `Stage1SettlementSeed`
  Initial buildings/sites, starting resources, starting workers
- `Stage1WorldSeed`
  Starting unlocked region, expedition availability, raid availability
- `Stage1ProgressionSeed`
  Starting progress flags, starting module/building unlocks

These can still be provided as code-defined arrays for MVP. The important change is ownership and shape, not authoring technology.

## Domain vs adapter split

## Pure domain data

These should stay domain-owned and free from network/UI concerns:

- all typed content ids
- construction costs
- recipe inputs and outputs
- build work requirements
- worker role gameplay permissions
- module unlock requirements
- links between buildings, recipes, modules, expeditions, raids, and rewards
- raid triggers and consequences
- reward grants and progression flags
- region and expedition availability rules
- settlement-side numeric values used by authoritative simulation

Examples:

- `BuildingDefinition`
- `RecipeDefinition`
- `BuildModuleDefinition`
- `RewardPackageDefinition`
- `RaidDefinition`

## Network adapter data

These should live in explicit network catalogs or replication contracts:

- `BuildingId -> blueprint archetype id / finished archetype id`
- status archetype ids
- network entity type ids
- network schema versions
- replicated component GUIDs
- replicated event GUIDs
- request/result payload DTOs
- any packed numeric representation required by serializers

Examples from the current codebase:

- `BuildingNetworkCatalog`
- `BuildingNetworkArchetypeIds`
- `StatusNetworkArchetypes`
- replicated component/event GUIDs in `ConstructionSiteState`, `ResourcesInventory`, and building request events

Recommendation:

- keep typed ids in the gameplay-facing API
- convert to raw `ushort` only inside explicit serialization or generated replication boundaries

## Presentation adapter data

These should live in feature presentation catalogs:

- localized display names
- descriptions
- icons
- sort order
- menu grouping
- prefab/view paths
- VFX/SFX keys
- map marker visuals
- tooltip copy

Examples from the current codebase:

- `BuildingPresentationCatalog`
- `CombatPresentationConfig`

Recommendation:

- move `DisplayName` out of the building domain definition
- keep presentation text and paths replaceable without touching simulation data

## Stability requirements

## IDs that must be stable from the start

The following ids should be treated as append-only and stable across saves/network/content references:

- `BuildingId`
- `ResourceId`
- `RecipeId`
- `WorkerRoleId`
- `BuildModuleId`
- `BuildArchetypeId`
- `RegionId`
- `ExpeditionId`
- `RewardPackageId`
- `RaidId`
- `ProgressFlagId`
- `EncounterProfileId`
- `CombatAbilityId`

If Stage 2+ introduces explicit status ids beyond tag types, those must also become stable immediately.

## Runtime/wire contracts that must be stable from the start

- replicated component GUIDs
- replicated event GUIDs
- request/result payload shapes
- network entity type ids
- network schema versions
- network archetype ids once assigned
- any saved progression keys or unlock flag ids

Current examples that should be treated as real contracts already:

- building request event GUIDs
- building replicated component GUIDs
- `CombatAbilityId`
- `BuildingNetworkArchetypeIds`
- `StatusNetworkArchetypes`

## What does not need stability yet

These can be tuned freely during MVP:

- numeric balance values
- build times
- damage values
- reward amounts
- UI copy
- icons
- view paths
- initial spawn positions in stage seeds

## Migration steps

1. Freeze existing stable ids now.

- Keep `BuildingId` and `CombatAbilityId` as the precedent.
- Add typed ids for resources, recipes, worker roles, modules, regions, expeditions, rewards, raids, and progression flags before more content lands.

2. Keep the current building catalog split, but make the domain side cleaner.

- Keep the three-layer pattern:
  domain catalog, network catalog, presentation catalog
- Move `DisplayName` out of `BuildingDefinition`
- Stop adding new prefab/network/UI fields to domain definitions

3. Replace raw content ids at gameplay boundaries with typed ids.

- `ConstructionSiteState.BuildingId`
- `PlaceBuildingRequestEvent.BuildingId`
- `InitialConstructionSiteDefinition.BuildingId`

Use typed ids in gameplay code and only touch raw numeric ids inside serialization/wire boundaries.

4. Replace `ResourcesInventoryMinimal` with settlement-owned resource state.

- Introduce a settlement resource store keyed by `ResourceId`
- Keep `Wood` and `Stone` only for Stage 1
- Do not build per-item stack or loot-bag systems yet

5. Introduce `RecipeCatalog` and `WorkerRoleCatalog` before adding more buildings.

- Building effects should unlock recipes and worker capacity through data
- Do not hardcode each new building as a custom system branch

6. Introduce `BuildModuleCatalog` before expanding combat selection.

- Replace hardcoded ability cycling in client combat selection with available module ids
- Keep the link `BuildModuleId -> CombatAbilityId`
- Do not refactor all combat internals at once; wrap them with the build-module layer first

7. Introduce `WorldCatalog` and `ProgressionCatalog` before expeditions and raids expand.

- regions
- expeditions
- rewards
- raids
- progress flags

This gives one authoritative place for unlock routing instead of scattering conditions across settlement and combat code.

8. Move hardcoded bootstrap slice setup into owned manifests.

- move initial construction sites out of `StaticMlpMultiplayerBootstrap`
- move initial workers/bots out of bootstrap
- let bootstrap inject a `Stage1*Seed` resource instead of defining content directly

9. Keep Stage 1 authoring code-first.

- Use static readonly arrays and small catalogs first
- Only consider external JSON/asset authoring later if content volume becomes the real bottleneck
- Preserve the same ids and domain structs if authoring technology changes

## Recommended outcome

For MVP, the right target is not a universal content framework. The right target is:

- one clean domain catalog per owned feature
- small typed ids everywhere
- explicit network adapters
- explicit presentation adapters
- bootstrap reduced to seed injection

That gives enough structure to scale through Stage 2-5 quickly without locking the project into a full-release data architecture too early.
