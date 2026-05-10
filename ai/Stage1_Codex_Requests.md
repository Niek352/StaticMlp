# Stage 1 Codex Requests

This document breaks **Stage 1. Core Vertical Slice Definition** from [MVP_plan.md](C:/Users/pavel/_UnityProjects/StaticMlp/ai/MVP_plan.md) into separate Codex requests.

Each request is intended for a **new Codex window**.

The goal of Stage 1 is not to implement gameplay yet. The goal is to lock the MVP vertical slice, define clear feature boundaries, and make architecture decisions that will reduce rework in Stages 2-9.

## How to Use These Requests

- Open one new Codex window per request.
- Paste one request exactly as-is or adjust the target filename if needed.
- Keep the requests independent.
- Prefer document outputs over code changes during Stage 1 unless the request explicitly justifies a small supporting edit.

## Request 1. Architecture Boundary Audit

```text
Read these files first:
- AGENTS.md
- ai/MVP_plan.md
- ai/GDD_Frontier_Buildlords.md
- ai/static_ecs_multiplayer_architecture.md
- ai/ECS_Feature_Architecture_Layout_StaticEcs.md
- ai/networked_feature_recipes.md

Then inspect the current runtime feature modules under:
- Assets/Scripts/StaticMlp/Features
- Assets/Scripts/StaticMlp/Game
- Assets/Scripts/StaticMlp/Composition

Task:
Produce a document at ai/stage1_architecture_boundary_audit.md.

What I need:
1. A concise audit of the current architecture relevant to the MVP.
2. A recommended domain split for the MVP around Settlement, Build, Combat, World, and Progression.
3. A clear mapping of what should remain in existing modules and what should become new feature modules.
4. Explicit boundary rules between gameplay logic, presentation, replication, networking, and composition.
5. A list of architecture decisions that are important to lock before implementation starts.

Important constraints:
- Do not propose hidden cross-feature calls.
- Do not move transport or packet concerns into gameplay systems.
- Respect the project's Runtime/Logic vs Runtime/Presentation split.
- Prefer fail-fast architecture and explicit contracts.
- Call out risks where a wrong boundary now would create expensive rework later.

Deliverable format:
- Findings
- Recommended feature/module boundaries
- Decisions to lock now
- Risks if delayed
```

## Request 2. Vertical Slice Gameplay Loop Spec

```text
Read these files first:
- AGENTS.md
- ai/MVP_plan.md
- ai/GDD_Frontier_Buildlords.md
- ai/stage1_architecture_boundary_audit.md if it exists

Then inspect the current implemented feature entry points and major systems in:
- Assets/Scripts/StaticMlp/Features/Buildings
- Assets/Scripts/StaticMlp/Features/ResourcesInventoryMinimal
- Assets/Scripts/StaticMlp/Features/Player
- Assets/Scripts/StaticMlp/Features/Combat
- Assets/Scripts/StaticMlp/Features/Effects
- Assets/Scripts/StaticMlp/Features/Statuses
- Assets/Scripts/StaticMlp/Features/AiBots
- Assets/Scripts/StaticMlp/Features/AiTaskExecution

Task:
Produce a document at ai/stage1_vertical_slice_loop_spec.md.

What I need:
1. One locked MVP gameplay loop from damaged camp start to boss completion.
2. A step-by-step sequence of game states and transitions:
   - base start
   - resource gathering
   - construction
   - worker assignment
   - build preparation
   - expedition
   - reward return
   - counterattack
   - boss preparation
   - boss fight
3. For each step, define:
   - player goal
   - required game state
   - required system/domain ownership
   - output to the next step
4. A short list of mandatory gates that prove the loop is coherent.

Important constraints:
- Keep it MVP-sized and realistic for the current project.
- Do not invent broad post-MVP branches.
- Prefer a loop that reuses the current combat/building/network foundation.
- Highlight where the loop currently has missing systems.

Deliverable format:
- Vertical slice overview
- Sequential loop steps
- Required state transitions
- Missing links in the current project
```

## Request 3. MVP Content Manifest

```text
Read these files first:
- AGENTS.md
- ai/MVP_plan.md
- ai/GDD_Frontier_Buildlords.md
- ai/stage1_architecture_boundary_audit.md if it exists
- ai/stage1_vertical_slice_loop_spec.md if it exists

Inspect the existing project content and code references for:
- buildings
- resources
- combat archetypes
- statuses/effects
- enemies/bots
- views/prefabxml assets

Task:
Produce a document at ai/stage1_mvp_content_manifest.md.

What I need:
1. A locked MVP content list for:
   - buildings
   - resources
   - worker roles
   - build archetypes
   - build modules
   - enemy types
   - regions/objectives
   - rewards
2. A distinction between:
   - content already partially supported by the current codebase
   - content that requires net-new systems
3. A recommendation for the smallest viable content set that still proves the game hypothesis.
4. A dependency view showing which content must be defined first for later implementation to stay clean.

Important constraints:
- Be ruthless about scope control.
- Prefer content that aligns with current implemented combat/status/building foundations.
- Do not turn this into a wishlist.
- Flag any content that would force premature architecture expansion.

Deliverable format:
- Locked MVP content table
- Reuse vs new work
- Dependency notes
- Scope risks
```

## Request 4. Player-Facing Flow and UX Contract

```text
Read these files first:
- AGENTS.md
- ai/MVP_plan.md
- ai/GDD_Frontier_Buildlords.md
- ai/stage1_vertical_slice_loop_spec.md if it exists
- ai/stage1_mvp_content_manifest.md if it exists

Inspect any existing UI/MVC-related code in:
- Assets/Scripts/StaticMlp/Features/Mvc
- Assets/Scripts/StaticMlp/Features/Buildings/Runtime/Presentation
- Assets/Scripts/StaticMlp/Composition/Runtime/UI

Task:
Produce a document at ai/stage1_player_flow_and_ux_contract.md.

What I need:
1. The minimum player-facing flows required for the MVP:
   - base overview
   - building interaction
   - worker assignment
   - build preparation
   - expedition selection
   - reward return
   - threat/raid awareness
2. For each flow, define:
   - player intent
   - required information on screen
   - required gameplay state behind it
   - which feature owns the state
   - which layer owns the presentation
3. A minimal UI contract for MVP without deep mockups.
4. A list of UX flows that must be decided now because they affect architecture later.

Important constraints:
- Do not let MonoBehaviours become gameplay orchestration roots.
- Keep UI ownership explicit and ECS/MVC-friendly.
- Focus on information architecture and state ownership, not visual polish.
- Prefer flows that reduce ambiguity about “what should I do next?”

Deliverable format:
- MVP player flows
- UI/state ownership matrix
- Decisions to lock now
- UX architecture risks
```

## Request 5. Data and Config Architecture Plan

```text
Read these files first:
- AGENTS.md
- ai/MVP_plan.md
- ai/GDD_Frontier_Buildlords.md
- ai/stage1_architecture_boundary_audit.md if it exists
- ai/stage1_mvp_content_manifest.md if it exists

Inspect the current project for existing data/config patterns in:
- Building catalog
- Combat config
- Statuses config
- Any other runtime resource/config classes

Task:
Produce a document at ai/stage1_data_config_architecture.md.

What I need:
1. A recommended MVP data/config model for:
   - buildings
   - resources
   - recipes/production
   - worker roles
   - build modules
   - expeditions/regions
   - rewards
   - raids
2. A recommendation for what should be:
   - pure domain data
   - network adapter data
   - presentation adapter data
3. A migration path from current hardcoded catalogs/configs toward a scalable MVP-ready structure.
4. A list of IDs/contracts that must remain stable from the start.

Important constraints:
- Keep domain definitions free from prefab paths, transport ids, and UI concerns.
- Do not over-engineer for full release scope.
- Design for Stage 2-5 implementation speed without painting the project into a corner.

Deliverable format:
- Recommended config model
- Domain vs adapter split
- Stability requirements
- Migration steps
```

## Request 6. Stage 2-Ready Execution Backlog

```text
Read these files first:
- AGENTS.md
- ai/MVP_plan.md
- ai/stage1_architecture_boundary_audit.md
- ai/stage1_vertical_slice_loop_spec.md
- ai/stage1_mvp_content_manifest.md
- ai/stage1_player_flow_and_ux_contract.md
- ai/stage1_data_config_architecture.md

Task:
Produce a document at ai/stage1_stage2_ready_backlog.md.

What I need:
1. A Stage 2-ready backlog derived from Stage 1 decisions.
2. The backlog must be grouped by implementation sequence, not by abstract topic.
3. Each item should define:
   - goal
   - owning feature/module
   - prerequisites
   - architectural dependency
   - expected output artifact
4. Identify which tasks are safe to parallelize and which must stay sequential because of architecture coupling.
5. Mark the first 3-5 implementation tasks that should actually be handed to Codex after Stage 1 is approved.

Important constraints:
- Use the architecture decisions from Stage 1 as hard constraints.
- Avoid backlog items that would force later rewrites of module boundaries or data contracts.
- Keep the backlog actionable for the existing project layout.

Deliverable format:
- Ordered backlog
- Parallelization notes
- Critical path
- First implementation tasks
```

## Recommended Order

Run the requests in this order:

1. Architecture Boundary Audit
2. Vertical Slice Gameplay Loop Spec
3. MVP Content Manifest
4. Player-Facing Flow and UX Contract
5. Data and Config Architecture Plan
6. Stage 2-Ready Execution Backlog

## Why This Split Works

- Request 1 locks structural decisions before content and UX expand the scope.
- Request 2 locks the playable loop before feature implementation starts.
- Request 3 limits content sprawl and keeps the MVP testable.
- Request 4 prevents UI/state ownership mistakes that are expensive to unwind.
- Request 5 protects the project from ad-hoc hardcoded data growth.
- Request 6 converts all Stage 1 decisions into an implementation-ready path.
