---
name: staticmlp-ui-mvvm
description: Use when changing StaticMlp UI, Aspid.MVVM ViewModels, Aspid.StaticEcs bindings, Aspid.StaticEcs.Windows windows, presentation systems, view synchronization, MonoBehaviour view classes, PrefabXML, Canvas wiring, or other client-only Unity presentation.
---

# StaticMlp UI/MVVM

Use this skill for StaticMlp UI and client-only presentation work built on:

```text
feature-owned ECS presentation state
-> Aspid.StaticEcs EcsLinkRegistry<TWorld>
-> Aspid.MVVM generated ViewModel
-> Aspid.StaticEcs.Windows shell and slot binding
-> Unity View / MonoBinder rendering
```

Do not recreate the old MVC/controller bridge under MVVM names.

## Required Context

Read these first when relevant:

- `CLAUDE.md`
- `.claude/skills/staticmlp-code-writing/SKILL.md`
- `.agents/skills/aspid-mvvm-skill/SKILL.md`
- `ai/refactor/mvvm-static-ecs/00_context_handoff.md`
- `ai/refactor/mvvm-static-ecs/01_architecture_decision.md`
- `ai/static_ecs_view_feature.md`
- `ai/input_feature.md`
- `ai/prefabxml/SKILL.md` when serializing or reading PrefabXML.

## Presentation Boundary

Presentation code is client-only and must not own gameplay truth.

Use the feature folder-layout rule in `.claude/skills/staticmlp-code-writing/SKILL.md`:

- view-state components and UI read-model components go in `Components/`;
- presentation `IResource` types go in `WorldResources/`, not `Resources/`;
- ViewModels go in `ViewModels/`;
- passive Unity views go in `Views/` or the established feature presentation folder;
- `IEntityViewPart`/MonoBehaviour view parts go in `ViewParts/`;
- client visual, input-intent, and read-model writer systems go in `Systems/Client`;
- legacy `Controllers/` folders are not the target pattern for new UI.

Allowed in `Runtime/Presentation`:

- view state components/tags and feature-owned UI read models;
- client visual systems and view sync;
- Aspid.MVVM ViewModels, generated bindable fields, and relay commands;
- Aspid.StaticEcs binding registration;
- Aspid.StaticEcs.Windows window, slot, shell, open/close request wiring;
- Unity view bindings;
- forwarding user intent into typed ECS events or registered network commands.

Not allowed in `Runtime/Presentation`:

- server authority logic;
- replicated component mutation that belongs to gameplay authority;
- transport calls or packet serialization;
- gameplay validation;
- durable gameplay rules;
- scene search composition that replaces explicit setup.

## MVVM Model Source

The model side for UI is feature-owned ECS presentation state.

For entity-backed windows:

```text
window input or presentation state contains EntityGID
-> WindowsController creates the ViewModel
-> RegisterLinkedViewModel attaches it to the ECS entity
-> EcsLinkRegistry bindings update generated ViewModel properties
```

For global HUD/popups:

```text
feature creates or owns a presentation entity
-> feature systems write presentation components/tags
-> window ViewModel links to that entity
-> EcsLinkRegistry sync updates the ViewModel
```

Do not add final `...ViewModelSyncSystem` classes that manually copy ECS state into a ViewModel every frame. If such a class exists, treat it as migration debt unless the task explicitly asks for a temporary step.

## ViewModel Rules

Use Aspid.MVVM generated ViewModels:

```csharp
using Aspid.MVVM;

[ViewModel]
public sealed partial class SomeWindowViewModel
{
    [OneWayBind] private string _summary;
    [OneWayBind] private bool _confirmInteractable;

    public void Apply(in SomeViewData data)
    {
        Summary = data.Summary;
        ConfirmInteractable = data.CanConfirm;
        ConfirmCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm()
    {
        CW.SendEvent(new SomeConfirmIntent());
    }

    private bool CanConfirm()
    {
        return ConfirmInteractable;
    }
}
```

Allowed in ViewModels:

- `[ViewModel] sealed partial` classes;
- `[OneWayBind]`, `[TwoWayBind]`, `[OneTimeBind]`, and `[OneWayToSourceBind]` fields;
- `[RelayCommand]` methods for UI commands;
- small formatting of already-owned presentation/read-model data;
- sending typed local UI intent events.

Not allowed in ViewModels:

- `WindowsController`;
- window stack logic;
- Unity `GameObject`, `MonoBehaviour`, or view references;
- `Mut<T>()`, `ReplicationMut.Mut<T>()`, `ClientProjection.Mut<T>()`, or gameplay state writes;
- gameplay validation or server authority decisions;
- direct controller/factory behavior copied from old MVC.

## Aspid.StaticEcs Binding

Register bindings once during client presentation feature setup:

```csharp
var registry = CW.GetResource<EcsLinkRegistry<ClientCoreWT>>();
registry.RegisterComponent<SomeWindowViewModel, SomeViewData>(
    static (vm, in data) => vm.Apply(in data));
```

Component bindings require `IComponent`, `ITrackableAdded`, and `ITrackableChanged`.
Tag bindings require `ITag`, `ITrackableAdded`, and `ITrackableDeleted`.
Multi bindings should read `Multi<T>` rows by reference.

`EcsLinkSyncSystem<ClientCoreWT>` must run after presentation/read-model writer systems and before the view needs rendered values.

## Aspid.StaticEcs.Windows

Use Aspid.StaticEcs.Windows for lifecycle and shell binding:

- define one `IEcsWindow` marker per window;
- define one `IEcsWindowSlot` marker per slot/section;
- use `IEcsWindowOpenRequest<TWindow, TInput>` and `IEcsWindowCloseRequest<TWindow>` for typed open/close requests when UI flow needs events;
- register shell factories through `WindowsController<TWorld>.RegisterWindow`;
- register ECS-linked slot ViewModels through `RegisterLinkedViewModel` when the ViewModel should track an ECS entity or presentation entity;
- use `RegisterViewModel` only for ViewModels that do not need ECS linking.

Window lifecycle belongs to ECS/bootstrap/presentation systems. Feature MonoBehaviours and ViewModels do not own it.

## Unity Views And Binders

Views are passive Unity bindings:

```csharp
using Aspid.MVVM;
using Aspid.MVVM.StarterKit;
using UnityEngine;

[View]
public sealed partial class SomeWindowView : MonoView, IView<SomeWindowViewModel>
{
    [RequireBinder(typeof(string))]
    [SerializeField] private MonoBinder[] _summary;

    [SerializeField] private ButtonCommandBinder[] _confirmCommand;
}
```

Rules:

- serialized binder fields must match generated ViewModel members;
- use `MonoBinder[]` when several UI objects consume the same property or command;
- use StarterKit binders before writing custom binders;
- write custom binders only for real component-specific behavior;
- keep `MonoBehaviour` view classes view-only: inspector references, Unity callbacks, passive rendering, and forwarding UI intent.

## Composite UI

A screen may aggregate sections from several features, but each section must read state owned by the feature that owns the data.

Use owner-provided presentation components, contracts, projected read models, or section ViewModels. Do not make a composite HUD depend on foreign `*.Logic` internals for convenience.

Split screen state when fields belong to different owners, modes, write systems, or UI sections. Avoid one flattened `*State` or one giant ViewModel that mixes settlement, loadout, frontier, progression, and worker ownership.

## UI To Gameplay Flow

For local UX that changes gameplay:

```text
Unity UI event
-> Aspid.MVVM RelayCommand
-> typed local ECS intent/request
-> client ownership check or network command
-> server validation when crossing trust boundary
-> owner gameplay system mutates owner state
-> replicated/presentation state changes
-> EcsLinkRegistry updates ViewModel
-> View binders render
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

Humans assemble prefabs, Canvas hierarchy, binders, and inspector references in the Unity Editor.

PrefabXML may be used only as project documentation/serialization workflow according to `ai/prefabxml/SKILL.md`. When using PrefabXML, serialize enum values as numbers.

## Fail-Fast UI Code

Required UI references are mandatory state:

- wire them explicitly in the Unity Editor;
- fail fast if missing in the binding path;
- do not hide missing references behind null guards;
- do not add `ValidateReferences` methods that search children or the scene;
- do not use `FindObjectOfType`, hierarchy walks, or name-based scene search to compensate for missing wiring.

Null UI references are bugs.

## Before Finishing

Check:

- No gameplay rules moved into `Runtime/Presentation`.
- No old MVC/controller bridge was recreated under MVVM names.
- No final per-window `...ViewModelSyncSystem` was added.
- ViewModels use Aspid.MVVM generated properties and commands.
- ECS data reaches ViewModels through `EcsLinkRegistry` bindings.
- Window lifecycle is owned by `WindowsController` and presentation systems.
- MonoBehaviours remain passive views.
- Required UI references fail fast.
- Composite screens read owner feature contracts/read models instead of foreign logic internals.
- UI intent crosses into gameplay through typed events or network commands.
- No prefab assets, prefab YAML, Canvas hierarchies, or `.meta` files were generated or patched.
- The user is asked to verify Unity scene/prefab wiring when needed.
