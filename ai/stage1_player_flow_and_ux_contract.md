# Stage 1 Player Flow And UX Contract

This document locks the minimum player-facing UX contract for the Stage 1 MVP loop.

It is derived from:

- `AGENTS.md`
- `ai/MVP_plan.md`
- `ai/GDD_Frontier_Buildlords.md`
- `ai/stage1_vertical_slice_loop_spec.md`
- `ai/stage1_mvp_content_manifest.md`
- current UI/MVC code under `Assets/Scripts/StaticMlp/Features/Mvc`
- current building presentation code under `Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Presentation`
- current composition UI under `Assets/Scripts/StaticMlp/Composition/Runtime/UI`

The intent is not to design polished screens. The intent is to remove ambiguity about:

- what the player should do next;
- which runtime state must exist for that answer to be stable;
- which feature owns that state;
- which presentation layer may render it.

## MVP Player Flows

### 1. Base Overview

**Player intent**

- Understand the current camp status.
- Identify the next meaningful action without opening secondary panels first.

**Required information on screen**

- Current primary objective.
- Camp core status: damaged, under repair, repaired, raid threatened, defended.
- Shared camp resources: `Wood`, `Stone`.
- Worker summary: available, assigned, blocked.
- Build preparation summary: not ready, ready to choose, prepared.
- Expedition summary: unavailable, available, active, completed.
- Threat summary: calm, rising, raid pending, raid active.

**Required gameplay state behind it**

- One authoritative camp anchor state for the whole slice.
- Shared settlement resource state, not only owner-only player inventory.
- Worker roster summary and assignment summary.
- Build readiness summary.
- Expedition availability summary.
- Threat and raid summary.
- One explicit active objective state.

**Which feature owns the state**

- `Settlement`: camp anchor, resources, local camp milestones, active objective.
- `Settlement.Workers`: worker roster and assignment status.
- `Build`: build readiness and prepared snapshot summary.
- `World`: expedition availability and threat summary.
- `Progression`: unlock flags that affect what the overview may promise.

**Which layer owns the presentation**

- Persistent HUD in `Runtime/Presentation`.
- Recommended owner: `Settlement.Runtime.Presentation` as the shell that renders a combined read model.
- MVC role: one persistent HUD controller only. It renders state; it does not advance progression.

### 2. Building Interaction

**Player intent**

- Inspect the current camp building target.
- Repair, build, or inspect what is missing.

**Required information on screen**

- Focused building name.
- Current phase: waiting for resources, ready to build, building, completed.
- `Wood` and `Stone` required vs delivered.
- Build progress.
- Available interaction for the current focus: deposit, build, inspect completed state.
- Why an action is unavailable if blocked.

**Required gameplay state behind it**

- Focused building identity stored as `EntityGID` on the client UX side.
- `ConstructionSiteState`, `ConstructionResources`, `ConstructionProgress`.
- Building definition data.
- Interaction availability derived from authoritative building state and local input context.

**Which feature owns the state**

- `Settlement`: authoritative building and construction state.
- Local UI session state may store focus and open/closed panel state, but never authoritative construction state.

**Which layer owns the presentation**

- World-space read model in `Settlement.Runtime.Presentation` for site status.
- One MVC context panel in `Settlement.Runtime.Presentation` for focused building details and actions.
- Existing `BuildingMenuState` pattern is acceptable only for local UI session state, not as a progression owner.

### 3. Worker Assignment

**Player intent**

- Assign one worker to the camp current job and confirm that the worker is doing useful work.

**Required information on screen**

- Worker count summary.
- For the single Stage 1 worker role: assigned or unassigned.
- Current assigned task.
- Current blocking reason if work cannot proceed.
- Immediate effect of assignment on the camp: delivering resources, building progress, or both.

**Required gameplay state behind it**

- One worker roster state.
- One assignment state per worker or per camp slot.
- One job execution summary that can be surfaced without exposing AI internals.
- One worker role only: `Camp Builder`.

**Which feature owns the state**

- `Settlement.Workers`: worker entities, jobs, assignment state, execution summary.
- `Settlement`: the camp-side task slot or task demand the worker can satisfy.

**Which layer owns the presentation**

- `Settlement.Runtime.Presentation`.
- Prefer a worker subpanel or context panel, not a separate management screen in Stage 1.
- MVC controller sends assignment intent; ECS validates and applies the assignment.

### 4. Build Preparation

**Player intent**

- Choose the combat build that the repaired camp currently enables.

**Required information on screen**

- Available build choices: `Poison Archer`, `Fire Bomber`.
- Current selected build.
- Short gameplay difference between the two choices.
- Why a build is locked or unavailable.
- Confirmation that the build is prepared for the next expedition.

**Required gameplay state behind it**

- Build availability state.
- Selected build or module state.
- Prepared build snapshot that combat can consume directly.
- Gating inputs from camp repair and progression.

**Which feature owns the state**

- `Build`: authoritative owner of build selection, availability, and prepared snapshot.
- `Settlement` and `Progression` contribute prerequisites but do not own the prepared combat build.

**Which layer owns the presentation**

- `Build.Runtime.Presentation`.
- One fullscreen or modal preparation controller is enough for Stage 1.
- The controller may show prerequisite reasons, but it must not inspect combat systems directly to decide what is legal.

### 5. Expedition Selection

**Player intent**

- Choose the currently available expedition and commit to leaving camp.

**Required information on screen**

- One available destination.
- Clear goal for that destination.
- Expected risk.
- Reward preview.
- Required prepared build.
- Commit or cancel action.

**Required gameplay state behind it**

- Expedition catalog data.
- Expedition availability state.
- Current prepared build snapshot reference.
- Active expedition state once committed.

**Which feature owns the state**

- `World`: authoritative owner of expedition availability and activation.
- `Build`: read-only contributor of the prepared snapshot used by the expedition.
- `Progression`: reward descriptor metadata if needed for the preview.

**Which layer owns the presentation**

- `World.Runtime.Presentation`.
- This may be a dedicated fullscreen controller or the second step after build preparation, but state ownership must stay split.

### 6. Reward Return

**Player intent**

- Understand what came back from the expedition and what it changes in the camp.

**Required information on screen**

- Reward package received.
- What the reward unlocked, advanced, or enabled.
- What the player should do next because of that reward.
- Whether threat increased as a result.

**Required gameplay state behind it**

- Completed expedition result.
- Persistent reward application state.
- Unlock delta for camp, build, or boss preparation.
- Post-reward objective update.

**Which feature owns the state**

- `Progression`: authoritative owner of reward application and unlock routing.
- `Settlement`, `Build`, and `World` consume the applied result.

**Which layer owns the presentation**

- `Progression.Runtime.Presentation` for the result summary.
- Base HUD in `Settlement.Runtime.Presentation` must also refresh immediately so the player sees the new next step without reopening the reward panel.

### 7. Threat / Raid Awareness

**Player intent**

- Know that expedition success has consequences for the camp and understand when defense becomes the next priority.

**Required information on screen**

- Threat state: calm, rising, raid pending, raid active.
- Why threat changed.
- Whether the camp is ready or exposed.
- Countdown or immediate-active signal if a raid is scheduled.
- Clear next action: prepare, defend now, or finish boss prep after defense.

**Required gameplay state behind it**

- Threat state.
- Raid scheduled or active state.
- Camp defense gate state.
- Post-raid completion state.

**Which feature owns the state**

- `World`: authoritative owner of threat escalation, raid scheduling, and raid activation.
- `Settlement`: authoritative owner of the defended camp anchor being attacked.
- `Progression`: consumes the defense result for final boss gating if needed.

**Which layer owns the presentation**

- Persistent threat banner or badge in `World.Runtime.Presentation`.
- Optional overlay notification in `World.Runtime.Presentation` for transitions into raid pending or raid active.

## UI/State Ownership Matrix

| UX concern | Authoritative gameplay owner | Client read-model owner | MVC / view owner | Notes |
| --- | --- | --- | --- | --- |
| Base overview HUD | `Settlement`, with summary inputs from `Settlement.Workers`, `Build`, `World`, `Progression` | Client summary resource built in client core ECS | `Settlement.Runtime.Presentation` persistent HUD controller | This is the main "what next?" surface. |
| Focused building context | `Settlement` | Focused building presentation state plus local focus state keyed by `EntityGID` | `Settlement.Runtime.Presentation` context controller and world-space building views | Do not keep raw `Entity` handles across frames. |
| Building menu open and selection session | local UX only | client-only UI resource | `Settlement.Runtime.Presentation` MVC controller | Equivalent to current `BuildingMenuState`. It is not authoritative game state. |
| Worker assignment panel | `Settlement.Workers` and `Settlement` | client summary resource | `Settlement.Runtime.Presentation` context controller or worker subpanel | One worker role only in Stage 1. |
| Build preparation state | `Build` | client build-prep summary resource | `Build.Runtime.Presentation` fullscreen or modal controller | Build legality must not live in the controller. |
| Expedition selection state | `World` | client expedition selection summary resource | `World.Runtime.Presentation` fullscreen controller | Reads the prepared build snapshot, does not own it. |
| Reward return state | `Progression` | client reward-result summary resource | `Progression.Runtime.Presentation` popup or modal controller | Reward must be applied before UI promises new unlocks. |
| Threat and raid banner | `World` | client threat summary resource | `World.Runtime.Presentation` persistent badge, banner, and optional overlay | Must stay visible from camp overview. |
| World-space construction feedback | `Settlement` | `ConstructionViewState`-style view state | Entity view parts in `Settlement.Runtime.Presentation` | Current building presentation already follows this pattern. |
| Debug multiplayer controls | `Composition` only | bootstrap state | `Composition/Runtime/UI` | Keep separate from gameplay MVP UI. Do not repurpose as settlement HUD. |

## Minimal UI Contract For MVP

The MVP only needs five gameplay UI surfaces:

1. **Persistent base HUD**
   Shows objective, camp status, `Wood`, `Stone`, worker summary, build readiness, expedition readiness, and threat summary.

2. **Focused context panel**
   Reused for building interaction and worker assignment. The focused target is chosen by local UI state, but the contents come from ECS read models.

3. **Build preparation screen**
   Minimal choice surface for `Poison Archer` or `Fire Bomber`, with readiness and confirmation.

4. **Expedition selection screen**
   One destination, one reward preview, one commit action.

5. **Reward and threat feedback**
   A small result popup for reward return plus a persistent threat banner or badge for raid awareness.

Recommended MVC layering:

- `Persistent` sort `0`: base HUD.
- `Persistent` sort `100`: focused context panel.
- `Persistent` sort `200`: threat banner and notifications.
- `Fullscreen`: build preparation.
- `Fullscreen`: expedition selection.
- `Popup`: reward result if presented as an interrupting summary.

Recommended control contract:

- MonoBehaviours remain passive views only.
- Controllers translate button clicks and local focus changes into ECS intent.
- Client core systems build read models from authoritative gameplay state.
- `ControllerEcsBridgeSystem<TController>` or equivalent active-only sync may be used for view refresh, but never for gameplay decisions.

Recommended information architecture rule:

- The HUD must always answer one sentence for the player: `what is the next blocking step for the camp loop right now?`
- Secondary screens must refine that answer, not replace it with unrelated management detail.

## Decisions To Lock Now

1. **Shared camp state must exist early**
   Stage 1 cannot rely on owner-only `ResourcesInventory` as the long-term camp overview source. The base HUD needs shared settlement truth.

2. **The camp must be the single progression anchor**
   Repair, worker assignment, build prep, reward return, threat, and boss prep must all resolve against the same camp state chain.

3. **Build prep must export a stable prepared snapshot**
   Combat should consume a finalized build contract from `Build`, not read building state, raw unlock flags, or UI selections directly.

4. **Expedition and raid ownership belongs to `World`**
   Do not let `Settlement` or controllers become the de facto scheduler of expeditions or raids.

5. **Reward application belongs to `Progression`**
   The reward return flow must write persistent state before the UI advertises new options or raises threat consequences.

6. **Local UI selection must use stable ids**
   Focused building or worker state should use `EntityGID` or stable domain ids, never raw `Entity` handles across frames.

7. **One focused context surface is enough**
   Building interaction and worker assignment should share the same context-panel pattern in Stage 1 instead of spawning several competing side panels.

8. **The overview HUD must own the "what next?" answer**
   Do not scatter objective truth across separate popups, world-space labels, and debug panels.

9. **Build prep and expedition selection may be sequential, but not state-merged**
   The UX may move the player directly from build prep to expedition select, but `Build` and `World` remain separate authoritative owners.

10. **Gameplay UI must stay out of `Composition/Runtime/UI`**
    The current multiplayer status panel is composition and debug UI. MVP gameplay UI should live under feature presentation modules and MVC infrastructure.

## UX Architecture Risks

- If the HUD keeps reading prototype player inventory instead of shared settlement state, the player will see the wrong bottleneck once workers and reward return exist.
- If `BuildingMenuState`-style local UI resources start carrying authoritative progression flags, multiplayer authority and replayability logic will become unreliable.
- If build legality is evaluated inside controllers or views, build prep will leak domain rules out of `Build`.
- If expedition selection also owns build selection, the screen will become a cross-domain orchestrator instead of a presentation surface.
- If reward return is only a transient popup and not a persisted progression write, the next-action contract will break immediately after the panel closes.
- If threat is only shown once the raid starts, the player will not understand the causal link between expedition success and camp danger.
- If world-space building feedback is the only construction UI, the player will not get a clear camp-wide answer about what blocks progress next.
- If separate panels independently own objective text, readiness text, and threat text, the project will accumulate contradictory UX states.
- If MonoBehaviours start binding directly to gameplay services or bootstrap objects for camp logic, the MVC and ECS boundary will erode and later multiplayer debugging will get harder.
