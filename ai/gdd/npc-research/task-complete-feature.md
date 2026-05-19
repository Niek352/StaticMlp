# Task Complete Feature - NPC Design Lock Completion

## Goal

Bring the current NPC feature work into a stable state against `01-design-lock.md` and the NPC research tasks.

This task is not a new gameplay expansion. It is a completion and correction pass for the foundation already added on the `design-lock` branch.

## Current State

The branch already adds a useful foundation:

- NPC contracts and split `Npc.Contracts` / `Npc.Logic` assemblies.
- NPC definition catalog and validation.
- NPC roster records as server-authoritative replicated ECS state.
- Extraction, rescue, and incubation server-side pipelines.
- Settlement worker integration with `NpcTag` and `NpcIdentity`.
- Resource family contracts and initial settlement resource catalog.
- Build/equipment slot kinds and slot rule validation helpers.

The work is directionally aligned with Design Lock, but it is not yet complete enough to treat as production-ready foundation.

## Blocking Fixes

### 1. Track generated NPC replication files

`ReplicatedComponentRegistration.Generated.cs` references:

```text
NpcIdentityReplication
NpcRosterRecordReplication
```

The generated files currently exist in the working tree under NPC `Generated` folders but are not tracked by git.

Fix:

- Include generated `.cs` and `.meta` files for `NpcIdentityReplication`.
- Include generated `.cs` and `.meta` files for `NpcRosterRecordReplication`.
- Verify a clean checkout compiles in Unity.

Do not hand-write generated code. Regenerate through the project codegen workflow if needed.

### 2. Make extraction single-consume

Accepted extraction currently creates a captured roster record but leaves the target extractable.

Required behavior:

- A valid extraction must not be repeatable against the same target.
- After accepted extraction, consume NPC-owned extraction state.
- If the extracted target belongs to another feature, emit an owner-feature event instead of mutating foreign feature state directly.

Allowed minimal fix if `ExtractableState` is NPC-owned:

```text
Delete ExtractableState
Delete ExtractionTargetTag
```

Do not hide duplicate extraction with client-side UI assumptions.

### 3. Validate interaction permission for extraction and rescue

The current handlers only check that the source peer has a player.

Required behavior:

- Reject extraction when the requesting player cannot interact with the target.
- Reject rescue when the requesting player cannot interact with the rescue site.
- Use server-authoritative player position/state.
- Prefer existing `ServerPeerPlayers.IsPlayerNear(...)` style validation when a target position component exists.

If a target has no authoritative position component, stop and define the correct owner feature contract instead of accepting remote interaction by GID alone.

## Architecture Rework

### 4. Move request/result contracts to the correct contract boundary

The typed request/result events are network-facing gameplay contracts. They should be readable by client intent systems and generated event code without depending on NPC internal logic.

Evaluate moving these from `Npc.Logic/Contracts` to `Npc.Contracts/Events` or another approved contracts bucket:

- `ExtractNpcRequestEvent`
- `ExtractNpcResultEvent`
- `RescueNpcRequestEvent`
- `RescueNpcResultEvent`

Keep handlers in `Npc.Logic/Requests`.

### 5. Remove unnecessary `Settlement.Logic` dependency from `Npc.Logic`

`Npc.Logic` currently references `StaticMlp.Features.Settlement.Logic` to use `ResourceCatalog` in incubation recipes.

Design Lock says Settlement owns resource ids, families, catalog validation, and storage; other features may read stable resource contracts from `Settlement.Contracts`.

Fix direction:

- Move stable resource ids/catalog access needed by other features into `Settlement.Contracts`, or
- Add a narrow settlement-owned public contract that exposes stable resource ids without exposing settlement logic internals.

`Npc.Logic` should not depend on ordinary settlement simulation logic just to define incubation recipe costs.

### 6. Keep generated output reviewable

The branch moves some generated replication output into feature-local `Generated` folders while other generated registries remain global.

Before merging:

- Confirm the codegen convention is intentional.
- Ensure all generated files are tracked.
- Ensure `.Generated.cs` diffs are produced by codegen, not manually edited.
- Keep generated changes separate enough to review.

## Missing Design Lock Scope

### 7. Complete resource source of truth

Current implementation defines `ResourceFamily`, but the concrete catalog only contains `Wood` and `Stone`.

Design Lock wants the project to have a source of truth for at least:

- `Raw`
- `Flow`
- `Refined`
- `Progression`
- `Stability`

Completion options:

- Add at least one stable placeholder resource definition per family, or
- Explicitly document that only family enum stabilization is in this branch and create follow-up tasks for concrete resources.

Do not create a parallel resource catalog outside Settlement.

### 8. Add startup/bootstrap validation path

Current validation mostly happens in static catalog constructors and tests.

Design Lock asks for startup validation through gameplay world initialization.

Add a clear validation path, either:

- explicit feature bootstrap validation system/resource registration, or
- a documented composition-time validation call owned by the relevant feature.

It must fail fast for:

- duplicate ids
- missing NPC classes
- missing acquisition paths
- invalid resource families
- invalid module slot kinds
- missing recipe/resource references

Do not convert invalid config into silent runtime fallback.

### 9. Finish slot activation foundation

The branch adds slot kinds and slot limit rules, but Design Lock acceptance also requires active module state.

Needed follow-up:

- ECS state for active modules/loadout.
- Server-authoritative activate/deactivate request path.
- Validation that a module cannot be activated into the wrong slot kind.
- Validation that slot limits cannot be exceeded.
- Query path for active modules through ECS state.

Do not treat `BuildModuleDefinition.SlotType` compatibility mapping as the long-term activation model.

### 10. Clarify incubation job lifecycle

Current incubation completion consumes `NpcIncubationJobState` and creates a roster record.

Before considering the pipeline complete:

- Define where incubation jobs are started.
- Validate resource cost payment.
- Validate station tier requirement.
- Emit `NpcIncubationJobStartedEvent` from the actual start path.
- Ensure completed jobs cannot be processed twice.

Resource/station validation is server-authoritative economy logic, not UI state.

### 11. Clarify specialist progression effect

Design Lock says `Specialist` must not be just `+1 worker`.

Current code defines specialist roles and rescue/incubation paths, but no progression effect exists yet.

Needed follow-up:

- Define what accepted specialist acquisition unlocks or improves.
- Implement it in the owning progression/economy feature by consuming NPC acquisition facts.
- Do not hide recipe unlocks directly inside rescue/extraction handlers.

## Tests To Add Or Strengthen

Add tests for:

- Accepted extraction cannot create two roster records from the same target.
- Extraction rejects a target outside player interaction range.
- Rescue rejects a site outside player interaction range.
- Clean world/bootstrap registration includes NPC generated replication types.
- `Npc.Logic` does not require settlement simulation logic for stable resource ids.
- Slot activation rejects wrong slot kind.
- Slot activation rejects over-limit loadouts.
- Active module list is queryable through ECS state.

Update existing tests that only assert request success so they also assert the created ECS state:

- roster record definition id
- class
- acquisition path
- roster state
- created tick

## Acceptance Criteria

- Clean checkout compiles after Unity codegen/import.
- NPC acquisition is server-authoritative and cannot be duplicated by repeated valid requests.
- Client-originated NPC requests validate interaction permission on the server.
- NPC contracts needed by client intent systems live in a contracts assembly.
- `Npc.Logic` does not depend on unrelated settlement simulation internals for stable resource ids.
- Resource families, NPC definitions, and slot rules fail fast through a startup/bootstrap validation path.
- Slot activation has server-owned ECS state and validation.
- No MonoBehaviour gameplay state is introduced.
- No prefab assets are generated by AI agents.
