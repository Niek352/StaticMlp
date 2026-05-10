# SimulationTime.ServerTick

How and when to use authoritative server simulation time.

## Purpose

`SimulationTime.ServerTick` is the authoritative clock for server gameplay simulation.

Use it when the code answers questions like:

- Has a cooldown finished?
- Has a status expired?
- Should a periodic effect fire this step?
- Should server AI make a decision now?
- What tick was this server-side effect created on?

Do not use it as a general replacement for all time in the project. It is for authoritative server simulation only.

## Mental Model

`GameTime`:

- Frame time
- Wall-clock style time
- Useful for client UX, interpolation, and presentation

`SimulationTime.ServerTick`:

- Fixed-step simulation time
- Advances once per server simulation step
- Defines server-authoritative gameplay order

`SW.Tick()`:

- StaticEcs tracking/history tick
- Not the same thing as gameplay `ServerTick`

Keep these concepts separate.

## When To Use ServerTick

Use `SimulationTime.ServerTick` in server gameplay, validation, and server-side simulation logic:

- Server combat cooldowns
- Server status duration and expiry
- Server poison/burning/area tick effects
- Server AI decision cadence
- Server-side effect creation metadata
- Any gameplay deadline that must be deterministic relative to the fixed server step

Typical pattern:

```csharp
var simulationTime = SW.GetResource<SimulationTime>();
var currentTick = simulationTime.ServerTick;
```

## When Not To Use ServerTick

Do not use `ServerTick` in:

- Client presentation
- Client local UX timers
- Client interpolation
- Render smoothing
- Camera/UI animation
- `MonoBehaviour.Update()` visual code

Those should keep using `GameTime` or direct Unity frame time where appropriate.

## Storage Rules

Prefer absolute tick deadlines, not countdown floats.

Correct:

```csharp
public uint NextAttackTick;
public uint EndTick;
public uint NextDecisionTick;
public uint NextTick;
```

Avoid:

```csharp
public float NextAttackAt;
public float RemainingTime;
public float DecisionCooldown;
public float Timer;
```

Reason:

- Absolute deadlines are simpler to validate
- They survive catch-up steps naturally
- They are deterministic relative to the server fixed step
- They avoid frame-rate-dependent drift

## Converting Seconds To Ticks

Authored configs still stay in seconds.

Convert them at runtime through `SimulationTime`:

```csharp
var cooldownTicks = simulationTime.SecondsToTicks(config.FireInterval);
var nextAttackTick = simulationTime.DeadlineAfter(config.FireInterval);
```

Conversion policy:

- `0f` becomes `0` ticks
- Non-zero values use ceiling
- A duration must not end earlier than the authored seconds value

## Common Patterns

### Cooldown

```csharp
if (currentTick < attackState.NextAttackTick)
    return;

attackState.NextAttackTick = simulationTime.DeadlineAfter(config.FireInterval);
```

### Expiry

```csharp
if (currentTick >= lifeTime.EndTick)
    entity.Set<IsDestroyed>();
```

### Periodic Effect

Store:

- `IntervalTicks`
- `NextTick`

Run:

```csharp
while (currentTick >= state.NextTick && currentTick < lifeTime.EndTick)
{
    state.NextTick += state.IntervalTicks;
    FireEffect();
}
```

Use `while`, not `if`, so catch-up steps do not lose periodic events.

### Continuous Per-Second Rates

Do not convert everything into deadlines.

For values like "fear per second", "build work per second", or "resource decay per second", apply the rate once per server step:

```csharp
var deltaTime = simulationTime.FixedStepSeconds;
value += ratePerSecond * deltaTime;
```

This is still server-authoritative because the step size is fixed.

## Recommended Resource Access

Server gameplay should read:

```csharp
var simulationTime = SW.GetResource<SimulationTime>();
```

Client gameplay and presentation should read:

```csharp
var gameTime = CW.GetResource<GameTime>();
```

Do not read `GameTime` in new authoritative server gameplay unless the logic is explicitly non-authoritative diagnostics or presentation-like glue.

## Current Project Rules

In this project:

- Server authoritative gameplay uses `SimulationTime`
- Client local UX and presentation use `GameTime`
- `EffectCreatedTick.Tick` stores `SimulationTime.ServerTick`
- Status lifetime uses `LifeTime.EndTick`
- Periodic status state uses `StatusTickState.IntervalTicks` and `StatusTickState.NextTick`
- Area effects use `AreaEffectState.TickIntervalTicks` and `AreaEffectState.NextDamageTick`

## Code Review Checklist

When reviewing server gameplay code, ask:

1. Is this authoritative server simulation?
2. If yes, should it be based on `SimulationTime.ServerTick`?
3. Is the state stored as an absolute tick deadline instead of a float countdown?
4. If the logic is periodic, does it use `while` for catch-up?
5. If the logic is per-second continuous, does it use `FixedStepSeconds` exactly once per step?
6. Is `GameTime` being used only for client UX/presentation and not for server authority?

If a server gameplay system uses `Time.deltaTime`, `Time.time`, `GameTime.Time`, or countdown floats for authoritative deadlines, treat that as a design bug unless there is an explicit exception.
