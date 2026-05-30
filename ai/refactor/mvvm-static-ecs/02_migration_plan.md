# Migration Plan: Feature Views To Aspid.StaticEcs MVVM

## Objective

Refactor the remaining StaticMlp window Views and ViewModels so they use:

- Aspid.MVVM generated ViewModels;
- Aspid.StaticEcs ECS-to-ViewModel bindings;
- Aspid.StaticEcs.Windows lifecycle and shell contracts;
- feature-owned ECS presentation/read-model state.

Do not preserve old MVC compatibility for its own sake.

## Hard Rules

- Do not restore `EcsWindowPresentationBridgeSystem`.
- Do not copy old controller logic into ViewModels.
- Do not create feature-specific final `...ViewModelSyncSystem` classes.
- Do not let ViewModels mutate gameplay state through `Mut<T>()`.
- Do not let ViewModels own window lifecycle.
- Do not generate or patch Unity prefab assets.
- Do not launch Unity BatchMode or run `dotnet build`.

## Phase 0: Package Integration

Goal: make package APIs support window ViewModels linked to ECS entities without
double ownership.

Tasks:

1. Add `EcsLinkRegistry<TWorld>.Attach<TViewModel>(entity, viewModel, disposeViewModel = false)`.
2. Add window integration API if required, for example `WindowsController.RegisterLinkedViewModel(...)`.
3. Ensure linked window slots dispose their `EcsLink` when the slot ViewModel is disposed.
4. Add `Aspid.StaticEcs` reference to `Aspid.StaticEcs.Windows.asmdef` only if the window package needs to call `EcsLinkRegistry<TWorld>`.
5. Register `EcsLinkRegistry<ClientCoreWT>` and `EcsLinkSyncSystem<ClientCoreWT>` in client presentation bootstrap.

Verification:

```powershell
rg "Attach<" Assets/Plugins/Aspid/StaticEcs/Runtime Assets/Plugins/Aspid/StaticEcs/Windows/Runtime
rg "EcsLinkSyncSystem<ClientCoreWT>" Assets/Scripts/StaticMlp Assets/Plugins/Aspid
git diff --check
```

Unity compile/import is verified by the user in Editor.

## Phase 1: LoadoutPreparation

Goal: rewrite the current LoadoutPreparation work to use `Aspid.StaticEcs`.

Current anti-pattern to remove if present:

```text
ClientLoadoutPreparationViewModelSyncSystem
```

Target shape:

1. Create a feature-owned ECS presentation component, for example:

```csharp
public struct LoadoutPreparationViewData
    : IComponent, ITrackableAdded, ITrackableChanged
{
    public bool IsAvailable;
    public bool CanPrepareBoss;
    public bool IsBossCommitted;
    public LoadoutModuleId SelectedPrimaryModuleId;
}
```

2. Create or identify the presentation entity that owns this component.
3. Feature system updates `LoadoutPreparationViewData` from owned/read-model sources.
4. Register binding:

```csharp
registry.RegisterComponent<LoadoutPreparationViewModel, LoadoutPreparationViewData>(
    static (vm, in data) => vm.Apply(in data));
```

5. `LoadoutPreparationViewModel` stays `[ViewModel] sealed partial`.
6. Commands remain `[RelayCommand]` and send typed intents:

```text
LoadoutPreparationSelectModuleIntent
LoadoutPreparationConfirmIntent
LoadoutPreparationCloseIntent
```

7. `LoadoutPreparationView` remains passive.

Done when:

```powershell
rg "LoadoutPreparationScreenState" Assets/Scripts/StaticMlp/Features/Loadout
rg "ClientLoadoutPreparationViewModelSyncSystem" Assets/Scripts/StaticMlp/Features/Loadout
```

return zero matches.

## Phase 2: Simple Read-Only Windows

Migrate first because they validate the package pattern with low risk.

### ThreatBanner

Files:

- `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Presentation/ViewModels/ThreatBannerViewModel.cs`
- `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Presentation/ThreatBannerView.cs`
- `Assets/Scripts/StaticMlp/Features/Frontier/Runtime/Presentation/ThreatBannerBridgeSystem.cs`

Target:

- `[ViewModel] sealed partial ThreatBannerViewModel`
- one presentation component/tag source;
- no commands unless the existing UI has an explicit interaction;
- binding registered through `EcsLinkRegistry`.

### ResourcesInventoryHud

Files:

- `Assets/Scripts/StaticMlp/Features/ResourcesInventoryMinimal/Runtime/Presentation/ViewModels/ResourcesInventoryHudViewModel.cs`
- `Assets/Scripts/StaticMlp/Features/ResourcesInventoryMinimal/Runtime/Presentation/Views/ResourcesInventoryHudView.cs`
- `Assets/Scripts/StaticMlp/Features/ResourcesInventoryMinimal/Runtime/Presentation/Systems/Client/ResourcesInventoryHudBridgeSystem.cs`

Target:

- ViewModel exposes generated resource labels/counts or a small read-only row model.
- Inventory feature owns the presentation component.
- No manual per-frame ViewModel sync.

## Phase 3: Popup With Command

### RewardResultPopup

Files:

- `Assets/Scripts/StaticMlp/Features/Progression/Runtime/Presentation/ViewModels/RewardResultPopupViewModel.cs`
- `Assets/Scripts/StaticMlp/Features/Progression/Runtime/Presentation/Views/RewardResultPopupView.cs`
- `Assets/Scripts/StaticMlp/Features/Progression/Runtime/Presentation/RewardResultPopupBridgeSystem.cs`

Target:

- ViewModel fields: title, body, reward labels, button state.
- Command: close/continue/claim, depending on current UI.
- Command sends typed local intent.
- Existing owner system handles close/request logic.

## Phase 4: Entity-Backed Windows

These should use window input or a presentation resource to resolve the linked
entity.

### BuildingMenu

Files:

- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Presentation/ViewModels/BuildingMenuViewModel.cs`
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Presentation/BuildingMenuView.cs`
- `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Systems/Client/ClientBuildingMenuBridgeSystem.cs`

Target:

- VM has generated fields for selected building, labels, availability.
- Commands send building menu intents.
- Building feature system applies state changes.
- No cross-feature mutation from VM.

### InteractionPrompt

Files:

- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/ViewModels/InteractionPromptViewModel.cs`
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/InteractionPromptView.cs`
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Systems/InteractionPromptBridgeSystem.cs`

Target:

- VM fields: prompt text, action text, visible/interactable.
- Command sends interaction intent if the prompt has a button/key action.
- Bind to selected/interactable entity or a dedicated prompt presentation entity.

## Phase 5: Settlement Panels

These are more complex and should be migrated after simple windows prove the
package path.

### SettlementContextPanel

Files:

- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/ViewModels/SettlementContextPanelViewModel.cs`
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/SettlementContextPanelView.cs`
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Systems/SettlementContextPanelCompositeBridgeSystem.cs`

Target:

- Split presentation state by owner/mode.
- Do not make one flattened nullable context state.
- VM exposes only current panel presentation fields and commands.
- Commands send typed intents.

### BuildingManagementPanel

Files:

- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/ViewModels/BuildingManagementPanelViewModel.cs`
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/BuildingManagementPanelView.cs`
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Systems/BuildingManagementPanelBridgeSystem.cs`

Target:

- VM fields: selected building, upgrade/repair/assign labels, availability, costs, warnings.
- Commands send typed intents.
- Owner systems perform ECS mutation.

## Phase 6: SettlementHud Composite

Files:

- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/ViewModels/SettlementHudViewModel.cs`
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/SettlementHudView.cs`
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Systems/SettlementHudCompositeBridgeSystem.cs`

Target:

- Do this last.
- Avoid a giant ViewModel that repeats the old `Stage1HudState` problem.
- Prefer owner-scoped presentation components linked into section ViewModels.
- If the current shell has one slot only, either add section slots in package/UI authoring later or keep one VM with clearly separated owner-owned apply methods as an interim step.
- Do not introduce new cross-feature reads to make the composite easy.

Related debt:

- `ai/refactor/settlement/06_problem_presentation_state_flattening.md`

## Final Verification

Run:

```powershell
rg "EcsWindowPresentationBridgeSystem" Assets/Scripts/StaticMlp/Features
rg "EcsWindowViewModelBase" Assets/Scripts/StaticMlp/Features
rg "ViewModelSyncSystem" Assets/Scripts/StaticMlp/Features
rg "StaticMlp.Features.MvvmWindows|StaticMlpWindowViewBase|StaticMlpViewModelBase" Assets/Scripts/StaticMlp/Features
git diff --check
```

Check asmdef JSON:

```powershell
Get-ChildItem Assets -Filter *.asmdef -Recurse |
    ForEach-Object { Get-Content $_.FullName -Raw | ConvertFrom-Json | Out-Null }
```

Check missing/orphan metas as appropriate.

Unity compile/import is run by the user in Editor.

