---
name: staticmlp-ui-mvc
description: Use when changing UI, MVC windows, presentation systems, view synchronization, MonoBehaviour view classes, PrefabXML usage, Canvas wiring, or client-only Unity presentation in StaticMlp.
---

# StaticMlp UI/MVC

Use this skill for UI, MVC, presentation systems, view sync, MonoBehaviour, PrefabXML, Canvas, and client-only Unity-facing changes.

## Required Context

Read these first when relevant:

- `AGENTS.md`
- `.agents/skills/staticmlp-code-writing/SKILL.md`
- `ai/mvc_usage_guidelines.md`
- `ai/mvc_package_review.md`
- `ai/static_ecs_view_feature.md`
- `ai/input_feature.md`
- `ai/prefabxml/SKILL.md` when serializing or reading PrefabXML.

## Presentation Boundary

Presentation code is client-only and must not own gameplay truth.

Use the feature folder-layout rule in `.agents/skills/staticmlp-code-writing/SKILL.md` for all presentation files. In particular:

- view-state components go in `Components/`;
- client visual systems go in `Systems/Client`;
- MVC controllers go in `Controllers/`;
- passive view classes go in `Views/`;
- `IEntityViewPart`/MonoBehaviour view parts go in `ViewParts/`;
- presentation `IResource` types go in `WorldResources/`, not `Resources/`.

Allowed in `Runtime/Presentation`:

- view state components/tags;
- client visual systems;
- MVC controller/window lifecycle;
- Unity view bindings;
- smoothing/interpolation of remote state;
- forwarding user intent into typed ECS events or registered network commands.

Not allowed in `Runtime/Presentation`:

- server authority logic;
- replicated component mutation that belongs to gameplay authority;
- transport calls or packet serialization;
- gameplay validation;
- durable gameplay rules;
- scene search composition that replaces explicit setup.

## MonoBehaviour Policy

Feature `MonoBehaviour` classes must stay view-only:

- inspector references;
- Unity callbacks;
- passive rendering methods;
- input forwarding;
- animation/audio/visual glue.

They must not become:

- ECS hosts;
- MVC hosts;
- composition roots;
- bootstrap systems;
- gameplay/presentation orchestrators;
- service locators.

If a `MonoBehaviour` needs to trigger gameplay, it should forward intent to the owning controller/system boundary. The ECS/bootstrap side owns lifecycle and composition.

## Inspector References

Required UI references are mandatory state:

- wire them explicitly in the Unity Editor;
- fail fast if missing in the binding path;
- do not hide missing references behind null guards;
- do not add `ValidateReferences` methods that search children or the scene;
- do not use `FindObjectOfType`, hierarchy walks, or name-based scene search to compensate for missing wiring.

Null UI references are bugs.

## MVC Lifecycle

MVC lifecycle belongs to ECS/bootstrap/composition-owned systems, not random view classes.

When adding a window/controller:

1. Identify the composition-owned MVC manager/resource that already exists.
2. Define the controller/window/view state in the presentation module.
3. Register/open/close through a presentation feature/system.
4. Keep gameplay requests as typed events or network commands.
5. Keep view rendering passive and deterministic from state/controller input.

Do not store feature-specific controllers or Unity objects in ordinary gameplay resources unless there is a clear project-wide composition contract.

## Composite UI

A screen may contain sections from multiple features, but each section should read state owned by the feature that owns the data. Use owner-provided contracts, projected read models, or feature-local presentation resources; do not make the screen depend on foreign `*.Logic` internals.

If a composite screen needs new data, add a narrow read model or presentation state in the owner feature first, then let the shell/controller assemble sections. Do not keep adding unrelated fields to one flattened `*State` resource.

Split screen state when fields belong to different owners, modes, write systems, or UI sections. Mode-specific state should be separate, for example a building context state and a worker context state instead of one nullable/flag-heavy panel state.

## UI To Gameplay Flow

For local UX that changes gameplay:

```text
Unity UI intent
-> view/controller forwarding
-> local ECS event/request
-> client ownership check or network command
-> server validation when command crosses trust boundary
-> gameplay state mutation
-> replicated state
-> presentation view update
```

Do not mutate server-authoritative gameplay state directly from UI.

If the client owns the state, the client-core system should query `LocalOwned`.

If the client does not own the state, send a typed replicated event and validate on the server.

## Remote View Sync

Remote presentation should query `RemoteOwned` entities and render/smooth replicated state.

It should not:

- simulate gameplay;
- infer authoritative decisions from visuals;
- write replicated gameplay state;
- create local fallbacks for missing replicated data.

View-only interpolation/smoothing state belongs in presentation components.

## Prefab And Canvas Authoring

AI agents must not create prefab assets.

Do not:

- generate `.prefab` files;
- patch prefab YAML;
- create editor scripts that build prefabs;
- launch Unity in BatchMode;
- assemble Canvas hierarchies through code.

Humans assemble prefabs, Canvas hierarchy, and inspector references in the Unity Editor.

PrefabXML may be used only as project documentation/serialization workflow according to `ai/prefabxml/SKILL.md`. When using PrefabXML, serialize enum values as numbers.

## Fail-Fast UI Code

Prefer direct required access:

```csharp
_window.Open(model);
_label.text = value;
```

Do not silently skip required UI work:

```csharp
// Wrong by default: hides broken composition or inspector wiring.
if (_label == null)
{
    return;
}
```

If the UI can legitimately be absent because the feature is not installed, do not register the presentation system in that context.

## Before Finishing

Check:

- No gameplay rules moved into `Runtime/Presentation`.
- MonoBehaviours remain passive views.
- Required UI references fail fast.
- MVC lifecycle is owned by ECS/bootstrap/presentation systems.
- Composite screens read owner feature contracts/read models instead of foreign logic internals.
- Screen `*State` resources are split by owner, mode, and UI section instead of flattened across features.
- UI intent crosses into gameplay through typed events or network commands.
- No prefab assets, prefab YAML, or Canvas hierarchies were generated.
- The user is asked to verify Unity scene/prefab wiring when needed.
