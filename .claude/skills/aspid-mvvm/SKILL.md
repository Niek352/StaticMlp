---
name: aspid-mvvm
description: Use when writing, modifying, or reviewing AspidMVVM code — ViewModels, Views, MonoBinders, RelayCommands, observable collections, or MVVM binding wiring in Unity. Also trigger when the user mentions Aspid, AspidMVVM, MVVM binding, MonoViewModel, MonoView, MonoBinder, OneWayBind, TwoWayBind, RelayCommand, ViewModel source generator, or any Aspid.MVVM namespace usage. Trigger even if the user does not explicitly say "AspidMVVM" but is working with MVVM patterns, data binding, ViewModel/View pairs, or UI binder components in this project.
---

# AspidMVVM

AspidMVVM is a Unity MVVM framework with C# Source Generators. It eliminates boilerplate by generating `IViewModel` implementations, bindable properties, and `IView` wiring from attributes.

## Architecture Context

AspidMVVM lives in the **Presentation** layer. Follow `staticmlp-ui-mvc` skill boundaries:

- ViewModels and Views go in `Runtime/Presentation`.
- MonoBinders are passive view components; they read from ViewModels and forward user input.
- ViewModels are presentation state containers; they do not own gameplay truth.
- Gameplay requests from ViewModels go through typed ECS events or network commands, not direct gameplay mutation.

Do not place AspidMVVM ViewModels or Views in `Runtime/Logic`.

## Core Concepts

```
ViewModel (IViewModel) ──bind──> Binder (IBinder<T>) ──> Unity Component
       ^                                                            |
       └──────── reverse bind (IReverseBinder<T>) ───────────────────┘
```

- **ViewModel**: Owns presentation state. Marked `[ViewModel]`, must be `partial`. SG generates properties, setters, and change events from attributed fields.
- **View**: Owns binder references and lifecycle. Marked `[View]`, must be `partial`, inherits `MonoView`. SG generates `Initialize`/`Deinitialize` that auto-binds/unbinds all `MonoBinder` fields.
- **Binder**: Bridges ViewModel property ↔ Unity component. Inherits `MonoBinder` (or `ComponentMonoBinder<T>`). Implements `IBinder<T>` to receive VM→View updates. Optionally implements `IReverseBinder<T>` to send View→VM updates.
- **Command**: `RelayCommand` (or `[RelayCommand]`-generated) encapsulates user actions. Binds to buttons via `IBinder<IRelayCommand>` binders.

## Source Generator Rules

The SG is strict. Violations produce build errors.

1. `[ViewModel]` classes must be `partial`.
2. `[View]` classes must be `partial` and inherit from `MonoView` (or implement `IView` manually).
3. Field names in View must match ViewModel field names, ignoring prefixes: `_field`, `m_field`, `s_field`, and `field` are equivalent.
4. One binder field can bind to one ViewModel member. Arrays of binders (`MonoBinder[]`) bind multiple UI elements to the same member.
5. `[RelayCommand]` methods generate read-only command properties (only `OneWay`/`OneTime` bind modes).

## ViewModel

### Basic ViewModel

```csharp
using Aspid.MVVM;

[ViewModel]
public sealed partial class HealthBarViewModel : IDisposable
{
    // Generates: public int CurrentHealth { get; }
    // Generates: public void SetCurrentHealth(int value)
    // Generates: public event Action<int> CurrentHealthChanged
    [OneWayBind] private int _currentHealth;

    // Generates: public int MaxHealth { get; }
    // Generates: public void SetMaxHealth(int value)
    // Generates: public event Action<int> MaxHealthChanged
    [OneWayBind] private int _maxHealth;

    // Generates: public IRelayCommand HealCommand { get; }
    [RelayCommand]
    private void Heal()
    {
        // forward to gameplay via event or command
    }

    public void Dispose() { }
}
```

### Two-Way Binding

```csharp
[ViewModel]
public sealed partial class SettingsViewModel : IDisposable
{
    // TwoWay: View can both read and write this value.
    [TwoWayBind] private string _playerName;

    // TwoWay with bool
    [TwoWayBind] private bool _soundEnabled;

    public void Dispose() { }
}
```

### Command with CanExecute

```csharp
[ViewModel]
public sealed partial class CraftViewModel : IDisposable
{
    [OneWayBind] private bool _canCraft;

    // The SG looks for a method named CanCraft() returning bool.
    [RelayCommand(CanExecute = nameof(CanCraft))]
    private void Craft() { }

    // Generated: IRelayCommand CraftCommand { get; }
    // CanCraft() is called automatically by the SG-generated command.

    public void Dispose() { }
}
```

### BindId for Custom Names

When ViewModel field name and View binder field name cannot match:

```csharp
[ViewModel]
public sealed partial class ShopViewModel : IDisposable
{
    [BindId("GoldAmount")]
    [OneWayBind] private int _currency;

    public void Dispose() { }
}
```

View side: `[SerializeField] private MonoBinder _goldAmount;` (matches the BindId).

## View

### Basic View

```csharp
using Aspid.MVVM;
using UnityEngine;

[View]
public sealed partial class HealthBarView : MonoView
{
    [RequireBinder(typeof(int))]
    [SerializeField] private MonoBinder _currentHealth;

    [RequireBinder(typeof(int))]
    [SerializeField] private MonoBinder _maxHealth;

    [RequireBinder(typeof(IRelayCommand))]
    [SerializeField] private MonoBinder _healCommand;
}
```

The SG generates:
- `public IViewModel? ViewModel { get; }`
- `public void Initialize(IViewModel viewModel)` — binds all MonoBinder fields
- `public void Deinitialize()` — unbinds all MonoBinder fields
- `public void Initialize(HealthBarViewModel viewModel)` — strongly-typed overload

### Multiple Binders for One Member

```csharp
[View]
public sealed partial class PartyView : MonoView
{
    // Array binds multiple UI elements to the same ViewModel member.
    [RequireBinder(typeof(int))]
    [SerializeField] private MonoBinder[] _memberHealth;
}
```

### View Lifecycle

```csharp
// Create and initialize
var viewModel = new HealthBarViewModel();
var view = Instantiate(healthBarPrefab);
view.Initialize(viewModel);

// Cleanup
view.Deinitialize();
viewModel.Dispose();
```

`MonoView` calls `Deinitialize()` automatically in `OnDestroy`.

## Custom MonoBinder

Create a binder when the StarterKit does not cover a component.

### One-Way Binder (VM → View only)

```csharp
using Aspid.MVVM;
using Aspid.MVVM.StarterKit;
using UnityEngine;

[AddComponentMenu("Aspid/MVVM/Binders/UI/Custom/Progress Bar Binder")]
public sealed partial class ProgressBarMonoBinder : ComponentMonoBinder<ProgressBarUI>, IBinder<float>
{
    public void SetValue(float value)
    {
        CachedComponent.SetFill(value);
    }
}
```

### Two-Way Binder (VM ↔ View)

```csharp
using Aspid.MVVM;
using Aspid.MVVM.StarterKit;
using UnityEngine;

[AddComponentMenu("Aspid/MVVM/Binders/UI/Custom/Slider Binder")]
public sealed partial class CustomSliderMonoBinder : ComponentMonoBinder<Slider>,
    IBinder<float>, IReverseBinder<float>
{
    public event Action<float>? ValueChanged;

    protected override void OnBound()
    {
        if (Mode is BindMode.TwoWay or BindMode.OneWayToSource)
            CachedComponent.onValueChanged.AddListener(OnSliderChanged);
    }

    protected override void OnUnbound()
    {
        if (Mode is BindMode.TwoWay or BindMode.OneWayToSource)
            CachedComponent.onValueChanged.RemoveListener(OnSliderChanged);
    }

    public void SetValue(float value)
    {
        CachedComponent.SetValueWithoutNotify(value);
    }

    private void OnSliderChanged(float value) =>
        ValueChanged?.Invoke(value);
}
```

### Non-MonoBehaviour Binder (for code-instantiated bindings)

```csharp
using Aspid.MVVM;
using Aspid.MVVM.StarterKit;

public sealed class TextBinder : TargetBinder<TextMeshProUGUI>, IBinder<string>
{
    public TextBinder(TextMeshProUGUI target, BindMode mode = BindMode.OneWay)
        : base(target, mode) { }

    public void SetValue(string value)
    {
        Target.text = value;
    }
}
```

### Binder Mode Override

Force a binder to accept all modes (useful when the component inherently supports both directions):

```csharp
[BindModeOverride(IsAll = true)]
public sealed partial class MyBinder : ComponentMonoBinder<MyComponent>,
    IBinder<string>, IReverseBinder<string>
{
    // ...
}
```

## Bind Modes

| Mode | Direction | Use Case |
|------|-----------|----------|
| `OneWay` | VM → View | Labels, icons, health bars |
| `TwoWay` | VM ↔ View | Input fields, toggles, sliders |
| `OneTime` | VM → View once | Static labels, command buttons |
| `OneWayToSource` | View → VM | Rare; forms that only submit |
| `None` | — | Disabled |

Default mode on MonoBinder fields is `TwoWay`. Override in inspector or constructor.

## Commands

### Manual RelayCommand

```csharp
public sealed class MyViewModel : IDisposable, IViewModel
{
    public IRelayCommand ClickCommand { get; }

    public MyViewModel()
    {
        ClickCommand = new RelayCommand(OnClick, CanClick);
    }

    private bool CanClick() => _isEnabled;
    private void OnClick() { /* ... */ }

    public void Dispose() { }

    // Manual IViewModel implementation required when not using [ViewModel]
    public FindBindableMemberResult FindBindableMember(in FindBindableMemberParameters parameters)
    {
        // ... or let the SG handle it by marking the class [ViewModel]
    }
}
```

### SG-Generated Command

```csharp
[ViewModel]
public sealed partial class MyViewModel : IDisposable
{
    [RelayCommand]
    private void Close() { }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private void Submit() { }

    private bool CanSubmit() => _isValid;

    public void Dispose() { }
}
```

Notify command executability changed:

```csharp
_canSubmit = Validate();
SubmitCommand.NotifyCanExecuteChanged();
```

## Observable Collections

Use Aspid observable collections when binding lists or dictionaries to UI.

```csharp
using Aspid.Collections.Observable;

[ViewModel]
public sealed partial class InventoryViewModel : IDisposable
{
    // Generates property, change events, collection change events
    [OneWayBind] private ObservableList<ItemViewModel> _items;

    public void Dispose()
    {
        foreach (var item in _items)
            item.Dispose();
        _items.Clear();
    }
}
```

Available collections:
- `ObservableList<T>` / `IReadOnlyObservableList<T>`
- `ObservableDictionary<TKey, TValue>` / `IReadOnlyObservableDictionary<TKey, TValue>`
- `ObservableHashSet<T>`
- `ObservableQueue<T>`
- `ObservableStack<T>`

Synchronizers mirror one observable collection into another (useful for filtered views):
- `ObservableListSync<T>`
- `FilteredList<T>` / `IReadOnlyFilteredList<T>`

## Naming Conventions

### ViewModel Fields

```csharp
// All these generate a property named "Health"
[OneWayBind] private int _health;
[OneWayBind] private int m_health;
[OneWayBind] private int s_health;
[OneWayBind] private int health;   // discouraged but works
```

Generated members:
- Property: `public int Health { get; }`
- Setter: `public void SetHealth(int value)`
- Event: `public event Action<int> HealthChanged`

### View Binder Fields

View binder field names must match the ViewModel field name (ignoring prefix):

```csharp
// ViewModel
[OneWayBind] private int _health;

// View — all valid
[SerializeField] private MonoBinder _health;
[SerializeField] private MonoBinder m_health;
[SerializeField] private MonoBinder health;
```

Use the same prefix style as the ViewModel for readability.

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Class not `partial` | Add `partial` to `[ViewModel]` and `[View]` classes |
| `[View]` class does not inherit `MonoView` | Inherit `MonoView` or manually implement `IView` |
| Binder field name does not match ViewModel | Match names exactly (ignoring prefix) or use `[BindId]` |
| `[RelayCommand]` on non-void method | Commands must be `void` or `Task` (no return value) |
| Missing `IReverseBinder<T>` on two-way binder | Implement `IReverseBinder<T>` and raise `ValueChanged` |
| Subscribing to Unity events in constructor | Subscribe in `OnBound`, unsubscribe in `OnUnbound` |
| Forgetting `NotifyCanExecuteChanged()` | Call after state that affects `CanExecute` changes |
| Storing `IBinder` references across rebind | Binders are rebound per Initialize/Deinitialize cycle |

## Integration with StaticMlp ECS

AspidMVVM is **Presentation**. Gameplay state lives in ECS.

### ViewModel reads ECS state

```csharp
public sealed partial class UnitPanelViewModel : IDisposable
{
    [OneWayBind] private int _health;
    [OneWayBind] private string _unitName;

    private readonly EntityGID _entityGid;

    public UnitPanelViewModel(EntityGID entityGid)
    {
        _entityGid = entityGid;
        // subscribe to ECS changes and call SetHealth / SetUnitName
    }

    public void Dispose()
    {
        // unsubscribe from ECS
    }
}
```

### ViewModel forwards intent to ECS

```csharp
[ViewModel]
public sealed partial class BuildMenuViewModel : IDisposable
{
    [RelayCommand]
    private void SelectBuilding(BuildingType type)
    {
        // Send typed ECS event — do not mutate gameplay directly
        W.NewEvent(new SelectBuildingEvent { BuildingType = type });
    }

    public void Dispose() { }
}
```

The ViewModel is instantiated and managed by a presentation system (not a MonoBehaviour). The presentation system:
1. Creates the ViewModel from ECS state.
2. Instantiates the View prefab.
3. Calls `view.Initialize(viewModel)`.
4. Calls `view.Deinitialize()` and `viewModel.Dispose()` on cleanup.

## Before Finishing

Check:

- `[ViewModel]` and `[View]` classes are `partial`.
- View inherits `MonoView`.
- Binder field names match ViewModel field names (or `[BindId]` is used).
- Two-way binders implement `IReverseBinder<T>` and raise `ValueChanged`.
- Unity event subscriptions happen in `OnBound` / `OnUnbound`, not constructor.
- `RelayCommand.CanExecuteChanged` is notified when relevant state changes.
- ViewModels are disposed and views are deinitialized on cleanup.
- No gameplay rules or replicated state mutation in ViewModels.
- UI intent flows through typed ECS events or network commands, not direct mutation.
