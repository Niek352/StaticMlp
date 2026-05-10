# Frontier Buildlords MVP Plan

## MVP Goal

Validate the core game promise:

**the player builds and improves a settlement to unlock stronger combat builds, uses those builds in a local combat run, brings rewards back, and strengthens the settlement further.**

## MVP Target Loop

1. Start from a small damaged camp.
2. Gather basic resources.
3. Build and complete core settlement structures.
4. Assign workers to simple jobs.
5. Produce basic combat preparation assets.
6. Assemble a combat build before an expedition.
7. Clear a small combat objective.
8. Bring back rewards that unlock new settlement power.
9. Defend the base from one enemy counterattack.
10. Prepare for and defeat one boss encounter.

## Stage 1. Core Vertical Slice Definition

- Lock one playable MVP loop from camp start to boss kill.
- Lock the MVP content list for buildings, resources, enemies, regions, rewards, and builds.
- Define the minimum player-facing flow for base management, expedition preparation, combat, return, and defense.
- Align the feature boundaries around `Settlement`, `Build`, `Combat`, and `World`.

## Stage 2. Settlement Foundation

- Expand the current building flow into the MVP building set.
- Support a functional base center, storage, housing, food, crafting, military, and defense layer.
- Make building placement, construction progress, and completed building state work as one consistent gameplay loop.
- Expose the settlement state clearly enough for the player to understand what can be built and used next.

## Stage 3. Workers and Basic Economy

- Introduce MVP settlers/workers as gameplay actors inside the ECS simulation.
- Add simple worker assignment to a small set of jobs.
- Implement the first production loop for base resources and crafted outputs.
- Connect workers, construction, and inventory into one usable settlement economy.
- Make the base progress even when the player is not directly building by hand.

## Stage 4. Build Preparation Layer

- Add a dedicated MVP build/loadout layer between settlement and combat.
- Define the first build slots and the first unlockable build modules.
- Connect buildings and produced assets to available combat options.
- Support two clear combat archetypes for the first playable slice.
- Show the player what build is currently available before leaving for combat.

## Stage 5. Combat Expedition Slice

- Turn the existing player, AI, combat, effect, and status systems into one complete expedition flow.
- Add one small expedition zone with one clear objective.
- Support the MVP enemy set, one elite step, and one boss step.
- Make combat rewards return to progression in the base instead of ending as isolated combat output.
- Ensure the player can feel a real difference between the available MVP builds.

## Stage 6. World Progression and Capture

- Add a small world layer with the starting area plus a few capture targets.
- Implement one resource point, one hostile camp, and one boss destination.
- Make captured points unlock useful rewards for the settlement or build system.
- Add one simple outpost/capture result that changes future preparation options.
- Connect progression so the next goal after each success is obvious.

## Stage 7. Base Threat and Defense

- Add one enemy counterattack/raid flow against the settlement.
- Use base state and player build strength as part of the defense outcome.
- Make settlement defense a direct continuation of the main loop, not a separate mode.
- Reward successful defense with continued progression pressure toward the final MVP objective.

## Stage 8. MVP UX Layer

- Add the minimum interface for resources, workers, construction, production, build prep, expedition choice, and threat visibility.
- Surface the current bottleneck, next available action, and next meaningful goal.
- Make reward sources and unlock results understandable at a glance.
- Keep the management layer readable and fast enough for repeated loop play.

## Stage 9. MVP Integration and Playtest Pass

- Integrate all MVP systems into one stable 30-45 minute vertical slice.
- Tune pacing from first camp actions to first boss completion.
- Tune the economy so base growth and combat preparation reinforce each other.
- Tune the combat layer so the chosen build meaningfully changes the run.
- Run focused playtests against the MVP hypothesis and use the results to decide the next production step.

## MVP Deliverable

The MVP is complete when one player can:

1. start from a basic camp,
2. build a functional small settlement,
3. assign workers and produce useful outputs,
4. prepare a combat build from settlement progress,
5. clear at least one expedition,
6. bring rewards back into the settlement loop,
7. survive one base defense event,
8. defeat one boss and finish the vertical slice with a clear sense that the base created the build.
