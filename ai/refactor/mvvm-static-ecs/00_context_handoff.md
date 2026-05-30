# MVVM StaticEcs Context Handoff

## Purpose

This handoff is for continuing the UI/MVVM migration in a fresh context window.
Do not continue the previous "rename MVC to ViewModel" direction.

The target architecture is:

```text
StaticEcs presentation/read-model component or tag
-> Aspid.StaticEcs EcsLinkRegistry<TWorld>
-> Aspid.MVVM generated ViewModel
-> Aspid.StaticEcs.Windows shell/view binding
-> Unity View or Aspid.MVVM binders
```

The important correction is that feature code should not manually sync window
ViewModels every frame through feature-specific bridge systems. The package
`Aspid.StaticEcs` exists specifically to connect ECS state to Aspid.MVVM
ViewModels.

## User Direction

- Remove the old MVC-style replacement mindset.
- Do not copy controller logic 1:1 into ViewModels.
- Use Aspid.MVVM ViewModels.
- Use `Aspid.StaticEcs` as the ECS-to-ViewModel binding layer.
- Use `Aspid.StaticEcs.Windows` for window shell lifecycle and slot binding.
- Start from `LoadoutPreparationView`, then migrate the remaining views.
- `LoadoutPreparationScreenState` should not remain as the UI state pattern.

## Relevant Files

- `Assets/Plugins/Aspid/StaticEcs/Runtime/Aspid.StaticEcs.asmdef`
- `Assets/Plugins/Aspid/StaticEcs/Runtime/EcsLinkRegistry.cs`
- `Assets/Plugins/Aspid/StaticEcs/Runtime/EcsLink.cs`
- `Assets/Plugins/Aspid/StaticEcs/Runtime/EcsLinkSyncSystem.cs`
- `Assets/Plugins/Aspid/StaticEcs/Runtime/EcsComponentBinding.cs`
- `Assets/Plugins/Aspid/StaticEcs/Runtime/EcsTagBinding.cs`
- `Assets/Plugins/Aspid/StaticEcs/Runtime/EcsMultiBinding.cs`
- `Assets/Plugins/Aspid/StaticEcs/Windows/Runtime/WindowsController.cs`
- `Assets/Plugins/Aspid/StaticEcs/Windows/Unity/EcsWindowShellViewBase.cs`
- `Assets/Plugins/Aspid/StaticEcs/Windows/Unity/EcsWindowViewBase.cs`
- `Assets/Scripts/StaticMlp/Features/Loadout/Runtime/Presentation/ViewModels/LoadoutPreparationViewModel.cs`

## Current Problem

The earlier migration created or encouraged code shaped like:

```text
feature system reads ECS
-> feature system gets ViewModel from WindowsController
-> feature system calls viewModel.Sync(...)
```

That is the wrong final architecture because it bypasses `Aspid.StaticEcs`.
It also recreates the old MVC bridge/controller shape under new names.

The file `ClientLoadoutPreparationViewModelSyncSystem.cs`, if present, should be
treated as an intermediate mistake, not as the pattern to copy.

## Correct Boundary

Feature systems may build presentation/read-model ECS state:

```text
gameplay/projection state
-> feature-owned presentation component/tag/multi
```

`Aspid.StaticEcs` owns binding from ECS state to ViewModel:

```text
component/tag/multi changed
-> EcsLinkRegistry<TWorld>
-> generated Aspid.MVVM ViewModel properties
```

Window systems own window lifecycle:

```text
open/close requests
-> WindowsController<TWorld>
-> shell creates/binds/unbinds ViewModel slots
```

ViewModels own presentation fields and UI commands only:

```text
generated properties
generated RelayCommand properties
typed UI intent events
```

Views own Unity references and passive rendering/binding only.

## Do Not Do

- Do not restore `EcsWindowPresentationBridgeSystem`.
- Do not create new per-window `...ViewModelSyncSystem` as the final pattern.
- Do not put `WindowsController`, `Mut<T>()`, factory logic, or window stack logic into ViewModels.
- Do not let ViewModels directly mutate gameplay or cross-feature state.
- Do not create local feature packages under `Assets/Scripts/StaticMlp/Features/MvvmWindows/`.
- Do not generate or patch Unity prefab assets.

## First Required Design Step

Before migrating the remaining windows, fix the package integration gap:

`WindowsController<TWorld>` creates and owns window slot ViewModels, while
`EcsLinkRegistry<TWorld>` currently creates and owns linked ViewModels.

There must be a package-level API that links an existing window ViewModel to an
ECS entity without double ownership. See `01_architecture_decision.md`.

