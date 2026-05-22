---
name: staticmlp-code-writing
description: Use when changing C# gameplay, ECS systems, feature modules, domain rules, spawners, resources, or general runtime code in StaticMlp.
---

# StaticMlp Code Writing

Use this skill before editing ordinary gameplay/runtime C# code. If the change touches replication, ownership, transport, generated code, UI, MVC, or MonoBehaviour presentation, also use the matching specialized skill.

## Required Context

Read these first when relevant:

- `AGENTS.md`
- `Assets/Scripts/StaticMlp/Features/AGENTS.md`
- Feature-local `AGENTS.md` files when present.
- `ai/ECS_Feature_Architecture_Layout_StaticEcs.md`
- `ai/gameplay_systems_memo.md`
- `ai/static_ecs_reference.md`
- `ai/static-ecs FULL.txt` only when the short reference is insufficient.

## First Classification

Before writing code, classify the work:

1. Is this server simulation, client-core logic, client UX, or tooling?
2. Is the data replicated state, local-only ECS state, presentation state, a gameplay request, or a domain definition?
3. Is the actor `LocalOwned`, `RemoteOwned`, `ServerOwned`, or `ClientOwned`?
4. Is the correct boundary `Game.Core`, a feature `Contracts` asmdef, feature `Runtime/Logic`, feature `Runtime/Presentation`, `Networking`, or `Composition`?
5. Should this be a component, tag, event component, resource, domain rule, spawner, system, or passive view?
6. What exact files/asmdefs are in write scope, what is read-only context, and what is out of scope?

If the answer is unclear, inspect nearby features and stop before adding a new abstraction.

## Feature Layout

Feature folder layout is strict for new files and for existing files touched by the current change. Do not create files in arbitrary feature folders.

For split features, choose the runtime boundary first:

```text
Assets/Scripts/StaticMlp/Features/<Feature>/Runtime/
  Contracts/       public contracts shared by logic/presentation/other modules
  Logic/           gameplay state, rules, server/client-core systems, validation
  Presentation/    client-only view state, view sync, MVC, Unity-facing code
```

Legacy unsplit features may keep existing `Runtime/<Bucket>` layout, but new or touched files must still use an approved bucket.

Only feature entry points and asmdefs may live directly in an assembly root:

```text
Runtime/Logic/BuildLogicFeature.cs
Runtime/Presentation/BuildPresentationFeature.cs
Runtime/Logic/StaticMlp.Features.Build.Logic.asmdef
```

Approved buckets under `Runtime/Logic`, `Runtime/Presentation`, `Runtime/Contracts`, or legacy `Runtime`:

```text
Components/          IComponent, ITag, view-state components
Events/              IEvent, request/result events, network event registration, gameplay commands implemented as events
Systems/             ECS systems, with Systems/Client and Systems/Server when context differs
Factories/           *Factory types, including NetEntityFactory-based resources
WorldResources/      StaticEcs IResource types; do not use Resources/ for scripts
Ids/                 *Id value types and *Ids constants
EntityTypes/         StaticEcs entity type markers
NetworkEntityTypes/  network entity archetype/types
Domain/              pure rules/calculation/model code only
Catalogs/            static or data catalogs
Definitions/         immutable domain definitions
Validation/          explicit validation rules at trust/gameplay boundaries
Queries/             named ECS/domain query helpers
Seeds/               seed data and manifests
Input/               feature-local input mapping/translation
Controllers/         MVC controllers
Views/               passive view classes
ViewParts/           IEntityViewPart/MonoBehaviour view parts
Actions/             AI/action packages; semantic subfolders like Actions/AttackEnemy are allowed here
```

Type/role buckets are the primary axis. Semantic grouping is allowed only inside an approved bucket, for example `Actions/AttackEnemy`, not as a new first-level folder.

Forbidden by default:

- New first-level folders not listed above.
- `Resources/` for ECS `IResource`; use `WorldResources/`.
- Singular/plural drift such as `Factory/` when the rule says `Factories/`.
- Putting components, events, ids, factories, or resources directly into `Logic/`, `Presentation/`, or `Runtime/`.
- Creating a semantic folder at the first level just because a feature is growing.

If no approved bucket fits, stop before adding the file. Treat it as a candidate for a new feature boundary or propose a new folder-layout rule explicitly.

When touching existing features, use current project drift as cleanup guidance:

- Flat `Runtime/Logic` folders with many files, such as `Frontier`, `Settlement`, or `Progression`, should be split into buckets as touched.
- Existing `Spawning/` folders that contain `*Factory` should move touched factories to `Factories/`. True spawn orchestration belongs in `Domain/` or in a future documented `Spawners/` rule.
- Existing script `Resources/` folders under logic or presentation should become `WorldResources/` when touched.
- `Presentation/Presentation` should become `ViewParts/` or `Views/` depending on the contained type.

Rules:

- Keep one top-level type per `.cs` file and match file names to type names.
- Add ordinary gameplay features in their own `StaticMlp.Features.FeatureX` asmdef.
- Put shared contracts only in `Game.Core` when they are truly cross-cutting bootstrap/game contracts.
- Do not place Unity presentation code, view components, or client-only visuals in `Runtime/Logic`.
- Do not place gameplay rules, replicated mutation, server authority, or validation logic in `Runtime/Presentation`.
- Keep domain definitions free from network ids, prefab/view paths, transport state, and concrete UI concerns.

## System Design

Use ECS data flow rather than service calls:

- Systems communicate through event components and component state.
- Client gameplay systems query `LocalOwned`.
- Remote client presentation systems query `RemoteOwned` and render/smooth only; they do not simulate gameplay.
- Server gameplay systems query `ServerOwned` or validated `ClientOwned`.
- Add feature systems through the feature's `GameplayFeature` entry point.
- Do not edit global bootstrap classes unless the architecture explicitly owns the composition there.
- Gameplay and presentation behavior should live in systems, not in free-floating static classes.
- Systems must have one clear responsibility and stay short enough that their ECS query/event flow is easy to read.
- If a system grows multiple responsibilities, split it into ordered systems that communicate through components/events.

Prefer explicit system names that reveal context and ownership:

- `Server<Feature><Action>System`
- `Client<Feature><Action>System`
- `<Feature>LogicFeature`
- `<Feature>PresentationFeature`

Avoid names that hide intent:

- `Helper`
- `Utility`
- `Manager` unless the surrounding architecture already uses that exact role.
- Generic static orchestration classes.
- Static classes that exist only to hold feature behavior outside the ECS system pipeline.

## Components, Events, And Resources

Use components for durable ECS state and tags for marker-only state.

Use event components for transient requests or facts that systems consume in the frame/update flow.

Use resources only for composition-owned global state that is intentionally world-scoped. Do not store feature-specific controllers, Unity objects, or bridge systems in ordinary gameplay resources by default.

Keep state shapes narrow. A `*State` component or `IResource` should have one owner, authority, lifecycle, writer, and UI section. Split it before adding fields from another feature, another mode, another replication cadence, or another presentation section.

For composite presentation, prefer owner feature read models and section states over one flattened screen state. The composite shell may assemble sections, but it should not become the source of truth for foreign feature data.

Required mutable state should already exist by architecture. Access it directly:

```csharp
ref var state = ref entity.Mut<MyState>();
var config = world.GetResource<MyConfig>();
var input = entity.Read<MyInput>();
```

Do not replace architectural guarantees with lazy state creation:

```csharp
// Wrong by default: hides missing registration/spawn/setup bugs.
if (!entity.Has<MyState>())
{
    entity.Set(new MyState());
}
```

Only validate at trust boundaries: player input, client network events, external data/config, optional gameplay states, and real domain rules.

## Domain Rules

Extract rules only when they are shared, stable, and testable without presentation/transport state.

Allowed shape:

```text
Runtime/Logic/Domain/<Feature>Rules.cs
```

The rule type must not know about `NetworkDriver`, packets, prefabs, UI controllers, scene objects, or concrete ECS world composition.

If a rule is only used by one system and is easier to understand inline, keep it in the system.

## Spawning

Use explicit spawner/factory boundaries when entity creation has enough structure to name.

Rules:

- Create `NetEntity` only through `NetEntityFactory`.
- The factory role must be named `Factory` at API/composition level, with local casing like `factory`, `_factory`, or `Factory`.
- Do not hand-assemble network entities in unrelated systems.
- Do not store `Entity` as a persistent reference after spawning; store `EntityGID`.

## Fail-Fast Policy

Invalid ECS architecture state should crash loudly:

- Missing required resource: direct `GetResource<T>()` or throw an explicit exception at the composition boundary.
- Missing required component: direct `Read<T>()` or `Mut<T>()`.
- Missing required entity reference: direct `Unpack` where guaranteed.
- Missing required Unity binding: throw in the binding path; do not silently skip rendering or input.

Do not write `Ensure*`, `Require*`, `TryGetOrCreate*`, scene search, or null-guard helpers for required runtime state.

## StaticEcs Notes

- Register every component/tag/event/link type between `Create()` and `Initialize()`.
- Prefer `W.Types().RegisterAll(...)` when the local codebase uses it.
- Call `W.Tick()` once after systems update.
- Default query mode is Strict. Do not mutate filtered component/tag types on other entities while iterating.
- During `ForParallel`, modify only the current entity and avoid structural changes.
- Use `EntityGID` for persistent references.

## Before Finishing

Check:

- The code respects `Logic` versus `Presentation`.
- Any new or touched `*State` remains cohesive by owner, lifecycle, writer, and UI section.
- Required state fails fast instead of being hidden behind guards.
- No new mutable runtime state lives in a `static class`.
- No `.Generated.cs` files were edited manually.
- No prefabs or Unity assets were generated.
- The feature entry point registers the new systems/events.
- The user is asked to run Unity/build checks if compilation verification is needed.
