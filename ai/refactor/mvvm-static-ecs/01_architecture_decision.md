# Architecture Decision: Aspid.StaticEcs MVVM Windows

## Decision

The MVVM migration must use `Aspid.StaticEcs` as the ECS-to-ViewModel binding
layer. Window feature code should not manually copy ECS state into ViewModels
every frame.

The target flow is:

```text
Feature-owned presentation ECS state
-> EcsLinkRegistry<TWorld>
-> Aspid.MVVM generated ViewModel
-> EcsWindowShellViewBase slot binding
-> View or MonoBinder rendering
```

## Package Responsibilities

### Aspid.StaticEcs

Assembly:

```text
Assets/Plugins/Aspid/StaticEcs/Runtime/Aspid.StaticEcs.asmdef
```

Current references:

```json
[
  "Aspid.MVVM",
  "FFS.StaticEcs"
]
```

This package owns the runtime bridge between ECS and ViewModels:

- `EcsLinkRegistry<TWorld>`
- `EcsLink<TWorld, TViewModel>`
- `EcsLinkSyncSystem<TWorld>`
- `EcsComponentBinding<TViewModel, TComponent>`
- `EcsTagBinding<TViewModel, TTag>`
- `EcsMultiBinding<TWorld, TViewModel, TElement>`

### Aspid.StaticEcs.Windows

Assembly:

```text
Assets/Plugins/Aspid/StaticEcs/Windows/Runtime/Aspid.StaticEcs.Windows.asmdef
```

This package owns window lifecycle and slot-to-ViewModel binding:

- `WindowsController<TWorld>`
- `EcsWindowRequestSystem<TWorld>`
- `IEcsWindow`
- `IEcsWindowSlot`
- `IEcsWindowOpenRequest<TWindow, TInput>`
- `IEcsWindowCloseRequest<TWindow>`
- `EcsWindowContext<TWorld, TWindow, TInput>`

### Aspid.StaticEcs.Windows.Unity

Assembly:

```text
Assets/Plugins/Aspid/StaticEcs/Windows/Unity/Aspid.StaticEcs.Windows.Unity.asmdef
```

This package owns Unity shell/view integration:

- `EcsWindowShellViewBase`
- `EcsWindowViewBase<TSlot, TViewModel>`
- prefab/resource shell factories

## Integration Gap To Fix

`EcsLinkRegistry<TWorld>.Create(...)` currently creates the ViewModel:

```csharp
registry.Create(entity, gid => new SomeViewModel(...));
```

`WindowsController<TWorld>.RegisterViewModel(...)` also creates the ViewModel:

```csharp
windows.RegisterViewModel<TWindow, TInput, TSlot, TViewModel>(
    factory,
    applyInput);
```

This creates an ownership mismatch for window ViewModels:

- `WindowsController` must create, bind, unbind, and dispose slot ViewModels.
- `EcsLinkRegistry` must link those ViewModels to ECS entities.
- The ViewModel must not be disposed twice.
- The ECS link must be disposed when the window slot ViewModel is disposed.

## Required Package API

Add a package-level way to attach an existing ViewModel to an ECS entity.

Preferred low-level API in `Aspid.StaticEcs`:

```csharp
public EcsLink<TWorld, TViewModel> Attach<TViewModel>(
    World<TWorld>.Entity entity,
    TViewModel viewModel,
    bool disposeViewModel = false)
    where TViewModel : class, IViewModel;
```

Semantics:

- `Attach` registers an existing ViewModel with the same binding machinery as `Create`.
- `disposeViewModel` defaults to `false` for window ViewModels because `WindowsController` owns them.
- Duplicate `(EntityGID, ViewModel type)` links fail fast.
- Initial bindings are applied immediately, same as `Create`.
- Dead entity cleanup still removes the link.

Then add window-level API in `Aspid.StaticEcs.Windows` if needed:

```csharp
public void RegisterLinkedViewModel<TWindow, TInput, TSlot, TViewModel>(
    Func<EcsWindowContext<TWorld, TWindow, TInput>, TViewModel> factory,
    Func<EcsWindowContext<TWorld, TWindow, TInput>, EntityGID> entityResolver,
    EcsWindowOpenInputBinding<TWorld, TWindow, TInput, TViewModel> applyInput)
```

This API should:

- create the ViewModel through the existing window lifecycle;
- resolve the entity from window input or a presentation resource;
- attach the created ViewModel to `EcsLinkRegistry<TWorld>`;
- store the returned `EcsLink` in the slot entry;
- dispose the link when the slot ViewModel is disposed.

If `Aspid.StaticEcs.Windows` needs to call `EcsLinkRegistry<TWorld>`, add an
assembly reference from `Aspid.StaticEcs.Windows` to `Aspid.StaticEcs`.
This is not a cycle because `Aspid.StaticEcs` does not reference Windows.

## ViewModel Rules

Each feature ViewModel should be:

```csharp
using Aspid.MVVM;

[ViewModel]
public sealed partial class SomeWindowViewModel
{
    [OneWayBind] private string _title;
    [OneWayBind] private bool _confirmInteractable;

    public void Apply(in SomePresentationState state)
    {
        Title = state.Title;
        ConfirmInteractable = state.CanConfirm;
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

Allowed in ViewModel:

- generated Aspid.MVVM fields and properties;
- command methods through `[RelayCommand]`;
- small intent methods for UI clicks;
- formatting of already-owned presentation/read-model data.

Not allowed in ViewModel:

- `WindowsController`;
- window stack logic;
- Unity view references;
- `Mut<T>()` or cross-feature state writes;
- gameplay validation;
- server authority logic;
- direct controller/factory behavior copied from old MVC.

## Feature Presentation State Rules

Feature systems should write feature-owned presentation ECS components, tags, or
multi components. Those are the model side of MVVM for UI.

For entity-backed windows:

```text
window input contains EntityGID
-> ViewModel links to that entity
-> component/tag/multi bindings update ViewModel
```

For global or composite windows:

```text
feature creates a presentation entity
-> feature systems write presentation components to that entity
-> ViewModel links to the presentation entity
-> bindings update ViewModel
```

Avoid one giant global screen state. Split presentation state by owner, mode,
and UI section.

## Required Runtime Registration

At client presentation bootstrap:

```csharp
CW.SetResource(new EcsLinkRegistry<ClientCoreWT>());
systems.Add(new EcsLinkSyncSystem<ClientCoreWT>(), order);
```

The sync system must run after systems that update presentation/read-model ECS
components and before rendering needs the bound values.

Each feature registers bindings once:

```csharp
var registry = CW.GetResource<EcsLinkRegistry<ClientCoreWT>>();
registry.RegisterComponent<SomeWindowViewModel, SomePresentationState>(
    static (vm, in state) => vm.Apply(in state));
```

Component bindings require:

```csharp
where TComponent : struct, IComponent, ITrackableAdded, ITrackableChanged
```

Tag bindings require:

```csharp
where TTag : struct, ITag, ITrackableAdded, ITrackableDeleted
```

## Verification

The final migration should satisfy:

```powershell
rg "EcsWindowPresentationBridgeSystem" Assets/Scripts/StaticMlp/Features
rg "EcsWindowViewModelBase" Assets/Scripts/StaticMlp/Features
rg "ViewModelSyncSystem" Assets/Scripts/StaticMlp/Features
rg "StaticMlp.Features.MvvmWindows|StaticMlpWindowViewBase|StaticMlpViewModelBase" Assets/Scripts/StaticMlp/Features
git diff --check
```

Expected result for the old bridge/base patterns is zero matches in feature
code.

