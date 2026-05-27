---
phase: 02-resource-flow-economy
research: 02-08
type: research
domain: unity-static-ecs
---

<objective>
Research the barter and merchant design boundary before implementation.

Purpose: Trading/barter depends on factions, item pools, NPC/merchant ownership, and progression facts that are not fully owned by Phase 2. This research decides what can be specified now and what must wait for Phase 3-4.
Output: `.planning/phases/02-resource-flow-economy/02-08-FINDINGS.md` with recommended barter architecture and deferral boundaries.
</objective>

<execution_context>
@.agents/skills/create-plans/workflows/research-phase.md
@.agents/skills/create-plans/templates/summary.md
@.agents/skills/staticmlp-code-writing/SKILL.md
@.agents/skills/staticmlp-networked-feature/SKILL.md
</execution_context>

<context>
@AGENTS.md
@docs/Roadmap по GDD.md
@.planning/phases/02-resource-flow-economy/02-07-SUMMARY.md
@Assets/Scripts/StaticMlp/Features/Settlement
@Assets/Scripts/StaticMlp/Features/Npc
@Assets/Scripts/StaticMlp/Features/Progression
@Assets/Scripts/StaticMlp/Features/Loadout
</context>

<research_questions>
1. Which feature should own merchant identity and barter offer lifecycle: Settlement, Npc, Progression, or a future dedicated merchant/faction feature?
2. Should Phase 2 implement only static `BarterOfferDefinition` contracts, or should all merchant runtime wait until NPC/faction systems exist?
3. What data must barter read from Settlement.Contracts without creating a parallel resource/item catalog?
4. How should barter offers be accepted in multiplayer: typed replicated event, request/result handler, or settlement-owned operation?
5. Which item/build-system dependencies make barter unsafe to implement before Phase 4?
</research_questions>

<constraints>
- Do not implement barter runtime during this research.
- Do not create a merchant feature only to satisfy Phase 2 unless ownership is clear.
- Do not add currency; roadmap says exchange items for resources rather than currency.
- Do not put packet serialization, raw inbox/outbox, or transport calls into gameplay systems.
- Cross-feature writes must go through typed events or owner-owned request handlers.
</constraints>

<expected_findings>
The findings should recommend one of these outcomes:
- Defer full barter runtime until NPC/faction/item systems exist, and add only Phase 2 offer-definition contracts if useful.
- Implement a minimal settlement-owned barter board only if it can operate entirely on existing Settlement resource contracts and has no NPC/item dependency.
- Split barter into a later Phase 3/4 plan with exact owner feature and typed event boundaries.
</expected_findings>

<output>
After research, create `.planning/phases/02-resource-flow-economy/02-08-FINDINGS.md`:

# Phase 2 Research 8: Barter Boundary Findings

**[Substantive one-liner with the recommendation]**

## Recommendation
[Chosen path and rationale]

## Ownership Boundary
[Feature owner, read dependencies, write path]

## Safe Phase 2 Scope
[What can be done now, if anything]

## Deferred Scope
[What waits for Phase 3/4 and why]

## Follow-up Plan
[If implementation is recommended, name the next PLAN.md to create]
</output>
