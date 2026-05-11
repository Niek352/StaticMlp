# Research Plan: Architectural Problems Around Stage1 Bugs

## Why This Document Exists

During the Stage1 repair/build/expedition bugfix work, several bugs turned out to be symptoms of deeper architectural problems rather than isolated mistakes.

The recurring pattern was:

- runtime behavior diverged from green tests;
- netcode bugs appeared at entity spawn/despawn boundaries;
- feature flow gates were spread across multiple modules;
- small UX fixes required touching request handlers, projectors, HUD state, context panels, and server progression;
- test infrastructure repeatedly failed because it did not reflect the real runtime bootstrap and replication pipeline.

This document is a research plan for understanding those architectural issues before more Stage1 or multiplayer features are added.

## Observed Problem Clusters

### 1. Authoritative state is attached to transient replicated entities

Observed symptom:

- `Stage1SettlementProgression` lived on the construction site and then had to be moved to the finished building.
- Client HUD logic depended on `Stage1SettlementProgressionQuery.TryGetClientAnchor(...)`, but the underlying carrier entity changed during construction completion.
- A real runtime bug appeared because finished building spawn state and post-spawn state mutation did not match.

Architectural concern:

- camp progression is modeled as if it belongs to a building instance, but gameplay treats it as camp-global authoritative state.
- This creates identity instability exactly where replication is most fragile: spawn, initial state, despawn, and lookup.

### 2. Spawn-time replicated state and post-spawn mutation are not treated as a hard boundary

Observed symptom:

- finished building spawned without the authoritative progression state in its initial replicated snapshot;
- the state was written after spawn, which tests did not catch because test scopes bypassed the real spawn/apply pipeline.

Architectural concern:

- the codebase does not treat `spawn payload` as a strict contract.
- It is too easy to create a server entity whose critical state is attached after the initial broadcast.

### 3. Feature flow ownership is split across modules with no single Stage1 flow owner

Observed symptom:

- repair gate, worker assignment gate, build preparation gate, expedition availability gate, and HUD objective logic are split between `Settlement`, `Settlement.Workers`, `Build`, and `Frontier`.
- `BuildButton` became available before `WorkerAssigned`, while expedition required `WorkerAssigned` plus `BuildPrepared`.
- From the player’s point of view, the flow felt contradictory even when some of the code behaved as written.

Architectural concern:

- the Stage1 vertical slice has no single authoritative flow model.
- The game has local read-models for objective and UI gating, but no explicit domain owner for the sequence of required steps.

### 4. The same gameplay action can travel through multiple inconsistent paths

Observed symptom:

- world interaction build input and context-panel build click used different work amounts and different expectations.
- Fixing UX required touching request payload amounts, request handler clamps, and client projector behavior.

Architectural concern:

- gameplay intent is not normalized at the boundary.
- Different input surfaces can mean different gameplay operations even when they look like the same action to the player.

### 5. Runtime bootstrap and test bootstrap are too far apart

Observed symptom:

- many tests failed because assemblies, projections, view components, or feature registrations were missing in the test scopes.
- Some tests passed while runtime still failed because the tests did not exercise the real replication lifecycle.

Architectural concern:

- test worlds are hand-assembled in a way that duplicates composition knowledge.
- This makes tests expensive to maintain and weak as architectural regression protection.

### 6. Replication setup has too many sources of truth

Observed symptom:

- behavior depended on a combination of:
  - `NetworkEntityManifest`
  - generated replication registration
  - feature `RegisterNetworkEvents`
  - feature `RegisterPrefabs`
  - server spawn helpers
  - client spawn apply
- Small omissions caused runtime failures far away from the original change.

Architectural concern:

- replicated entity lifecycle is defined across too many disconnected places.
- This increases the chance of “green compile, broken runtime” changes.

### 7. Presentation code still leaks infrastructure complexity into feature work

Observed symptom:

- edit-mode view tests failed because runtime object destruction paths were not edit-safe;
- passive auto-attack presentation contained hidden assumptions about which abilities should produce tracers;
- context panel and HUD required additional explanatory hints because gameplay truth was not obvious from presentation state.

Architectural concern:

- presentation is not purely passive enough.
- It still encodes gameplay-specific assumptions and Unity lifecycle details in a way that increases feature complexity.

## Research Workstreams

### Workstream A: Stable authoritative anchor model

Goal:

- determine whether Stage1 camp progression, threat state, worker summary, expedition availability, and similar camp-global state should live on a dedicated server-owned camp anchor entity instead of being attached to buildable world entities.

Questions:

- Which current components are truly camp-global and should never migrate between entities?
- Which lookups currently assume “the anchor is whichever entity happens to carry `Stage1SettlementProgression`”?
- What runtime bugs disappear if the anchor becomes a stable entity with independent visual/building children?

Expected output:

- a target anchor model with a component ownership table;
- a migration list of components that must leave construction/building entities.

### Workstream B: Replicated spawn/despawn contract audit

Goal:

- define explicit rules for what must exist in initial spawn state versus what may be mutated later.

Questions:

- Which authoritative components are illegal to attach after `SpawnServerEntity(...)`?
- Which spawn helpers currently allow unsafe post-spawn critical state writes?
- Can we enforce a pattern where required initial replicated state is built entirely inside the spawn initializer?

Expected output:

- a spawn contract checklist;
- a list of critical replicated entities that currently violate the contract;
- candidate guardrails for spawn helpers and code review.

### Workstream C: Stage1 flow ownership and gating audit

Goal:

- make the Stage1 loop explicit as a domain contract, not an emergent result of scattered systems.

Questions:

- Which module is the authoritative owner of each Stage1 gate?
- Which gates are gameplay truth, and which are only UX constraints?
- Where do current HUD objectives and button interactability disagree with actual gameplay progression?

Expected output:

- a “Stage1 gate ownership matrix” mapping each user-visible step to one authoritative owner;
- a list of contradictory or redundant gates;
- a proposed normalized progression contract for repair -> worker -> build prep -> expedition.

### Workstream D: Intent normalization and command semantics

Goal:

- ensure identical user actions translate into identical gameplay intent regardless of input surface.

Questions:

- Which actions currently exist in both direct world input and MVC/UI paths?
- Where are action constants duplicated between controllers, request handlers, projectors, and gameplay systems?
- Which actions need shared command profiles or domain-owned constants?

Expected output:

- a list of duplicated action semantics;
- a proposal for shared per-action command profiles or domain constants;
- a rule for what belongs in controller/UI code versus gameplay/request code.

### Workstream E: Runtime/test parity

Goal:

- reduce false confidence from tests that do not exercise the same bootstrap, replication registration, and spawn/apply lifecycle as runtime.

Questions:

- Which current test scopes are most divergent from the real composition root?
- Which runtime bugs could not be caught because tests bypassed actual networked entity spawn/apply behavior?
- Can feature test scopes be rebuilt around shared bootstrap fixtures instead of manual per-test registration?

Expected output:

- a test infrastructure gap list;
- a target fixture strategy for server/client worlds;
- a short list of “must go through real replication lifecycle” regression test categories.

### Workstream F: Presentation boundary audit

Goal:

- reduce gameplay and lifecycle assumptions inside view/presentation code.

Questions:

- Which presentation systems currently hardcode gameplay ability assumptions or domain rules?
- Which view parts are unsafe in edit mode or overly coupled to runtime-only lifecycle?
- Which UI read-models are missing explicit explanation because gameplay state is too indirect?

Expected output:

- a boundary audit of presentation systems and view parts;
- a list of view/presentation rules that should be standardized across the project.

## Research Order

Recommended order:

1. Workstream A: stable anchor model
2. Workstream B: spawn/despawn contract
3. Workstream C: Stage1 gate ownership
4. Workstream E: runtime/test parity
5. Workstream D: intent normalization
6. Workstream F: presentation boundary audit

Reason:

- A and B are the deepest netcode/state-identity problems.
- C determines whether the Stage1 loop itself is coherent.
- E is needed so future fixes are actually protected.
- D and F are important, but they depend on the upstream ownership model being clear first.

## Research Success Criteria

This research is successful only if it ends with explicit architectural decisions, not just observations.

At minimum it should produce:

- one stable camp anchor model decision;
- one spawn/despawn replication contract for critical state;
- one authoritative owner per Stage1 gate;
- one test/bootstrap strategy that mirrors runtime composition closely enough to catch spawn/apply bugs;
- one rule set for normalizing gameplay intent across world input and UI input.

If those decisions are not locked, the project will likely keep producing the same class of bugs under different feature names.
