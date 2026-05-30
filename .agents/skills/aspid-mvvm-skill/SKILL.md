---
name: aspid-mvvm
description: >
  Generate and guide creation of MVVM components in Unity using the Aspid.MVVM
  framework. Use this whenever a user asks to create or modify ViewModels,
  Views, binders or commands in a Unity project, or requests guidance on
  data‑binding, command creation or MVVM patterns with Aspid.MVVM. The skill
  provides code skeletons, templates and clear instructions tailored to the
  user’s input so they can quickly integrate Aspid.MVVM into their workflow.
---

# Aspid.MVVM skill

This skill helps you build Model‑View‑ViewModel (MVVM) code for Unity using the
**Aspid.MVVM** library. It covers generating ViewModel classes, View
definitions, binder classes and commands based on user requirements. The
instructions below explain how to parse a user request, choose the correct
task and construct the appropriate C# code.

## When to use this skill

Activate this skill whenever a user:

* Mentions **Aspid.MVVM**, *MVVM* or *ViewModel* in the context of Unity.
* Requests to create a ViewModel, View, binder or command for their UI.
* Asks how to bind data between a ViewModel and a View in Unity.
* Needs a template or skeleton code to implement MVVM patterns.

## Supported tasks

### 1. Create a ViewModel

Use this pattern when the user wants a class that holds data and commands. A
ViewModel class must be marked with `[ViewModel]` and be `partial`. For each
field supplied by the user, choose the appropriate binding attribute based on
the desired synchronisation:

| User input            | Binding attribute                          |
|-----------------------|--------------------------------------------|
| One‑way view update   | `[OneWayBind]`                             |
| Two‑way synchronisation | `[TwoWayBind]`                           |
| Initialise once only  | `[OneTimeBind]`                            |
| View writes to model  | `[OneWayToSourceBind]`                     |

After listing the fields, generate private fields with the chosen attribute.
If the user wants commands, collect method names and optional `canExecute`
conditions. Annotate command methods with `[RelayCommand]` so the generator
creates an `IRelayCommand` property. Place any long‑running logic outside
the UI thread.

**Example**

Input: *Create a ViewModel called `HealthViewModel` with a two‑way bound
`float health` field and a one‑time bound `string name` field. Add a
command called `Heal`.*

Output:

```csharp
using Aspid.MVVM;

[ViewModel]
public sealed partial class HealthViewModel
{
    // TwoWay bound field: UI and ViewModel stay in sync.
    [TwoWayBind] private float _health;

    // OneTime bound field: only initialised from ViewModel.
    [OneTimeBind] private string _name;

    // Method becomes a command via [RelayCommand].
    [RelayCommand]
    private void Heal()
    {
        // Implement healing logic here.
    }
}
```

### 2. Create a View

Views display data from the ViewModel. Each View must be a `partial` class
annotated with `[View]` and typically inherits from `MonoView` to support
Unity’s inspector. For every field in the ViewModel that you want to display
or edit, declare a serialized field of type `MonoBinder` (single value) or
`MonoBinder[]` (multiple binders) and mark it with `[RequireBinder(typeof(T))]`.
Use `typeof(string)`, `typeof(float)`, etc., or `typeof(IRelayCommand)`
for command binders. Fields must be named to match the corresponding
ViewModel property (e.g., `_health` matches `_health` in the ViewModel).

**Example**

Input: *Create a View for `HealthViewModel` showing `health` and a Heal button.*

Output:

```csharp
using Aspid.MVVM;
using UnityEngine;

[View]
public sealed partial class HealthView : MonoView
{
    // Binder for the health value. Use a single binder when one UI element
    // displays this value.
    [RequireBinder(typeof(float))]
    [SerializeField] private MonoBinder _health;

    // Binder for the Heal command. Use an array if multiple UI elements
    // trigger the same command.
    [RequireBinder(typeof(IRelayCommand))]
    [SerializeField] private MonoBinder[] _heal;
}
```

After generating the class, instruct the user to add the component to their
GameObject in Unity and assign StarterKit binders via the inspector. Then
initialise the view from code:

```csharp
private void Awake()
{
    var viewModel = new HealthViewModel();
    healthView.Initialize(viewModel);
}
```

### 3. Create a binder

Binders connect View fields with specific UI components. To write a
custom binder, inherit from `MonoBinder` (for Unity components) or `Binder`
(for plain C# classes) and implement `IBinder<T>` for each supported type. Use
`IAnyBinder` for generic support and override type‑specific behaviour when
needed.

**Example**

Input: *Create a binder for `TMP_Text` that displays strings and floats.*

Output:

```csharp
using TMPro;
using Aspid.MVVM;
using UnityEngine;
using System.Globalization;

public class TextMonoBinder : MonoBinder, IBinder<string>, IBinder<float>
{
    [SerializeField] private TMP_Text _text;

    public void SetValue(string value) => _text.text = value;

    public void SetValue(float value) =>
        _text.text = value.ToString(CultureInfo.InvariantCulture);
}
```

### 4. Commands and extension methods

Use `[RelayCommand]` on private methods inside your ViewModel to generate
`IRelayCommand` properties automatically. To manually create commands from
delegates or handle null commands gracefully, call:

* `command.GetSelfOrEmpty()` – return the command if not null, otherwise an
  empty command.
* `action.CreateCommand()` or `action.CreateCommand(canExecute)` – create an
  `IRelayCommand` from a delegate.
* `action.CreateCommandOrEmpty()` – create a command or return an empty
  command if the delegate is null.
* `command.CreateCommandWithoutParameters(this)` – convert a parameterised
  command into a parameterless one, passing the current ViewModel.

These helpers reduce boilerplate when wiring commands from ViewModel to View.

## How to fulfil user requests

1. **Identify the task**. Look for phrases such as “create ViewModel”,
   “generate View”, “make binder”, or mention of fields, commands or binding
   types. If the user asks generally about Aspid.MVVM, summarise the library
   capabilities using the reference document.
2. **Gather details**. Ask the user for the names of classes, fields (with
   types) and desired binding modes. If they mention commands, ask for the
   command names and optional CanExecute conditions.
3. **Generate code**. Assemble the appropriate C# class definitions using
   the templates above. Ensure class names are sealed and partial, fields
   are private with proper binding attributes, and commands are annotated.
4. **Explain usage**. After providing code, briefly describe how to add the
   generated classes to Unity, assign binders via the inspector and
   initialise Views from code.
5. **Refer to reference**. For deep understanding or edge cases (e.g.,
   observable collections, advanced bindings), instruct the model to load
   `references/aspid-mvvm-guide.md` for more detail.

## Additional notes

* Only create Unity `GameObject` instances and inspector assignments when
  explicitly asked. Otherwise focus on generating class definitions.
* When multiple UI elements share the same ViewModel property or command,
  declare the binder field as an array (`MonoBinder[]`).
* Remind users that fields in the View must be named consistently with the
  corresponding private fields in the ViewModel.
* Do not include installation instructions; assume Aspid.MVVM is already
  imported in the project. Focus on code generation and guidance.

For more comprehensive information on Aspid.MVVM concepts—such as binding
modes, View initialisation rules and guidelines for creating custom binders—
read the reference document in `references/aspid-mvvm-guide.md`.