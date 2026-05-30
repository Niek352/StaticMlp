# Execution Summary: 01 Architecture Decision

## Tasks Completed

- Added `EcsLinkRegistry<TWorld>.Attach<TViewModel>(...)` for linking existing ViewModels to ECS entities without default ViewModel ownership.
- Added `WindowsController<TWorld>.RegisterLinkedViewModel(...)` so window-owned slot ViewModels can be attached to `EcsLinkRegistry<TWorld>` and unlink on slot disposal.
- Added missing window package support systems used by the migrated windows:
  - `PersistentEcsWindowHostSystem`
  - `StateDrivenEcsWindowHostSystem`
  - `EcsWindowOpenInputBindings.Ignore`
- Registered `EcsLinkRegistry<ClientCoreWT>` during client startup and `EcsLinkSyncSystem<ClientCoreWT>` before view presentation application.
- Migrated feature window registrations from manual `RegisterViewModel` to linked ViewModels backed by singleton presentation entities.
- Replaced feature systems that copied ECS/resource state directly into ViewModels with systems that write feature-owned presentation components.
- Removed the remaining `ClientLoadoutPreparationViewModelSyncSystem` pattern and moved Loadout to `LoadoutPreparationViewData`.
- Updated editor tests for `Attach` and linked window slot lifecycle.

## Deviations

- The plan file is an architecture decision rather than a structured `<tasks>` plan, so execution used the concrete package API, registration, and verification sections as the task list.
- Unity Editor compile/import was not run because project rules forbid agent-side `dotnet build` and Unity batchmode. Verification is limited to text checks, asmdef JSON parsing, and `git diff --check`.

## Verification

- `rg "EcsWindowPresentationBridgeSystem" Assets/Scripts/StaticMlp/Features`: zero matches.
- `rg "EcsWindowViewModelBase" Assets/Scripts/StaticMlp/Features`: zero matches.
- `rg "ViewModelSyncSystem" Assets/Scripts/StaticMlp/Features`: zero matches.
- `rg "StaticMlp.Features.MvvmWindows|StaticMlpWindowViewBase|StaticMlpViewModelBase" Assets/Scripts/StaticMlp/Features`: zero matches.
- `Get-ChildItem Assets -Filter *.asmdef -Recurse | ForEach-Object { Get-Content $_.FullName -Raw | ConvertFrom-Json | Out-Null }`: passed.
- `git diff --check`: passed; warnings were limited to existing git ignore permission and line-ending notices.
