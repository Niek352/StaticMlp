# MVC Package Review

Review target:

```text
Assets/Scripts/StaticMlp/Features/Mvc
```

Review goals:

- convenience of use;
- readability;
- fit with StaticEcs and `IEvent` flow;
- whether it helps keep UI solutions clean by SRP.

## Short Verdict

The package has a good minimal core for client-only UI orchestration:

- lifecycle is explicit;
- layering model is understandable;
- view/controller split is readable;
- async show/hide flow is small enough to reason about.

But in its current form it is **not yet a fully natural StaticEcs-style package**.

Main reason:

- the ECS bridge is built around a bound controller reference and controller-driven activation, not around ECS data and events as the primary contract.

So the package is best treated as:

- a **client UX bridge layer over ECS**,

and not as:

- a core feature architecture pattern for gameplay or shared domain flow.

Also, a few parts conflict with this repository's local rules, especially explicit inspector wiring for UI references.

## What Is Good

### 1. Small public surface

The main concepts are easy to grasp:

- `IView`
- `IController`
- `MvcManager`
- `WindowStackManager`
- `ControllerEcsBridgeSystem`

There is no giant framework feel. That is a plus for onboarding and maintenance.

### 2. Readable lifecycle

`ControllerBase<TView, TInputData>` makes the window lifecycle easy to follow:

- create view lazily;
- push ordering;
- show;
- wait for close intent;
- hide.

The override points are also well chosen:

- `OnViewInstantiated`
- `OnBeforeViewShow`
- `OnViewShow`
- `OnViewClose`
- `OnFocus`
- `OnBlur`

This makes simple windows straightforward to write.

### 3. Layer model is understandable

`Persistent / Fullscreen / Popup / Overlay` is a clear mental model.

For UI work this is more readable than having each screen manually manage sibling order and focus rules.

### 4. Bridge activation idea is useful

`BridgeSystemBinding<TSystem, TController>` has a useful intent:

- when the controller is active, the bridge system is active;
- when the controller is hidden or blurred, the bridge system is paused;
- on activation it can do an immediate sync.

For client UX this is practical and easy to reason about.

## What Is Weak

### 1. It is not battle-tested in this repository yet

At the moment there are no usages of this package outside its own folder.

That means:

- API ergonomics are still theoretical;
- edge cases are not exercised by feature code yet;
- naming quality is not validated by real screens.

### 2. The ECS bridge is OO-first, not ECS-first

`ControllerEcsBridgeSystem<TController>` stores a direct controller reference and calls `Process()` from an `ISystem`.

That means the main dependency direction becomes:

```text
Controller lifecycle -> activates system -> system reads controller directly
```

instead of the more StaticEcs-native style:

```text
UI intent -> IEvent / resource / component state -> ECS systems -> presentation state
```

This is acceptable for a thin UX adapter layer, but weak as a general ECS architecture pattern.

Risks:

- logic becomes hidden inside controller-bound systems instead of visible in ECS contracts;
- systems become harder to reuse without this MVC package;
- the code nudges authors toward polling controller state rather than modeling intent with events.

### 3. Local project UI rules are violated

`PrefabViewBase` currently auto-searches `Canvas` and `GraphicRaycaster` in `Awake()` and treats them as optional.

This conflicts with local project rules:

- required UI references should be explicitly wired in inspector;
- null UI references should fail fast;
- code should not search hierarchy instead of explicit setup.

In this repository, this is a real architectural mismatch, not just a style preference.

### 4. `MvcManager` and `WindowStackManager` carry too much implicit behavior

Both classes are still readable, but they already concentrate a lot of policy:

- view showing;
- close orchestration;
- focus switching;
- popup ordering;
- fullscreen replacement;
- persistent blur/focus restore;
- overlay rules.

This is still manageable now, but it is the main future risk area for readability.

### 5. Some API pieces look unfinished or misleading

Examples:

- `_useSelfCanvas` is unused;
- `PopupBackdropOrdering` is produced but not used inside the package;
- `OnViewShown` fires before the actual show flow completes;
- `OnViewClosed` is more "show flow finished" than "view actually closed";
- `ViewOrdering.OrderInLayer` stores a value with layer offset already applied, so the name is a little misleading.

These are not catastrophic, but they reduce trust in the API.

### 6. Persistent windows do not have a real stacking policy

`PushPersistent()` gives the same order to every persistent controller.

If more than one persistent view is used, draw order becomes unclear and depends on external factors instead of explicit policy.

## StaticEcs Assessment

### Good fit

The package can fit StaticEcs well if it is used only as a client-side presentation shell:

- buttons call controller methods;
- controller sends ECS intent through `IEvent` or resource changes;
- ECS systems own all actual state transitions;
- ECS writes presentation state;
- bridge systems only mirror ECS state into the view.

In that role the package is fine.

### Weak fit

The package is a weak fit if:

- controller-bound systems start containing feature logic;
- systems read controller state directly as the main source of truth;
- screen flow becomes the place where game state decisions are made.

That would work technically, but it would drift away from the project's StaticEcs rules.

### About `IEvent`

The package itself barely uses StaticEcs events as a first-class architectural primitive.

So the answer is:

- it does **not misuse** StaticEcs;
- but it also does **not strongly leverage** StaticEcs `IEvent` flow by design.

The package is event-compatible, not event-centered.

## SRP Assessment

The package can produce clean SRP solutions, but only under a strict usage discipline.

### SRP-friendly usage

- `View`: rendering, button wiring, animation hooks.
- `Controller`: open/close flow and translation between Unity callbacks and ECS intent.
- ECS systems: state changes, validation, derived presentation state.
- Bridge system: copy ECS state into the controller/view when active.

This is clean.

### SRP-breaking usage

- `Controller`: starts deciding game rules.
- Bridge system: becomes feature logic plus UI logic plus ECS access at once.
- `MvcManager` consumers: start depending on exact window stack internals.

This would make the pattern feel heavy very quickly.

So SRP outcome is **potentially good, but usage-sensitive**.

## Recommended Refactor Plan

Priority order is based on impact on readability and safe usage.

### Stage 1. Align the package with this repository's UI rules

1. Remove hierarchy auto-search from `PrefabViewBase`.
2. Make required references explicit.
3. Fail fast when required UI parts are missing.
4. Remove `_useSelfCanvas` if it has no real purpose.

Why first:

- this is the clearest mismatch with project standards;
- it improves reliability immediately;
- it makes prefab setup more honest.

### Stage 2. Make the ECS bridge explicitly adapter-only

1. Document that `ControllerEcsBridgeSystem<TController>` is for client UX/presentation only.
2. State that gameplay and domain decisions must not live there.
3. Encourage bridge systems to only copy ECS state to view/controller, send UI intent as ECS events, and react to active/inactive controller state.
4. Add one small canonical example showing the flow:

```text
button click -> IEvent -> ECS system -> presentation state -> bridge/view
```

Why second:

- this is the biggest architectural ambiguity in the package;
- a small example will prevent a lot of misuse.

### Stage 3. Simplify and clarify manager semantics

1. Rename or redefine `OnViewShown` / `OnViewClosed` to match actual timing.
2. Decide whether overlays are single-instance or stackable, and encode that rule explicitly.
3. Decide whether persistent windows can be multiple, and if yes give them deterministic ordering.
4. Consider extracting close/focus policy from `MvcManager` into narrower rule objects only if complexity keeps growing.

Why third:

- current code is still understandable;
- the bigger issue is semantic sharpness, not raw size.

### Stage 4. Reduce runtime ambiguity in the generic API

1. Consider a stronger registration API than `Dictionary<Type, IController>`.
2. Reduce places that rely on runtime casts for core flow.
3. Rename `ViewOrdering.OrderInLayer` to something closer to actual meaning, such as total sorting order.

Why fourth:

- these are usability/readability improvements;
- they matter more once real screens begin using the package.

### Stage 5. Remove dead or incomplete API pieces

1. Remove unused fields and events.
2. Either implement popup backdrop support fully or remove the related return values for now.
3. Keep the public surface only around concepts that are actually used.

Why fifth:

- this improves trust and makes the package easier to learn.

## Recommended Usage Policy

If the package is kept, I would adopt these rules:

1. Use it only for client UI/UX.
2. Do not place gameplay logic in controllers.
3. Do not place domain decisions in bridge systems.
4. Send user intent to ECS through `IEvent` or explicit client resources.
5. Keep ECS as the owner of actual state.
6. Let views stay passive and dumb.
7. Mirror ECS state into views only from presentation-side adapters.

## Final Recommendation

The package is worth keeping, but I would **not yet bless it as a project-wide standard without a small refactor pass**.

My practical verdict:

- keep the core idea;
- refactor the UI wiring rules first;
- narrow the intended role of the ECS bridge;
- then write official usage documentation after one real screen is implemented on top of it.

After that, the package can become a solid client UX adapter for StaticEcs instead of a second architecture competing with it.
