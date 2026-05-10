# Stage 1 Vertical Slice Loop Spec

## Vertical Slice Overview

This document locks one MVP gameplay loop for the current project from damaged camp start to boss completion.

The loop is intentionally narrow and built around the runtime that already exists:

- `Buildings` provides placement, construction-site state, resource deposit, and build completion.
- `ResourcesInventoryMinimal` provides the only implemented resource state, currently owner-only wood/stone on the player.
- `Player`, `Combat`, `Effects`, `Statuses`, `AiBots`, and `AiTaskExecution` already provide a usable server-authoritative combat slice with hostile bots, health, damage, abilities, and death.

The locked loop should therefore be:

1. Start at a damaged camp represented by one pre-seeded incomplete camp structure.
2. Gather enough wood and stone to repair the camp.
3. Finish camp construction.
4. Assign one worker to help the camp recover.
5. Use the repaired camp to prepare one combat build.
6. Run one expedition against a small hostile group.
7. Return one reward package to the camp.
8. Survive one enemy counterattack against the same camp.
9. Spend the returned reward on final boss preparation.
10. Defeat one boss encounter.

This loop is realistic for the current codebase only if Stage 1 keeps the content minimal:

- one camp building tier,
- one worker assignment path,
- one expedition destination,
- one reward type,
- one counterattack,
- one boss.

It should not branch into multiple regions, multiple build trees, or a full economy in Stage 1.

## Sequential Loop Steps

| Step | Locked MVP step | Player goal | Required game state | Required system/domain ownership | Output to next step |
| --- | --- | --- | --- | --- | --- |
| 1 | Base start | Understand the camp is damaged and identify the first repair target. | Player avatar is spawned. One damaged camp site exists as an initial construction site. Starter wood/stone is present or nearby. No expedition is available yet. | Current foundation: `Player`, `Buildings`, `ResourcesInventoryMinimal`. Target owner: `Settlement`. | `CampStartReady` state with one explicit repair objective. |
| 2 | Resource gathering | Obtain enough wood and stone to repair the camp. | Gatherable resource sources exist, or a temporary MVP collection source grants wood/stone into the same camp-facing resource pool. Required amount is visible. | Missing in current project. Target owner: `Settlement` or `World` resource nodes, with inventory application in `Settlement`. | `RepairResourcesReady` once the camp repair cost is payable. |
| 3 | Construction | Deposit resources and complete the damaged camp repair. | The damaged camp site is in `WaitingForResources`, then `ReadyToBuild`, then `BuildingInProgress`, then `Completed`. | Current foundation: `Buildings` request handlers, `ConstructionRules`, server completion spawn. Target owner: `Settlement`. | `CampRepaired`, which unlocks worker assignment and build preparation. |
| 4 | Worker assignment | Assign one worker to the repaired camp so the base starts acting on its own. | One worker NPC exists, can be assigned to the camp, and can either deliver build resources or perform build work for assigned tasks. | Partial foundation exists: `AiBots`, `AiTaskExecution`, `AiActions` already contain `BuildConstruction` and `DeliveryResourceToBuilding` execution. Missing: worker entity role, assignment UI/state, worker ownership rules, and worker-specific spawn/setup. Target owner: `Settlement.Workers`. | `WorkerAssigned`, proving the camp can drive one non-player task. |
| 5 | Build preparation | Select one combat-ready build that the camp now enables. | Repaired camp unlocks one build choice. Minimum viable choice is between the currently implemented combat ability patterns, not a full loadout tree. | Partial foundation exists: `Combat` already supports `BasicMeleeAuto`, `PoisonArrow`, and `FireFlask`. Missing: explicit `Build` domain, build unlock state, and settlement-to-combat bridge. | `PreparedBuildSnapshot`, a locked combat loadout used by the expedition. |
| 6 | Expedition | Leave camp and clear one small hostile encounter. | One expedition destination is available. Hostile bots spawn there. Combat uses the prepared build snapshot. Encounter completes when all required hostiles die. | Current combat foundation exists: `Player`, `Combat`, `Effects`, `Statuses`, `AiBots`, `AiTaskExecution`. Missing: explicit expedition state, destination ownership, encounter completion contract, and zone transition ownership. Target owner: `World` + `Combat`. | `ExpeditionCleared` plus one reward package. |
| 7 | Reward return | Bring expedition rewards back into base progression. | The expedition result converts into a persistent camp reward, such as boss-key progress, camp supplies, or a single build unlock token. | Missing in current project. Target owner: `Progression` to validate/apply rewards, with `Settlement` and `Build` consuming the result. | `RewardApplied`, which raises threat and enables counterattack scheduling. |
| 8 | Counterattack | Defend the repaired camp from one reactive enemy raid. | Camp exists in the same world state. A counterattack is scheduled because the expedition succeeded. Hostile bots spawn near the camp and attack there. | Partial combat/AI foundation exists for the actual fight. Missing: raid trigger state, spawn rules tied to camp progression, defense success/failure contract, and world authority over attack scheduling. Target owner: `World` + `Combat`. | `CampDefended`, which unlocks final boss preparation. |
| 9 | Boss preparation | Spend the returned reward on one final preparation action before the boss. | The camp has a post-raid preparation state and one spendable reward. The player chooses the final build variant or one boss-ready upgrade. | Missing in current project. Target owner: `Build` + `Progression`, with `Settlement` surfacing readiness. | `BossUnlocked` and `BossBuildReady`. |
| 10 | Boss fight | Defeat one boss and complete the vertical slice. | One boss encounter becomes available. Boss can be implemented as a stronger AI bot encounter with larger health and support adds for MVP. Completion is the boss death event. | Current combat and AI foundation can support the fight itself. Missing: boss encounter owner, boss unlock state, boss spawn orchestration, and slice completion contract. Target owner: `World` + `Combat` + `Progression`. | `VerticalSliceComplete`. |

## Required State Transitions

The slice should use one explicit forward-only chain of gameplay states:

`DamagedCampStart`
-> `RepairObjectiveActive`
-> `RepairResourcesReady`
-> `CampRepaired`
-> `WorkerAssigned`
-> `BuildPrepared`
-> `ExpeditionActive`
-> `ExpeditionCleared`
-> `RewardApplied`
-> `CounterattackActive`
-> `CampDefended`
-> `BossPrepared`
-> `BossActive`
-> `BossDefeated`

Each transition should be driven by one concrete state change, not by loose UI flow:

- `DamagedCampStart -> RepairObjectiveActive`: the initial damaged camp site exists and is flagged as the active settlement objective.
- `RepairObjectiveActive -> RepairResourcesReady`: the required repair cost is fully present in the valid resource pool.
- `RepairResourcesReady -> CampRepaired`: the camp site reaches `ConstructionPhase.Completed`.
- `CampRepaired -> WorkerAssigned`: a worker is bound to one valid camp task and begins execution.
- `WorkerAssigned -> BuildPrepared`: one build snapshot is selected and stored for combat use.
- `BuildPrepared -> ExpeditionActive`: the player commits to the expedition and the combat encounter is instantiated.
- `ExpeditionActive -> ExpeditionCleared`: the encounter completion condition is met.
- `ExpeditionCleared -> RewardApplied`: the reward package is persisted into progression/base state, not left as transient combat output.
- `RewardApplied -> CounterattackActive`: threat logic schedules and starts one raid against the same camp.
- `CounterattackActive -> CampDefended`: raid victory condition is met while the camp remains valid.
- `CampDefended -> BossPrepared`: the defense result unlocks the final preparation step.
- `BossPrepared -> BossActive`: the boss encounter is committed and instantiated.
- `BossActive -> BossDefeated`: the boss death condition is met.

Mandatory gates that prove the loop is coherent:

- The repaired camp must be the same progression anchor for build preparation, reward return, counterattack, and boss unlock. If those steps use separate disconnected state, the loop is not coherent.
- The expedition reward must write into persistent base/progression state before counterattack and boss prep. A combat-only win screen is not enough.
- Worker assignment must produce real gameplay value in the same loop, not exist as decorative UI only.
- Build preparation must change combat behavior using the already implemented combat ability set. If camp progress does not alter combat output, the loop collapses.
- Boss access must be locked behind prior expedition success and counterattack survival.
- The slice must be completable with one content lane only. No branch should require additional settlements, regions, professions, or research trees.

## Missing Links In The Current Project

The current codebase already supports part of the loop, but several links are still absent or only partial.

Implemented or mostly implemented foundations:

- Camp construction exists through `Buildings`: placement, construction-site state, resource deposit, build work, and completion.
- Player combat exists through `Player`, `Combat`, `Effects`, and `Statuses`.
- Hostile AI exists through `AiBots` and `AiTaskExecution`.
- Worker-like task execution foundations already exist in `AiActions` for `BuildConstruction` and `DeliveryResourceToBuilding`.

Missing or incomplete links that Stage 1 must add:

- No real resource gathering loop exists yet. `ResourcesInventoryMinimal` only seeds player-owned wood/stone; it does not model harvesting, hauling, or shared camp storage.
- No explicit `Settlement` owner exists yet. Buildings and resources are still split prototype modules.
- No explicit worker domain exists. There is no worker spawn/setup flow, no assignment state, no worker UI, and no clear separation between combat bots and settlement workers.
- No explicit `Build` domain exists. Combat abilities exist, but there is no settlement-driven loadout/build snapshot that gates expedition prep.
- No explicit `World` domain exists. There is no owner for expedition destinations, encounter activation, raid pressure, counterattack scheduling, or boss encounter selection.
- No explicit `Progression` domain exists. There is nowhere authoritative to apply reward return, unlock boss access, or persist loop milestones.
- No shared camp progression state currently bridges base and combat. Stage 1 needs a single authoritative chain of flags/contracts for repaired camp, worker assigned, build prepared, expedition cleared, reward applied, raid defended, and boss unlocked.
- No dedicated boss logic exists. MVP should therefore treat the boss as a special world/combat encounter built from the existing bot/combat stack, not as a separate subsystem.

Recommended Stage 1 implementation stance:

- Reuse the current `Buildings` construction pipeline for the damaged camp repair.
- Add the smallest possible resource gathering source that feeds the same authoritative resource state used by construction.
- Promote one AI worker path from the existing AI execution stack instead of inventing a new worker framework.
- Treat build preparation as a thin bridge that selects from the already implemented combat abilities.
- Implement expedition, counterattack, and boss steps as thin world-state wrappers around the existing combat/AI runtime.
